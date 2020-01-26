namespace Queuing_Simulation
{
    public class ServerParameters
    {
        public string Type { get; set; } = "Exponential";
        public double Average { get; set; } = 1;
        public double CV { get; set; } = 1;
    }

    public class CustomerParameters
    {
        public double Fraction { get; set; } = 1;
    }

    public class Data
    {
        public bool[][] Eligibility { get; set; } // [server][customer]

        public int nrServers { get; set; }

        public int nrCustomers { get; set; }

        public ServerParameters[] ServerParameters { get; set; }

        public CustomerParameters[] CustomerParameters { get; set; }

        public int Runs { get; set; }

        public double Stepsize { get; set; }

        public int Threads { get; set; }
    }
}
