using EntryPoint.Features.DrivingApplication.Interfaces;
using EntryPoint.Features.DrivingApplication.Repositories;
using EntryPoint.Features.DrivingApplication.Validations;
using FluentValidation;

namespace EntryPoint.CommonMethods;

public static class ServiceCollectionDependencies
{
    public static IServiceCollection AddDependency(this IServiceCollection services)
    {
        services.AddTransient<IApplicationRepository, ApplicationRepository>();

        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<DrivingLicenceApplicationValidator>();
        services.AddValidatorsFromAssemblyContaining<DrivingLicenceApplicationPhotoValidator>();

        return services;
    }
}
