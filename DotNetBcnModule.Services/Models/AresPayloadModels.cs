using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// ARES Inventory Payload Model - Option 01
    /// </summary>
    public class AresInventoryPayload
    {
        public string dtContabilizacion { get; set; }
        public string idRecurso { get; set; }
        public string idProducto { get; set; }
        public string idCELO { get; set; }
        public string idALMACEN { get; set; }
        public string idMaterial { get; set; }
        public string vlContable { get; set; }
        public string idUMContable { get; set; }
        public string idUsrAuditoria { get; set; }
        public string dtUsrAuditoria { get; set; }
    }

    /// <summary>
    /// ARES Movement Payload Model - Option 02
    /// </summary>

    public class AresMovementPayload
    {
        public string IDMessage { get; set; }

        public string dtContabilizacion { get; set; }

        public string idMovimiento { get; set; }

        public string dtMovimientoIni { get; set; }

        public string dtMovimientoFin { get; set; }

        public string tpMovimiento { get; set; }

        public string clsMovimiento { get; set; }

        public string TransactionCodeSAP { get; set; }

        public string StockTypeSAP { get; set; }

        public string NumPedido { get; set; }

        public string PosPedido { get; set; }

        public string idRecOrigen { get; set; }

        public string idProdOrigen { get; set; }

        public string idRecDestino { get; set; }

        public string idProdDestino { get; set; }

        public string idSRCCELO { get; set; }

        public string idSRCALMACEN { get; set; }

        public string idSRCMaterial { get; set; }

        public string idDSTCCELO { get; set; }

        public string idDSTALMACEN { get; set; }

        public string idDSTMaterial { get; set; }

        public string vlContable { get; set; }

        public string idUMContable { get; set; }

        public string idCentroCosto { get; set; }

        public string txEstadoEnvio { get; set; }

        public string vlAtrCalidad { get; set; }

        public string idUMAtrCalidad { get; set; }

        public string vlCantidaadQCI { get; set; }

        public string idUMCantidadQCI { get; set; }

        public string txCantidadQCI { get; set; }

        public string IdPropiedad { get; set; }

        public string jsMovimiento { get; set; }

        public string idUsrAuditoria { get; set; }

        public string dtUsrAuditoria { get; set; }
    }


    /// <summary>
    /// ARES Cost Payload Model - Option 03
    /// </summary>
    public class AresCostPayload
    {
        public string idMessage { get; set; }
        
        public string tpObjCostos { get; set; }
        
        public string txMovimiento { get; set; }
        
        public string dtContabilizacion { get; set; }
        
        public string idObjCosto { get; set; }
        
        public string idValEstadistico { get; set; }
        
        public string nmProducto { get; set; }
        
        public string idUM { get; set; }
        
        public string vlContabilizado { get; set; }
        
        public string jsMovimiento { get; set; }
        
        public string idUsrAuditoria { get; set; }
        
        public string dtUsrAuditoria { get; set; }
    }
} 