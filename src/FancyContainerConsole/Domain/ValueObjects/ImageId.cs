namespace FancyContainerConsole.Domain.ValueObjects;

public sealed class ImageId
{
    public string Value { get; }

    public ImageId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Image ID cannot be null or empty.", nameof(value));
        }

        Value = value;
    }

    public override bool Equals(object? obj)
    {
        return obj is ImageId other && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }
}
