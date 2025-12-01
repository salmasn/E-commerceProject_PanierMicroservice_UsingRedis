using PanierService.Models;
using PanierService.Models.DTOs;

namespace PanierService.Services
{
    public interface IPanierService
    {
        Task<string> CreerNouveauPanierAsync();
        Task<PanierResponseDto> CreerPanierAvecIdAsync(string panierId); // ✅ AJOUTER
        Task<PanierResponseDto> ObtenirPanierAsync(string panierId);
        Task<PanierResponseDto> AjouterArticleAsync(string panierId, AjouterArticleDto dto);
        Task<PanierResponseDto> ModifierQuantiteAsync(string panierId, ModifierQuantiteDto dto);
        Task<bool> SupprimerArticleAsync(string panierId, int articleId);
        Task<bool> ViderPanierAsync(string panierId);
    }
}