using System;

namespace Queuing_Simulation_2.Distributions
{
	public class HyperExponentialDistribution : Distribution
	{
		private double[] lambda { get; set; }
		private double[] prob { get; set; }

		public HyperExponentialDistribution(Random rng, double[] lambda, double[] prob)
		{
			this.Rng = rng;
            Array.Sort(prob, lambda);
			this.prob = prob;
			this.lambda = lambda;
			for (int i = 0; i < prob.Length; i++)
			{
				Average += prob[i] / lambda[i];
				Variance += prob[i] / lambda[i] / lambda[i];
			}
			Variance = 2 * Variance - Average * Average;
            Residual = (Variance + Average * Average) / (2 * Average);
        }

        public override double Next()
		{
			double a = Rng.MyNextDouble();
			int index = 0;
			for (int i = 0; i < prob.Length; i++)
			{
				if (a < prob[i])
				{
					index = i;
					break;
				}
			}
			return -Math.Log(Rng.MyNextDouble()) / lambda[index];
		}
	}
}