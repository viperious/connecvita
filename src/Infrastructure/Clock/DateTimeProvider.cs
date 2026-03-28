using Connecvita.Application.Abstractions.Clock;

namespace Connecvita.Infrastructure.Clock;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}