using Domain.Transitions;
using Domain.Utilities;

namespace Tests.Tests.UnitTests;

public class StateMachineTests
{
    [Fact]
    public void Should_allow_valid_transition()
    {
        var result = TransitionValidator.CanTransition(
            ApplicationStatus.Submitted,
            ApplicationStatus.ValidatingData);

        Assert.True(result);
    }

    [Fact]
    public void Should_reject_invalid_transition()
    {
        var result = TransitionValidator.CanTransition(
            ApplicationStatus.Submitted,
            ApplicationStatus.Approved);

        Assert.False(result);
    }

    [Fact]
    public void Should_not_allow_transition_from_terminal_state()
    {
        var result = TransitionValidator.CanTransition(
            ApplicationStatus.Approved,
            ApplicationStatus.Submitted);

        Assert.False(result);
    }

    [Theory]
    [InlineData(ApplicationStatus.Approved)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Failed)]
    public void Should_identify_terminal_states(ApplicationStatus status)
    {
        var result = TransitionLogic.IsTerminal(status);

        Assert.True(result);
    }

    [Theory]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.ValidatingData)]
    [InlineData(ApplicationStatus.RiskAssessment)]
    public void Should_identify_non_terminal_states(ApplicationStatus status)
    {
        var result = TransitionLogic.IsTerminal(status);

        Assert.False(result);
    }

    [Fact]
    public void Should_map_submitted_to_correct_event()
    {
        var result = TransitionLogic.returnEventBasedOnApplicationStatus(
            ApplicationStatus.Submitted);

        Assert.Equal(ApplicationEvents.ApplicationCreated, result);
    }

    [Fact]
    public void Should_return_default_event_for_unknown_state()
    {
        var result = TransitionLogic.returnEventBasedOnApplicationStatus(
            (ApplicationStatus)999);

        Assert.Equal(ApplicationEvents.ValidationFailed, result);
    }



}
