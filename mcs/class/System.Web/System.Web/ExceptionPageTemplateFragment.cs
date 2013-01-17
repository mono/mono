//
// Authors:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com/)
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
using System.Resources;
using System.Text;
using System.Web.Util;

namespace System.Web
{
	//
	// Value of the fragment is determined by looking at the FilePath, ResourceName and Value
	// properties. Should more than one of them be set, the first one to return a valid
	// string wins. The order is as follows, string validity is given in parentheses:
	//
	//   Value (not null)
	//   FilePath (not empty)
	//   ResourceName (not empty)
	//
	// If FilePath or ResourceName is specified and it doesn't exist or attempt to access it causes
	// an exception to be thrown, an empty string will be returned and a warning will be printed
	// to stderr.
	//
	class ExceptionPageTemplateFragment
	{
		// Name - used to look up value in ExceptionPageTemplate values dictionary
		public string Name { get; set; }

		// FilePath from which the value will be loaded
		public string FilePath { get; set; }

		// Name of the assembly resource which contains the value. If ResourceAssembly is
		// not specified, System.Web is assumed.
		public string ResourceName { get; set; }

		// Full assembly name
		public string ResourceAssembly { get; set; }

		// Macro names which are used by this fragment. Only the named macros will be
		// replaced in the value passed to ReplaceMacros.
		// It is done this way to avoid parsing the fragment for macro references.
		public List <string> MacroNames { get; set; }

		// Names of macros which are required to be present in order for the fragment to be
		// visible. If it is null or empty, the macro is always visible.
		public List <string> RequiredMacros { get; set; }
		
		// Directly specified value of the fragment
		public string Value { get; set; }

		// Which page types this fragment is valid for
		public ExceptionPageTemplateType ValidForPageType { get; set; }

		public ExceptionPageTemplateFragment ()
		{
			ValidForPageType = ExceptionPageTemplateType.Any;
		}
		
		// Must load the value of the fragment and store it in the values collection
		public virtual void Init (ExceptionPageTemplateValues values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			string tmp = Value;
			if (tmp != null) {
				values.Add (Name, tmp);
				return;
			}

			tmp = FilePath;
			if (!String.IsNullOrEmpty (tmp)) {
				values.Add (Name, LoadFile (tmp));
				return;
			}

			tmp = ResourceName;
			if (!String.IsNullOrEmpty (tmp)) {
				values.Add (Name, LoadResource (tmp));
				return;
			}
		}

		public virtual bool Visible (ExceptionPageTemplateValues values)
		{
			List <string> required = RequiredMacros;
			if (required == null || required.Count == 0)
				return true;

			if (values == null || values.Count == 0)
				return false;
			
			foreach (string macro in required) {
				if (values.Get (macro) == null)
					return false;
			}

			return true;
		}
		
		public string ReplaceMacros (string value, ExceptionPageTemplateValues values)
		{
			if (String.IsNullOrEmpty (value))
				return value;

			if (values == null)
				throw new ArgumentNullException ("values");

			List <string> macroNames = MacroNames;
			if (macroNames == null || macroNames.Count == 0)
				return value;

			var sb = new StringBuilder (value);
			string macroValue;
			
			foreach (string macro in macroNames) {
				if (String.IsNullOrEmpty (macro))
					continue;

				macroValue = values.Get (macro);
				if (macroValue == null)
					macroValue = String.Empty;

				sb.Replace ("@" + macro + "@", macroValue);
			}
				
			return sb.ToString ();
		}
		
		protected virtual string LoadFile (string path)
		{
			if (!File.Exists (path)) {
				Console.Error.WriteLine ("File '{0}' not found. Required for exception template.", path);
				return String.Empty;
			}

			try {
				return File.ReadAllText (path);
			} catch (Exception ex) {
				Console.Error.WriteLine ("Error reading file '{0}'. Required for exception template. Exception {1} has been thrown: {2}",
							 path, ex.GetType (), ex.Message);
				if (RuntimeHelpers.DebuggingEnabled)
					Console.Error.WriteLine (ex.StackTrace);
				return String.Empty;
			}
		}

		protected virtual string LoadResource (string resourceName)
		{
			string assemblyName = ResourceAssembly;
			Assembly asm;
			
			if (String.IsNullOrEmpty (assemblyName))
				asm = this.GetType ().Assembly;
			else {
				try {
					asm = Assembly.Load (assemblyName);
				} catch (Exception ex) {
					Console.Error.WriteLine ("Unable to load assembly '{0}' needed to retrieve an exception template resource '{1}'. Exception {2} has been thrown: {3}",
								 assemblyName, resourceName, ex.GetType (), ex.Message);
					if (RuntimeHelpers.DebuggingEnabled)
						Console.Error.WriteLine (ex.StackTrace);
					return String.Empty;
				}
			}

			try {
				Stream st = asm.GetManifestResourceStream (resourceName);
				if (st == null) {
					Console.Error.WriteLine ("Manifest resource '{0}' required for exception template not found in assembly '{1}'.", resourceName, assemblyName);
					return String.Empty;
				}

				using (StreamReader sr = new StreamReader (st))
					return sr.ReadToEnd ();
			} catch (Exception ex) {
				Console.Error.WriteLine ("Error reading manifest resource '{0}' from assembly '{1}', required for exception template. Exception {2} has been thrown: {3}",
							 resourceName, assemblyName, ex.GetType (), ex.Message);
				if (RuntimeHelpers.DebuggingEnabled)
					Console.Error.WriteLine (ex.StackTrace);
				return String.Empty;
			}
		}
	}
}