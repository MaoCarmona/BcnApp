using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Web Service Logistic Inventory Model
    /// </summary>
    public class WsLogisticInventoryModel
    {
        public DateTime DtContabilizacion { get; set; }
        public string NmRecAlmacen { get; set; }
        public string NmRecProducto { get; set; }
        public string NbCenLog { get; set; }
        public string NbAlmLog { get; set; }
        public string NbMaterial { get; set; }
        public decimal VlContable { get; set; }
        public string IdUM { get; set; }
        public string NmUsrAuditoria { get; set; }
        public DateTime? DtUsrAuditoria { get; set; }
    }
}