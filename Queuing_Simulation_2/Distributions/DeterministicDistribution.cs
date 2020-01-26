using System;

namespace Queuing_Simulation.Distributions
{
	public class DeterministicDistribution : Distribution
	{
		public DeterministicDistribution(Random rng, double average, double CV)
		{
			Rng = rng;
			Average = average;
			Variance = 0;
            CoefficientOfVariation = 0;
            Residual = Average * 0.5;
		}

		public override double Next()
		{
			return Average;
		}
	}
}