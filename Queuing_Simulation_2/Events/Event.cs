using Queuing_Simulation.Objects;

namespace Queuing_Simulation.Events
{
    public class Event
    {
        public double time { get; set; }
        public int type { get; set; }
        public Customer customer { get; set; }

        public const int ARRIVAL = 0;
        public const int DEPARTURE = 1;

        public Event(int type, double time, Customer customer)
        {
            this.time = time;
            this.type = type;
            this.customer = customer;
        }
    }
}
