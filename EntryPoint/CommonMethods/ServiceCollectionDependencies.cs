using EntryPoint.Features.DrivingApplication.CreateApplication.Validations;
using EntryPoint.Features.DrivingApplication.Interfaces;
using EntryPoint.Features.DrivingApplication.Repositories;
using EntryPoint.Features.DrivingApplication.UploadPhoto;
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
        services.AddValidatorsFromAssemblyContaining<CreateApplicationValidator>();
        services.AddValidatorsFromAssemblyContaining<UploadPhotoValidator>();

        return services;
    }
}
