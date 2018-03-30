#define DEBUG
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

using BoltFreezer.Interfaces;
using BoltFreezer.PlanTools;
using BoltFreezer.Enums;
using System;

namespace BoltFreezer.FileIO
{
    public static class Parser
    {
        public static string path = @"D:\Documents\Frostbow\VHPOP\";

        // Returns the project's top directory as a string.
        public static string GetTopDirectory ()
        {
            // Split the current directory path by \.
            string[] splPath = Directory.GetCurrentDirectory().Split('\\');

            // Create a string to hold the directory path.
            string topDir = "";

            // Loop through the split path.
            for (int i = 0; i < splPath.Length - 3; i++)
                // Add the first n - 2 directories from the path.
                topDir += splPath[i] + '\\';

            // Return the new path string.
            //return @"C:\MediationService\";
            //return @"J:\Code\Mediation\GME\";
            #if (DEBUG)
                //path = @"J:\Code\Mediation\GME\";
            #endif

            if (path.Equals("")) return topDir;
            else return path;
        }

        // Returns the project's top directory for a sh script.
        public static string GetScriptDirectory ()
        {
            // Remove the drive colon from the top directory's path.
            string shDir = Regex.Replace(GetTopDirectory(), ":", "");

            // Replace all backslashes with forwardslashes.
            shDir = Regex.Replace(shDir, @"\\", "/");

            // Remove leading and trailing slashes.
            shDir = shDir.Trim('/');

            // Return the computed string.
            return shDir;
        }

        // Reads in a domain from a file.
        public static Domain GetDomain (string file, PlanType type)
        {
            bool readInStat = true;
            int start = 0;

            // The domain object.
            Domain domain = new Domain();

            // Set the domain's type.
            domain.Type = type;

            // Read the domain file into a string.
            string input = System.IO.File.ReadAllText(file);

            // Split the input string by space, line feed, character return, and tab.
            string[] words = input.Split(new char[] {' ', '\r', '\n', '\t'});

            // Remove all empty elements of the word array.
            words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // Loop through the word array.
            for (int i = 0; i < words.Length; i++)
            {
                // Set the domain name.
                if (words[i].Equals("(domain"))
                {
                    start = i - 1;
                    domain.Name = words[i + 1].Remove(words[i + 1].Length - 1);
                }

                // Begin types definitions.
                if (words[i].Equals("(:types"))
                {
                    // If the list is not empty.
                    if (!words[i + 1].Equals(")"))
                        // Loop until list is finished.
                        while (words[i][words[i].Length - 1] != ')')
                        {
                            // Create a list for sub-types.
                            List<string> subTypes = new List<string>();

                            // Read in the sub-types.
                            while (!Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("-"))
                                subTypes.Add(Regex.Replace(words[i], @"\t|\n|\r", ""));

                            // Associate sub-types with type in domain object.
                            domain.AddTypeList(subTypes, Regex.Replace(words[++i], @"\t|\n|\r|[()]", ""));
                        }
                }

                // Begin constants definitions.
                if (words[i].Equals("(:constants"))
                {
                    // If the list is not empty.
                    if (!words[i + 1].Equals(")"))
                        // Loop until list is finished.
                        while (words[i][words[i].Length - 1] != ')')
                        {
                            // Create a list for sub-types.
                            List<string> constants = new List<string>();

                            // Read in the sub-types.
                            while (!Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("-"))
                                constants.Add(Regex.Replace(words[i], @"\t|\n|\r", ""));

                            // Associate sub-types with type in domain object.
                            domain.AddConstantsList(constants, Regex.Replace(words[++i], @"\t|\n|\r|[()]", ""));
                        }
                }

                // Begin predicates definitions.
                if (words[i].Equals("(:predicates"))
                {
                    // If the list is not empty.
                    if (!words[i + 1].Equals(")"))
                        // Loop until list is finished.
                        while ((words[i][words[i].Length - 1] != ')' || words[i][words[i].Length - 2] != ')')
                                && (words[i][words[i].Length - 1] != ')' || !words[i + 1].Equals(")")))
                        {
                            Predicate pred = new Predicate();

                            pred.Name = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "");

                            while (words[i][words[i].Length - 1] != ')')
                            {
                                Term term = new Term();
                                term.Variable = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "");
                                if (Regex.Replace(words[i + 1], @"\t|\n|\r", "").Equals("-"))
                                {
                                    i++;
                                    term.Type = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "");
                                    pred.Terms.Add(term);
                                }
                            }
                            domain.Predicates.Add(pred);
                        }
                }

