using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BoltFreezer.CacheTools
{
    public class ProblemFreezer
    {
        public string testDomainName;
        public string testDomainDirectory;
        public Domain testDomain;
        public Problem testProblem;

        private string FileName;

        public ProblemFreezer()
        {
            testDomainName = "batman";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

            FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
        }

        public ProblemFreezer(string _testDomainName, string _testDomainDirectory, Domain _testDomain, Problem _testProblem)
        {
            testDomainName = _testDomainName;
            testDomainDirectory = _testDomainDirectory;
            testDomain = _testDomain;
            testProblem = _testProblem;

            FileName = Parser.GetTopDirectory() + @"Cached\CachedOperators\" + testDomainName + "_" + testProblem.Name;
        }

        public void Serialize()
        {
            Console.Write("Creating Ground Operators");
            GroundActionFactory.PopulateGroundActions(testDomain.Operators, testProblem);

            foreach (var op in GroundActionFactory.GroundActions)
            {
                BinarySerializer.SerializeObject(FileName + op.GetHashCode().ToString() + ".CachedOperator", op);
            }
        }

        public void Deserialize()
        {
            GroundActionFactory.GroundActions = new List<IOperator>();
            GroundActionFactory.GroundLibrary = new Dictionary<int, IOperator>();
            foreach (var file in Directory.GetFiles(Parser.GetTopDirectory() + @"Cached\CachedOperators\", testDomainName + "_" + testProblem.Name + "*.CachedOperator"))
            {
                var op = BinarySerializer.DeSerializeObject<IOperator>(file);
                GroundActionFactory.GroundActions.Add(op);
                GroundActionFactory.GroundLibrary[op.ID] = op;
            }
            // THIS is so that initial and goal steps created don't get matched with these
            Operator.SetCounterExternally(GroundActionFactory.GroundActions.Count + 1);

        }

        public void FreezeProblem(bool serialize, bool deserialize)
        {
            if (serialize)
            {
                Serialize();
            }
            else if (deserialize)
            {
                Deserialize();
            }
        }
    }
}
