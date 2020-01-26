using System.Collections.Generic;

namespace Queuing_Simulation.Events
{
    public class FutureEvents
    {
        public List<Event> list { get; set; }

        public FutureEvents()
        {
            list = new List<Event>();
        }

        public Event Next()
        {
            Event tmp = list[0];
            list.RemoveAt(0);
            return tmp;
        }

        public void Add(Event e)
        {
            int insertionIndex = 0;
            while(insertionIndex < list.Count)
            {
                if(list[insertionIndex].time < e.time)
                {
                    insertionIndex++;
                }
                else { break; }
            }
            list.Insert(insertionIndex, e);
        }
    }
}