                // Begin an action definition.
                if (words[i].Equals("(:action"))
                {
                    if (readInStat)
                    {
                        for (int stat = start; stat < i; stat++)
                            domain.staticStart += " " + words[stat];
                        readInStat = false;
                    }

                    IOperator temp = null;

                    if (type == PlanType.PlanSpace)
                        // Create an operator object.
                        temp = new Operator();
                    else if (type == PlanType.StateSpace)
                        // Create an action object.
                        temp = new Operator();

                    // Name the operator's predicate.
                    temp.Name = Regex.Replace(words[i + 1], @"\t|\n|\r", "");

                    // Add the operator to the domain object.
                    domain.Operators.Add(temp);
                }

                // Fill in an operator's internal information.
                if (words[i].Equals(":parameters"))
                {
                    // Add the operator's parameters.
                    while (!Regex.Replace(words[i++], @"\t|\n|\r", "").Equals(":precondition"))
                        if (words[i][0] == '(' || words[i][0] == '?')
                        {
                            // Create a new term using the variable name.
                            Term term = new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", ""));
                            
                            // Check if the term has a specified type.
                            if (Regex.Replace(words[i + 1], @"\t|\n|\r", "").Equals("-"))
                            {
                                // Iterate the counter past the dash.
                                i++;

                                // Add the type to the term object.
                                term.Type = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "");
                            }

                            // Add the term to the operator's predicate.
                            domain.Operators.Last().Predicate.Terms.Add(term);
                        }

                    // Create a list to hold the preconditions.
                    List<IPredicate> preconditions = new List<IPredicate>();

                    // Add the operator's preconditions.
                    while (!Regex.Replace(words[i++], @"\t|\n|\r", "").Equals(":effect"))
                    {
                        if (words[i][0] == '(')
                        {
                            if(!words[i].Equals("(and"))
                            {
                                // Create a new precondition object.
                                Predicate pred = new Predicate();

                                // Check for a negative precondition.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the effect's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the precondition's name.
                                pred.Name = Regex.Replace(words[i], @"\t|\n|\r|[()]", "");

                                // Add the precondition to the operator.
                                preconditions.Add(pred);
                            }
                        }
                        else
                        {
                            // Add the precondition's terms.
                            if (!Regex.Replace(words[i], @"\t|\n|\r", "").Equals(":effect") && !words[i].Equals(")"))
                                if (Regex.Replace(words[i], @"\t|\n|\r|[()]", "")[0] == '?')
                                    preconditions.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                                else
                                    preconditions.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", ""), true));
                        }
                    }

                    // Add the preconditions to the last created operator.
                    domain.Operators.Last().Preconditions = preconditions;

                    // Create a list to hold the effects.
                    List<IPredicate> effects = new List<IPredicate>();

