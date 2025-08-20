using System;

namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// Logistic Balance Model
    /// </summary>
    public class LogisticBalanceModel
    {
        public int? Item { get; set; }
        public string IdRecurso { get; set; }
        public string NbRecurso { get; set; }
        public string NmRecurso { get; set; }
        public string InvIni { get; set; }
        public string VlEntradas { get; set; }
        public string VlSalidas { get; set; }
        public string InvFin { get; set; }
        public string VlDesbalance { get; set; }
        public string UM { get; set; }
    }
}