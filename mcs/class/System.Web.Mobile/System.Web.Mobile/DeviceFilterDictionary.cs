
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
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
