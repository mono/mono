//
// LoggerInfo.cs: Contains information about logger parameters.
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

using System;
using System.Globalization;
using System.Reflection;
using Mono.XBuild.Framework;

namespace Mono.XBuild.CommandLine {
	internal class LoggerInfo : AssemblyLoadInfo {
	
		string	parameters;
	
		public LoggerInfo ()
		{
		}
		
		public LoggerInfo (string s)
		{
			string version = null;
			string culture = null;
			string name = null;
			string filename = null;
			string loggerClass = null;
		
			string[] temp1 = s.Split (':');
			string[] temp2 = temp1[1].Split (',');
			
			// FIXME: replace all of this with readable code
			loggerClass = temp2 [0];
			if (temp2.Length == 4) {
				name = temp2 [1];
				version = temp2 [2].Split ('=') [1];
				string[] temp3 = temp2 [3].Split (';');
				if (temp3.Length == 1) {
					culture = temp2 [3].Split ('=') [1];
				}
				if (temp3.Length > 1 ) {
					culture = temp3 [0].Split ('=') [1];
					parameters = temp3 [1];
				}
			}
			if (temp2.Length == 2) {
				string[] temp3 = temp2 [1].Split (';');
				if (temp3 [0].EndsWith (".dll")) {
					filename = temp2 [1];
				} else {
					name = temp2 [1];
				}
				if (temp3.Length > 1)
					parameters = temp3 [1];
			}
			
			if (name != null)
				SetAssemblyName (LoadInfoType.AssemblyName, null, name, version, culture, null,loggerClass);
			else if (filename != null)
				SetAssemblyName (LoadInfoType.AssemblyFilename, filename, null, null, null, null, loggerClass);
		}
		
		public string Parameters {
			get { return parameters; }
		}
	}
}

#endif
