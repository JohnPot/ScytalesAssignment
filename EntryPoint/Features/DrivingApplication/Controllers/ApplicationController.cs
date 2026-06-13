using Microsoft.AspNetCore.Mvc;
using EntryPoint.Features.DrivingApplication.Interfaces;
using EntryPoint.Features.DrivingApplication.Models.Requests;
using Domain.Utilities;
using EntryPoint.CommonMethods;
using EntryPoint.Features.DrivingApplication.Models.Responses;

namespace EntryPoint.Features.DrivingApplication.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationRepository _applicationRepository;
    public ApplicationController(IApplicationRepository mainRepository)
    {
        _applicationRepository = mainRepository;
    }

    /// <summary>
    /// Get driving licence application status
    /// </summary>
    [HttpGet("{applicationId}")]
    [ProducesResponseType(typeof(GetApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetApplicationStatus([FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);

        if (result.IsFailure)
        {
            return result.ReturnStates switch
            {
                ReturnStates.NotFound => NotFound(result.Error),
                _ => BadRequest(result.Error),
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Input for driving licence application
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostApplicationData([FromBody] CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await _applicationRepository.CreateApplication(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Input for driving licence application photo
    /// </summary>
    [HttpPut("upload/photo/{applicationId}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PostApplicationPhoto([FromForm] CreateApplicationPhotoRequest photo, [FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await photo.Photo.CopyToAsync(ms);

        var result = await _applicationRepository.AddPhotoAsync(applicationId, ms.ToArray(), cancellationToken);

        if (result.IsFailure)
        {
            return result.ReturnStates switch
            {
                ReturnStates.NotFound => NotFound(result.Error),
                ReturnStates.Conflict => Conflict(result.Error),
                _ => BadRequest(result.Error),
            };
        }

        return Ok();
    }

    /// <summary>
    /// Application submit
    /// </summary>
    [HttpPut("submit/{applicationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApplicationSubmit([FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _applicationRepository.ApplicationSubmit(applicationId, cancellationToken);

        if (result.IsFailure)
        {
            return result.ReturnStates switch
            {
                ReturnStates.NotFound => NotFound(result.Error),
                ReturnStates.Conflict => Conflict(result.Error),
                _ => BadRequest(result.Error),
            };
        }

        return Ok();
    }

    /// <summary>
    /// Manual application review, approve the application
    /// </summary>
    [HttpPut("{applicationId}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ApproveApplication([FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _applicationRepository.UpdateStatusAsync(applicationId, ApplicationStatus.Approved, null , cancellationToken);

        if (result.IsFailure)
        {
            return result.ReturnStates switch
            {
                ReturnStates.NotFound => NotFound(result.Error),
                ReturnStates.Conflict => Conflict(result.Error),
                _ => BadRequest(result.Error),
            };
        }

        return Ok();
    }

    /// <summary>
    /// Manual application review, reject the application
    /// </summary>
    [HttpPut("{applicationId}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectApplication([FromRoute] Guid applicationId, [FromBody] RejectionRequest rejection, CancellationToken cancellationToken)
    {
        var result = await _applicationRepository.UpdateStatusAsync(applicationId, ApplicationStatus.Rejected, rejection?.reason, cancellationToken);

        if (result.IsFailure)
        {
            return result.ReturnStates switch
            {
                ReturnStates.NotFound => NotFound(result.Error),
                ReturnStates.Conflict => Conflict(result.Error),
                _ => BadRequest(result.Error),
            };
        }

        return Ok();
    }

    /// <summary>
    /// Get application state transition history
    /// </summary>
    [HttpGet("{applicationId}/history")]
    [ProducesResponseType(typeof(List<GetApplicationHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplicationStateTransitionHistory([FromRoute] Guid applicationId, CancellationToken cancellationToken)
    {
        var result = await _applicationRepository.ApplicationHistoryAsync(applicationId, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
