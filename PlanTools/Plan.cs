using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

using BoltFreezer.Interfaces;

namespace BoltFreezer.PlanTools
{
    [Serializable]
    public class Plan : IPlan
    {
        private List<IPlanStep> steps;
        private IState initial;
        private IState goal;
        private IState currentstate;
        private List<IObject> objects;

        public List<IObject> Objects
        {
            get { return objects; }
            set { objects = value; }
        }

        // Access the plan's steps.
        public List<IPlanStep> Steps
        {
            get { return steps; }
            set { steps = value; }
        }

        // Access the plan's initial state.
        public IState Initial
        {
            get { return initial; }
            set { initial = value; }
        }

        // Access the plan's goal state.
        public IState Goal
        {
            get { return goal; }
            set { goal = value; }
        }

        public IState CurrentState { get => currentstate; set => currentstate = value; }

        public Plan ()
        {
            // S
            steps = new List<IPlanStep>();
            initial = new State();
            goal = new State();

        }

        public Plan(IState _initial, IState _goal)
        {
            steps = new List<IPlanStep>();
            initial = _initial;
            goal = _goal;
            CurrentState = _initial;
        }

        public Plan(List<IPlanStep> steps, IState initial, IState goal, IState currentstate)
        {
            this.steps = steps;
            this.initial = initial;
            this.goal = goal;
            this.currentstate = currentstate;
        }

        public void Insert(IPlanStep newStep)
        {
            steps.Add(newStep);
            var cs = CurrentState as State;
            CurrentState = cs.NewStateSimple(newStep.Action as Operator) as IState;
        }


        // Return the first state of the plan.
        public State GetFirstState ()
        {
            return (State)Initial.Clone();
        }

        public List<IPlanStep> TopoSort()
        {
            return Steps;
        }

        // Displays the contents of the plan.
        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var step in steps)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Displays the contents of the plan. THIS IS BROKEN
        public string ToStringOrdered ()
        {
            StringBuilder sb = new StringBuilder();

            var topoSort = TopoSort();
            foreach (var step in topoSort)
                sb.AppendLine(step.ToString());

            return sb.ToString();
        }

        // Creates a clone of the plan. (orderings, and Links are Read-only, so only their host containers are replaced)
        public Object Clone ()
        {
            var newSteps = new List<IPlanStep>();

            foreach (var step in steps)
            {
                // need clone because these have fulfilled conditions that are mutable.
                newSteps.Add(step.Clone() as IPlanStep);
            }

            //return new Plan(newSteps, newInitial, newGoal, newInitialStep, newGoalStep, newOrderings, newLinks, flawList);
            return new Plan(newSteps, Initial, Goal, CurrentState)
            {
                Objects = objects
            };
        }
    }
}