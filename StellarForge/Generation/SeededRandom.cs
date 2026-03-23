namespace StellarForge.Generation;

public class SeededRandom
{
    private readonly Random _rng;

    public SeededRandom(int seed)
    {
        _rng = new Random(seed);
    }

    public int Next(int min, int maxExclusive) => _rng.Next(min, maxExclusive);

    public double NextDouble() => _rng.NextDouble();

    public double NextDouble(double min, double max) => min + _rng.NextDouble() * (max - min);

    public float NextFloat(float min, float max) => (float)(min + _rng.NextDouble() * (max - min));

    /// <summary>Box-Muller transform for Gaussian distribution</summary>
    public double NextGaussian(double mean, double stddev)
    {
        double u1 = 1.0 - _rng.NextDouble(); // avoid log(0)
        double u2 = _rng.NextDouble();
        double normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stddev * normal;
    }

    public T Pick<T>(T[] array) => array[_rng.Next(array.Length)];

    public T Pick<T>(IReadOnlyList<T> list) => list[_rng.Next(list.Count)];

    public bool Chance(double probability) => _rng.NextDouble() < probability;

    /// <summary>Weighted random selection. weights[i] is relative weight for index i.</summary>
    public int WeightedPick(double[] weights)
    {
        double total = 0;
        foreach (var w in weights) total += w;
        double roll = _rng.NextDouble() * total;
        double cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative) return i;
        }
        return weights.Length - 1;
    }
}
