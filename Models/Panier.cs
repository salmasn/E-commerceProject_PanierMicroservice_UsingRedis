namespace PanierService.Models
{
    public class Panier
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = "default-user"; // Pour l'instant
        public List<PanierItem> Items { get; set; } = new();
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime DerniereModification { get; set; } = DateTime.UtcNow;

        // Propriétés calculées
        public int NombreArticles => Items.Sum(i => i.Quantite);
        public decimal Total => Items.Sum(i => i.Total);
    }
}