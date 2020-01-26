namespace Queuing_Simulation.Objects
{
    public class Server
    {
        public int sID { get; set; }
        public bool[] eligibility { get; set; }

        public Server(int sID, bool[] eligibility)
        {
            this.sID = sID;
            this.eligibility = eligibility;
        }
    }
}
