using BoltFreezer.CacheTools;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltFreezer.PlanSpace
{
    public class StateSpacePlanner : IPlanner
    {
        private ISelection selection;
        private ISearch search;
        private bool console_log;
        private int opened, expanded = 0;
        public int problemNumber;
        public string directory;

        public bool Console_log
        {
            get { return console_log; }
            set { console_log = value; }
        }

        public int Expanded
        {
            get { return expanded; }
            set { expanded = value; }
        }

        public int Open
        {
            get { return opened; }
            set { opened = value; }
        }

        public ISearch Search
        {
            get { return search; }
        }

        public static IPlan CreateInitialPlan(ProblemFreezer PF)
        {
            return new Plan(new State(PF.testProblem.Initial) as IState, new State(PF.testProblem.Goal) as IState) as IPlan;
        }

        public StateSpacePlanner(IPlan initialPlan, ISelection _selection, ISearch _search, bool consoleLog)
        {
            // Initial Plan has initial State
            console_log = consoleLog;
            selection = _selection;
            search = _search;
            Insert(initialPlan);
        }

        public void Insert(IPlan plan)
        {
            search.Frontier.Enqueue(plan, Score(plan));
            opened++;
        }

        public float Score(IPlan plan)
        {
            return selection.Evaluate(plan);
        }

        public List<IPlan> Solve(int k, float cutoff)
        {
            return search.Search(this, k, cutoff);
        }

        public void AddStep(IPlan plan)
        {
            foreach (var cndt in GetActions(plan.CurrentState as State))
            {
                var planClone = plan.Clone() as IPlan;
                var newStep = new PlanStep(cndt.Clone() as IOperator);
                planClone.Insert(newStep);
                Insert(planClone);
            }
        }
          
        public List<Operator> GetActions(State state)
        {
            // Create a list of operators to hold the actions.
            var applicableOperators = new List<Operator>();

            // Loop through the operators in the domain.
            foreach (var op in GroundActionFactory.GroundActions)
            {
                if (state.Satisfies(op.Preconditions))
                {
                    applicableOperators.Add(op as Operator);
                }
            }

            // Return the list of actions.
            return applicableOperators;
        }

    }
}

