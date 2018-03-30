using BoltFreezer.Interfaces;
using BoltFreezer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace BoltFreezer.PlanTools
{
    [Serializable]
    public static class GroundActionFactory
    {
        public static Dictionary<int, IOperator> GroundLibrary;
        public static List<IOperator> GroundActions;
        public static List<IObject> Objects;
        public static Hashtable typeDict;

        private static List<IPredicate> Init;
        private static HashSet<string> NonStaticPredicates;

        // Those predicates which are not established by an effect of an action but which are a precondition. They either hold initially or not at all.
        public static List<IPredicate> Statics = new List<IPredicate>();


        public static void PopulateGroundActions(List<IOperator> ops, Problem _prob)
        {
            GroundActions = new List<IOperator>();
            GroundLibrary = new Dictionary<int, IOperator>();
            typeDict = _prob.TypeList;
            Init = _prob.Initial;
            Objects = _prob.Objects;
            NonStaticPredicates = new HashSet<string>();
            foreach (var op in ops)
            {
                foreach (var eff in op.Effects)
                {
                    NonStaticPredicates.Add(eff.Name);
                }
            }
            FromOperators(ops);
        }

        public static void FromOperator(IOperator op)
        {

            var permList = new List<List<IObject>>();
            foreach (Term variable in op.Terms)
            {
                permList.Add(typeDict[variable.Type] as List<IObject>);
            }

            foreach (var combination in EnumerableExtension.GenerateCombinations(permList))
            {
                // Add bindings
                var opClone = op.Clone() as Operator;
                var termStringList = from term in opClone.Terms select term.Variable;
                var constantStringList = from objConst in combination select objConst.Name;
                opClone.AddBindings(termStringList.ToList(), constantStringList.ToList());
                var legal = true;
                foreach (var precon in opClone.Preconditions)
                {
                    if (!NonStaticPredicates.Contains(precon.Name))
                    {
                        // this predicate is static
                        if (!Init.Contains(precon))
                        {
                            legal = false;
                            break;
                        }
                    }
                }
                if (!legal)
                {
                    continue;
                }

                // this ensures that this ground operator has a unique ID
                var groundOperator = new Operator(opClone.Name, opClone.Terms, opClone.Bindings, opClone.Preconditions, opClone.Effects);

                if (GroundLibrary.ContainsKey(groundOperator.ID))
                    throw new System.Exception();

                GroundActions.Add(groundOperator as IOperator);
                GroundLibrary[groundOperator.ID] = groundOperator;
            }
        }

        public static void FromOperators(List<IOperator> operators)
        {
            foreach (var op in operators)
            {
                FromOperator(op);
            }
        }

    }
}