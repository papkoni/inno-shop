using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.DeactivateUser;
using UserService.Application.Handlers.Commands.Users.UpdateUser;
using UserService.Application.Handlers.Queries.Users.GetUserById;

namespace UserService.Presentation.Controllers;

[ApiController]
[Route("users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetUserById(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       
        var user = await _mediator.Send(new GetUserByIdQuery(userIdClaim), cancellationToken);

        return Ok(user);
    }
    
    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateUserDto updateParameters, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var user = await _mediator.Send(
            new UpdateUserCommand(
                userIdClaim, updateParameters), 
            cancellationToken);

        return Ok(user);
    }
    
    [HttpPut("me/status")]
    public async Task<IActionResult> UpdateStatus(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var user = await _mediator.Send(
            new DeactivateUserCommand(
                userIdClaim, false), 
                cancellationToken);

        return Ok(user);
    }
}
