namespace OdooScanner.Models
{
    public class StockPicking
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PickingTypeId { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string Origin { get; set; } = string.Empty;
    }

    public class StockMove
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double ProductQty { get; set; }
        public double QuantityDone { get; set; }
        public string State { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
    }
}
