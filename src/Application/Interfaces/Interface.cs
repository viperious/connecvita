using Connecvita.Domain.Entities;
using Connecvita.Domain.Enums;

namespace Connecvita.Application.Interfaces;

public interface IHealthDataService
{
    Task<List<WearableMetrics>> GetMetricsAsync(string userId, int days = 30);
    Task<WearableMetrics?> GetLatestMetricsAsync(string userId);
}