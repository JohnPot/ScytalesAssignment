using ApplicationWorker.Activities;
using ApplicationWorker.Services;
using Domain.Utilities;
using Domain.Transitions;

namespace ApplicationWorker.Workflows;

public class ApplicationWorkflow
{
    private readonly ApplicationService _service;

    public ApplicationWorkflow(ApplicationService service)
    {
        _service = service;
    }

    public async Task Execute(Guid applicationId, CancellationToken ct)
    {
        var app = await _service.Get(applicationId, ct);

        if (app is null)
            return;

        ApplicationStatus current = app.Status;

        current = await _service.MoveToAndReturnState(applicationId, current, ApplicationStatus.ValidatingData, ct);

        var next = CommonHelper.ChanceToFail();
        current = await _service.MoveToAndReturnState(applicationId, current, next, ct);

        if (TransitionLogic.IsTerminal(current))
            return;

        next = CommonHelper.CheckPhoto();
        current = await _service.MoveToAndReturnState(applicationId, current, next, ct);

        if (TransitionLogic.IsTerminal(current))
            return;

        next = CommonHelper.RiskAssessment(app);
        current = await _service.MoveToAndReturnState(applicationId, current, next, ct);
    }
}