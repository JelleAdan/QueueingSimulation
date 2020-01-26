using CommandLine;
using Queuing_Simulation.Distributions;
using Queuing_Simulation.Events;
using Queuing_Simulation.Objects;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Queuing_Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                if (options.OutputDirectory == null)
                {
                    options.OutputDirectory = Path.Combine(Directory.GetParent(options.InputDirectory).FullName, "Output");
                }
                if (Directory.Exists(options.OutputDirectory))
                {
                    Directory.Delete(options.OutputDirectory, true);
                }
                Directory.CreateDirectory(options.OutputDirectory);
                foreach (string directory in Directory.GetDirectories(options.InputDirectory))
                {
                    Data data = new Data();
                    data.Runs = options.Runs;
                    data.Threads = options.Threads;
                    data.Stepsize = options.Stepsize;
                    ReadInput(directory, data);
                    // Print progress
                    Console.WriteLine($"{Path.GetFileNameWithoutExtension(directory)} started.");
                    Results[] allResults = DoSimulation(options, data);
                    WriteOutput(Path.Combine(options.OutputDirectory, Path.GetFileName(directory)), data, allResults);
                }
            }
            // TODO: Ask to specify options. 
        }

        private static void WriteOutput(string directory, Data data, Results[] results)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);
            using (StreamWriter writer1 = new StreamWriter(Path.Combine(directory, "Means.txt")))
            using (StreamWriter writer2 = new StreamWriter(Path.Combine(directory, "ConfigurationProbabilities.txt")))
            {
                writer1.Write($"utilization\t\t{"meanWTotal",20}\t{"95ciWTotal",20}\t{"meanPW",20}\t{"95ciPW",20}");
                for (int cID = 0; cID < data.nrCustomers; cID++)
                {
                    writer1.Write($"\t{$"meanW{cID}",20}\t{$"ciW{cID}",20}");
                }
                writer1.WriteLine();
                for (int i = 1; i < results.Length; i++) // Note: Start at 1 because Results[0] is always null because Parallel.For starts at 1.
                {
                    results[i].GetMeans(writer1);
                    results[i].GetConfigurationProbabilities(writer2);
                }
            }
        }

        private static void ReadInput(string directory, Data data)
        {
            ReadEligibility(directory, data);
            ReadServers(directory, data);
            ReadCustomers(directory, data);
            TestCustomers(data);
        }

        private static void ReadCustomers(string directory, Data data)
        {
            try
            {
                data.CustomerParameters = new CustomerParameters[data.nrCustomers];
                for (int i = 0; i < data.nrCustomers; i++)
                {
                    data.CustomerParameters[i] = new CustomerParameters();
                    string[] lines = File.ReadAllLines(Path.Combine(directory, string.Concat($"Customer_{i}.txt")));
                    foreach (string line in lines.Where(line => !string.IsNullOrEmpty(line)))
                    {
                        var key = data.CustomerParameters[i].GetType().GetProperties().Where(x => x.Name == line.Split('=').First()).First();
                        var tmp = key.PropertyType;
                        // Convert to target type
                        if (key.PropertyType == typeof(double))
                        {
                            var value = double.Parse(line.Split('=').Last());
                            key.SetValue(data.CustomerParameters[i], value);
                        }
                        if (key.PropertyType == typeof(string))
                        {
                            var value = line.Split('=').Last();
                            key.SetValue(data.CustomerParameters[i], value);
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Error in read customers.");
            }
        }

        private static void TestCustomers(Data data)
        {
            double sum = 0;
            for (int i = 0; i < data.nrCustomers; i++)
            {
                sum += data.CustomerParameters[i].Fraction;
            }
            if (sum != 1)
            {
                throw new Exception("The sum of the customer arrival rate fractions does not add up to 1.");
            }
        }

        private static void ReadServers(string directory, Data data)
        {
            try
            {
                data.ServerParameters = new ServerParameters[data.nrServers];
                for (int i = 0; i < data.nrServers; i++)
                {
                    data.ServerParameters[i] = new ServerParameters();
                    string[] lines = File.ReadAllLines(Path.Combine(directory, string.Concat($"Server_{i}.txt")));
                    foreach (string line in lines.Where(line => !string.IsNullOrEmpty(line)))
                    {
                        var key = data.ServerParameters[i].GetType().GetProperties().Where(x => x.Name == line.Split('=').First()).First();
                        var tmp = key.PropertyType;
                        // Convert to target type
                        if (key.PropertyType == typeof(double))
                        {
                            var value = double.Parse(line.Split('=').Last());
                            key.SetValue(data.ServerParameters[i], value);
                        }
                        if (key.PropertyType == typeof(string))
                        {
                            var value = line.Split('=').Last();
                            key.SetValue(data.ServerParameters[i], value);
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Error in read servers.");
            }
        }

        private static void ReadEligibility(string directory, Data data)
        {
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(directory, "Eligibility.txt"));
                data.nrServers = lines.Length;
                data.nrCustomers = lines[0].Length;
                data.Eligibility = new bool[data.nrServers][];
                for (int server = 0; server < data.nrServers; server++)
                {
                    data.Eligibility[server] = new bool[data.nrCustomers];
                    for (int customer = 0; customer < data.nrCustomers; customer++)
                    {
                        if (lines[server][customer] == '0')
                        {
                            data.Eligibility[server][customer] = false;
                        }
                        else if (lines[server][customer] == '1')
                        {
                            data.Eligibility[server][customer] = true;
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Error in read eligibility.");
            }
        }

        private static Results[] DoSimulation(Options options, Data data)
        {
            int limit = DetermineLimit(data);
            ThreadLocal<Random> rng = new ThreadLocal<Random>(() => new Random());
            Results[] allResults = new Results[limit];
            //for (int rho = 1; rho < limit; rho++)
            //{
            Parallel.For(1, limit, new ParallelOptions { MaxDegreeOfParallelism = data.Threads }, rho => // MaxDegreeOfParallelism n + 1, where n is the number of cores
            {
                // Initialize servers
                Distribution[] serviceDistributions = CreateServiceDistributions(data, rng.Value);

                // Exponential arrival rate
                double lambda = data.ServerParameters.Sum(x => 1 / x.Average) * ((rho - 1) * data.Stepsize + 0.05);
                //double lambda = data.ServerParameters.Sum(x => 1 / x.Average) * rho * data.Stepsize;

                // Initialize customers
                Distribution[] arrivalDistributions = CreateArrivalDistributions(data, rng.Value, lambda);

                // Main simulation loop
                //Results results = new Results(data, rho * data.Stepsize, arrivalDistributions, serviceDistributions);
                Results results = new Results(data, ((rho - 1) * data.Stepsize + 0.05), arrivalDistributions, serviceDistributions);
                for (int r = 0; r < data.Runs; r++)
                {
                    double t = 0;

                    FutureEvents futureEvents = new FutureEvents();
                    for (int i = 0; i < data.nrCustomers; i++)
                    {
                        Customer customer = new Customer(i);
                        futureEvents.Add(new Event(Event.ARRIVAL, arrivalDistributions[i].Next(), customer));
                    }

                    CustomerQueue customerQueue = new CustomerQueue();

                    ServerQueue idleServerQueue = new ServerQueue();
                    for (int i = 0; i < data.nrServers; i++)
                    {
                        Server server = new Server(i, data.Eligibility[i]);
                        idleServerQueue.Add(server);
                    }

                    while (results.DepartureCounter(r) < options.DeparturesToSimulate)
                    {
                        Event e = futureEvents.Next();
                        t = e.time;
                        results.Register(r, e, customerQueue, idleServerQueue);

                        if (e.type == Event.ARRIVAL)
                        {
                            customerQueue.CustomerCheckIn(e.customer, t);
                            Server server = idleServerQueue.FindServer(e.customer.cID);
                            if (server != null) // Eligible server available
                            {
                                idleServerQueue.Remove(server);
                                e.customer.server = server;
                                e.customer.serviceTime = t;
                                e.customer.departureTime = t + serviceDistributions[server.sID].Next();
                                futureEvents.Add(new Event(Event.DEPARTURE, e.customer.departureTime, e.customer));
                            }
                            futureEvents.Add(new Event(Event.ARRIVAL, t + arrivalDistributions[e.customer.cID].Next(), new Customer(e.customer.cID)));
                        }
                        else if (e.type == Event.DEPARTURE)
                        {
                            Customer customer = customerQueue.FindCustomer(e.customer.server);
                            if (customer != null)
                            {
                                customer.serviceTime = t;
                                customer.departureTime = t + serviceDistributions[customer.server.sID].Next();
                                futureEvents.Add(new Event(Event.DEPARTURE, customer.departureTime, customer));
                            }
                            else
                            {
                                idleServerQueue.Add(e.customer.server);
                            }
                            customerQueue.CustomerCheckOut(e.customer);
                        }
                        else
                        {
                            Console.WriteLine("Invalid event type.");
                        }
                    }
                }
                allResults[rho] = results;
            });
            //}
            return allResults;
        }

        private static int DetermineLimit(Data data)
        {
            double rho = 0.05;
            while (rho < 0.95) // NOTE: Utilization always between 0.05 and 0.95!
            {
                //double lambda = data.nrServers * rho;
                double lambda = rho * data.ServerParameters.Sum(x => 1 / x.Average);
                for (int i = 0; i < data.nrCustomers; i++)
                {
                    double muSubset = DetermineSubsetServiceRate(i, data);
                    double lambdaSubset = DetermineSubsetArrivalRate(i, lambda, data);
                    if (!(lambdaSubset < muSubset))
                    {
                        return (int)((rho - data.Stepsize - 0.05) / data.Stepsize) + 2; // NOTE: Utilization range divided by step size, one added to include 0.05 and one added to include 0.95.
                    }
                }
                rho += data.Stepsize;
            }
            return (int)((rho - 0.05) / data.Stepsize) + 2; // NOTE: Utilization range divided by step size, one added to include 0.05 and one added to include 0.95.
        }

        private static double DetermineSubsetServiceRate(int customer, Data data)
        {
            double muSubset = 0;
            for (int i = 0; i < data.nrServers; i++)
            {
                muSubset += data.Eligibility[i][customer] ? 1 / data.ServerParameters[i].Average : 0;
            }
            return muSubset;
        }

        private static double DetermineSubsetArrivalRate(int customer, double lambda, Data data)
        {
            double lambdaSubset = data.CustomerParameters[customer].Fraction * lambda;
            for (int i = 0; i < data.nrCustomers; ++i)
            {
                if (i != customer && CustomerInSubset(customer, i, data))
                {
                    lambdaSubset += data.CustomerParameters[i].Fraction * lambda;
                }
            }
            return lambdaSubset;
        }

        private static bool CustomerInSubset(int customer, int customerTwo, Data data)
        {
            for (int i = 0; i < data.nrServers; i++)
            {
                if (!data.Eligibility[i][customer] && data.Eligibility[i][customerTwo])
                {
                    return false;
                }
            }
            return true;
        }

        private static Distribution[] CreateServiceDistributions(Data data, Random rng)
        {
            Distribution[] serviceDistributions = new Distribution[data.nrServers];
            for (int i = 0; i < data.nrServers; i++)
            {
                switch (data.ServerParameters[i].Type)
                {
                    case "Deterministic":
                        serviceDistributions[i] = new DeterministicDistribution(rng, data.ServerParameters[i].Average, data.ServerParameters[i].CV);
                        break;
                    case "Uniform":
                        serviceDistributions[i] = new UniformDistribution(rng, data.ServerParameters[i].Average, data.ServerParameters[i].CV);
                        break;
                    case "Exponential":
                        serviceDistributions[i] = new ExponentialDistribution(rng, data.ServerParameters[i].Average, data.ServerParameters[i].CV);
                        break;
                    case "HyperExponential2":
                        serviceDistributions[i] = new Hyper2ExponentialDistribution(rng, data.ServerParameters[i].Average, data.ServerParameters[i].CV);
                        break;
                }
            }
            return serviceDistributions;
        }

        private static Distribution[] CreateArrivalDistributions(Data data, Random rng, double lambda)
        {
            Distribution[] arrivalDistributions = new Distribution[data.nrCustomers];
            for (int i = 0; i < data.nrCustomers; i++)
            {
                arrivalDistributions[i] = new ExponentialDistribution(rng, 1 / (data.CustomerParameters[i].Fraction * lambda), 1);
            }
            return arrivalDistributions;
        }
    }
}
