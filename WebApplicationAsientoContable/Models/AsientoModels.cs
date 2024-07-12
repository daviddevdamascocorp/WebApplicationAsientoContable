using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata;

namespace WebApplicationAsientoContable.Models
{
    public class AsientoModels
    {
        public class Mydate
        {
            public string Fecha { get; set; }
            
        }
        public class AsientoContable
        {
            public int Sucursal { get; set; }
            public string comentario { get; set; }
            public string referencia { get; set; }

            public string referencia2 { get; set; }
            public List<AsientoContableLinea> LineasContables { get; set; }

            public DateTime fecha { get; set; }
        }

        public class AsientoContableLinea
        {
            public string AccountCode { get; set; }
            public double Debit { get; set; }
            public double Credit { get; set; }
            public string LineMemo { get; set; }

            public int Sucursal { get; set; }
            public string  Referencia1 { get; set; }
        }

        public List<SelectListItem>? Sucursales { get; set; }

        public List<SelectListItem>? CuentasContables { get; set; }

        public List<AsientoContableLinea> LineasContables { get; set; }






    }
}
