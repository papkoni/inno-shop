using MediatR;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Auth.Login;

public class LoginCommandHandler: IRequestHandler<LoginCommand, TokensDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    
    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<TokensDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken)
                           ?? throw new NotFoundException($"User with email: {request.Email} doesn't exists");

        var tokens = _jwtProvider.GenerateTokens(existingUser);
        UpdateRefreshToken(existingUser, tokens.RefreshToken, _jwtProvider.GetRefreshTokenExpirationMinutes() );

        var isPasswordValid = _passwordHasher.Verify(request.Password, existingUser.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid password or email");
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return tokens;
    }
    
    private void UpdateRefreshToken(User user, string newToken, int expirationMinutes)
    {
        user.RefreshToken.Token = newToken;
        user.RefreshToken.CreatedDate = DateTime.UtcNow;
        user.RefreshToken.ExpiryDate = DateTime.UtcNow.AddMinutes(expirationMinutes);
        user.RefreshToken.IsRevoked = false;
        
        _unitOfWork.RefreshTokenRepository.Update(user.RefreshToken);
    }
}