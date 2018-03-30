
using BoltFreezer.Enums;
using BoltFreezer.Interfaces;
using System.Collections.Generic;

namespace BoltFreezer.PlanTools
{


    public class AddReuseHeuristic : IHeuristic
    {
        public new string ToString()
        {
            return HType.ToString();
        }

        public HeuristicType HType
        {
            get { return HeuristicType.AddReuseHeuristic; }
        }

        public float Heuristic(IPlan plan)
        {
            return HeuristicMethods.HSP(plan);
        }

    }



    public class ZeroHeuristic : IHeuristic
    {
        public new string ToString()
        {
            return HType.ToString();
        }

        public HeuristicType HType
        {
            get { return HeuristicType.ZeroHeuristic; }
        }

        public float Heuristic(IPlan plan)
        {
            return 0f;
        }
    }


    public static class HeuristicMethods
    {

        private static Dictionary<IOperator, int> visitedOps = new Dictionary<IOperator, int>();
        private static Dictionary<IPredicate, int> visitedPreds = new Dictionary<IPredicate, int>();

        private static List<IPredicate> currentlyEvaluatedPreds;

        // h^r_add(pi) = sum_(oc in plan) 0 if exists a step possibly preceding oc.step and h_add(oc.precondition) otherwise.
        public static int HSP(IPlan plan)
        {
            return 0;
        }

    }
}