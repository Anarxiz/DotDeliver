using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Application.Configurations;
using Web.Application.Interfaces.Service;
using Web.Application.Services;
using Web.Application.Validators;

namespace Web.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration) {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<MailSettings>(configuration.GetSection("MailSetting"));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateAddressRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateCategoryRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateDishRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateAddressRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateCategoryRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateDishRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateOrderStatusRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProfileRequestValidator>();

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }
}