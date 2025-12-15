namespace FancyContainerConsole.Domain.ValueObjects;

public sealed class VolumeId
{
    public string Value { get; }

    public VolumeId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Volume ID cannot be null or empty.", nameof(value));

        Value = value;
    }

    public override bool Equals(object? obj)
    {
        return obj is VolumeId other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString() => Value;
}
