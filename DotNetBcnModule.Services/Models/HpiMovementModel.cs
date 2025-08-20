using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// HPI (Provisional Inventory Enablement) Movement Model
    /// </summary>
    public class HpiMovementModel
    {
        public string Tag { get; set; }
        public DateTime DtMovIni { get; set; }
        public DateTime DtMovFin { get; set; }
        public string TpCategoria { get; set; }
        public string IdRecOrigen { get; set; }
        public string TpRecOrigen { get; set; }
        public string IdProdOrigen { get; set; }
        public string IdRecDestino { get; set; }
        public string TpRecDestino { get; set; }
        public string IdProdDestino { get; set; }
        public decimal VFuente { get; set; }
        public string VUM { get; set; }
        public decimal WFuente { get; set; }
        public string WUM { get; set; }
        public decimal API { get; set; }
        public string NbMuestra { get; set; }
        public string NumPedido { get; set; }
        public string PosPedido { get; set; }
        public string UomPedido { get; set; }
        public decimal CantPedido { get; set; }
        public int IdMovSigno { get; set; }
    }
}