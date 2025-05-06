using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Behaviors;
using UserService.Application.Handlers.Commands.Users.UserRegistration;

namespace UserService.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(
        this IServiceCollection services
    )
    {
        services.AddMapster(); 
        services.AddValidatorsFromAssemblyContaining<UserRegistrationCommandValidator>();
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );
    }
}