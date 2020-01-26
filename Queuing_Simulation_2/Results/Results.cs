using Queuing_Simulation.Distributions;
using Queuing_Simulation.Events;
using Queuing_Simulation.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Queuing_Simulation
{
    public class Results
    {
        private int runs { get; set; }

        private double utilization { get; set; }

        private Distribution[] arrivalDistributions { get; set; }

        private Distribution[] serviceDistributions { get; set; }

        private int nrServers { get; set; }

        private int nrCustomers { get; set; }

        private string filename { get; set; }

        public int[,] nrArrivals { get; set; }

        public int[,] nrDepartures { get; set; }

        private double[] time { get; set; }

        private double[] sumLs { get; set; }

        private double[] sumLq { get; set; }

        private double[] sumS { get; set; }

        private double[,] sumW { get; set; }

        private double[] nrCustomersWait { get; set; }

        private Dictionary<List<int>, double>[,] configurations { get; set; }

        public Results(Data data, double utilization, Distribution[] arrivalDistributions, Distribution[] serviceDistributions)
        {
            runs = data.Runs;
            this.utilization = utilization;
            this.arrivalDistributions = arrivalDistributions;
            this.serviceDistributions = serviceDistributions;
            nrServers = data.nrServers;
            nrCustomers = data.nrCustomers;
            nrArrivals = new int[runs, nrCustomers];
            nrDepartures = new int[runs, nrCustomers];
            time = new double[runs];
            sumLs = new double[runs];
            sumLq = new double[runs];
            sumS = new double[runs];
            sumW = new double[runs, nrCustomers];
            nrCustomersWait = new double[runs];
            configurations = new Dictionary<List<int>, double>[runs, nrCustomers];
            for (int r = 0; r < runs; r++)
            {
                for (int i = 0; i < nrCustomers; i++)
                {
                    configurations[r, i] = new Dictionary<List<int>, double>(new ListComparer<int>());
                }
            }
        }

        public int DepartureCounter(int r)
        {
            int minimum = int.MaxValue;
            for (int i = 0; i < nrCustomers; i++)
            {
                if (nrDepartures[r, i] < minimum)
                {
                    minimum = nrDepartures[r, i];
                }
            }
            return minimum;
        }

        public void Register(int r, Event e, CustomerQueue customerQueue, ServerQueue idleServerQueue)
        {
            sumLs[r] += customerQueue.GetLength() * (e.time - time[r]);
            sumLq[r] += (customerQueue.GetLength() - (nrServers - idleServerQueue.GetLength())) * (e.time - time[r]);
            if (e.type == Event.ARRIVAL)
            {
                nrArrivals[r, e.customer.cID]++;
                List<int> tmp = new List<int>(nrServers - idleServerQueue.list.Count);
                foreach (Customer customer in customerQueue.list)
                {
                    if (customer.server != null)
                    {
                        tmp.Add(customer.server.sID);
                    }
                    if (tmp.Count == (nrServers - idleServerQueue.list.Count))
                    {
                        break;
                    }
                }
                if (configurations[r, e.customer.cID].ContainsKey(tmp))
                {
                    configurations[r, e.customer.cID][tmp]++;
                }
                else
                {
                    configurations[r, e.customer.cID].Add(tmp, 1);
                }
            }
            else if (e.type == Event.DEPARTURE)
            {
                nrDepartures[r, e.customer.cID]++;
                sumS[r] += e.customer.departureTime - e.customer.arrivalTime;
                sumW[r, e.customer.cID] += e.customer.serviceTime - e.customer.arrivalTime;
                if (e.customer.arrivalTime != e.customer.serviceTime)
                {
                    nrCustomersWait[r]++;
                }
            }
            time[r] = e.time;
        }

        public void GetMeans(StreamWriter writer)
        {
            int[] nrArrivalsTotal = new int[runs];
            int[] nrDeparturesTotal = new int[runs];
            double[] sumWTotal = new double[runs];
            for (int r = 0; r < runs; r++)
            {
                for (int i = 0; i < nrCustomers; i++)
                {
                    nrArrivalsTotal[r] += nrArrivals[r, i];
                    nrDeparturesTotal[r] += nrDepartures[r, i];
                    sumWTotal[r] += sumW[r, i];
                }
            }

            double runsumLs = new double();
            double runsumLs2 = new double();
            double runsumLq = new double();
            double runsumLq2 = new double();
            double runsumS = new double();
            double runsumS2 = new double();
            double runsumWTotal = new double();
            double runsumWTotal2 = new double();
            double[] runsumW = new double[nrCustomers];
            double[] runsumW2 = new double[nrCustomers];
            double runsumPW = new double();
            double runsumPW2 = new double();

            for (int r = 0; r < runs; r++)
            {
                runsumLs += sumLs[r] / time[r];
                runsumLs2 += sumLs[r] / time[r] * sumLs[r] / time[r];
                runsumLq += sumLq[r] / time[r];
                runsumLq2 += sumLq[r] / time[r] * sumLq[r] / time[r];
                runsumS += sumS[r] / nrDeparturesTotal[r];
                runsumS2 += sumS[r] / nrDeparturesTotal[r] * sumS[r] / nrDeparturesTotal[r];
                runsumWTotal += sumWTotal[r] / nrDeparturesTotal[r];
                runsumWTotal2 += sumWTotal[r] / nrDeparturesTotal[r] * sumWTotal[r] / nrDeparturesTotal[r];
                for (int i = 0; i < nrCustomers; i++)
                {
                    runsumW[i] += sumW[r, i] / nrDepartures[r, i];
                    runsumW2[i] += sumW[r, i] / nrDepartures[r, i] * sumW[r, i] / nrDepartures[r, i];
                }
                runsumPW += nrCustomersWait[r] / nrDeparturesTotal[r];
                runsumPW2 += nrCustomersWait[r] / nrDeparturesTotal[r] * nrCustomersWait[r] / nrDeparturesTotal[r];
            }

            double meanLs = runsumLs / runs;
            double sigmaLs = Math.Sqrt(runsumLs2 / runs - meanLs * meanLs);
            double ciLs = 1.96 * sigmaLs / Math.Sqrt(runs);

            double meanLq = runsumLq / runs;
            double sigmaLq = Math.Sqrt(runsumLq2 / runs - meanLq * meanLq);
            double ciLq = 1.96 * sigmaLq / Math.Sqrt(runs);

            double meanS = runsumS / runs;
            double sigmaS = Math.Sqrt(runsumS2 / runs - meanS * meanS);
            double ciS = 1.96 * sigmaS / Math.Sqrt(runs);

            double meanWTotal = runsumWTotal / runs;
            double sigmaWTotal = Math.Sqrt(runsumWTotal2 / runs - meanWTotal * meanWTotal);
            double ciWTotal = 1.96 * sigmaWTotal / Math.Sqrt(runs);

            double[] meanW = new double[nrCustomers];
            double[] sigmaW = new double[nrCustomers];
            double[] ciW = new double[nrCustomers];
            for (int i = 0; i < nrCustomers; i++)
            {
                meanW[i] = runsumW[i] / runs;
                sigmaW[i] = Math.Sqrt(runsumW2[i] / runs - meanW[i] * meanW[i]);
                ciW[i] = 1.96 * sigmaW[i] / Math.Sqrt(runs);
            }

            double meanPW = runsumPW / runs;
            double sigmaPW = Math.Sqrt(runsumPW2 / runs - meanPW * meanPW);
            double ciPW = 1.96 * sigmaPW / Math.Sqrt(runs);

            // TODO: REMOVE THIS!
            #region // Mean waiting time estimation
            //double totalRate = new double();
            //foreach (Distribution distribution in serviceDistributions)
            //{
            //    totalRate += 1 / distribution.Average;
            //}
            //double averageResidual = new double();
            //foreach (Distribution distribution in serviceDistributions)
            //{
            //    averageResidual += 1 / distribution.Average / totalRate * distribution.Residual;
            //}
            //double meanWEstimation = meanPW / (1 - utilization) * averageResidual / nrServers;
            #endregion

            writer.Write($"{utilization,11:0.00}\t\t{meanWTotal,20:F16}\t{ciWTotal,20:F16}\t{meanPW,20:F16}\t{ciPW,20:F16}");
            for (int cID = 0; cID < nrCustomers; cID++)
            {
                writer.Write($"\t{meanW[cID],20:F16}\t{ciW[cID],20:F16}");
            }
            writer.WriteLine();

            // -----------------------------



            #region // Nasty code for N-system
            //double arrivalRateTotal = new double();
            //for (int i = 0; i < arrivalDistributions.Length; i++)
            //{
            //    arrivalRateTotal += arrivalDistributions[i].rate;
            //}
            //double serviceRateTotal = new double();
            //for (int i = 0; i < serviceDistributions.Length; i++)
            //{
            //    serviceRateTotal += serviceDistributions[i].rate;
            //}
            //double meanW01 = 0.5 * (serviceDistributions[0].rate / serviceRateTotal * serviceDistributions[0].residual + serviceDistributions[1].rate / serviceRateTotal * serviceDistributions[1].residual) / (1 - arrivalRateTotal / serviceRateTotal);
            //double meanW0 = serviceDistributions[0].residual / (1 - arrivalDistributions[0].rate / serviceDistributions[0].rate);
            //double meanW1 = serviceDistributions[1].residual / (1 - arrivalDistributions[1].rate / serviceDistributions[1].rate);
            //double C = 1 / (serviceDistributions[0].rate + serviceDistributions[1].rate - arrivalDistributions[0].rate - arrivalDistributions[1].rate) * 1 / (serviceDistributions[1].rate - arrivalDistributions[1].rate) +
            //    1 / (serviceDistributions[0].rate + serviceDistributions[1].rate - arrivalDistributions[0].rate - arrivalDistributions[1].rate) * 1 / serviceDistributions[0].rate +
            //    1 / serviceDistributions[0].rate * 1 / (arrivalDistributions[0].rate + arrivalDistributions[1].rate) +
            //    1 / (serviceDistributions[1].rate - arrivalDistributions[1].rate) * 1 / arrivalDistributions[0].rate +
            //    1 / (arrivalDistributions[0].rate + arrivalDistributions[1].rate) * 1 / (arrivalDistributions[0].rate + arrivalDistributions[1].rate) +
            //    1 / (arrivalDistributions[0].rate + arrivalDistributions[1].rate) * 1 / arrivalDistributions[0].rate;
            //double meanPW01 = 1 / C * 1 / (serviceDistributions[0].rate + serviceDistributions[1].rate - arrivalDistributions[0].rate - arrivalDistributions[1].rate) * 1 / (serviceDistributions[1].rate - arrivalDistributions[1].rate);
            //double meanPW10 = 1 / C * 1 / (serviceDistributions[0].rate + serviceDistributions[1].rate - arrivalDistributions[0].rate - arrivalDistributions[1].rate) * 1 / serviceDistributions[0].rate;
            //double meanPW0 = 1 / C * 1 / serviceDistributions[0].rate * 1 / (arrivalDistributions[0].rate + arrivalDistributions[1].rate);
            //double meanPW1 = 1 / C * 1 / (serviceDistributions[1].rate - arrivalDistributions[1].rate) * 1 / arrivalDistributions[0].rate;
            //double[] meanWEstimation = new double[nrCustomers];
            //meanWEstimation[0] = meanPW01 * meanW01 + meanPW10 * meanW01;
            //meanWEstimation[1] = meanPW01 * (meanW01 + meanW1) + meanPW10 * meanW01 + meanPW1 * meanW1;
            #endregion


            //Console.WriteLine("\n{0}RESULTS\n{0}", new String('\u2500', 80), new String('\u2500', 80));
            //Console.WriteLine("\t\t\t\tSimulation");
            //Console.WriteLine("E(customers in the system):\t{0:0.0000} \u00B1 {1:0.0000} (95% C.I.)", meanLs, ciLs);
            //Console.WriteLine("E(customers in the queue):\t{0:0.0000} \u00B1 {1:0.0000} (95% C.I.)", meanLq, ciLq);
            //Console.WriteLine("E(sojourn time):\t\t{0:0.0000} \u00B1 {1:0.0000} (95% C.I.)", meanS, ciS);
            //Console.WriteLine("E(waiting time):\t\t{0:0.0000} \u00B1 {1:0.0000} (95% C.I.)", meanW, ciW);
            //Console.WriteLine("P(wait):\t\t\t{0:0.0000} \u00B1 {1:0.0000} (95% C.I.)", meanPW, ciPW);
        }

        public void GetConfigurationProbabilities(StreamWriter writer)
        {
            // Determine all possible configurations
            double tmp = new double();
            for (int i = 2; i <= nrServers; i++) { tmp += 1.0 / Factorial(i); }
            int nrConfigurations = (int)(2 * Factorial(nrServers) + Factorial(nrServers) * tmp); // According to theory
            HashSet<List<int>> possibleConfigurations = new HashSet<List<int>>();
            for (int r = 0; r < runs; r++)
            {
                for (int cID = 0; cID < nrCustomers; cID++)
                {
                    foreach (KeyValuePair<List<int>, double> configuration in configurations[r, cID])
                    {
                        possibleConfigurations.Add(configuration.Key);
                        if (possibleConfigurations.Count == nrConfigurations) { goto Proceed; }
                    }
                }
            }
            Proceed:
            // Translate configuration count to probability
            for (int r = 0; r < runs; r++)
            {
                for (int cID = 0; cID < nrCustomers; cID++)
                {
                    foreach (List<int> configuration in possibleConfigurations)
                    {
                        configurations[r, cID][configuration] = configurations[r, cID][configuration] / nrArrivals[r, cID];
                    }
                }
            }
            // Determine mean configuration probability and confidence interval
            // The using statement automatically flushes AND CLOSES the stream and calls IDisposable.Dispose on the stream object
            // NOTE: do not use FileStream for text files because it writes bytes, but StreamWriter encodes the output as text
            double runsumConfigurationProbability = new double();
            double runsumConfigurationProbability2 = new double();
            for (int cID = 0; cID < nrCustomers; cID++)
            {
                writer.WriteLine($"Customer {cID}, Utilization {utilization}");
                writer.WriteLine("Configuration\t\tProbability");
                foreach (List<int> configuration in possibleConfigurations)
                {
                    runsumConfigurationProbability = 0;
                    runsumConfigurationProbability2 = 0;
                    for (int i = 0; i < nrServers; i++) { if (i < configuration.Count) { writer.Write("{0} ", configuration[i]); } else { writer.Write("x "); } }
                    for (int r = 0; r < runs; r++)
                    {
                        runsumConfigurationProbability += configurations[r, cID][configuration];
                        runsumConfigurationProbability2 += configurations[r, cID][configuration] * configurations[r, cID][configuration];
                    }
                    writer.Write("\t\t\t\t\t{0} \u00B1 {1} (95% C.I.)", string.Format("{0:0.000}", runsumConfigurationProbability / runs), string.Format("{0:0.000}", 1.96 * Math.Sqrt(runsumConfigurationProbability2 / runs - runsumConfigurationProbability / runs * runsumConfigurationProbability / runs) / Math.Sqrt(runs)));
                    writer.WriteLine("");
                }
            }
        }

        private static int Factorial(int number)
        {
            if (number == 0) { return 1; } // 0! is defined as equal to 1
            int factorial = number;
            for (int i = number - 1; i > 0; i--)
            {
                factorial *= i;
            }
            return factorial;
        }

        public class ListComparer<T> : IEqualityComparer<List<T>>
        {
            public bool Equals(List<T> x, List<T> y)
            {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(List<T> obj)
            {
                int hashcode = 0;
                foreach (T t in obj)
                {
                    hashcode ^= t.GetHashCode();
                }
                return hashcode;
            }
        }
    }
}