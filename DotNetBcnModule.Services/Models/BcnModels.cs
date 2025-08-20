using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for BCN inventory data
    /// </summary>
    public class BcnInventoryModel
    {
        public DateTime DtInventario { get; set; }
        public string NbRecSAP { get; set; }
        public string NbRecFuente { get; set; }
        public string NmRecProducto { get; set; }
        public string NmRecAlmacen { get; set; }
        public string BoVoBoAlmacen { get; set; }
        public string BoFotoInventario { get; set; }
        public string IdUMFotoInventario { get; set; }
        public decimal NbAPI60 { get; set; }
        public decimal CantVolTotal { get; set; }
        public decimal CantVolBombeable { get; set; }
        public decimal CantVolRemanente { get; set; }
        public string IdUMVolumen { get; set; }
        public decimal CantMasTotal { get; set; }
        public decimal CantMasBombeable { get; set; }
        public decimal CantMasRemanente { get; set; }
        public string IdUMMasa { get; set; }
        public string NbMuestra { get; set; }
        public string DtMuestra { get; set; }
        public string DtCargado { get; set; }
        public string NmUsrAuditoria { get; set; }
        public string NmEstado { get; set; }
    }

    /// <summary>
    /// Model for BCN movement data
    /// </summary>
    public class BcnMovementModel
    {
        public string NbMovimientoTag { get; set; }
        public string TpMovimientoCls { get; set; }
        public DateTime DtMovimientoIni { get; set; }
        public DateTime DtMovimientoFin { get; set; }
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
        public decimal NbAPI60_2 { get; set; }
        public string IdUMPedido { get; set; }
        public string NmEstado { get; set; }
        public DateTime? DtCargado { get; set; }
        public string NmUsrAuditoria { get; set; }
    }

    /// <summary>
    /// Model for BCN inventory comparison data
    /// </summary>
    public class BcnInventoryComparisonModel
    {
        public DateTime DtInventario { get; set; }
        public string NbRecSAP { get; set; }
        public string NmRecProducto { get; set; }
        public string NmRecAlmacen { get; set; }
        public string BoVoBoAlmacen { get; set; }
        public string BoFotoInventario { get; set; }
        public string IdUMFotoInventario { get; set; }
        public decimal NbAPI60 { get; set; }
        public decimal CantVolTotal { get; set; }
        public decimal CantVolBombeable { get; set; }
        public decimal CantVolRemanente { get; set; }
        public string IdUMVolumen { get; set; }
        public decimal CantMasTotal { get; set; }
        public decimal CantMasBombeable { get; set; }
        public decimal CantMasRemanente { get; set; }
        public string IdUMMasa { get; set; }
        public string NbMuestra { get; set; }
        public string DtMuestra { get; set; }
        public string DtCargado { get; set; }
        public string NmUsrAuditoria { get; set; }
        public string NmEstado { get; set; }
    }

    /// <summary>
    /// Model for BCN cost comparison data
    /// </summary>
    public class BcnCostComparisonModel
    {
        public int IdRegCosto { get; set; }
        public DateTime DtContabilizacion { get; set; }
        public string TxMovimiento { get; set; }
        public string TpObjCosto { get; set; }
        public string IdObjCosto { get; set; }
        public string IdValEstadistico { get; set; }
        public string NmProducto { get; set; }
        public decimal VlContabilizado { get; set; }
        public string IdUM { get; set; }
    }

    /// <summary>
    /// Model for BCN Balance Operativo data - Option 08
    /// Based on Python query BALANCEOPER
    /// </summary>
    public class BcnBalanceOperativoModel
    {
        public long NbRN { get; set; }
        public int IdRecurso { get; set; }
        public string NbRecurso { get; set; }
        public string NmRecurso { get; set; }
        public string UMBalance { get; set; }
        public string NmProductoIni { get; set; }
        public string NmProductoFin { get; set; }
        public decimal? InvIniVol { get; set; }
        public decimal? VlEntVol { get; set; }
        public decimal? VlSalVol { get; set; }
        public decimal? InvFinVol { get; set; }
        public decimal? VlDesbalanceVol { get; set; }
        public string UMVol { get; set; }
        public decimal? InvIniMas { get; set; }
        public decimal? VlEntMas { get; set; }
        public decimal? VlSalMas { get; set; }
        public decimal? InvFinMas { get; set; }
        public decimal? VlDesbalanceMas { get; set; }
        public string UMMas { get; set; }
        public decimal? SGInvFin { get; set; }
        public decimal? FC { get; set; }
    }
}