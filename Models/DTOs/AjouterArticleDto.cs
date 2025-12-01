namespace PanierService.Models.DTOs
{
    public class AjouterArticleDto
    {
        public int ArticleId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal Prix { get; set; }
        public int Quantite { get; set; } = 1;
    }
}