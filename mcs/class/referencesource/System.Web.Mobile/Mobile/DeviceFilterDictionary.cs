//------------------------------------------------------------------------------
// <copyright file="DeviceFilterDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Mobile
{
    using System.Web;
    using System.Collections;
    using System.Reflection;
    using System.Diagnostics;
    using System.ComponentModel;

    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DeviceFilterDictionary
    {
        internal class ComparisonEvaluator
        {
            internal readonly String capabilityName;
            internal readonly String capabilityArgument;

            internal ComparisonEvaluator(String name, String argument)
            {
                Debug.Assert(name != null);

                capabilityName = name;
                capabilityArgument = argument;
            }
        }

        private Hashtable _comparisonEvaluators = null;
        private Hashtable _delegateEvaluators = null;


        internal DeviceFilterDictionary()
        {
            _comparisonEvaluators = new Hashtable();
            _delegateEvaluators = new Hashtable();
        }


        internal DeviceFilterDictionary(DeviceFilterDictionary original)
        {
            _comparisonEvaluators = (Hashtable)original._comparisonEvaluators.Clone();
            _delegateEvaluators = (Hashtable)original._delegateEvaluators.Clone();
        }


        internal void AddCapabilityDelegate(String delegateName,
            MobileCapabilities.EvaluateCapabilitiesDelegate evaluator)
        {
            _delegateEvaluators[delegateName] = evaluator;
        }


        private void CheckForComparisonDelegateLoops(String delegateName)
        {
            String nextDelegateName = delegateName;
            Hashtable alreadyReferencedDelegates = new Hashtable();

            while(true)
            {
                ComparisonEvaluator nextComparisonEvaluator =
                    (ComparisonEvaluator)_comparisonEvaluators[nextDelegateName];
                if(nextComparisonEvaluator == null)
                {
                    break;
                }

                if(alreadyReferencedDelegates.Contains(nextDelegateName))
                {
                    String msg = SR.GetString(SR.DevFiltDict_FoundLoop,
                                              nextComparisonEvaluator.capabilityName,
                                              delegateName);
                    throw new Exception(msg);
                }

                alreadyReferencedDelegates[nextDelegateName] = null;
                nextDelegateName = nextComparisonEvaluator.capabilityName;
            }
        }


        internal void AddComparisonDelegate(String delegateName, String comparisonName,
            String argument)
        {
            _comparisonEvaluators[delegateName] = new ComparisonEvaluator(comparisonName,
                argument);

            CheckForComparisonDelegateLoops(delegateName);
        }


        internal bool FindComparisonEvaluator(String evaluatorName, out String capabilityName,
            out String capabilityArgument)
        {
            capabilityName = null;
            capabilityArgument = null;

            ComparisonEvaluator evaluator = (ComparisonEvaluator)_comparisonEvaluators[evaluatorName];
            if(evaluator == null)
            {
                return false;
            }

            capabilityName = evaluator.capabilityName;
            capabilityArgument = evaluator.capabilityArgument;

            return true;
        }


        internal bool FindDelegateEvaluator(String evaluatorName,
            out MobileCapabilities.EvaluateCapabilitiesDelegate evaluatorDelegate)
        {
            evaluatorDelegate = null;

            MobileCapabilities.EvaluateCapabilitiesDelegate evaluator;
            evaluator = (MobileCapabilities.EvaluateCapabilitiesDelegate)
                            _delegateEvaluators[evaluatorName];
            if(evaluator == null)
            {
                return false;
            }

            evaluatorDelegate = evaluator;

            return true;
        }


        internal bool IsComparisonEvaluator(String evaluatorName)
        {
            return _comparisonEvaluators.Contains(evaluatorName);
        }

        internal bool IsDelegateEvaluator(String evaluatorName)
        {
            return _delegateEvaluators.Contains(evaluatorName);
        }
    }
}

