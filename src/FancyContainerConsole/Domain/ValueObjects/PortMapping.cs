namespace FancyContainerConsole.Domain.ValueObjects;

public sealed record PortMapping(int PrivatePort, int PublicPort, string Type)
{
    public override string ToString() => $"{PublicPort}:{PrivatePort}/{Type}";
}
