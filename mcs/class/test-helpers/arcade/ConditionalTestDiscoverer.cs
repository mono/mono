// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.DotNet.XUnitExtensions
{
    // Internal helper class for code common to conditional test discovery through
    // [ConditionalFact] and [ConditionalTheory]
    internal static class ConditionalTestDiscoverer
    {
        // This helper method evaluates the given condition member names for a given set of test cases.
        // If any condition member evaluates to 'false', the test cases are marked to be skipped.
        // The skip reason is the collection of all the condition members that evaluated to 'false'.
        internal static string EvaluateSkipConditions(ITestMethod testMethod, object[] conditionArguments)
        {
            Type calleeType = null;
            string[] conditionMemberNames = null;

            if (CheckInputToSkipExecution(conditionArguments, ref calleeType, ref conditionMemberNames, testMethod)) return null;

            MethodInfo testMethodInfo = testMethod.Method.ToRuntimeMethod();
            Type testMethodDeclaringType = testMethodInfo.DeclaringType;
            List<string> falseConditions = new List<string>(conditionMemberNames.Count());

            foreach (string entry in conditionMemberNames)
            {
                string conditionMemberName = entry;

                // Null condition member names are silently tolerated
                if (string.IsNullOrWhiteSpace(conditionMemberName))
                {
                    continue;
                }

                Type declaringType;

                if (calleeType != null)
                {
                    declaringType = calleeType;
                }
                else
                {
                    declaringType = testMethodDeclaringType;

                    string[] symbols = conditionMemberName.Split('.');
                    if (symbols.Length == 2)
                    {
                        conditionMemberName = symbols[1];
                        ITypeInfo type = testMethod.TestClass.Class.Assembly.GetTypes(false).Where(t => t.Name.Contains(symbols[0])).FirstOrDefault();
                        if (type != null)
                        {
                            declaringType = type.ToRuntimeType();
                        }
                    }
                }

                MethodInfo conditionMethodInfo;
                if ((conditionMethodInfo = LookupConditionalMethod(declaringType, conditionMemberName)) == null)
                {
                    throw new ConditionalDiscovererException(GetFailedLookupString(conditionMemberName, declaringType));
                }

                // In the case of multiple conditions, collect the results of all
                // of them to produce a summary skip reason.
                try
                {
                    if (!(bool)conditionMethodInfo.Invoke(null, null))
                    {
                        falseConditions.Add(conditionMemberName);
                    }
                }
                catch (Exception exc)
                {
                    falseConditions.Add($"{conditionMemberName} ({exc.GetType().Name})");
                }
            }

            // Compose a summary of all conditions that returned false.
            if (falseConditions.Count > 0)
            {
                return string.Format("Condition(s) not met: \"{0}\"", string.Join("\", \"", falseConditions));
            }

            // No conditions returned false (including the absence of any conditions).
            return null;
        }

        internal static bool TryEvaluateSkipConditions(ITestFrameworkDiscoveryOptions discoveryOptions, IMessageSink diagnosticMessageSink, ITestMethod testMethod, object[] conditionArguments, out string skipReason, out ExecutionErrorTestCase errorTestCase)
        {
            skipReason = null;
            errorTestCase = null;
            try
            {
                skipReason = EvaluateSkipConditions(testMethod, conditionArguments);
                return true;
            }
            catch (ConditionalDiscovererException e)
            {
                errorTestCase = new ExecutionErrorTestCase(
                    diagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(),
                    testMethod,
                    e.Message);
                return false;
            }
        }

        internal static string GetFailedLookupString(string name, Type type)
        {
            return
                $"An appropriate member '{name}' could not be found. " +
                $"The conditional method needs to be a static method or property on the type {type} or any ancestor, " +
                "of any visibility, accepting zero arguments, and having a return type of Boolean.";
        }
        
        internal static MethodInfo LookupConditionalMethod(Type t, string name)
        {
            if (t == null || name == null)
                return null;

            TypeInfo ti = t.GetTypeInfo();

            MethodInfo mi = ti.GetDeclaredMethod(name);
            if (mi != null && mi.IsStatic && mi.GetParameters().Length == 0 && mi.ReturnType == typeof(bool))
                return mi;

            PropertyInfo pi = ti.GetDeclaredProperty(name);
            if (pi != null && pi.PropertyType == typeof(bool) && pi.GetMethod != null && pi.GetMethod.IsStatic && pi.GetMethod.GetParameters().Length == 0)
                return pi.GetMethod;

            return LookupConditionalMethod(ti.BaseType, name);
        }

        internal static bool CheckInputToSkipExecution(object[] conditionArguments, ref Type calleeType, ref string[] conditionMemberNames, ITestMethod testMethod = null)
        {
            // A null or empty list of conditionArguments is treated as "no conditions".
            // and the test cases will be executed.
            // Example: [ConditionalClass()]
            if (conditionArguments == null || conditionArguments.Length == 0) return true;

            calleeType = conditionArguments[0] as Type;
            if (calleeType != null)
            {
                if (conditionArguments.Length < 2)
                {
                    // [ConditionalFact(typeof(x))] no provided methods.
                    return true;
                }

                // [ConditionalFact(typeof(x), "MethodName")]
                conditionMemberNames = conditionArguments[1] as string[];
            }
            else
            {
                // For [ConditionalClass], unable to get the Type info. All test cases will be executed.
                if (testMethod == null) return true;

                // [ConditionalFact("MethodName")]
                conditionMemberNames = conditionArguments[0] as string[];
            }

            // [ConditionalFact((string[]) null)]
            if (conditionMemberNames == null || conditionMemberNames.Count() == 0) return true;

            return false;
        }
    }
}
