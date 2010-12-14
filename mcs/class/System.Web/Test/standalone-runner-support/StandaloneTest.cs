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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Xml;

using MonoTests.SystemWeb.Framework;
using NUnit.Framework;

namespace StandAloneRunnerSupport
{
	public sealed class StandaloneTest
	{
		const string HTML_NAMESPACE = "http://www.w3.org/1999/xhtml";
		
		string failureDetails;
		
		public TestCaseFailureException Exception {
			get; private set;
		}

		public string FailureDetails {
			get {
				if (!String.IsNullOrEmpty (failureDetails))
					return failureDetails;

				TestCaseFailureException ex = Exception;
				if (ex == null)
					return String.Empty;

				return ex.Details;
			}

			private set {
				failureDetails = value;
			}
		}

		public string FailedUrl {
			get; private set;
		}

		public string FailedUrlDescription {
			get; private set;
		}

		public string FailedUrlCallbackName {
			get; private set;
		}
		
		public TestCaseAttribute Info {
			get; private set;
		}

		public bool Success {
			get; private set;
		}
		
		public Type TestType {
			get; private set;
		}
		
		public StandaloneTest (Type testType, TestCaseAttribute info)
		{
			if (testType == null)
				throw new ArgumentNullException ("testType");
			if (info == null)
				throw new ArgumentNullException ("info");
			
			TestType = testType;
			Info = info;
		}

		public void Run (ApplicationManager appMan, bool verbose)
		{
			try {
				Success = true;
				RunInternal (appMan, verbose);
			} catch (TestCaseFailureException ex) {
				Exception = ex;
				Success = false;
			} catch (Exception ex) {
				FailureDetails = String.Format ("Test failed with exception of type '{0}':{1}{2}",
								ex.GetType (), Environment.NewLine, ex.ToString ());
				Success = false;
			}
		}
		
		void RunInternal (ApplicationManager appMan, bool verbose)
		{
			ITestCase test = Activator.CreateInstance (TestType) as ITestCase;
			var runItems = new List <TestRunItem> ();			
			if (!test.SetUp (runItems)) {
				Success = false;
				FailureDetails = "Test aborted in setup phase.";
				return;
			}

			if (runItems.Count == 0) {
				Success = false;
				FailureDetails = "No test run items returned by the test case.";
				return;
			}
			
			Response response, previousResponse = null;
			TestRunner runner;
			string[] formValues;
			
			try {
				Console.Write ('[');
				if (verbose)
					Console.WriteLine ("{0} ({1}: {2})]", TestType, Info.Name, Info.Description);
				foreach (var tri in runItems) {
					if (tri == null)
						continue;

					if (verbose)
						Console.Write ("\t{0} ({1}) ", tri.Callback != null ? tri.Callback.Method.ToString () : "<null>", 
							       !String.IsNullOrEmpty (tri.UrlDescription) ? tri.UrlDescription : tri.Url);
					runner = null;
					response = null;
					try {
						runner = appMan.CreateObject (Info.Name, typeof (TestRunner), test.VirtualPath, test.PhysicalPath, true) as TestRunner;
						if (runner == null) {
							Success = false;
							throw new InvalidOperationException ("runner must not be null.");
						}
						
						if (tri.PostValues != null && previousResponse != null)
							formValues = ExtractFormAndHiddenControls (previousResponse);
						else
							formValues = null;

						SetRunnerDomainData (tri.AppDomainData, runner.Domain);
						response = runner.Run (tri.Url, tri.PathInfo, tri.PostValues, formValues);
						if (tri.Callback == null)
							continue;

						tri.TestRunData = runner.TestRunData;
						tri.StatusCode = runner.StatusCode;
						tri.Redirected = runner.Redirected;
						tri.RedirectLocation = runner.RedirectLocation;	

						if (tri.Callback != null)
							tri.Callback (response.Body, tri);

						Console.Write ('.');
					} catch (Exception) {
						FailedUrl = tri.Url;
						FailedUrlDescription = tri.UrlDescription;

						if (tri.Callback != null) {
							MethodInfo mi = tri.Callback.Method;
							FailedUrlCallbackName = FormatMethodName (mi);
						}
						Console.Write ('F');
						throw;
					} finally {
						if (runner != null) {
							runner.Stop (true);
							AppDomain.Unload (runner.Domain);
						}
						runner = null;
						previousResponse = response;
					}
				}
			} catch (AssertionException ex) {
				throw new TestCaseFailureException ("Assertion failed.", ex.Message, ex);
			} finally {
				if (verbose)
					Console.WriteLine ();
				else
					Console.Write (']');
			}
		}

