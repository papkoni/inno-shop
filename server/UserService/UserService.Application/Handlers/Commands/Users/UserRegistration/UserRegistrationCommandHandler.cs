using MediatR;
using UserService.Application.DTOs;
using UserService.Application.Interfaces.Authentication;
using UserService.Application.Interfaces.DB;
using UserService.Application.Interfaces.Security;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserService.Application.Handlers.Commands.Users.UserRegistration;

public class UserRegistrationCommandHandler: IRequestHandler<UserRegistrationCommand, TokensDto>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    
    public UserRegistrationCommandHandler(
        IPasswordHasher passwordHasher, 
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork
        )
    {
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<TokensDto> Handle(UserRegistrationCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new BadRequestException($"A user with the email '{request.Email}' already exists");
        }
        
        var hashedPassword = _passwordHasher.Generate(request.Password);

        var user = new User( request.Name, hashedPassword, request.Email);

        var tokens = _jwtProvider.GenerateTokens(user);

        var refreshToken = new RefreshToken(tokens.RefreshToken, _jwtProvider.GetRefreshTokenExpirationMinutes(), user.Id);

        await _unitOfWork.UserRepository.CreateAsync(user, cancellationToken);
        await _unitOfWork.RefreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tokens;
    }
}