using System;
using System.Collections.Generic;
using BoltFreezer.CacheTools;
using BoltFreezer.Enums;
using BoltFreezer.FileIO;
using BoltFreezer.Interfaces;
using BoltFreezer.PlanSpace;
using BoltFreezer.PlanTools;

namespace TestVHPOP
{
    class TestAll
    {

        public static void RunPlanner(IPlan initPi, List<IOperator> domainOps, ISelection SelectMethod, ISearch SearchMethod, int k, float cutoff, string directoryToSaveTo, int problem)
        {
            var POP = new StateSpacePlanner(initPi, SelectMethod, SearchMethod, domainOps, true)
            {
                directory = directoryToSaveTo,
                problemNumber = problem,
            };
            var Solutions = POP.Solve(k, cutoff);
            if (Solutions != null)
            {
                Console.WriteLine(Solutions[0].ToString());
            }
        }


        static void Main(string[] args)
        {

            Console.Write("hello world\n");
            Parser.path = @"D:\documents\frostbow\VHSP-Csharp-Frostbow\";

            var directory = Parser.GetTopDirectory() + @"/Results/";
            var cutoff = 6000f;
            var k = 1;

            var testDomainName = "batman";
            var testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            var testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.PlanSpace);
            var testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");

            var initPlan = new Plan(new State(testProblem.Initial) as IState, new State(testProblem.Goal) as IState);
            initPlan.Objects = testProblem.Objects;

            // BFS
            RunPlanner(initPlan.Clone() as IPlan, testDomain.Operators, new Nada(new ZeroHeuristic()), new BFS(), k, cutoff, directory, 1);
            Console.ReadLine();

            //RunPlanner(initPlan.Clone() as IPlan, testDomain.Operators, new E0(new AddReuseHeuristic()), new ADstar(), k, cutoff, directory, 1);
            //Console.ReadLine();


            //RunPlanner(initPlan.Clone() as IPlan, new Nada(new ZeroHeuristic()), new DFS(), k, cutoff, directory, 1);


            Console.ReadLine();
        }
    }
}
