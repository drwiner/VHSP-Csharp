using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using BoltFreezer.PlanTools;

namespace BoltFreezer.Interfaces
{
    public interface IPlan
    {

        // Plans have an ordered list of steps.
        List<IPlanStep> Steps { get; set; }

        IState CurrentState { get; set; }

        // The plan will have an initial state.
        IState Initial { get; set; }

        // The plan will have a goal state.
        IState Goal { get; set; }

        // Insert step
        void Insert(IPlanStep newStep);

        // The plan can be cloned.
        Object Clone();

        string ToStringOrdered();
    }
}
