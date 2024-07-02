using SAPbobsCOM;

namespace WebApplicationAsientoContable.Controllers
{
        public class Conexionsap
        {
            public static class Conexion
            {
                public static Company myCompany = null;

                public static bool Conectar()
                {
                    try
                    {
                        myCompany = new Company();
                        myCompany.Server = "ERPSAP\\SAPB1";
                        myCompany.CompanyDB = "DAMASCO_PRODUCTIVA";
                        myCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2019;
                        myCompany.UserName = "emahmud";
                        myCompany.Password = "Zizou..10";
                        int ret = myCompany.Connect();
                        string errMsg = myCompany.GetLastErrorDescription();
                        int ErrNo = myCompany.GetLastErrorCode();
                        if (ErrNo != 0)
                        {
                            // Manejar el error de conexión
                            Console.WriteLine($"Error de conexión SAP: {errMsg}");
                            return false;
                        }
                        else
                        {
                            // La conexión fue exitosa
                            Console.WriteLine("Conexión SAP establecida");
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar otras excepciones
                        Console.WriteLine($"Error: {ex.Message}");
                        return false;
                    }
                }

                public static void Desconectar()
                {
                    try
                    {
                        if (myCompany != null && myCompany.Connected)
                        {
                            myCompany.Disconnect();
                            Console.WriteLine("Desconexión SAP exitosa");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar posibles errores al desconectar
                        Console.WriteLine($"Error al desconectar: {ex.Message}");
                    }
                }
            }
        }

}
