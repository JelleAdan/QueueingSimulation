using System.Collections.Generic;

namespace Queuing_Simulation.Objects
{
    public class CustomerQueue
    {
        public List<Customer> list { get; set; }
        
        public CustomerQueue()
        {
            list = new List<Customer>();
        }

        public int GetLength()
        {
            return list.Count;
        }

        public void CustomerCheckIn(Customer customer, double time)
        {
            list.Add(customer);
            customer.arrivalTime = time;
        }

        public void CustomerCheckOut(Customer customer)
        {
            list.Remove(customer);
        }

        public Customer FindCustomer(Server server)
        {
            foreach(Customer customer in list)
            {
                if(customer.server == null && server.eligibility[customer.cID])
                {
                    customer.server = server;
                    return customer;
                }
            }
            return null;
        }
    }
}
