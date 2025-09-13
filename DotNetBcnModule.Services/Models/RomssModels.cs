using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for ROMSS inventory data
    /// </summary>
    public class RomssInventoryModel
    {
        public DateTime DtInventario { get; set; }
        public string BoInvFoto { get; set; }
        public string UmInvFoto { get; set; }
        public string NbAlmacen { get; set; }
        public string NbProducto { get; set; }
        public decimal NbAPI60 { get; set; }
        public decimal TotalNSV { get; set; }
        public decimal BombeableNSV { get; set; }
        public decimal RemanenteNSV { get; set; }
        public string VUM { get; set; }
        public decimal TotalNSW { get; set; }
        public decimal PumpableNSW { get; set; }
        public decimal RemanenteNSW { get; set; }
        public string WUM { get; set; }
        public string BoVoBo { get; set; }
        public string NbMuestra { get; set; }
        public DateTime? DtMuestra { get; set; }
    }

    /// <summary>
    /// Model for ROMSS movement data
    /// </summary>
    public class RomssMovementModel
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
        public string NmProdDestino { get; set; }
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