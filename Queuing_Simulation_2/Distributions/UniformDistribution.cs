using System;

namespace Queuing_Simulation.Distributions
{
	public class UniformDistribution : Distribution
	{
        private double a;
        private double b;

        public UniformDistribution(Random rng, double average, double CV)
        {
            Rng = rng;
            a = 0;
            b = 2 * average;
            Average = average;
            Variance = (1 / 12.0) * (b - a) * (b - a);
            Residual = (Variance + Average * Average) / (2 * Average);
        }

        public override double Next()
		{
			return a + (b - a) * Rng.MyNextDouble();
		}
	}
}