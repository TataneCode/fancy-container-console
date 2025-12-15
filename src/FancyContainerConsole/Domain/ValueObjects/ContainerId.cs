namespace FancyContainerConsole.Domain.ValueObjects;

public sealed record ContainerId
{
    public string Value { get; }

    public ContainerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Container ID cannot be null or empty", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ContainerId containerId) => containerId.Value;
}
