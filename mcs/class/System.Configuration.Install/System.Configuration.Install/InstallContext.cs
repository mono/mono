// System.Configuration.Install.InstallContext.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
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

using System.Collections.Specialized;

namespace System.Configuration.Install
{
	public class InstallContext
	{
		private StringDictionary parameters;
		string log_file;
		bool log = false;
		
		public InstallContext ()
		{
			log_file = null;
			log = false;
			parameters = ParseCommandLine (new string [0]);
		}

		public InstallContext (string logFilePath, string[] commandLine)
		{
			log_file = logFilePath;
			parameters = ParseCommandLine (commandLine);
			log = IsParameterTrue ("LogtoConsole");
		}

		public StringDictionary Parameters {
			get {
				return parameters;
			}
		}

		public bool IsParameterTrue (string paramName)
		{
			return parameters [paramName] == "true";
		}

		public void LogMessage (string message)
		{
			if (log)
				Console.WriteLine (message);
		}

		protected static StringDictionary ParseCommandLine (string[] args)
		{
			StringDictionary d = new StringDictionary ();
			
			foreach (string s in args){
				int p = s.IndexOf ("=");
				if (p == -1)
					d [s] = "true";
				else {
					string key = s.Substring (0, p);
					string value = s.Substring (p+1).ToLower ();
					if (value == "yes" || value == "true" || value == "1")
						value = "true";

					d [key] = value;
				}
			}
			return d;
		}
	}
}
