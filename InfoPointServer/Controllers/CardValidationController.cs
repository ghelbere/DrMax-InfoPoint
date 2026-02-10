using InfoPointServer.Interfaces;
using InfoPointServer.Services;
using InfoPointShared.Models;
using Microsoft.AspNetCore.Mvc;

namespace InfoPointServer.Controllers;

[ApiController]
[Route("api/card-validate")]
public class CardValidationController : ControllerBase
{
    private readonly ILogger<CardValidationController> _logger;

    public CardValidationController(ILogger<CardValidationController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ValidateCard(
        [FromBody] CardValidationRequest request,
        CancellationToken cancellationToken)
    {
        var tabletId = Request.Headers["Tablet-ID"].FirstOrDefault() ?? "UNKNOWN";
        _logger.LogInformation($"💳 Tablet {tabletId} validating card: {request.CardNumber}");

        try
        {
            var _cardService = new CardService();
            var result = await _cardService.ValidateCardAsync(request.CardNumber, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Card {request.CardNumber} validation failed for tablet {tabletId}");
            return BadRequest(new { error = "Card validation failed" });
        }
    }
}

