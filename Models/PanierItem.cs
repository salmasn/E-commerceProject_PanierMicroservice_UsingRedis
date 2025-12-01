namespace PanierService.Models
{
    public class PanierItem
    {
        public int ArticleId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal Prix { get; set; }
        public int Quantite { get; set; }
        public decimal Total => Prix * Quantite;
    }
}