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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities
{
	public class CommandLineBuilder
	{
		StringBuilder commandLine;
		static char [] chars;
	
		static CommandLineBuilder ()
		{
			chars = new char [] {' ', '\t', '\n', '\u000b', '\u000c', '\'', '\"', ';'};
		}
		
		public CommandLineBuilder ()
		{
			commandLine = new StringBuilder ();
		}
		
		public void AppendFileNameIfNotNull (string fileName)
		{
			if (fileName == null)
				return;
			
			VerifyThrowNoEmbeddedDoubleQuotes (null, fileName);
			AppendSpaceIfNotEmpty ();
			AppendFileNameWithQuoting (fileName);
		}
		
		public void AppendFileNameIfNotNull (ITaskItem fileItem)
		{
			if (fileItem == null)
				return;
			
			string filename = fileItem.ToString ();
			VerifyThrowNoEmbeddedDoubleQuotes (null, filename);
			AppendSpaceIfNotEmpty ();
			AppendFileNameWithQuoting (filename);
		}
		
		public void AppendFileNamesIfNotNull (string[] fileNames,
						      string delimiter)
		{
			if (delimiter == null)
				throw new ArgumentNullException (null, "Parameter \"delimiter\" cannot be null.");
		
			if (fileNames == null)
				return;
			
			bool appendDelimiter = false;
			AppendSpaceIfNotEmpty ();
			for (int i = 0; i < fileNames.Length; i++) {
				string filename = fileNames [i];
				if (filename == null)
					continue;
				VerifyThrowNoEmbeddedDoubleQuotes (null, filename);
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					AppendFileNameWithQuoting (filename);
				} else {
					AppendFileNameWithQuoting (filename);
					appendDelimiter = true;
				}
			}
		}
		
		public void AppendFileNamesIfNotNull (ITaskItem[] fileItems,
						      string delimiter)
		{
			if (delimiter == null)
				throw new ArgumentNullException (null, "Parameter \"delimiter\" cannot be null.");
		
			if (fileItems == null)
				return;
			
			bool appendDelimiter = false;
			AppendSpaceIfNotEmpty ();
			for (int i = 0; i < fileItems.Length; i++) {
				string filename = fileItems [i].ToString ();
				if (fileItems [i] == null)
					continue;

				VerifyThrowNoEmbeddedDoubleQuotes (null, filename);
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					AppendFileNameWithQuoting (filename);
				} else {
					AppendFileNameWithQuoting (filename);
					appendDelimiter = true;
				}
			}
		}
		
		protected void AppendFileNameWithQuoting (string fileName)
		{
			if (fileName == null)
				return;

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
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");

			AppendSpaceIfNotEmpty ();
			commandLine.Append (switchName);
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   string parameter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (parameter == null)
				return;

			VerifyThrowNoEmbeddedDoubleQuotes (switchName, parameter);
			AppendSpaceIfNotEmpty ();
			commandLine.Append (switchName);
			AppendTextWithQuoting (parameter);
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   ITaskItem parameter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (parameter == null)
				return;
			
			string value = parameter.ToString ();
			VerifyThrowNoEmbeddedDoubleQuotes (switchName, value);
			AppendSpaceIfNotEmpty ();
			commandLine.Append (switchName);
			AppendTextWithQuoting (value);
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   string[] parameters,
						   string delimiter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (delimiter == null)
				throw new ArgumentNullException (null, "Parameter \"delimiter\" cannot be null.");

			if (parameters == null)
				return;
			
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}",switchName);
			bool appendDelimiter = false;
			for (int i = 0; i < parameters.Length; i++) {
				string value = parameters [i];
				if (value == null)
					continue;

				VerifyThrowNoEmbeddedDoubleQuotes (switchName, value);
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					AppendTextWithQuoting (value);
				} else {
					AppendTextWithQuoting (value);
					appendDelimiter = true;
				}
			}
		}
		
		public void AppendSwitchIfNotNull (string switchName,
						   ITaskItem[] parameters,
						   string delimiter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (delimiter == null)
				throw new ArgumentNullException (null, "Parameter \"delimiter\" cannot be null.");

			if (parameters == null)
				return;
			
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}",switchName);
			bool appendDelimiter = false;
			for (int i = 0; i < parameters.Length; i++) {
				string value = parameters [i].ToString ();
				if (value == null)
					continue;

				VerifyThrowNoEmbeddedDoubleQuotes (switchName, value);
				if (appendDelimiter) {
					commandLine.Append (delimiter);
					AppendTextWithQuoting (value);
				} else {
					AppendTextWithQuoting (value);
					appendDelimiter = true;
				}
			}
		}
		
		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   string parameter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (parameter == null)
				return;
			
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}{1}", switchName, parameter);
		}

		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   ITaskItem parameter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (parameter == null)
				return;
			
			AppendSpaceIfNotEmpty ();
			commandLine.AppendFormat ("{0}{1}", switchName, parameter.ItemSpec);
		}

		public void AppendSwitchUnquotedIfNotNull (string switchName,
							   string[] parameters,
							   string delimiter)
		{
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (delimiter == null)
				throw new ArgumentNullException (null, "Parameter \"delimiter\" cannot be null.");

			if (parameters == null)
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
			if (switchName == null)
				throw new ArgumentNullException (null, "Parameter \"switchName\" cannot be null.");
		
			if (delimiter == null)
				throw new ArgumentNullException (null, "Parameter \"delimiter\" cannot be null.");

			if (parameters == null)
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
			if (textToAppend == null)
				return;

			if (IsQuotingRequired (textToAppend))
				commandLine.AppendFormat ("\"{0}\"",textToAppend);
			else
				commandLine.Append (textToAppend);
		}
		
		protected virtual bool IsQuotingRequired (string parameter)
		{
			return parameter != null && parameter.IndexOfAny (chars) >= 0;
		}
		
		protected virtual void VerifyThrowNoEmbeddedDoubleQuotes (string switchName,
									 string parameter)
		{
			if (parameter != null && parameter.IndexOf ('"') >= 0)
				throw new ArgumentException (
					String.Format ("Illegal quote passed to the command line switch named \"{0}\". The value was [{1}].",
						switchName, parameter));
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
