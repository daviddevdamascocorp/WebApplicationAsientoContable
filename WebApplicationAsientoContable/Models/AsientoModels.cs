using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplicationAsientoContable.Models
{
    public class AsientoModels
    {
        public class AsientoContableLinea
        {
            public string AccountCode { get; set; }
            public double Debit { get; set; }
            public double Credit { get; set; }
            public string LineMemo { get; set; }
        }

        public string memo { get; set; }
        public string referencia { get; set; }
        public int Sucursal { get; set; }

        public List<SelectListItem>? Sucursales { get; set; }

        public List<SelectListItem>? CuentasContables { get; set; }

        public List<AsientoContableLinea> LineasContables { get; set; }

    }
}
