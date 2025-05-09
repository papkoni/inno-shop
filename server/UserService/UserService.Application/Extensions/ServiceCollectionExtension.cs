using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Behaviors;
using UserService.Application.DTOs;
using UserService.Application.Handlers.Commands.Users.UserRegistration;
using UserService.Domain.Entities;

namespace UserService.Application.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddApplication(
        this IServiceCollection services
    )
    {
        services.AddMapster(); 
        TypeAdapterConfig<UpdateUserDto, User>
            .NewConfig()
            .IgnoreNullValues(true);
        
        services.AddValidatorsFromAssemblyContaining<UserRegistrationCommandValidator>();
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );
    }
}