using System;

namespace Queuing_Simulation.Distributions
{
    public class Hyper2ExponentialDistribution : Distribution
    {
        private double[] lambda { get; set; }
        private double[] prob { get; set; }
        public Hyper2ExponentialDistribution(Random rng, double average, double CV)
        {
            Rng = rng;
            prob = new double[2];
            prob[0] = 0.5 * (1 + Math.Sqrt((CV - 1) / (CV + 1))); // Balanced means
            prob[1] = 1 - prob[0];
            lambda = new double[2];
            lambda[0] = 2 * prob[0] / average;
            lambda[1] = 2 * prob[1] / average;
            Average = average;
            Variance = 2 * (prob[0] / lambda[0] / lambda[0] + prob[1] / lambda[1] / lambda[1]) - average * average;
            CoefficientOfVariation = CV;
            Residual = (Variance + average * average) / (2 * average);
        }

        public override double Next()
        {
            double a = Rng.NextDouble();
            int index = 0;
            if(a < prob[0])
            {
                index = 0;
            }
            else
            {
                index = 1;
            }
            return -Math.Log(Rng.MyNextDouble()) / lambda[index];
        }
    }
}