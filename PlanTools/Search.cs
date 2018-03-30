using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanTools
{
    public class ADstar : ISearch
    {
        private IFrontier frontier;

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public ADstar()
        {
            frontier = new PriorityQueue();
        }

        public SearchType SType {
                get { return SearchType.BestFirst;}
        }

        public List<IPlan> Search(IPlanner IP)
        {
            return Search(IP, 1, 6000);
        }

        public List<IPlan> Search(IPlanner IP, int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();

            while (Frontier.Count > 0)
            {
                var plan = Frontier.Dequeue();

                IP.Expanded++;

                if (IP.Console_log)
                {
                    Console.WriteLine(plan);
                }

                if (plan.CurrentState.Satisfies(plan.Goal.Predicates))
                {
                    // Termination criteria
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        Console.WriteLine(IP.Open.ToString() + ", " + IP.Expanded.ToString());
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    Console.WriteLine(IP.Open.ToString() + ", " + IP.Expanded.ToString());
                    return null;
                }

                IP.AddStep(plan);

            }

            return null;
        }

        public new string ToString()
        {
            return SType.ToString();
        }
    }

    public class DFS : ISearch
    {
        private IFrontier frontier;

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public DFS()
        {
            frontier = new DFSFrontier();
        }

        public SearchType SType
        {
            get { return SearchType.DFS; }
        }

        public List<IPlan> Search(IPlanner IP)
        {
            return Search(IP, 1, 6000f);
        }

        public new string ToString()
        {
            return SType.ToString();
        }

        public List<IPlan> Search(IPlanner IP, int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();

            while (Frontier.Count > 0)
            {
                var plan = Frontier.Dequeue();

                IP.Expanded++;

                if (IP.Console_log)
                {
                    Console.WriteLine(plan);
                }

                if (plan.CurrentState.Satisfies(plan.Goal.Predicates))
                {
                    // Termination criteria
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        Console.WriteLine(IP.Open.ToString() + ", " + IP.Expanded.ToString());
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    Console.WriteLine(IP.Open.ToString() + ", " + IP.Expanded.ToString());
                    return null;
                }

                IP.AddStep(plan);

            }

            return null;
        }
    }

    public class BFS : ISearch
    {
        private IFrontier frontier;

        public IFrontier Frontier
        {
            get { return frontier; }
        }

        public BFS()
        {
            frontier = new BFSFrontier();
        }

        public SearchType SType
        {
            get { return SearchType.BFS; }
        }

        public new string ToString()
        {
            return SType.ToString();
        }

        public List<IPlan> Search(IPlanner IP)
        {
            return Search(IP, 1, 6000f);
        }

        public List<IPlan> Search(IPlanner IP, int k, float cutoff)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var Solutions = new List<IPlan>();

            while (Frontier.Count > 0)
            {
                var plan = Frontier.Dequeue();

                IP.Expanded++;

                if (IP.Console_log)
                {
                    Console.WriteLine(plan);
                }

                if (plan.CurrentState.Satisfies(plan.Goal.Predicates))
                { 
                    // Termination criteria
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Solutions.Add(plan);
                    if (Solutions.Count >= k)
                    {
                        Console.WriteLine(IP.Open.ToString() + ", " + IP.Expanded.ToString());
                        return Solutions;
                    }
                    continue;
                }

                if (watch.ElapsedMilliseconds > cutoff)
                {
                    watch.Stop();
                    Console.WriteLine(IP.Open.ToString() + ", " + IP.Expanded.ToString());
                    return null;
                }

                IP.AddStep(plan);
        
            }

            return null;
        }
    }
}
