using Domain.Utilities;

namespace Domain.Transitions;

public static class TransitionLogic
{
    public static readonly Dictionary<ApplicationStatus, HashSet<ApplicationStatus>> TransitionDict = new()
    {
        [ApplicationStatus.Pending] = [ApplicationStatus.Submitted],
        [ApplicationStatus.Submitted] = [ApplicationStatus.PendingProcess, ApplicationStatus.Failed, ApplicationStatus.ValidatingData],
        [ApplicationStatus.PendingProcess] = [ApplicationStatus.ValidatingData, ApplicationStatus.Failed],
        [ApplicationStatus.CrossCheckingRegistry] = [ApplicationStatus.RiskAssessment, ApplicationStatus.CheckingPhoto],
        [ApplicationStatus.ValidatingData] = [ApplicationStatus.CheckingPhoto, ApplicationStatus.Failed, ApplicationStatus.CrossCheckingRegistry],
        [ApplicationStatus.CheckingPhoto] = [ApplicationStatus.RiskAssessment, ApplicationStatus.Failed, ApplicationStatus.Approved, ApplicationStatus.PendingManualReview],
        [ApplicationStatus.RiskAssessment] = [ApplicationStatus.PendingManualReview, ApplicationStatus.Approved, ApplicationStatus.Rejected],
        [ApplicationStatus.PendingManualReview] = [ApplicationStatus.Approved, ApplicationStatus.Rejected]
    };

    public static ApplicationEvents returnEventBasedOnApplicationStatus(ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.Pending => ApplicationEvents.Applicationpending,
            ApplicationStatus.Submitted => ApplicationEvents.ApplicationCreated,
            ApplicationStatus.PendingProcess => ApplicationEvents.ProcessQueued,
            ApplicationStatus.ValidatingData => ApplicationEvents.ValidationStarted,
            ApplicationStatus.CrossCheckingRegistry => ApplicationEvents.ValidationStarted,
            ApplicationStatus.CheckingPhoto => ApplicationEvents.PhotoCheckStarted,
            ApplicationStatus.RiskAssessment => ApplicationEvents.RiskAssessmentStarted,
            ApplicationStatus.PendingManualReview => ApplicationEvents.ManualReviewRequired,
            ApplicationStatus.Approved => ApplicationEvents.ValidationPassed,
            ApplicationStatus.Rejected => ApplicationEvents.ValidationFailed,
            ApplicationStatus.Failed => ApplicationEvents.ValidationFailed,
            _ => ApplicationEvents.ValidationFailed
        };
    }

    public static readonly HashSet<ApplicationStatus> TerminalStates = [ApplicationStatus.Approved, ApplicationStatus.Rejected, ApplicationStatus.Failed];

    public static bool IsTerminal(ApplicationStatus state) => TerminalStates.Contains(state);
}
