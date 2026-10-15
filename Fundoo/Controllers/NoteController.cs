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
        public NoteController(INoteBL noteBL) => _noteBL = noteBL;

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
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var result = await _noteBL.CreateNote(userId, model);
            return StatusCode(201, new { success = true, message = "Note created", data = result });
        }

        // CHANGED: now has a "type" dropdown (Active / Archive / Trash)
        [HttpGet("ViewNotes")]
        public async Task<IActionResult> ViewNotes([FromQuery] NoteViewType type = NoteViewType.Active)
        {
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var result = await _noteBL.ViewNotes(userId, type);
            return Ok(new { success = true, message = $"{type} notes fetched", data = result });
        }

        [HttpGet("ViewNotesById")]
        public async Task<IActionResult> ViewNotesById([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var result = await _noteBL.ViewNotesById(userId, model);
            return result == null ? Fail(404, "Note not found")
                                  : Ok(new { success = true, message = "Note fetched", data = result });
        }

        [HttpPut("EditNotes")]
        public async Task<IActionResult> EditNotes([FromBody] EditNotesModel model)
        {
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var result = await _noteBL.EditNotes(userId, model);
            return result == null ? Fail(404, "Note not found")
                                  : Ok(new { success = true, message = "Note updated", data = result });
        }

        [HttpDelete("DeleteNotes")]
        public async Task<IActionResult> DeleteNotes([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var deleted = await _noteBL.DeleteNotes(userId, model);
            return deleted ? Ok(new { success = true, message = "Note deleted permanently" })
                           : Fail(404, "Note not found");
        }

        [HttpPatch("ArchiveUnarchive")]
        public async Task<IActionResult> ArchiveUnarchive([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var result = await _noteBL.ArchiveUnarchive(userId, model);
            return result == null ? Fail(404, "Note not found")
                                  : Ok(new
                                  {
                                      success = true,
                                      message = result.IsArchived ? "Note archived" : "Note unarchived",
                                      data = result
                                  });
        }

        [HttpPatch("TrashUntrash")]
        public async Task<IActionResult> TrashUntrash([FromQuery] NotesIdModel model)
        {
            if (!TryGetUserId(out var userId)) return Fail(401, "Invalid token");
            var result = await _noteBL.TrashUntrash(userId, model);
            return result == null ? Fail(404, "Note not found")
                                  : Ok(new
                                  {
                                      success = true,
                                      message = result.IsDeleted ? "Note moved to trash" : "Note restored",
                                      data = result
                                  });
        }
    }
}