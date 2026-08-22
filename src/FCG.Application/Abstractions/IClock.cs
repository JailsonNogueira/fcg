namespace FCG.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
