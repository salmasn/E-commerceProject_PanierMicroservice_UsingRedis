using Microsoft.AspNetCore.Mvc;
using PanierService.Models.DTOs;
using PanierService.Services;

namespace PanierService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PanierController : ControllerBase
    {
        private readonly IPanierService _panierService;
        private readonly ILogger<PanierController> _logger;

        public PanierController(IPanierService panierService, ILogger<PanierController> logger)
        {
            _panierService = panierService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/panier/{panierId} - Créer un panier avec un ID spécifique
        /// </summary>
        [HttpPost("{panierId}")]
        public async Task<ActionResult<PanierResponseDto>> CreerPanierAvecId(string panierId)
        {
            try
            {
                _logger.LogInformation($"=== CreerPanierAvecId: {panierId} ===");

                try
                {
                    var panierExistant = await _panierService.ObtenirPanierAsync(panierId);
                    _logger.LogWarning($"Le panier {panierId} existe déjà");
                    return Ok(panierExistant);
                }
                catch (KeyNotFoundException) { }

                var panier = await _panierService.CreerPanierAvecIdAsync(panierId);
                _logger.LogInformation($"✅ Panier créé: {panierId}");
                return Ok(panier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Erreur création panier {panierId}");
                return StatusCode(500, new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: api/panier/{panierId} - Obtenir un panier
        /// </summary>
        [HttpGet("{panierId}")]
        public async Task<ActionResult<PanierResponseDto>> ObtenirPanier(string panierId)
        {
            try
            {
                var panier = await _panierService.ObtenirPanierAsync(panierId);
                _logger.LogInformation($"Panier récupéré: {panierId} - {panier.Items?.Count ?? 0} items");
                return Ok(panier);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Panier {panierId} introuvable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur récupération panier {PanierId}", panierId);
                return StatusCode(500, "Erreur serveur");
            }
        }

        /// <summary>
        /// POST: api/panier/{panierId}/ajouter - Ajouter un article
        /// </summary>
        [HttpPost("{panierId}/ajouter")]
        public async Task<ActionResult<PanierResponseDto>> AjouterArticle(
            string panierId,
            [FromBody] AjouterArticleDto dto)
        {
            try
            {
                _logger.LogInformation($"Ajout article {dto.ArticleId} au panier {panierId}");
                var panier = await _panierService.AjouterArticleAsync(panierId, dto);
                return Ok(panier);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Panier {panierId} introuvable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur ajout article");
                return StatusCode(500, "Erreur serveur");
            }
        }

        /// <summary>
        /// ✅ PUT: api/panier/{panierId}/quantite - Modifier la quantité d'un article
        /// </summary>
        [HttpPut("{panierId}/quantite")]
        public async Task<ActionResult<PanierResponseDto>> ModifierQuantite(
            string panierId,
            [FromBody] ModifierQuantiteDto dto)
        {
            try
            {
                _logger.LogInformation($"=== ModifierQuantite ===");
                _logger.LogInformation($"   PanierId: {panierId}");
                _logger.LogInformation($"   ArticleId: {dto.ArticleId}");
                _logger.LogInformation($"   NouvelleQuantite: {dto.NouvelleQuantite}");

                var panier = await _panierService.ModifierQuantiteAsync(panierId, dto);

                _logger.LogInformation($"✅ Quantité modifiée avec succès");
                return Ok(panier);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Panier ou article introuvable: {ex.Message}");
                return NotFound($"Panier {panierId} ou article introuvable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur modification quantité");
                return StatusCode(500, new { message = $"Erreur: {ex.Message}" });
            }
        }

        /// <summary>
        /// DELETE: api/panier/{panierId}/article/{articleId} - Supprimer un article
        /// </summary>
        [HttpDelete("{panierId}/article/{articleId}")]
        public async Task<ActionResult<PanierResponseDto>> SupprimerArticle(string panierId, int articleId)
        {
            try
            {
                var resultat = await _panierService.SupprimerArticleAsync(panierId, articleId);

                if (resultat)
                {
                    var panier = await _panierService.ObtenirPanierAsync(panierId);
                    _logger.LogInformation($"Article {articleId} supprimé du panier {panierId}");
                    return Ok(panier);
                }

                return NotFound(new { message = "Article non trouvé dans le panier" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur suppression article");
                return StatusCode(500, "Erreur serveur");
            }
        }

        /// <summary>
        /// DELETE: api/panier/{panierId} - Vider le panier
        /// </summary>
        [HttpDelete("{panierId}")]
        public async Task<ActionResult> ViderPanier(string panierId)
        {
            try
            {
                var resultat = await _panierService.ViderPanierAsync(panierId);

                if (resultat)
                {
                    _logger.LogInformation($"Panier {panierId} vidé");
                    return Ok(new { message = "Panier vidé" });
                }

                return NotFound($"Panier {panierId} introuvable");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur vidage panier");
                return StatusCode(500, "Erreur serveur");
            }
        }
    }
}