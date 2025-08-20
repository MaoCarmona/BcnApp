using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for AORA inventory data
    /// </summary>
    public class AoraInventoryModel
    {
        public int NbRN { get; set; }
        public DateTime DtInventario { get; set; }
        public int IdRecOrigen { get; set; }
        public string NmRecOrigen { get; set; }
        public int IdProdOrigen { get; set; }
        public string NmProdOrigen { get; set; }
        public decimal VFuente { get; set; }
        public string VUM { get; set; }
        public decimal WFuente { get; set; }
        public string WUM { get; set; }
    }

    /// <summary>
    /// Model for AORA movement data
    /// </summary>
    public class AoraMovementModel
    {
        public int NbRN { get; set; }
        public string Tag { get; set; }
        public DateTime DtMovIni { get; set; }
        public DateTime DtMovFin { get; set; }
        public int IdRecOrigen { get; set; }
        public string NmRecOrigen { get; set; }
        public int IdProdOrigen { get; set; }
        public string NmProdOrigen { get; set; }
        public int IdRecDestino { get; set; }
        public string NmRecDestino { get; set; }
        public int IdProdDestino { get; set; }
        public string NmProdDestino { get; set; }
        public decimal VFuente { get; set; }
        public decimal VReconciliado { get; set; }
        public string VUM { get; set; }
        public decimal WFuente { get; set; }
        public decimal WReconciliado { get; set; }
        public string WUM { get; set; }
    }

    /// <summary>
    /// Model for AORA flow data
    /// </summary>
    public class AoraFlowModel
    {
        public int NbRN { get; set; }
        public DateTime DtFlujo { get; set; }
        public string Tag { get; set; }
        public int IdRecOrigen { get; set; }
        public string NmRecOrigen { get; set; }
        public int IdRecDestino { get; set; }
        public string NmRecDestino { get; set; }
        public int IdProdOrigen { get; set; }
        public string NmProdOrigen { get; set; }
        public decimal VFuente { get; set; }
        public decimal VReconciliado { get; set; }
        public string VUM { get; set; }
        public decimal WFuente { get; set; }
        public decimal WReconciliado { get; set; }
        public string WUM { get; set; }
    }
}