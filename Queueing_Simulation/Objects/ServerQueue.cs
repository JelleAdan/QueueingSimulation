using System.Collections.Generic;

namespace Queuing_Simulation.Objects
{
    public class ServerQueue
    {
        public List<Server> list { get; set; }

        public ServerQueue()
        {
            list = new List<Server>();
        }

        public void Add(Server server)
        {
            list.Add(server);
        }

        public void Remove(Server server)
        {
            list.Remove(server);
        }

        public int GetLength()
        {
            return list.Count;
        }
        
        public Server FindServer(int cID)
        {
            foreach(Server server in list)
            {
                if (server.eligibility[cID])
                {
                    return server;
                }
            }
            return null;
        }
    }
}
