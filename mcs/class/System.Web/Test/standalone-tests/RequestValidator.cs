//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2010 Novell, Inc http://novell.com/
//

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
#if NET_4_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Util;

using StandAloneRunnerSupport;
using StandAloneTests;

using NUnit.Framework;

using StandAloneTests.RequestValidator.Generated;

namespace StandAloneTests.RequestValidator
{
	class RequestValidatorCallSet
	{
		List <Dictionary <string, object>> callSets;

		public List <Dictionary <string, object>> CallSets {
			get {
				if (callSets == null)
					callSets = new List <Dictionary <string, object>> ();

				return callSets;
			}
		}

		public string Name {
			get;
			protected set;
		}
		
		protected void RegisterCallSet (Dictionary <string, object> callSet) 
		{
			if (callSet == null || callSet.Count == 0)
				return;
			
			CallSets.Add (callSet);
		}

		public bool ContainsCallSet (Dictionary <string, object> callSet)
		{
			foreach (var dict in CallSets)
				if (DictionariesEqual (dict, callSet))
					return true;

			return false;
		}

		bool DictionariesEqual (Dictionary <string, object> first, Dictionary <string, object> second)
		{
			if (first == null ^ second == null)
				return false;
			
			if (first.Count != second.Count)
				return false;
			
			object left, right;
			foreach (string s in first.Keys) {
				if (s == "calledFrom")
					continue;
				
				if (!second.TryGetValue (s, out left))
					return false;

				right = first [s];
				if (left == null ^ right == null)
					return false;

				if (left == null)
					continue;
				
				if (!left.Equals (right))
					return false;
			}

			return true;
		}
	}

	static class RequestValidatorCallSetContainer
	{
		public static List <RequestValidatorCallSet> CallSets {
			get;
			private set;
		}
		
		static RequestValidatorCallSetContainer ()
		{
			CallSets  = new List <RequestValidatorCallSet> ();
		}

		public static RequestValidatorCallSet GetCallSet (string name)
		{
			foreach (RequestValidatorCallSet cs in CallSets)
				if (String.Compare (cs.Name, name, StringComparison.Ordinal) == 0)
					return cs;

			return null;
		}
		
		public static void Register (RequestValidatorCallSet callSet)
		{
			CallSets.Add (callSet);
		}
	}

	[TestCase ("RequestValidator", "4.0 extensible request validation tests.")]
	public sealed class RequestValidatorTests : ITestCase
	{
		public string PhysicalPath {
			get { return Path.Combine (Consts.BasePhysicalDir, "RequestValidator"); }
		}
		
		public string VirtualPath  {
			get { return "/"; }
		}

		public bool SetUp (List <TestRunItem> runItems)
		{
			GeneratedCallSets.Register ();

			runItems.Add (new TestRunItem ("Default.aspx", Default_Aspx));
			runItems.Add (new TestRunItem ("Default.aspx?key=invalid<script>value</script>", Default_Aspx_Script));
			
			return true;
		}

		string SummarizeCallSet (Dictionary <string, object> callSet)
		{
			return String.Format (@"                      URL: {0}
          Context present: {1}
                    Value: {2}
Request validation source: {3}
           Collection key: {4}
 Validation failure index: {5}
             Return value: {6}
",
					      callSet ["rawUrl"],
					      callSet ["context"],
					      callSet ["value"],
					      (int)callSet ["requestValidationSource"],
					      callSet ["collectionKey"],
					      callSet ["validationFailureIndex"],
					      callSet ["returnValue"]);
		}
		
		void Default_Aspx (string result, TestRunItem runItem)
		{
			if (runItem == null)
				throw new ArgumentNullException ("runItem");
			CompareCallSets (runItem, "000");
		}

		void Default_Aspx_Script (string result, TestRunItem runItem)
		{
			if (runItem == null)
				throw new ArgumentNullException ("runItem");

			CompareCallSets (runItem, "001");
		}

		void CompareCallSets (TestRunItem runItem, string name)
		{
			var dict = runItem.TestRunData as List <Dictionary <string, object>>;
			if (dict == null || dict.Count == 0)
				Assert.Fail ("No call data recorded.");

			RequestValidatorCallSet cs = RequestValidatorCallSetContainer.GetCallSet (name);
			if (cs == null)
				Assert.Fail ("Call set \"{0}\" not found.", name);

			foreach (Dictionary <string, object> calls in dict) {
				if (!cs.ContainsCallSet (calls))
					Assert.Fail ("{0}: call sequence not found:{1}{2}", name, Environment.NewLine, SummarizeCallSet (calls));
			}
			
		}
	}
}

#endif
