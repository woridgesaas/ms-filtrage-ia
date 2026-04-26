using Microsoft.AspNetCore.Mvc;
using ms_filtering_ia.DTOs;
using ms_filtering_ia.Services;

namespace ms_filtering_ia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilteringController : ControllerBase
{
    private readonly IFilteringService _filteringService;
    private readonly IFilteringProfileService _filteringProfileService;
    private readonly IMinioService _minioService;

    public FilteringController(
    IFilteringService filteringService,
    IMinioService minioService,
    IFilteringProfileService filteringProfileService)
    {
        _filteringService = filteringService;
        _minioService = minioService;
        _filteringProfileService = filteringProfileService;
    }

    // ── NOUVEAU — appelé par Angular "Filtrer par IA" ──────────────────
    [HttpPost("analyze-all")]
    public async Task<IActionResult> AnalyzeAll([FromBody] OfferForm offer)
    {
        var results = await _filteringService.AnalyzeAllCvsAsync(offer);
        return Ok(results);
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResults()
    {
        var results = await _filteringService.GetAllResultsAsync();
        return Ok(results);
    }
    [HttpGet("cv-url")]
    public async Task<IActionResult> GetCvUrl([FromQuery] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("fileName requis");

        var url = await _minioService.GetPresignedUrlAsync(fileName);
        return Ok(new { url });
    }
    // Nouvel endpoint à ajouter
    [HttpPost("profiles")]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateFilteringProfileRequest request)
    {
        var userCin = HttpContext.Items["UserCin"]?.ToString();

        if (string.IsNullOrEmpty(userCin))
            return Unauthorized(new
            {
                timestamp = DateTime.UtcNow,
                message = "UserCin manquant dans le contexte."
            });

        var response = await _filteringProfileService.CreateProfileAsync(request, userCin);
        return CreatedAtAction(nameof(CreateProfile), new { id = response.Id }, response);
    }
    [HttpPatch("accept/{id}")]
    public async Task<IActionResult> AcceptCandidate(int id, [FromQuery] int offerId)
    {
        var userCin = HttpContext.Items["UserCin"]?.ToString();

        if (string.IsNullOrEmpty(userCin))
            return Unauthorized(new
            {
                timestamp = DateTime.UtcNow,
                message = "UserCin manquant dans le contexte."
            });

        var response = await _filteringService.AcceptCandidateAsync(id, offerId, userCin);
        return Ok(response);
    }
    [HttpPatch("reject/{id}")]
    public async Task<IActionResult> RejectCandidate(int id)
    {
        var response = await _filteringService.RejectCandidateAsync(id);
        return Ok(response);
    }
}