namespace Queuing_Simulation_2.Objects
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
