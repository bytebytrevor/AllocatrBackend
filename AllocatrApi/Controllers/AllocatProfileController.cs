using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/allocats/profiles")]
[Authorize]
public class AllocatProfileController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly AllocatProfileService _allocatProfileService;
    private readonly UserDocumentService _userDocumentService;

    public AllocatProfileController(
        UserManager<AllocatrUser> userManager,
        AllocatProfileService allocatProfileService,
        UserDocumentService userDocumentService)
    {
        _userManager = userManager;
        _allocatProfileService = allocatProfileService;
        _userDocumentService = userDocumentService;
    }

    [HttpPost]
    public async Task<ActionResult<MyAllocatProfileDto>> CreateAllocatProfile(
        [FromBody] CreateAllocatProfileDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var profile = await _allocatProfileService.CreateAllocatProfileAsync(
            user.Id,
            dto);

        return CreatedAtAction(
            nameof(GetAllocatProfileById),
            new { allocatUserId = profile.AllocatrUserId },
            profile);
    }

    [HttpGet("me")]
    public async Task<ActionResult<MyAllocatProfileDto>> GetMyAllocatProfile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var profile = await _allocatProfileService.GetMyAllocatProfileAsync(
            user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<MyAllocatProfileDto>> UpdateMyAllocatProfile(
        [FromBody] UpdateAllocatProfileDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var profile = await _allocatProfileService.UpdateAllocatProfileAsync(
            user.Id,
            dto);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPatch("me/visibility")]
    public async Task<IActionResult> SetVisibility(
        [FromBody] SetAllocatVisibilityDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var updated = await _allocatProfileService.SetVisibilityAsync(
            user.Id,
            dto.IsVisible);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("me/availability")]
    public async Task<IActionResult> SetAvailability(
        [FromBody] SetAllocatAvailabilityDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var updated = await _allocatProfileService.SetAvailabilityAsync(
            user.Id,
            dto.Availability);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("me/documents")]
    public async Task<ActionResult<IReadOnlyList<UserDocumentDto>>> GetMyDocuments()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var documents = await _userDocumentService.GetMyDocumentsAsync(
            user.Id);

        return Ok(documents);
    }

    [HttpPost("me/documents")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserDocumentDto>> UploadDocument(
        [FromForm] UserDocumentType documentType,
        [FromForm] IFormFile file)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        try
        {
            var document = await _userDocumentService.UploadDocumentAsync(
                user.Id,
                documentType,
                file);

            return CreatedAtAction(
                nameof(GetDocumentDownloadUrl),
                new { documentId = document.Id },
                document);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid document",
                detail: exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Document upload conflict",
                detail: exception.Message);
        }
    }

    [HttpGet("me/documents/{documentId:guid}/download")]
    public async Task<IActionResult> GetDocumentDownloadUrl(Guid documentId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        var url = await _userDocumentService.GetDownloadUrlAsync(
            user.Id,
            documentId);

        if (url == null)
        {
            return NotFound();
        }

        return Ok(new { url });
    }

    [HttpDelete("me/documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid documentId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return AllocatAccessRequired();
        }

        try
        {
            var deleted = await _userDocumentService.DeleteDocumentAsync(
                user.Id,
                documentId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Document cannot be deleted",
                detail: exception.Message);
        }
    }

    [AllowAnonymous]
    [HttpGet("{allocatUserId:guid}")]
    public async Task<ActionResult<AllocatProfileDto>> GetAllocatProfileById(
        Guid allocatUserId)
    {
        var profile = await _allocatProfileService.GetPublicAllocatProfileAsync(
            allocatUserId);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AllocatProfileListItemDto>>>
        GetAllAllocatProfiles([FromQuery] AllocatProfileSearchDto query)
    {
        var profiles = await _allocatProfileService.GetAllAllocatProfilesAsync(
            query);

        return Ok(profiles);
    }

    private ObjectResult AllocatAccessRequired()
    {
        return Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Allocat access required",
            detail: "This account is not registered as an Allocat.");
    }
}