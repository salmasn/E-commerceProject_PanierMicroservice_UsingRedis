namespace PanierService.Models.DTOs
{
    public class PanierResponseDto
    {
        public string PanierId { get; set; } = string.Empty;
        public List<PanierItem> Items { get; set; } = new();
        public int NombreArticles { get; set; }
        public decimal Total { get; set; }
        public DateTime DerniereModification { get; set; }
    }
}