namespace Oks.Shared.Results;

public class Meta
{
    public string? CorrelationId { get; init; }
    public IDictionary<string, string>? Extra { get; init; }
}