using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// BCN Consolidated Balance Model
    /// </summary>
    public class BcnConsolidatedBalanceModel
    {
        public string IdRecurso { get; set; }
        public string NbRecurso { get; set; }
        public string NmRecurso { get; set; }
        public string UMBalance { get; set; }
        public decimal InvIniVol { get; set; }
        public decimal VlVolEntVol { get; set; }
        public decimal VlVolSalVol { get; set; }
        public decimal InvFinVol { get; set; }
        public decimal VlDesbalanceVol { get; set; }
        public string UMVol { get; set; }
        public decimal InvIniMas { get; set; }
        public decimal VlVolEntMas { get; set; }
        public decimal VlVolSalMas { get; set; }
        public decimal InvFinMas { get; set; }
        public decimal VlDesbalanceMas { get; set; }
        public string UMMas { get; set; }
    }
}