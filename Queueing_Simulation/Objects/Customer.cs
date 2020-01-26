namespace Queuing_Simulation.Objects
{
    public class Customer
    {
        public int cID { get; set; }
        public double arrivalTime { get; set; }
        public double departureTime { get; set; }
        public double serviceTime { get; set; }
        public Server server { get; set; }

        public Customer(int cID)
        {
            this.cID = cID;
        }
    }
}
