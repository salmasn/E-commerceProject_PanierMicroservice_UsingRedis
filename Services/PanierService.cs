using PanierService.Models;
using PanierService.Models.DTOs;

namespace PanierService.Services
{
    public class PanierServiceImpl : IPanierService
    {
        private readonly IRedisService _redis;
        private readonly ILogger<PanierServiceImpl> _logger;
        private const int EXPIRATION_JOURS = 7;

        public PanierServiceImpl(IRedisService redis, ILogger<PanierServiceImpl> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<string> CreerNouveauPanierAsync()
        {
            var panier = new Panier();
            var key = $"panier:{panier.Id}";
            await _redis.SetAsync(key, panier, TimeSpan.FromDays(EXPIRATION_JOURS));
            _logger.LogInformation("Nouveau panier créé: {PanierId}", panier.Id);
            return panier.Id;
        }

        public async Task<PanierResponseDto> CreerPanierAvecIdAsync(string panierId)
        {
            _logger.LogInformation("=== CreerPanierAvecIdAsync: {PanierId} ===", panierId);

            var panier = new Panier
            {
                Id = panierId,
                Items = new List<PanierItem>(),
                DateCreation = DateTime.UtcNow,
                DerniereModification = DateTime.UtcNow
            };

            var key = $"panier:{panierId}";
            await _redis.SetAsync(key, panier, TimeSpan.FromDays(EXPIRATION_JOURS));

            _logger.LogInformation("✅ Panier créé: {PanierId}", panierId);

            return MapToDto(panier);
        }

        public async Task<PanierResponseDto> ObtenirPanierAsync(string panierId)
        {
            var key = $"panier:{panierId}";
            var panier = await _redis.GetAsync<Panier>(key);

            if (panier == null)
            {
                _logger.LogWarning("Panier non trouvé: {PanierId}", panierId);
                throw new KeyNotFoundException($"Panier {panierId} introuvable");
            }

            return MapToDto(panier);
        }

        public async Task<PanierResponseDto> AjouterArticleAsync(string panierId, AjouterArticleDto dto)
        {
            var key = $"panier:{panierId}";
            var panier = await _redis.GetAsync<Panier>(key);

            if (panier == null)
            {
                throw new KeyNotFoundException($"Panier {panierId} introuvable");
            }

            var itemExistant = panier.Items.FirstOrDefault(i => i.ArticleId == dto.ArticleId);

            if (itemExistant != null)
            {
                itemExistant.Quantite += dto.Quantite;
            }
            else
            {
                panier.Items.Add(new PanierItem
                {
                    ArticleId = dto.ArticleId,
                    Nom = dto.Nom,
                    Prix = dto.Prix,
                    Quantite = dto.Quantite
                });
            }

            panier.DerniereModification = DateTime.UtcNow;
            await _redis.SetAsync(key, panier, TimeSpan.FromDays(EXPIRATION_JOURS));

            _logger.LogInformation("Article {ArticleId} ajouté au panier {PanierId}", dto.ArticleId, panierId);

            return MapToDto(panier);
        }

        /// <summary>
        /// ✅ MÉTHODE CORRIGÉE : Modifier la quantité d'un article
        /// </summary>
        public async Task<PanierResponseDto> ModifierQuantiteAsync(string panierId, ModifierQuantiteDto dto)
        {
            _logger.LogInformation($"=== ModifierQuantiteAsync ===");
            _logger.LogInformation($"   PanierId: {panierId}");
            _logger.LogInformation($"   ArticleId: {dto.ArticleId}");
            _logger.LogInformation($"   NouvelleQuantite: {dto.NouvelleQuantite}");

            var key = $"panier:{panierId}";
            var panier = await _redis.GetAsync<Panier>(key);

            if (panier == null)
            {
                _logger.LogWarning($"❌ Panier {panierId} introuvable");
                throw new KeyNotFoundException($"Panier {panierId} introuvable");
            }

            var item = panier.Items.FirstOrDefault(i => i.ArticleId == dto.ArticleId);

            if (item == null)
            {
                _logger.LogWarning($"❌ Article {dto.ArticleId} introuvable dans le panier");
                throw new KeyNotFoundException($"Article {dto.ArticleId} introuvable dans le panier");
            }

            // ✅ Modifier la quantité
            if (dto.NouvelleQuantite <= 0)
            {
                _logger.LogInformation($"Suppression de l'article {dto.ArticleId} (quantité <= 0)");
                panier.Items.Remove(item);
            }
            else
            {
                _logger.LogInformation($"Modification quantité: {item.Quantite} -> {dto.NouvelleQuantite}");
                item.Quantite = dto.NouvelleQuantite;
            }

            // ✅ Sauvegarder dans Redis
            panier.DerniereModification = DateTime.UtcNow;
            await _redis.SetAsync(key, panier, TimeSpan.FromDays(EXPIRATION_JOURS));

            _logger.LogInformation($"✅ Quantité modifiée avec succès dans Redis");

            return MapToDto(panier);
        }

        public async Task<bool> SupprimerArticleAsync(string panierId, int articleId)
        {
            var key = $"panier:{panierId}";
            var panier = await _redis.GetAsync<Panier>(key);

            if (panier == null)
            {
                return false;
            }

            var item = panier.Items.FirstOrDefault(i => i.ArticleId == articleId);

            if (item != null)
            {
                panier.Items.Remove(item);
                panier.DerniereModification = DateTime.UtcNow;
                await _redis.SetAsync(key, panier, TimeSpan.FromDays(EXPIRATION_JOURS));
                _logger.LogInformation($"Article {articleId} supprimé du panier {panierId}");
                return true;
            }

            return false;
        }

        public async Task<bool> ViderPanierAsync(string panierId)
        {
            var key = $"panier:{panierId}";
            var resultat = await _redis.DeleteAsync(key);

            if (resultat)
            {
                _logger.LogInformation("Panier {PanierId} vidé", panierId);
            }

            return resultat;
        }

        private PanierResponseDto MapToDto(Panier panier)
        {
            return new PanierResponseDto
            {
                PanierId = panier.Id,
                Items = panier.Items,
                NombreArticles = panier.NombreArticles,
                Total = panier.Total,
                DerniereModification = panier.DerniereModification
            };
        }
    }
}