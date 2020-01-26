using System;

namespace Queuing_Simulation.Distributions
{
    public static class MyRandom
    {
        public static double MyNextDouble(this Random rng)
        {
            double sample = rng.NextDouble();
            while (sample == 0) { sample = rng.NextDouble(); }
            return sample;
        }

        public static double NextGaussian(this Random rng)
        {
            return Math.Sqrt(-2 * Math.Log(rng.MyNextDouble())) * Math.Cos(2 * Math.PI * rng.NextDouble());
        }

        public static bool NextBernoulli(this Random rng, double p)
        {
            if (rng.NextDouble() < p) { return true; } else { return false; }
        }
    }
}