                    // Add the operator's effects.
                    while (!Regex.Replace(words[i + 1], @"\t|\n|\r", "").Equals("(:action") && !Regex.Replace(words[i], @"\t|\n|\r", "").Equals(":agents") && i < words.Length - 2)
                    {
                        if (words[i][0] == '(')
                        {
                            // Check for a conditional effect.
                            // THIS SHOULD PROBABLY BE CONDENSED
                            if (words[i].Equals("(forall") || words[i].Equals("(when"))
                            {
                                // Create a new axiom object.
                                Axiom axiom = new Axiom();
                                
                                if (words[i].Equals("(forall"))
                                {
                                    // Read in the axiom's terms.
                                    while (!Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("(when"))
                                        axiom.Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                                }

                                // If the preconditions are conjunctive.
                                if (Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("(and"))
                                {
                                    // Initialize a parentheses stack counter.
                                    int parenStack = 1;
                                    i++;

                                    // Use the stack to loop through the conjunction.
                                    while (parenStack > 0)
                                    {
                                        // Check for an open paren.
                                        if (words[i][0] == '(')
                                        {
                                            // Create new predicate.
                                            Predicate pred = new Predicate();

                                            // Check for a negative effect.
                                            if (words[i].Equals("(not"))
                                            {
                                                // Iterate the counter.
                                                i++;

                                                // Set the effect's sign to false.
                                                pred.Sign = false;
                                            }

                                            // Name the predicate.
                                            pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                            // Read in the terms.
                                            while (words[i][words[i].Length - 1] != ')')
                                                pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Read the last term.
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Add the predicate to the axiom's preconditions.
                                            axiom.Preconditions.Add(pred);
                                        }

                                        // Check for a close paren.
                                        if (words[i][words[i].Length - 1] == ')')
                                            parenStack--;
                                    }
                                }
                                else
                                {
                                    // Check for an open paren.
                                    if (words[i][0] == '(')
                                    {
                                        // Create new predicate.
                                        Predicate pred = new Predicate();

                                        // Check for a negative effect.
                                        if (words[i].Equals("(not"))
                                        {
                                            // Iterate the counter.
                                            i++;

                                            // Set the effect's sign to false.
                                            pred.Sign = false;
                                        }

                                        // Name the predicate.
                                        pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                        // Read in the terms.
                                        while (words[i][words[i].Length - 1] != ')')
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Read the last term.
                                        pred.Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));

                                        // Add the predicate to the axiom's preconditions.
                                        axiom.Preconditions.Add(pred);
                                    }
                                }

                                // If the preconditions are conjunctive.
                                if (Regex.Replace(words[++i], @"\t|\n|\r", "").Equals("(and"))
                                {
                                    // Initialize a parentheses stack counter.
                                    int parenStack = 1;
                                    i++;

                                    // Use the stack to loop through the conjunction.
                                    while (parenStack > 0)
                                    {
                                        // Check for an open paren.
                                        if (words[i][0] == '(')
                                        {
                                            // Create new predicate.
                                            Predicate pred = new Predicate();

                                            parenStack++;

                                            // Check for a negative effect.
                                            if (words[i].Equals("(not"))
                                            {
                                                // Iterate the counter.
                                                i++;

                                                // Set the effect's sign to false.
                                                pred.Sign = false;

                                                parenStack++;
                                            }

                                            // Name the predicate.
                                            pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                            // Read in the terms.
                                            while (words[i][words[i].Length - 1] != ')')
                                                pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Read the last term.
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                            // Add the predicate to the axiom's effects.
                                            axiom.Effects.Add(pred);
                                        }

                                        // Check for a close paren.
                                        if (words[i - 1][words[i - 1].Length - 1] == ')')
                                            parenStack--;

                                        if (words[i - 1].Length > 1)
                                            if (words[i - 1][words[i - 1].Length - 2] == ')')
                                                parenStack--;

                                        if (words[i - 1].Length > 2)
                                            if (words[i - 1][words[i - 1].Length - 3] == ')')
                                                parenStack--;
                                    }
                                }
                                else
                                {
                                    // Check for an open paren.
                                    if (words[i][0] == '(')
                                    {
                                        // Create new predicate.
                                        Predicate pred = new Predicate();

                                        // Check for a negative effect.
                                        if (words[i].Equals("(not"))
                                        {
                                            // Iterate the counter.
                                            i++;

                                            // Set the effect's sign to false.
                                            pred.Sign = false;
                                        }

                                        // Name the predicate.
                                        pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                                        // Read in the terms.
                                        while (words[i][words[i].Length - 1] != ')')
                                            pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Read the last term.
                                        pred.Terms.Add(new Term(Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));

                                        // Add the predicate to the axiom's effects.
                                        axiom.Effects.Add(pred);
                                    }
                                }

                                // Add the axiom to the set of conditional effects.
                                domain.Operators.Last().Conditionals.Add(axiom);
                            }
                            else if (!words[i].Equals("(and"))
                            {
                                // Create a new effect object.
                                Predicate pred = new Predicate();

                                // Check for a negative effect.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the effect's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the effect's name.
                                pred.Name = Regex.Replace(words[i], @"\t|\n|\r|[()]", "");

                                // Add the effect to the operator.
                                effects.Add(pred);
                            }
                        }
                        else
                        {
                            // Add the effect's terms.
                            if (!Regex.Replace(words[i], @"\t|\n|\r", "").Equals("(:action") && !words[i].Equals(")"))
                                if (Regex.Replace(words[i], @"\t|\n|\r|[()]", "")[0] == '?')
                                    effects.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                                else
                                    effects.Last().Terms.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", ""), true));
                        }

                        // Iterate the counter.
                        i++;
                    }

                    // Add the effects to the last created operator.
                    domain.Operators.Last().Effects = effects;

                    // Create a list for storing consenting agents.
                    List<ITerm> consenting = new List<ITerm>();

                    // Check if the action has any consenting agents.
                    if (Regex.Replace(words[i], @"\t|\n|\r", "").Equals(":agents"))
                    {
                        // If so, iterate through them.
                        while (Regex.Replace(words[++i], @"\t|\n|\r", "")[Regex.Replace(words[i], @"\t|\n|\r", "").Length - 1] != ')')
                            // And add them to the list.
                            consenting.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));

                        // Add the final item to the list.
                        consenting.Add(new Term(Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                    }

                    // Add the consenting agents to the action.
                    domain.Operators.Last().ConsentingAgents = consenting;
                }
            }

            // Create a working copy of the domain file.
            Writer.DomainToPDDL(Parser.GetTopDirectory() + @"Benchmarks\" + domain.Name.ToLower() + @"\domrob.pddl", domain);

            return domain;
        }

        // Reads in a problem from a file.
        public static Problem GetProblem (string file)
        {
            // Create the problem object.
            Problem problem = new Problem();

            // Read the domain file into a string.
            string input = System.IO.File.ReadAllText(file);

            // Split the input string by space, line feed, character return, and tab.
            string[] words = input.Split(new char[] { ' ', '\r', '\n', '\t' });

            // Remove all empty elements of the word array.
            words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            // Loop through the word array.
            for (int i = 0; i < words.Length; i++)
            {
                // Set the problem name.
                if (words[i].Equals("(problem"))
                    problem.Name = words[i + 1].Remove(words[i + 1].Length - 1);

                // Set the domain name.
                if (words[i].Equals("(:domain"))
                    problem.Domain = words[i + 1].Remove(words[i + 1].Length - 1);

                // Fill in the problem's internal information.
                if (words[i].Equals("(:objects"))
                {
                    // A list of temporary objects to store before we know their type.
                    List<string> tempObjects = new List<string>();

                    // Add the problem objects.
                    while (!Regex.Replace(words[i++], @"\t|\n|\r", "").ToLower().Equals("(:init"))
                        if (!Regex.Replace(words[i], @"\t|\n|\r", "").ToLower().Equals("(:init"))
                            if (!Regex.Replace(words[i], @"\t|\n|\r", "").ToLower().Equals("-"))
                                tempObjects.Add(Regex.Replace(words[i], @"\t|\n|\r|[()]", "").ToLower());
                            else
                            { 
                                // Store the specified type.
                                string type = Regex.Replace(words[++i], @"\t|\n|\r|[()]", "").ToLower();

                                // For all the stored objects...
                                foreach (string tempObj in tempObjects)
                                    if (tempObj != "")
                                        // ... associate them with their type and add them to the problem.
                                        problem.Objects.Add(new Obj(tempObj, type));

                                // Clear the temporary objects list.
                                tempObjects = new List<string>();
                            }

                    // Add objects with unspecified types to the problem.
                    foreach (string tempObj in tempObjects)
                        if (tempObj != "")
                            problem.Objects.Add(new Obj(tempObj, ""));

                    // Add the initial state.
                    while (!Regex.Replace(words[i], @"\t|\n|\r", "").ToLower().Equals("(:goal"))
                    {
                        if (words[i][0] == '(')
                        {
                           
                            // Create a new predicate object.
                            Predicate pred = new Predicate();

                            // Check for a negative predicate.
                            if (words[i].Equals("(not"))
                            {
                                // Iterate the counter.
                                i++;

                                // Set the predicate's sign to false.
                                pred.Sign = false;
                            }

                            // Set the predicate's name.
                            pred.Name = Regex.Replace(words[i++], @"\t|\n|\r|[()]", "");

                            // Add the predicates's terms.
                            while (words[i][0] != '(')
                                if (!Regex.Replace(words[i], @"\t|\n|\r|[()]", "").Equals(""))
                                    pred.Terms.Add(new Term("", Regex.Replace(words[i++], @"\t|\n|\r|[()]", "")));
                                else
                                    i++;

                            // Add the predicate to the initial state.
                            problem.Initial.Add(pred);
                            
                        }
                    }

                    // Add the goal state.
                    while (i++ < words.Length - 1)
                    {
                        if (words[i][0] == '(')
                        {
                            if (!words[i].ToLower().Equals("(and"))
                            {
                                // Create a new predicate object.
                                Predicate pred = new Predicate();

                                // Check for a negative predicate.
                                if (words[i].Equals("(not"))
                                {
                                    // Iterate the counter.
                                    i++;

                                    // Set the predicate's sign to false.
                                    pred.Sign = false;
                                }

                                // Set the predicate's name.
                                pred.Name = Regex.Replace(words[i], @"\t|\n|\r|[()]", "");

                                // Add the predicate to the goal state.
                                problem.Goal.Add(pred);
                            }
                        }
                        else
                        {
                            // Add the predicate's terms.
                            if (!words[i].Equals(")"))
                                problem.Goal.Last().Terms.Add(new Term("", Regex.Replace(words[i], @"\t|\n|\r|[()]", "")));
                        }
                    }
                }
            }

            // Kind of a hack.
            problem.OriginalName = problem.Name;

            return problem;
        }

        // Reads in a problem and fills in its object types.
        public static Problem GetProblemWithTypes (string file, Domain domain)
        {
            // Read the problem file into an object.
            Problem problem = GetProblem (file);

            // Add type associations to each object.
            foreach (string type in domain.ObjectTypes)
                foreach (string subtype in domain.GetSubTypesOf(type))
                    foreach (IObject obj in problem.Objects)
                        if (obj.SubType.Equals(subtype))
                            obj.Types.Add(type);

            // Return the problem object.
            return problem;
        }
    }
}