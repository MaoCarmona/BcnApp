using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Model for BCN movement data from Movimientos_vw view
    /// </summary>
    public sealed class BcnMovementViewModel
    {
        public string idFuente { get; set; }
        public short? idCaso { get; set; }
        public string txCaso { get; set; }
        public short? idMovimientoSgn { get; set; }
        public int idMovimientoReg { get; set; }
        public string nbMovimientoTag { get; set; }
        public string tpMovimientoCls { get; set; }
        public DateTime? dtMovimientoIni { get; set; }
        public DateTime? dtMovimientoFin { get; set; }
        public int? idRecOrigen { get; set; }
        public string nbRecOrigen { get; set; }
        public string nmRecOrigen { get; set; }
        public string tpRecOrigen { get; set; }
        public string nbProdOrigen { get; set; }
        public int? idProdOrigen { get; set; }
        public string nmProdOrigen { get; set; }
        public string tpProdOrigen { get; set; }
        public int? idRecDestino { get; set; }
        public string nbRecDestino { get; set; }
        public string nmRecDestino { get; set; }
        public string tpRecDestino { get; set; }
        public string nbProdDestino { get; set; }
        public int? idProdDestino { get; set; }
        public string nmProdDestino { get; set; }
        public string tpProdDestino { get; set; }
        public decimal? vlCantVolFuente { get; set; }
        public decimal? vlCantVolReconciliado { get; set; }
        public decimal? vlCantVolConciliado { get; set; }
        public string idUMCantVol { get; set; }
        public decimal? vlCantMasFuente { get; set; }
        public decimal? vlCantMasReconciliado { get; set; }
        public decimal? vlCantMasConciliado { get; set; }
        public string idUMCantMas { get; set; }
        public string nbAPI60 { get; set; }
        public string nbMuestra { get; set; }
        public string numPedido { get; set; }
        public string posPedido { get; set; }
        public string idUMPedido { get; set; }
        public string nbCentroCosto { get; set; }
        public short? idEstado { get; set; }
        public string nmEstado { get; set; }
        public DateTime dtCargado { get; set; }
        public string nmUsrAuditoria { get; set; }
        public DateTime? dtUsrAuditoria { get; set; }
        public string txXML { get; set; }
    }
} 