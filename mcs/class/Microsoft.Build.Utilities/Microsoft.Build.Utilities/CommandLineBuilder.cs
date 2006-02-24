//
// CommandLineBuilder.cs: Builds command line options string
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
using System.Text;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities
{
	public class CommandLineBuilder
	{
		StringBuilder commandLine;
	
		public CommandLineBuilder ()
		{
			commandLine = new StringBuilder ();
		}
		
		public void AppendFileNameIfNotNull (string fileName)
		{
			if (fileName == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.Append (fileName);
		}
		
		public void AppendFileNameIfNotNull (ITaskItem fileItem)
		{
			if (fileItem == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.Append (fileItem.ToString());
		}
		
		public void AppendFileNamesIfNotNull (string[] fileNames,
						      string delimiter)
		{
			if (fileNames == null)
				return;
			bool appendDelimiter = false;
			AppendSpaceIfNotEmpty ();
			for (int i = 0; i < fileNames.Length; i++) {
				if (fileNames [i] == null)
					continue;
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					commandLine.Append (fileNames [i]);
				} else {
					commandLine.Append (fileNames [i]);
					appendDelimiter = true;
				}
			}
		}
		
		public void AppendFileNamesIfNotNull (ITaskItem[] fileItems,
						      string delimiter)
		{
			if (fileItems == null)
				return;
			bool appendDelimiter = false;
			AppendSpaceIfNotEmpty ();
			for (int i = 0; i < fileItems.Length; i++) {
				if (fileItems [i] == null)
					continue;
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					commandLine.Append (fileItems [i].ToString ());
				} else {
					commandLine.Append (fileItems [i].ToString ());
					appendDelimiter = true;
				}
			}
		}
		
		protected void AppendFileNameWithQuoting (string fileName)
		{
			if (IsQuotingRequired (fileName))
				commandLine.AppendFormat ("\"{0}\"",fileName);
			else
				commandLine.Append (fileName);
		}
		
		protected void AppendSpaceIfNotEmpty ()
		{
			if (commandLine.Length != 0)
				commandLine.Append (' ');
		}
		
		public void AppendSwitch (string switchName)
		{
			if (switchName == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.Append (switchName);
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   string parameter)
		{
			if (switchName == null || parameter == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}{1}",switchName,
				parameter);
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   ITaskItem parameter)
		{
			if (switchName == null || parameter == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}{1}",switchName,
				parameter.ToString ());
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   string[] parameters,
						   string delimiter)
		{
			if (switchName == null || parameters == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}",switchName);
			bool appendDelimiter = false;
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters [i] == null)
					continue;
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					commandLine.Append (parameters [i]);
				} else {
					commandLine.Append (parameters [i]);
					appendDelimiter = true;
				}
			}
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   ITaskItem[] parameters,
						   string delimiter)
		{
			if (switchName == null || parameters == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}",switchName);
			bool appendDelimiter = false;
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters [i] == null)
					continue;
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					commandLine.Append (parameters [i].ToString ());
				} else {
					commandLine.Append (parameters [i].ToString ());
					appendDelimiter = true;
				}
			}
		}
		
		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   string parameter)
		{
			if (switchName == null || parameter == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}{1}", switchName, parameter);
		}

		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   ITaskItem parameter)
		{
			if (switchName == null || parameter == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}{1}", switchName, parameter.GetMetadata ("Include"));
		}

		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   string[] parameters,
							   string delimiter)
		{
			if (switchName == null || delimiter == null || parameters == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}",switchName);
			bool appendDelimiter = false;
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters [i] == null)
					continue; 
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					commandLine.Append (parameters [i]);
				} else {
					commandLine.Append (parameters [i]);
					appendDelimiter = true;
				}
			}
		}

		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   ITaskItem[] parameters,
							   string delimiter)
		{
			if (switchName == null || delimiter == null || parameters == null)
				return;
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}",switchName);
			bool appendDelimiter = false;
			for (int i = 0; i < parameters.Length; i++) {
				if (parameters [i] == null)
					continue;
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					commandLine.Append (parameters [i].ToString ());
				} else {
					commandLine.Append (parameters [i].ToString ());
					appendDelimiter = true;
				}
			}
		}
		
		protected void AppendTextUnquoted (string textToAppend)
		{
			commandLine.Append (textToAppend);
		}
		
		protected void AppendTextWithQuoting (string textToAppend)
		{
			if (IsQuotingRequired (textToAppend))
				commandLine.AppendFormat ("\"{0}\"",textToAppend);
			else
				commandLine.Append (textToAppend);
		}
		
		protected virtual bool IsQuotingRequired (string parameter)
		{
			parameter.Trim ();
			// FIXME: change this to regex
			foreach (char c in parameter) {
				if (c == ' ' || c == '\t' || c == '\u000b' || c == '\u000c')
					return true;
			}
			return false;
		}
		
		[MonoTODO]
		protected virtual void VerifyThrowNoEmbeddedDoubeQuotes (string switchName,
									 string parameter)
		{
		}
		
		public override string ToString ()
		{
			return commandLine.ToString ();
		}
		
		protected StringBuilder CommandLine {
			get {
				return commandLine;
			}
		}
	}
}

#endif