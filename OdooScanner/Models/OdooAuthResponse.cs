namespace OdooScanner.Models
{
    public class OdooAuthResponse
    {
        public int? UserId { get; set; }
        public string? SessionId { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
