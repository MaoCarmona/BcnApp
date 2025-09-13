using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for web service cost data
    /// </summary>
    public class WsCostModel
    {
        public int? Item { get; set; }
        public int IdRegCosto { get; set; }
        public string IdMsgCostos { get; set; }
        public DateTime DtContabilizacion { get; set; }
        public string TxMovimiento { get; set; }
        public string TpObjCosto { get; set; }
        public string IdObjCosto { get; set; }
        public string ObjPlantaPool { get; set; }
        public string ObjColector { get; set; }
        public string ObjVolTotal { get; set; }
        public string ObjEstadistico { get; set; }
        public string NmProducto { get; set; }
        public decimal VlContable { get; set; }
        public string IdUM { get; set; }
    }

    /// <summary>
    /// Model for web service logistic movement data
    /// </summary>
    public class WsLogisticMovementModel
    {
        public int? Item { get; set; }
        public int IdRegMovLogistico { get; set; }
        public DateTime DtContabilizacion { get; set; }
        public int IdMovimientoReg { get; set; }
        public DateTime DtMovimientoIni { get; set; }
        public DateTime DtMovimientoFin { get; set; }
        public string TpMovimiento { get; set; }
        public string NmRecOrigen { get; set; }
        public string NmProdOrigen { get; set; }
        public string NmRecDestino { get; set; }
        public string NmProdDestino { get; set; }
        public string NbMovimientoCls { get; set; }
        public string NmMovimientoCls { get; set; }
        public string NbGM { get; set; }
        public string TpInventario { get; set; }
        public string NumPedido { get; set; }
        public string PosPedido { get; set; }
        public string IdUMPedido { get; set; }
        public string NbCenLogOrigen { get; set; }
        public string NbAlmLogOrigen { get; set; }
        public string NmAlmLogOrigen { get; set; }
        public string NmProdLogOrigen { get; set; }
        public string NbProdLogOrigen { get; set; }
        public string NbCenLogDestino { get; set; }
        public string NmAlmLogDestino { get; set; }
        public string NmProdLogDestino { get; set; }
        public string NbAlmLogDestino { get; set; }
        public string NbProdLogDestino { get; set; }
        public decimal VlContable { get; set; }
        public string IdUM { get; set; }
        public string nbCentroCosto { get; set; }
        public string IdAtrCalidad { get; set; }
        public string VlAtrCalidad { get; set; }
        public string IdUMAtrCalidad { get; set; }
        public string VlQCI { get; set; }
        public string IdUMQCI { get; set; }
        public string UpQCI { get; set; }
        public string IdPropiedad { get; set; }
        public DateTime? DtProcesamiento { get; set; }
        public string TxProcesamiento { get; set; }
        public string NmEstado { get; set; }
        public string TxAtrCalidad { get; set; }
        public string TxQCI { get; set; }
    }
}