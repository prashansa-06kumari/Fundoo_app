using System.Security.Claims;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;

namespace Fundoo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly INoteBL _noteBL;
        private readonly ILogger<NoteController> _logger;   // NEW

        // CHANGED: logger added to the constructor
        public NoteController(INoteBL noteBL, ILogger<NoteController> logger)
        {
            _noteBL = noteBL;
            _logger = logger;
        }

        // Reads the logged-in user's id from the JWT claims
        private bool TryGetUserId(out int userId)
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub")
                        ?? User.FindFirstValue("UserId");
            return int.TryParse(value, out userId);
        }

        private IActionResult Fail(int status, string message) =>
            StatusCode(status, new { success = false, message });

        [HttpPost("CreateNote")]
        public async Task<IActionResult> CreateNote([FromBody] NoteCreationModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("CreateNote: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var result = await _noteBL.CreateNote(userId, model);
            _logger.LogInformation("CreateNote: user {UserId} created note {NoteId}", userId, result.NoteId);
            return StatusCode(201, new { success = true, message = "Note created", data = result });
        }

        // CHANGED: now has a "type" dropdown (Active / Archive / Trash)
        [HttpGet("ViewNotes")]
        public async Task<IActionResult> ViewNotes([FromQuery] NoteViewType type = NoteViewType.Active)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("ViewNotes: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var result = await _noteBL.ViewNotes(userId, type);
            _logger.LogInformation("ViewNotes: user {UserId} fetched {Count} {Type} notes", userId, result.Count, type);
            return Ok(new { success = true, message = $"{type} notes fetched", data = result });
        }

        [HttpGet("ViewNotesById")]
        public async Task<IActionResult> ViewNotesById([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("ViewNotesById: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var result = await _noteBL.ViewNotesById(userId, model);
            if (result == null)
            {
                _logger.LogWarning("ViewNotesById: note {NoteId} not found for user {UserId}", model.NoteId, userId);
                return Fail(404, "Note not found");
            }
            _logger.LogInformation("ViewNotesById: user {UserId} fetched note {NoteId}", userId, model.NoteId);
            return Ok(new { success = true, message = "Note fetched", data = result });
        }

        [HttpPut("EditNotes")]
        public async Task<IActionResult> EditNotes([FromBody] EditNotesModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("EditNotes: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var result = await _noteBL.EditNotes(userId, model);
            if (result == null)
            {
                _logger.LogWarning("EditNotes: note {NoteId} not found for user {UserId}", model.NoteId, userId);
                return Fail(404, "Note not found");
            }
            _logger.LogInformation("EditNotes: user {UserId} edited note {NoteId}", userId, model.NoteId);
            return Ok(new { success = true, message = "Note updated", data = result });
        }

        [HttpDelete("DeleteNotes")]
        public async Task<IActionResult> DeleteNotes([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("DeleteNotes: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var deleted = await _noteBL.DeleteNotes(userId, model);
            if (!deleted)
            {
                _logger.LogWarning("DeleteNotes: note {NoteId} not found for user {UserId}", model.NoteId, userId);
                return Fail(404, "Note not found");
            }
            _logger.LogInformation("DeleteNotes: user {UserId} permanently deleted note {NoteId}", userId, model.NoteId);
            return Ok(new { success = true, message = "Note deleted permanently" });
        }

        [HttpPatch("ArchiveUnarchive")]
        public async Task<IActionResult> ArchiveUnarchive([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("ArchiveUnarchive: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var result = await _noteBL.ArchiveUnarchive(userId, model);
            if (result == null)
            {
                _logger.LogWarning("ArchiveUnarchive: note {NoteId} not found for user {UserId}", model.NoteId, userId);
                return Fail(404, "Note not found");
            }
            _logger.LogInformation("ArchiveUnarchive: user {UserId} set note {NoteId} IsArchived={IsArchived}",
                userId, model.NoteId, result.IsArchived);
            return Ok(new
            {
                success = true,
                message = result.IsArchived ? "Note archived" : "Note unarchived",
                data = result
            });
        }

        [HttpPatch("TrashUntrash")]
        public async Task<IActionResult> TrashUntrash([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId))
            {
                _logger.LogWarning("TrashUntrash: invalid token, user id not found");
                return Fail(401, "Invalid token");
            }
            var result = await _noteBL.TrashUntrash(userId, model);
            if (result == null)
            {
                _logger.LogWarning("TrashUntrash: note {NoteId} not found for user {UserId}", model.NoteId, userId);
                return Fail(404, "Note not found");
            }
            _logger.LogInformation("TrashUntrash: user {UserId} set note {NoteId} IsDeleted={IsDeleted}",
                userId, model.NoteId, result.IsDeleted);
            return Ok(new
            {
                success = true,
                message = result.IsDeleted ? "Note moved to trash" : "Note restored",
                data = result
            });
        }
    }
}