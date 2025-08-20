using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBcnModule.Services.Models
{
    public class BcnInventoryPhotoModel
    {
        public DateTime DtInventario { get; set; }
        public string NbRecSAP { get; set; }
        public string NbRecFuente { get; set; }
        public string NmRecProducto { get; set; }
        public string NmRecAlmacen { get; set; }
        public string BoVoBoAlmacen { get; set; }
        public decimal CantTotal { get; set; }
        public decimal CantBombeableLU { get; set; }
        public decimal CantBombeableCC { get; set; }
        public decimal CantRemanente { get; set; }
        public decimal CantBloqueada { get; set; }
        public string IdUMFotoInventario { get; set; }
        public string NbMuestra { get; set; }
        public string DtMuestra { get; set; }
    }
}
