namespace NetBcnModule.Services.Models
{
    /// <summary>
    /// ARES configuration model
    /// </summary>
    public class AresConfig
    {
        public string idUsr { get; set; }
        public string pwUsr { get; set; }
        public string txURL { get; set; }
        public string txMetodoInventario { get; set; }
        public string txMetodoMovimiento { get; set; }
        public string txMetodoCosto { get; set; }
        public string TS { get; set; }
    }
} 