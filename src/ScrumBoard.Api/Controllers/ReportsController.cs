using Microsoft.AspNetCore.Mvc;
using ScrumBoard.Application.Boards;
using ScrumBoard.Application.Reports;

namespace ScrumBoard.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/report")]
public sealed class ReportsController : ControllerBase
{
    private readonly ReportService _reports;

    public ReportsController(ReportService reports) => _reports = reports;

    /// <summary>
    /// Same data, same query, different serialization. The format is just a key resolved against the
    /// registered exporters, so a new format needs no change in this controller either.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        Guid projectId,
        [FromQuery] string format,
        [FromQuery] Guid? assigneeId,
        [FromQuery] int? priority,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var file = await _reports.GenerateAsync(
            projectId,
            format,
            new BoardFilter(assigneeId, priority, search),
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("formats")]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<string>> Formats() => Ok(_reports.SupportedFormats);
}
