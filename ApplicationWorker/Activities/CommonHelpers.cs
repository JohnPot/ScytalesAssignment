using ApplicationWorker.Dtos;
using Domain.Utilities;

namespace ApplicationWorker.Activities;

public static class CommonHelper
{
    public static ApplicationStatus ChanceToFail()
    {
        int score = Random.Shared.Next(0, 100);

        return score < 40 ? ApplicationStatus.Failed : ApplicationStatus.CrossCheckingRegistry;
    }

    public static ApplicationStatus CheckPhoto()
    {
        bool passed = Random.Shared.Next(0, 10) > 5;

        return passed ? ApplicationStatus.RiskAssessment : ApplicationStatus.CheckingPhoto;
    }

    public static ApplicationStatus RiskAssessment(ApplicationDto application)
    {
        int age = CalculateAge(application.DateOfBirth);

        if (age < 21 || application.LicenceCategory.Equals("B", StringComparison.OrdinalIgnoreCase))
            return ApplicationStatus.PendingManualReview;

        bool lowRisk = Random.Shared.Next(0, 10) > 3;

        return lowRisk ? ApplicationStatus.Approved : ApplicationStatus.PendingManualReview;
    }

    private static int CalculateAge(DateTime dob)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dob.Year;

        if (dob.Date > today.AddYears(-age))
            age--;

        return age;
    }
}