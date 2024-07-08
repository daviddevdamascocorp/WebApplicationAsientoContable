using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using SAPbobsCOM;
using WebApplicationAsientoContable.Controllers;
using WebApplicationAsientoContable.Models;
using static WebApplicationAsientoContable.Controllers.Conexionsap;
using static WebApplicationAsientoContable.Models.AsientoModels;

namespace WebApplicationAsientoContable.Controllers
{
    public class CrearAsiento : Controller
    {

        public IActionResult CrearAsient()
        {
            try
            {
                var Sucursales = ObtenerSucursales();
                var cuentas = ObtenerCuentasContables();
                var model = new AsientoModels

                {
                    Sucursales = Sucursales,
                    CuentasContables = cuentas,
                   
                };

                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
                    Console.WriteLine("SuccessMessage: " + ViewBag.SuccessMessage);
                }
                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                    Console.WriteLine("errorprueba: " + ViewBag.ErrorMessage);
                }


                return View(model);
            }


            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar la vista: " + ex.Message;
                return View(new AsientoModels());
            }
        }


        private List<SelectListItem> ObtenerSucursales()
        {
            List<SelectListItem> sucursales = new List<SelectListItem>();

            try
            {
                if (Conexion.Conectar())
                {
                    Console.WriteLine("Conexión establecida correctamente para obtener sucursales");
                    SAPbobsCOM.Company company =Conexion.myCompany;
                    Recordset recordset = (Recordset)company.GetBusinessObject(BoObjectTypes.BoRecordset);

                    string query = "SELECT BPLID, BPLNAME FROM OBPL";
                    recordset.DoQuery(query);

                    while (!recordset.EoF)
                    {
                        string bplid = recordset.Fields.Item("BPLID").Value.ToString();
                        string bplname = recordset.Fields.Item("BPLNAME").Value.ToString();

                        sucursales.Add(new SelectListItem
                        {
                            Value = bplid,
                            Text = bplname
                        });

                        recordset.MoveNext();
                    }
                }
                else
                {
                    Console.WriteLine("Error de conexión a SAP Business One al intentar obtener sucursales");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener sucursales: " + ex.Message);
            }
            finally
            {
                Conexion.Desconectar();
            }

            return sucursales;
        }


        private List<SelectListItem> ObtenerCuentasContables()
        {
            List<SelectListItem> cuentasContables = new List<SelectListItem>();

            try
            {
                if (Conexion.Conectar())
                {
                    Console.WriteLine("Conexión establecida correctamente para obtener cuentas contables.");
                    SAPbobsCOM.Company company = Conexion.myCompany;
                    Recordset recordset = (Recordset)company.GetBusinessObject(BoObjectTypes.BoRecordset);

                    string query = "SELECT AcctCode, AcctName FROM OACT";
                    recordset.DoQuery(query);

                    while (!recordset.EoF)
                    {
                        string accountCode = recordset.Fields.Item("AcctCode").Value.ToString();
                        string accountName = recordset.Fields.Item("AcctName").Value.ToString();

                        cuentasContables.Add(new SelectListItem
                        {
                            Value = accountCode,
                            Text = accountName
                        });

                        recordset.MoveNext();
                    }
                }
                else
                {
                    Console.WriteLine("Error de conexión a SAP Business One al intentar obtener cuentas contables.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener cuentas contables: " + ex.Message);
            }
            finally
            {
                Conexion.Desconectar();
            }

            return cuentasContables;
        }


        [HttpPost]
        public JsonResult EnviarFactor([FromBody] Mydate fecha)
        {
            
            var factor = obtenerFactor(Convert.ToDateTime(fecha.Fecha));
            var resultado = factor.ToString();
            Console.WriteLine(resultado);
            return Json(new { resultado });
        }

        private string obtenerFactor(DateTime fecha)
        {
            string factor = string.Empty;
            try
            {
                if (Conexion.Conectar())
                {
                    SAPbobsCOM.Company company = Conexion.myCompany;
                    SAPbobsCOM.Recordset companyRecordSet = company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

                    string queryStore = $"select rate from ortt where RateDate = '{fecha}' AND Currency='USD'";
                    companyRecordSet.DoQuery(queryStore);

                    if (!companyRecordSet.EoF)
                    {
                        factor = companyRecordSet.Fields.Item("rate").Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener la descripción: " + ex.Message);
            }
            finally
            {
                Conexion.Desconectar();
            }

            return factor;
        }

        private int Crear(int sucursal, DateTime fecha ,List<AsientoContableLinea> lineas, string memo, string referencia)
        {
            try
            {
                if (Conexion.Conectar())
                {
                    Console.WriteLine("Conectado a SAP");

                    SAPbobsCOM.Company company = Conexion.myCompany;
                    SAPbobsCOM.JournalEntries asientoContable = (SAPbobsCOM.JournalEntries)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);

                    // Configuración del asiento contable
                    asientoContable.ReferenceDate = fecha;
                    asientoContable.TaxDate =fecha;
                    asientoContable.DueDate = fecha;
                    asientoContable.Memo = memo;
                    asientoContable.Reference = referencia;

                    foreach (var linea in lineas)
                    {
                        asientoContable.Lines.AccountCode = linea.AccountCode;
                        asientoContable.Lines.Debit = linea.Debit;
                        asientoContable.Lines.Credit = linea.Credit;
                        asientoContable.Lines.LineMemo = linea.LineMemo;
                        asientoContable.Lines.BPLID = sucursal;
                        asientoContable.Lines.Reference1 = linea.Referencia1;
                        asientoContable.Lines.Add();
                    }

                    int result = asientoContable.Add();

                    if (result != 0)
                    {
                        int errorCode;
                        string errorMessage;
                        company.GetLastError(out errorCode, out errorMessage);
                        Console.WriteLine("Error: " + errorCode + " - " + errorMessage);
                        return -1;
                    }
                    else
                    {
                        Console.WriteLine("Asiento contable creado exitosamente");
                        return int.Parse(company.GetNewObjectKey());
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Error de conexión a SAP Business One";
                    return -1;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return -1;
            }
            finally
            {
                Conexion.Desconectar();
            }
        }


        [HttpPost]
        public IActionResult CrearAsientodesdejson([FromBody] AsientoContable jsonAsiento)
        {
            try
            {
                AsientoContable asientoContable = jsonAsiento;

                int sucursal = asientoContable.Sucursal;
                string comentario = asientoContable.comentario;
                string referencia = asientoContable.referencia;
                DateTime fecha = asientoContable.fecha;
                List<AsientoContableLinea> lineas = asientoContable.LineasContables;

                int resultado = Crear(sucursal, fecha,lineas, comentario, referencia);

                if (resultado == -1)
                {
                    return BadRequest(new { success = false, message = "Error al crear el asiento contable." });
                }

                return Json(new { success = true, message = "Asiento contable creado exitosamente. DocEntry: " + resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error: " + ex.Message });
            }
        }

    }
}
