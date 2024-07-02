using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult CrearPago2([FromForm] AsientoModels detallesPago)
        {
            try
            {
                Console.WriteLine("Intentando crear pago...");

                if (Conexion.Conectar())
                {
                    Console.WriteLine("Conexión establecida correctamente.");
                    Conexion.myCompany.StartTransaction();

                    
                    int sucursal = detallesPago.Sucursal;
                    string comentario = detallesPago.memo;
                    string referencia= detallesPago.referencia;
                    List<AsientoContableLinea> lineas = detallesPago.LineasContables;

                   
                    var resultado = Crear(sucursal, lineas,comentario,referencia);

                    if (resultado != -1)
                    {
                        
                        Conexion.myCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
                        Console.WriteLine("Transacción completada con éxito.");
                    }
                    else
                    {
                        
                        Conexion.myCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                        Console.WriteLine("Error al crear el asiento contable.");
                        ViewData["Error"] = "Error al crear el asiento contable.";
                    }
                }
                else
                {
                    Console.WriteLine("Error de conexión a SAP Business One.");
                    ViewData["Error"] = "Error de conexión a SAP Business One.";
                    detallesPago.CuentasContables = ObtenerCuentasContables();
                    detallesPago.Sucursales = ObtenerSucursales();
                    return View(detallesPago);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error general: " + ex.Message);
                ViewData["Error"] = "Error general: " + ex.Message;
                detallesPago.CuentasContables = ObtenerCuentasContables();
                detallesPago.Sucursales = ObtenerSucursales();
                return View(detallesPago);
            }
            finally
            {
                Conexion.Desconectar();
            }

            
            return RedirectToAction("Index");
        }

        private int Crear(int sucursal, List<AsientoContableLinea> lineas,string memo,string referencia)
        {
            try
            {
                if (Conexion.Conectar())
                {
                    Console.WriteLine("Conectado a SAP");

                    SAPbobsCOM.Company company = Conexion.myCompany;
                    SAPbobsCOM.JournalEntries asientoContable = (SAPbobsCOM.JournalEntries)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);

                    // Configuración del asiento contable
                    asientoContable.ReferenceDate = DateTime.Now;  
                    asientoContable.TaxDate = DateTime.Now;       
                    asientoContable.DueDate = DateTime.Now;       
                    asientoContable.Memo =memo;

                    
                    foreach (var linea in lineas)
                    {
                        asientoContable.Lines.AccountCode = linea.AccountCode;
                        asientoContable.Lines.Debit = linea.Debit;
                        asientoContable.Lines.Credit = linea.Credit;
                        asientoContable.Lines.LineMemo = linea.LineMemo;
                        asientoContable.Lines.BPLID = sucursal;
                        asientoContable.Lines.Add();
                    }

                    // Intentar agregar el asiento contable a SAP Business One
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

    }
}
