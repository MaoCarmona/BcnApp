using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// BCN Consolidated Inventory Photo Model
    /// </summary>
    public class BcnConsolidatedInventoryPhotoModel
    {
        public DateTime DtInventario { get; set; }
        public string NbRecSAP { get; set; }
        public string NmProducto { get; set; }
        public decimal CantTotal { get; set; }
        public decimal CantBombeableLU { get; set; }
        public decimal CantBombeableCC { get; set; }
        public decimal CantRemanente { get; set; }
        public decimal CantBloqueada { get; set; }
        public string IdUMFotoInventario { get; set; }
    }
}