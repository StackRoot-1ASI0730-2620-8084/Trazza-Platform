namespace Trazza.Platform.Billing.Interfaces.Acl;

public interface IBillingContextFacade
{
    Task<bool> CanPublish(int userId, int usedThisMonth, CancellationToken cancellationToken);

    Task<string> FetchCurrentPlanCode(int userId, CancellationToken cancellationToken);
}
