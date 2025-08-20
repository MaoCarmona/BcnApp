using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for BCN inventory detail data from mInventarios table
    /// </summary>
    public sealed class BcnInventoryDetailModel
{
    public string IdFuente { get; set; }
    public short? IdCaso { get; set; }
    public string TxCaso { get; set; }
    public DateTime? DtInventario { get; set; }
    public int? IdRecAlmacen { get; set; }
    public string NmRecAlmacen { get; set; }
    public string BoVoBoAlmacen { get; set; }
    public int? IdRecProducto { get; set; }
    public string NmRecProducto { get; set; }
    public string NbRecFuente { get; set; }
    public string NbRecSAP { get; set; }
    public string BoFotoInventario { get; set; }
    public string IdUMFotoInventario { get; set; }
    public decimal? CantVolTotal { get; set; }
    public decimal? CantVolBombeable { get; set; }
    public decimal? CantVolRemanente { get; set; }
    public string IdUMVolumen { get; set; }
    public decimal? CantMasTotal { get; set; }
    public decimal? CantMasBombeable { get; set; }
    public decimal? CantMasRemanente { get; set; }
    public string IdUMMasa { get; set; }
    public decimal? NbAPI60 { get; set; }
    public string NbMuestra { get; set; }
    public string DtMuestra { get; set; }
    public short? IdEstado { get; set; }
    public string NmEstado { get; set; }
    public DateTime DtCargado { get; set; }
    public string NmUsrAuditoria { get; set; }
    public DateTime? DtUsrAuditoria { get; set; }
}
} 