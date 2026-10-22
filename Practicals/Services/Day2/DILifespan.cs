namespace Practicals.Services.Day2
{
    public class DILifespans
    {
        public record TransientValue { public Guid Id { get; } = Guid.NewGuid(); }
        public record ScopedValue { public Guid Id { get; } = Guid.NewGuid(); }
        public record SingletonValue { public Guid Id { get; } = Guid.NewGuid(); }
    }
}
