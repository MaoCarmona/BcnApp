using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for BCN Balance Difference data
    /// </summary>
    public class BcnBalanceDifferenceModel
    {
        public DateTime DtMovimientoIni { get; set; }
        public DateTime DtMovimientoFin { get; set; }
        public string NbMovimientoTag { get; set; }
        public string TpMovimientoCls { get; set; }
        public string NmRecOrigen { get; set; }
        public string NbProdOrigen { get; set; }
        public string NmProdOrigen { get; set; }
        public string NmRecDestino { get; set; }
        public string NbProdDestino { get; set; }
        public string NmProdDestino2 { get; set; }
        public decimal VlCantVolFuente { get; set; }
        public decimal VlCantVolReconciliado { get; set; }
        public decimal VlCantVolConciliado { get; set; }
        public string IdUMCantVol { get; set; }
        public decimal VlCantMasFuente { get; set; }
        public decimal VlCantMasReconciliado { get; set; }
        public decimal VlCantMasConciliado { get; set; }
        public string IdUMCantMas { get; set; }
        public decimal NbAPI60 { get; set; }
        public string NbMuestra { get; set; }
        public string NumPedido { get; set; }
        public string PosPedido { get; set; }
        public string IdUMPedido { get; set; }
        public string NmEstado { get; set; }
        public DateTime? DtCargado { get; set; }
        public string NmUsrAuditoria { get; set; }
        public decimal VlDiferenciaVol { get; set; }
        public decimal VlDiferenciaMas { get; set; }
        public string TpDiferencia { get; set; }
    }
} 