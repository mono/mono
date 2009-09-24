//
// ToolTaskExtension.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System.Collections;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public abstract class ToolTaskExtension : ToolTask {

		internal ToolTaskExtension ()
		{
		}
	
		Hashtable bag;

		protected internal virtual void AddCommandLineCommands (
						CommandLineBuilderExtension commandLine)
		{
		}

		protected internal virtual void AddResponseFileCommands (
						CommandLineBuilderExtension commandLine)
		{
		}
	
		protected override string GenerateCommandLineCommands ()
		{
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();
			AddCommandLineCommands (clbe);
			return clbe.ToString ();
		}

		protected override string GenerateResponseFileCommands ()
		{
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();
			AddResponseFileCommands (clbe);
			return clbe.ToString ();
		}

		protected internal bool GetBoolParameterWithDefault (string parameterName,
								     bool defaultValue)
		{
			if (Bag.Contains (parameterName))
				return (bool) Bag [parameterName];
			else
				return defaultValue;
		}

		protected internal int GetIntParameterWithDefault (string parameterName,
								   int defaultValue)
		{
			if (Bag.Contains (parameterName))
				return (int) Bag [parameterName];
			else
				return defaultValue;
		}

		protected override bool HasLoggedErrors {
			get { return Log.HasLoggedErrors; }
		}

		protected internal Hashtable Bag {
			get {
				if (bag == null)
					bag = new Hashtable ();
				return bag;
			}
		}

		public new TaskLoggingHelper Log {
			get { return base.Log; }
		}
	}
}

#endif
