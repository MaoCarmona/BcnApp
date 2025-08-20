using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for ARES Logistic Movement Processing Review - Option 04
    /// Maps to XTMOVIMIENTOSSAPBIC table
    /// </summary>
    public class AresLogisticProcessingReviewModel
    {
        public string IdMovimiento { get; set; }
        public string TxProcesamiento { get; set; }
        public DateTime? DtProcesamiento { get; set; }
        public DateTime DtContabilizacion { get; set; }
        public string Estado { get; set; }
    }

    /// <summary>
    /// Model for ARES Cost Movement Processing Review - Option 05
    /// Maps to XTCOSTOSSAPBIC table
    /// </summary>
    public class AresCostProcessingReviewModel
    {
        public string TpObjCostos { get; set; }
        public string IdObjCosto { get; set; }
        public string IdValEstadistico { get; set; }
        public DateTime DtContabilizacion { get; set; }
        public string TxProcesamiento { get; set; }
        public DateTime? DtProcesamiento { get; set; }
        public string Estado { get; set; }
    }

    /// <summary>
    /// DTO for ARES Logistic Movement Processing Review
    /// </summary>
    public class AresLogisticProcessingReviewDto
    {
        public long Item { get; set; }
        public string IdMovimiento { get; set; }
        public string DocumentoRespuesta { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaProcesamiento { get; set; }
        public DateTime FechaContabilizacion { get; set; }
    }

    /// <summary>
    /// DTO for ARES Cost Movement Processing Review
    /// </summary>
    public class AresCostProcessingReviewDto
    {
        public long Item { get; set; }
        public string TipoObjetoCosto { get; set; }
        public string IdObjetoCosto { get; set; }
        public string IdValorEstadistico { get; set; }
        public string DocumentoRespuesta { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaProcesamiento { get; set; }
        public DateTime FechaContabilizacion { get; set; }
    }
} 