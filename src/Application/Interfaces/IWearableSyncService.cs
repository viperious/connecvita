using Connecvita.Domain.Enums;

namespace Connecvita.Application.Interfaces;

public interface IWearableSyncService
{
    Task SyncAllUsersAsync(CancellationToken cancellationToken = default);
    Task SyncUserAsync(string userId, CancellationToken cancellationToken = default);
    Task ConnectPlatformAsync(string userId, WearablePlatform platform, string accessToken);
    Task DisconnectPlatformAsync(string userId, WearablePlatform platform);
}