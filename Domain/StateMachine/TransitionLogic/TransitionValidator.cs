using Domain.Utilities;

namespace Domain.Transitions;

public static class TransitionValidator
{
    public static bool CanTransition(ApplicationStatus from, ApplicationStatus to)
    {
        if (TransitionLogic.IsTerminal(from))
            return false;

        return TransitionLogic.TransitionDict.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}
