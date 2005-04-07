// InstallContext.cs
//   System.Configuration.Install.InstallContext class implementation
//
// Author:
//    Muthu Kannan (t.manki@gmail.com)
//
// (C) 2005 Novell, Inc.  http://www.novell.com/
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
using System.IO;

namespace System.Configuration.Install
{

public class InstallContext {
	private StringDictionary parameters;

	// Constructors
	public InstallContext () : this (null, null)
	{
	}

	public InstallContext (string logFilePath, string [] cmdLine)
	{
		parameters = ParseCommandLine(cmdLine);

		// Log file path specified in command line arguments
		// has higher priority than the logFilePath argument
		if (parameters.ContainsKey ("logFile")) {
			logFilePath = parameters ["logFile"];
			parameters.Remove ("logFile");
		}

		if (logFilePath == null)
			logFilePath = "";
		parameters.Add ("logFile", logFilePath);
	}

	// Properties
	public StringDictionary Parameters {
		get {
			return parameters;
		}
	}

	// Methods
	public bool IsParameterTrue (string paramName)
	{
		if (Parameters.ContainsKey (paramName)) {
			string val = (Parameters [paramName]).ToLower ();
			return val == "true" || val == "yes" || val == "1" || val == "";
		} else
			return false;
	}

	public void LogMessage (string message)
	{
		if (message == null)
			return;

		string logFilePath;
		if ((logFilePath = parameters ["logFile"]) != "") {
			StreamWriter logFile = new StreamWriter (logFilePath, true);
			try {
				logFile.WriteLine (message);
			} finally {
				logFile.Close ();
			}
		}

		if (IsParameterTrue ("logToConsole"))
			Console.WriteLine (message);
	}

	internal void addToLog (Exception e)
	{
		LogMessage ("Exception: " + e.ToString ());
		if (e.InnerException != null) {
			addToLog (e.InnerException);
			return;
		}
		LogMessage (e.StackTrace);
	}

	protected static StringDictionary ParseCommandLine (string [] args)
	{
		string key, val;

		StringDictionary sd = new StringDictionary ();

		if (args == null)
			return sd;

		foreach (string a in args) {
			// Remove leading / or - or --
			string x = a;	// I am using x instead of a
			if (a.StartsWith ("--"))
				x = x.Substring (2);
			else if (x.StartsWith ("/") || x.StartsWith ("-"))
				x = x.Substring (1);

			int index;
			if ((index = x.IndexOf ("=")) == -1) {
				key = x;
				val = "";
			} else {
				key = x.Substring (0, index);
				val = x.Substring (index + 1);
			}
			sd.Add (key, val);
		}

		return sd;
	}
}

}
