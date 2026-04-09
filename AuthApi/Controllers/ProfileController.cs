using System.Security.Claims;
using AuthApi.Data;
using AuthApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // nécessite un JWT valide
    public class ProfileController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public ProfileController(AuthDbContext context)
        {
            _context = context;
        }

        // --------- DTOs internes ---------

        public class ProfileDto
        {
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
        }

        public class UpdateProfileRequest
        {
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string? CurrentPassword { get; set; }
            public string? NewPassword { get; set; }
        }

        // --------- Helpers ---------

        private DbSet<ApplicationUser> Users => _context.Set<ApplicationUser>();

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            // On essaie plusieurs types de claims pour trouver l'ID
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("id")?.Value;

            if (idClaim == null) return null;
            if (!int.TryParse(idClaim, out var id)) return null;

            return await Users.FindAsync(id);
        }

        // --------- GET /api/profile ---------

        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            return new ProfileDto
            {
                FullName = user.FullName,
                Email = user.Email
            };
        }

        // --------- PUT /api/profile ---------

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // Mise à jour nom + email
            user.FullName = request.FullName;
            user.Email = request.Email;

            // ---- Changement de mot de passe sécurisé (BCrypt) ----
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                {
                    return BadRequest("Current password is required to change password.");
                }

                // Vérifier l'ancien mot de passe
                var ok = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
                if (!ok)
                {
                    return BadRequest("Invalid current password.");
                }

                // Stocker le hash BCrypt du nouveau mot de passe
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
