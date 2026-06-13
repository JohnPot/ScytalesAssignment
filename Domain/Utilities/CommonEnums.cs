namespace Domain.Utilities;

public enum ApplicationStatus
{
    Pending,
    Submitted,
    PendingProcess,
    ValidatingData,
    CrossCheckingRegistry,
    CheckingPhoto,
    RiskAssessment,
    PendingManualReview,
    Approved,
    Rejected,
    Failed
}

public enum ApplicationOutboxStatus
{
    Pending,
    Submitted
}

public enum ApplicationEvents
{
    Applicationpending,
    ApplicationCreated,
    ApplicationSubmitted,
    ProcessQueued,
    PhotoCheckStarted,
    ValidationStarted,
    RiskAssessmentStarted,
    ValidationPassed,
    ValidationFailed,
    ManualReviewRequired,
    PhotoUploaded
}
