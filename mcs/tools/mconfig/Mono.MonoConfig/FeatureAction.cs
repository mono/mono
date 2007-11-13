//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mono.MonoConfig
{
	public enum ActionType
	{
		Message,
		ShellScript,
		Exec
	}

	public enum ActionWhen
	{
		Before,
		After
	}
	
	public class FeatureAction
	{
		ActionType type;
		ActionWhen when;
		string command;
		string commandArguments;
		string message;
		string script;
		
		public ActionWhen When {
			get { return when; }
		}
		
		public FeatureAction (XPathNavigator nav)
		{
			string val = Helpers.GetRequiredNonEmptyAttribute (nav, "type");
			type = Helpers.ConvertEnum <ActionType> (val, "type");

			val = Helpers.GetRequiredNonEmptyAttribute (nav, "when");
			when = Helpers.ConvertEnum <ActionWhen> (val, "when");

			XPathNodeIterator iter;
			StringBuilder sb = new StringBuilder ();
			
			switch (type) {
				case ActionType.Message:
				case ActionType.ShellScript:
					iter = nav.Select ("./text()");
					while (iter.MoveNext ())
						sb.Append (iter.Current.Value);
					if (type == ActionType.Message)
						message = sb.ToString ();
					else
						script = sb.ToString ();
					break;
					
				case ActionType.Exec:
					command = Helpers.GetRequiredNonEmptyAttribute (nav, "command");
					commandArguments = Helpers.GetOptionalAttribute (nav, "commndArguments");
					break;
			}
		}

		public void Execute ()
		{
			switch (type) {
				case ActionType.Message:
					ExecuteMessage ();
					break;

				case ActionType.ShellScript:
					ExecuteShellScript ();
					break;

				case ActionType.Exec:
					ExecuteExec ();
					break;
			}
		}

		void ExecuteMessage ()
		{
			if (String.IsNullOrEmpty (message))
				return;

			string[] lines = message.Split ('\n');
			string line;
			int maxLineWidth = Console.WindowWidth;
			StringBuilder sb = new StringBuilder ();
			
			foreach (string l in lines) {
				if (l.Length == 0) {
					sb.Append ("\n");
					continue;
				}
				
				line = l.Trim ();
				if (line.Length > maxLineWidth)
					sb.AppendFormat ("{0}\n", Helpers.BreakLongLine (line, String.Empty, maxLineWidth));
				else
					sb.AppendFormat ("{0}{1}\n", String.Empty, line);
			}
			Console.WriteLine (sb.ToString ());
		}

		void ExecuteShellScript ()
		{
			if (String.IsNullOrEmpty (script))
				return;

			string script_temp = Path.GetTempFileName ();
			StreamWriter s = null;
			
			try {
				s = new StreamWriter (script_temp);
				s.Write (script);
				s.Flush ();
				s.Close ();
				RunCommand ("/bin/sh", script_temp);
			} catch (Exception ex) {
				throw new ApplicationException ("Error executing feature 'shell script' action.", ex);
			} finally {
				if (s != null)
					s.Close ();				
				try {
					File.Delete (script_temp);
				} catch (Exception) {
					// ignore
				}
			}
		}

		void ExecuteExec ()
		{
			if (String.IsNullOrEmpty (command))
				return;

			try {
				RunCommand (command, commandArguments);
			} catch (Exception ex) {
				throw new ApplicationException ("Error executing feature 'exec' action.", ex);
			}
		}
		
		void RunCommand (string commandPath, string format, params object[] arguments)
		{
			if (String.IsNullOrEmpty (commandPath))
				return;
			string args;

			if (!String.IsNullOrEmpty (format))
				args = String.Format (format, arguments);
			else
				args = String.Empty;
			
			Process p = null;

			try {
				p = new Process ();
				ProcessStartInfo pinfo = p.StartInfo;

				pinfo.UseShellExecute = false;
				pinfo.RedirectStandardOutput = true;
				pinfo.RedirectStandardError = true;
				pinfo.FileName = commandPath;
				pinfo.Arguments = args;
				p.Start ();

				string stdout = p.StandardOutput.ReadToEnd ();
				string stderr = p.StandardError.ReadToEnd ();
				p.WaitForExit ();

				if (!String.IsNullOrEmpty (stdout))
					Console.WriteLine (stdout);
				if (!String.IsNullOrEmpty (stderr))
					Console.Error.WriteLine (stderr);
				
				int exitCode = p.ExitCode;
				if (exitCode != 0)
					throw new ApplicationException (
						String.Format ("Process signalled failure code: {0}", exitCode));
			} finally {
				if (p != null)
					p.Close ();
			}
		}
	}
}
