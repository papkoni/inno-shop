using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Handlers.Commands.Auth.Login;
using UserService.Application.Handlers.Commands.Auth.RefreshTokens;
using UserService.Application.Handlers.Commands.Auth.RevokeToken;
using UserService.Application.Handlers.Commands.Users.UserRegistration;

namespace UserService.Presentation.Controllers;

[ApiController]
[Route("auth")]
public class AuthController: ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationCommand request, CancellationToken cancellationToken)
    {
        var context = HttpContext;

        var tokens = await _mediator.Send(request, cancellationToken);

        context.Response.Cookies.Append("secretCookie", tokens.RefreshToken);

        return Ok(tokens.AccessToken);
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Response.Cookies.Delete("secretCookie");
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        await _mediator.Send(new RevokeTokenCommand(Guid.Parse(userIdClaim)), cancellationToken);
        
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand request, CancellationToken cancellationToken)
    {
        var context = HttpContext;

        var tokens = await _mediator.Send(request, cancellationToken);

        context.Response.Cookies.Append("secretCookie", tokens.RefreshToken);

        return Ok(tokens.AccessToken);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<string>> RefreshTokens(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["secretCookie"];
        
        var tokens = await _mediator.Send(new RefreshTokensCommand(refreshToken), cancellationToken);

        Response.Cookies.Append("secretCookie", tokens.RefreshToken);
        
        return Ok(tokens.AccessToken);
    }
}