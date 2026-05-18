namespace FinWallet.Application.Common.Options;

public sealed class MassTransitOptions
{
    public const string SectionName = "MassTransit";

    public bool UseInMemoryBus { get; set; }
}
