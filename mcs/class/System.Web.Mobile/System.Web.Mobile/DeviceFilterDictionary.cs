/**
 * Project   : Mono
 * Namespace : System.Web.Mobile
 * Class     : DeviceFilterDictionary
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.Mobile
{
	class DeviceFilterDictionary
	{
		internal class ComparisonEvaluator
		{
			public readonly string Name;
			public readonly string Argument;

			public ComparisonEvaluator(string name, string argument)
			{
				this.Name = name;
				this.Argument = argument;
			}
		}

		// for comparisons
		private Hashtable cmpEvaluators;

		// for delegates
		private Hashtable dlgEvaluators;

		internal DeviceFilterDictionary()
		{
			this.cmpEvaluators = new Hashtable();
			this.dlgEvaluators = new Hashtable();
		}

		internal DeviceFilterDictionary(DeviceFilterDictionary init)
		{
			this.cmpEvaluators = (Hashtable)init.cmpEvaluators.Clone();
			this.dlgEvaluators = (Hashtable)init.dlgEvaluators.Clone();
		}

		internal void AddCapabilityDelegate(string name,
		                   MobileCapabilities.EvaluateCapabilitiesHandler handler)
		{
			dlgEvaluators[name] = handler;
		}

		internal void AddComparisonDelegate(string delegateName,
		                  string comparisonName, string argument)
		{
			this.cmpEvaluators[delegateName] =
			        new ComparisonEvaluator(comparisonName, argument);
		}

		private void InvalidateComparisonDelegateLoops(string name)
		{
			// throws exception in case of a loop
			throw new NotImplementedException();
		}

		internal bool LocateComparisonEvaluator(string evaluatorName,
		               out string capabilityName, out string capArgument)
		{
			throw new NotImplementedException();
		}

		internal bool LocateDelegateEvaluator(string name,
		     out MobileCapabilities.EvaluateCapabilitiesHandler result)
		{
			throw new NotImplementedException();
		}

		internal bool IsComparisonEvaluator(string name)
		{
			return cmpEvaluators.Contains(name);
		}

		internal bool IsDelegateEvaluator(string name)
		{
			return dlgEvaluators.Contains(name);
		}
	}
}
