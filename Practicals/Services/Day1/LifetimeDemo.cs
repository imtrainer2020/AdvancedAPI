namespace Practicals.Services.Day1
{
    public class LifetimeDemo
    {
        public class TransientStamp { public Guid Id { get; } = Guid.NewGuid(); }
        public class ScopedStamp { public Guid Id { get; } = Guid.NewGuid(); }
        public class SingletonStamp { public Guid Id { get; } = Guid.NewGuid(); }

    }
}