		void SetRunnerDomainData (object[] data, AppDomain domain)
		{
			int len = data != null ? data.Length : 0;
			if (len == 0)
				return;

			if (len % 2 != 0)
				throw new ArgumentException ("Must have an even number of elements.", "data");

			string name;
			for (int i = 0; i < len; i += 2) {
				name = data [i] as string;
				if (String.IsNullOrEmpty (name))
					throw new InvalidOperationException (String.Format ("Name at index {0} must not be null or empty.", i));

				domain.SetData (name, data [i + 1]);
			}
		}
		
		string[] ExtractFormAndHiddenControls (Response response)
                {
                        HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument ();
                        htmlDoc.LoadHtml (response.Body);

                        var tempxml = new StringBuilder ();
                        var tsw = new StringWriter (tempxml);
                        htmlDoc.OptionOutputAsXml = true;
                        htmlDoc.Save (tsw);

                        var doc = new XmlDocument ();
                        doc.LoadXml (tempxml.ToString ());

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
                        nsmgr.AddNamespace ("html", HTML_NAMESPACE);

                        XmlNode formNode = doc.SelectSingleNode ("//html:form", nsmgr);
                        if (formNode == null)
                                throw new ArgumentException ("Form was not found in document: " + response.Body);

                        string actionUrl = formNode.Attributes ["action"].Value;
                        XmlNode method = formNode.Attributes ["method"];
			var data = new List <string> ();
			string name, value;
			
                        foreach (XmlNode inputNode in doc.SelectNodes ("//html:input[@type='hidden']", nsmgr)) {
				name = inputNode.Attributes["name"].Value;
                                if (String.IsNullOrEmpty (name))
                                        continue;

				XmlAttribute attr = inputNode.Attributes["value"];
                                if (attr != null)
                                        value = attr.Value;
                                else
                                        value = String.Empty;

				data.Add (name);
				data.Add (value);
                        }

			return data.ToArray ();
                }
		
		static bool ShouldPrintFullName (Type type)
		{
                        return type.IsClass && (!type.IsPointer || (!type.GetElementType ().IsPrimitive && !type.GetElementType ().IsNested));
                }

                string FormatMethodName (MethodInfo mi)
		{
                        var sb = new StringBuilder ();
                        Type retType = mi.ReturnType;
                        if (ShouldPrintFullName (retType))
                                sb.Append (retType.ToString ());
                        else
                                sb.Append (retType.Name);
                        sb.Append (" ");
			sb.Append (mi.DeclaringType.FullName);
			sb.Append ('.');
                        sb.Append (mi.Name);
                        if (mi.IsGenericMethod) {
                                Type[] gen_params = mi.GetGenericArguments ();
                                sb.Append ("<");
                                for (int j = 0; j < gen_params.Length; j++) {
                                        if (j > 0)
                                                sb.Append (",");
                                        sb.Append (gen_params [j].Name);
                                }
                                sb.Append (">");
                        }
                        sb.Append ("(");
                        ParameterInfo[] p = mi.GetParameters ();
                        for (int i = 0; i < p.Length; ++i) {
                                if (i > 0)
                                        sb.Append (", ");
                                Type pt = p[i].ParameterType;
                                bool byref = pt.IsByRef;
                                if (byref)
                                        pt = pt.GetElementType ();
                                if (ShouldPrintFullName (pt))
                                        sb.Append (pt.ToString ());
                                else
                                        sb.Append (pt.Name);
                                if (byref)
                                        sb.Append (" ref");
                        }
                        if ((mi.CallingConvention & CallingConventions.VarArgs) != 0) {
                                if (p.Length > 0)
                                        sb.Append (", ");
                                sb.Append ("...");
                        }
                        
                        sb.Append (")");
                        return sb.ToString ();
                }
	}
}
