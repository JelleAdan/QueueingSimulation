using System;

namespace Queuing_Simulation.Distributions
{
    public class ExponentialDistribution : Distribution
    {
		public ExponentialDistribution(Random rng, double average, double CV)
        {
            Rng = rng;
            Average = average;
            Variance = average * average;
            CoefficientOfVariation = 1;
            Residual = average;
        }

        public override double Next()
        {
            return -Math.Log(Rng.MyNextDouble()) * Average;
        }
    }
}