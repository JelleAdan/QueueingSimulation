using System;

namespace Queuing_Simulation.Distributions
{
	public abstract class Distribution
    {
        public Random Rng { get; set; }
        public double Average { get; set; }
        public double Variance { get; set; }
        public double CoefficientOfVariation { get; set; }
        public double Residual { get; set; }
        public abstract double Next();
    }
}