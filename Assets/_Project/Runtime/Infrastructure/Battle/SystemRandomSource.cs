using System;
using FloatingIslandsRpg.Application.Battle;

namespace FloatingIslandsRpg.Infrastructure.Battle
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource()
            : this(new Random())
        {
        }

        public SystemRandomSource(int seed)
            : this(new Random(seed))
        {
        }

        private SystemRandomSource(Random random)
        {
            _random = random;
        }

        public double NextDouble() => _random.NextDouble();
    }
}
