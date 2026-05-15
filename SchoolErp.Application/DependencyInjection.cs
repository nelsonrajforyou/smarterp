using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SchoolErp.Application.Common.Mediator;
using FluentValidation;

namespace SchoolErp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Custom Mediator Registration
        services.AddScoped<IMediator, Mediator>();

        // Register Pipeline Behaviors
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Scan for all IRequestHandler implementations in the current assembly
        var assembly = Assembly.GetExecutingAssembly();
        
        var handlers = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

        foreach (var handler in handlers)
        {
            var interfaceType = handler.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            services.AddScoped(interfaceType, handler);
        }

        // Register all Validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
