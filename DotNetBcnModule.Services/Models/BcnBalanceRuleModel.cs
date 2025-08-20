using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for BCN Balance Rule data
    /// </summary>
    public class BcnBalanceRuleModel
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
        public string TpReglaBalance { get; set; }
        public decimal VlFactorBalance { get; set; }
        public string IdUMBalance { get; set; }
    }
} 