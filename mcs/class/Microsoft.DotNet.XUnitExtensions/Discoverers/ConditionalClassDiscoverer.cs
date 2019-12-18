// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.DotNet.XUnitExtensions
{
    /// <summary>
    /// This class discovers all of the tests and test classes that have
    /// applied the ConditionalClass attribute
    /// </summary>
    public class ConditionalClassDiscoverer : ITraitDiscoverer
    {
        /// <summary>
        /// Gets the trait values from the Category attribute.
        /// </summary>
        /// <param name="traitAttribute">The trait attribute containing the trait values.</param>
        /// <returns>The trait values.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            // If evaluated to false, skip the test class entirely.
            if (!EvaluateParameterHelper(traitAttribute))
            {
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.Failing);
            }
        }

        internal static bool EvaluateParameterHelper(IAttributeInfo traitAttribute)
        {
            // Parse the traitAttribute. We make sure it contains two parts:
            // 1. Type 2. nameof(conditionMemberName)
            object[] conditionArguments = traitAttribute.GetConstructorArguments().ToArray();
            Debug.Assert(conditionArguments.Count() == 2);

            Type calleeType = null;
            string[] conditionMemberNames = null;

            if (ConditionalTestDiscoverer.CheckInputToSkipExecution(conditionArguments, ref calleeType, ref conditionMemberNames))
            {
                return true;
            }

            foreach (string entry in conditionMemberNames)
            {
                // Null condition member names are silently tolerated.
                if (string.IsNullOrWhiteSpace(entry)) continue;

                MethodInfo conditionMethodInfo = ConditionalTestDiscoverer.LookupConditionalMethod(calleeType, entry);
                if (conditionMethodInfo == null)
                {
                    throw new InvalidOperationException($"Unable to get MethodInfo, please check input for {entry}.");
                }

                // If one of the conditions is false, then return the category failing trait.
                if (!(bool)conditionMethodInfo.Invoke(null, null)) return false;
            }

            return true;
        }
    }
}
