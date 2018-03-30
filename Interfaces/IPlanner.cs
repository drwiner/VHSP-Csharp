using BoltFreezer.PlanTools;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.Interfaces
{
    public interface IPlanner
    {
        ISearch Search { get; }

        List<IPlan> Solve(int k, float cutoff);

        float Score(IPlan pi);

        void Insert(IPlan pi);

        void AddStep(IPlan pi);

        bool Console_log { get; }

        int Expanded { get; set; }

        int Open { get; set; }

    }

}
