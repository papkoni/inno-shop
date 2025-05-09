using MediatR;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Auth.RefreshTokens;

public class RefreshTokensCommandHandler: IRequestHandler<RefreshTokensCommand, TokensDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    
    public RefreshTokensCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
    }

    public async Task<TokensDto> Handle(RefreshTokensCommand request, CancellationToken cancellationToken)
    {
        var isValidToken = _jwtProvider.ValidateRefreshToken(request.RefreshToken);
        if (!isValidToken)
        {
            throw new InvalidTokenException("Token was expire");
        }
        
        var extractedUserId = _jwtProvider.GetUserIdFromRefreshToken(request.RefreshToken);
        var user = await _unitOfWork.UserRepository.GetByIdAsync(extractedUserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        
        var isInvalidToken = user.RefreshToken == null || user.RefreshToken.IsRevoked || (user.RefreshToken.ExpiryDate < DateTime.UtcNow);
        if (isInvalidToken)
        {
            throw new InvalidTokenException("Token is invalid");
        }
        
        var tokens = _jwtProvider.GenerateTokens(user);
        UpdateRefreshToken(user, tokens.RefreshToken, _jwtProvider.GetRefreshTokenExpirationMinutes() );
        
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