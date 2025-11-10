using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;
using EnterpriseDocumentManagementSystem.Api.Filters;

namespace EnterpriseDocumentManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ServiceFilter(typeof(CurrentUserFilter))]
public class TagsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TagsController> _logger;

    public TagsController(IUnitOfWork unitOfWork, ILogger<TagsController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await _unitOfWork.Tags.GetAllAsync();
        var response = tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            Color = t.Color
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get popular tags
    /// </summary>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<TagResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularTags([FromQuery] int count = 10)
    {
        var tags = await _unitOfWork.Tags.GetPopularTagsAsync(count);
        var response = tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            Color = t.Color
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Check if tag already exists
        var existingTag = await _unitOfWork.Tags.GetByNameAsync(request.Name);
        if (existingTag != null)
        {
            return Conflict(new { error = "A tag with this name already exists" });
        }

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Color = request.Color,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Tags.AddAsync(tag);
        await _unitOfWork.SaveChangesAsync();

        var response = new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, response);
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTagById(Guid id)
    {
        var tag = await _unitOfWork.Tags.GetByIdAsync(id);
        
        if (tag == null)
        {
            return NotFound(new { error = "Tag not found" });
        }

        var response = new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        return Ok(response);
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var tag = await _unitOfWork.Tags.GetByIdAsync(id);
        
        if (tag == null)
        {
            return NotFound(new { error = "Tag not found" });
        }

        await _unitOfWork.Tags.DeleteAsync(tag);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}
