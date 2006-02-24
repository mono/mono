//
// TaskLoggingHelper.cs: Wrapper aroudn IBuildEngine.
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
using System.IO;
using System.Resources;
using System.Text;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Utilities
{
	public class TaskLoggingHelper : MarshalByRefObject
	{
		IBuildEngine	buildEngine;
		bool		hasLoggedErrors;
		string		helpKeywordPrefix;
		string		taskName;
		ResourceManager	taskResources;
	
		public TaskLoggingHelper (ITask taskInstance)
		{
			this.buildEngine = taskInstance.BuildEngine;
		}

		[MonoTODO]
		public string ExtractMessageCode (string message,
						  out string messageWithoutCodePrefix)
		{
			messageWithoutCodePrefix = "";
			return "";
		}

		[MonoTODO]
		public virtual string FormatResourceString (string resourceName,
							    params object[] args)
		{
			return null;
		}

		[MonoTODO]
		public virtual string FormatString (string unformatted,
						   params object[] args)
		{
			return "";
		}
		
		[MonoTODO]
		public void LogCommandLine (string commandLine)
		{
		}
		
		[MonoTODO]
		public void LogCommandLine (MessageImportance importance,
					    string commandLine)
		{
		}

		public void LogError (string message,
				     params object[] messageArgs)
		{
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, null, 0, 0, 0, 0, message,
				helpKeywordPrefix, null);
			buildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogError (string subcategory, string errorCode,
				      string helpKeyword, string file,
				      int lineNumber, int columnNumber,
				      int endLineNumber, int endColumnNumber,
				      string message,
				      params object[] messageArgs)
		{
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, errorCode, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				message, helpKeywordPrefix /*it's helpKeyword*/,
				null /*it's senderName*/);
			// FIXME: what with messageArgs?
			buildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogErrorFromException (Exception e)
		{
			LogErrorFromException (e, false);
		}

		public void LogErrorFromException (Exception e,
						   bool showStackTrace)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (e.Message);
			if (showStackTrace == true)
				sb.Append (e.StackTrace);
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, null, 0, 0, 0, 0, sb.ToString (),
				e.HelpLink, e.Source);
			buildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogErrorFromResources (string messageResourceName,
						   params object[] messageArgs)
		{
			LogErrorFromResources (null, null, null, null, 0, 0, 0,
				0, messageResourceName, null);
		}

		public void LogErrorFromResources (string subcategoryResourceName,
						   string errorCode,
						   string helpKeyword,
						   string file, int lineNumber,
						   int columnNumber,
						   int endLineNumber,
						   int endColumnNumber,
						   string messageResourceName,
						   params object[] messageArgs)
		{
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				taskResources.GetString (subcategoryResourceName),
				errorCode, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber,
				taskResources.GetString (messageResourceName),
				helpKeyword, null );
			buildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogErrorWithCodeFromResources (string messageResourceName,
							  params object[] messageArgs)
		{
			// FIXME: there should be something different than normal
			// LogErrorFromResources
			LogErrorFromResources (messageResourceName, messageArgs);
		}

		public void LogErrorWithCodeFromResources (string subcategoryResourceName,
							  string file,
							  int lineNumber,
							  int columnNumber,
							  int endLineNumber,
							  int endColumnNumber,
							  string messageResourceName,
							  params object[] messageArgs)
		{
			// FIXME: there should be something different than normal
			// LogErrorFromResources
			LogErrorFromResources (subcategoryResourceName, file,
				lineNumber, columnNumber, endLineNumber,
				endColumnNumber, messageResourceName,
				messageArgs);
		}

		public void LogMessage (string message,
				       params object[] messageArgs)
		{
			LogMessage (MessageImportance.Normal, message,
				messageArgs); 
		}

		public void LogMessage (MessageImportance importance,
					string message,
					params object[] messageArgs)
		{
			BuildMessageEventArgs bmea = new BuildMessageEventArgs (
				message, helpKeywordPrefix, /*helpKeyword*/
				null /*sender*/, importance);
			// FIXME: probably messageArgs contain helpKeyword or
			// senderName
			buildEngine.LogMessageEvent (bmea);
		}

		public void LogMessageFromResources (string messageResourceName,
						     params object[] messageArgs)
		{
			LogMessage (taskResources.GetString (messageResourceName),
				messageArgs);
		}

		public void LogMessageFromResources (MessageImportance importance,
						     string messageResourceName,
						     params object[] messageArgs)
		{
			LogMessage (importance, taskResources.GetString (
				messageResourceName), messageArgs);
		}

		public bool LogMessagesFromFile (string filename)
		{
			return LogMessagesFromFile (filename, MessageImportance.Normal);
		}

		public bool LogMessagesFromFile (string filename,
						 MessageImportance messageImportance)
		{
			try {
				StreamReader sr = new StreamReader (filename);
				LogMessage (messageImportance, sr.ReadToEnd (),
					null);
				sr.Close ();
				return true;
			}
			catch (Exception ex) {
				return false;
			}
		}

		public bool LogMessagesFromStream (TextReader stream,
						   MessageImportance messageImportance)
		{
			try {
				LogMessage (messageImportance, stream.ReadToEnd (), null);
				return true;
			}
			catch (Exception ex) {
				return false;
			}
			finally {
				// FIXME: should it be done here?
				stream.Close ();
			}
		}
		
		[MonoTODO]
		public bool LogMessageFromText (string lineOfText,
						MessageImportance importance)
		{
			if (lineOfText == null)
				throw new ArgumentNullException ("lineOfText");
			return true;
		}

		public void LogWarning (string message,
				       params object[] messageArgs)
		{
			// FIXME: what about all the parameters?
			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				null, null, null, 0, 0, 0, 0, message,
				helpKeywordPrefix, null);
			buildEngine.LogWarningEvent (bwea);
		}

		public void LogWarning (string subcategory, string warningCode,
					string helpKeyword, string file,
					int lineNumber, int columnNumber,
					int endLineNumber, int endColumnNumber,
					string message,
					params object[] messageArgs)
		{
			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				subcategory, warningCode, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				message, helpKeywordPrefix, null);
			buildEngine.LogWarningEvent (bwea);
		}

		public void LogWarningFromException (Exception e)
		{
			LogWarningFromException (e, false);
		}

		public void LogWarningFromException (Exception e,
						     bool showStackTrace)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (e.Message);
			if (showStackTrace)
				sb.Append (e.StackTrace);
			LogWarning (null, null, null, null, 0, 0, 0, 0,
				sb.ToString (), null);
		}

		public void LogWarningFromResources (string messageResourceName,
						     params object[] messageArgs)
		{
			LogWarning (taskResources.GetString (messageResourceName),
				messageArgs);
		}

		public void LogWarningFromResources (string subcategoryResourceName,
						     string warningCode,
						     string helpKeyword,
						     string file,
						     int lineNumber,
						     int columnNumber,
						     int endLineNumber,
						     int endColumnNumber,
						     string messageResourceName,
						     params object[] messageArgs)
		{
			LogWarning (taskResources.GetString (subcategoryResourceName),
				warningCode, helpKeyword, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				taskResources.GetString (messageResourceName),
				messageArgs);
		}

		public void LogWarningWithCodeFromResources (string messageResourceName,
							     params object[] messageArgs)
		{
			// FIXME: what's different from normal logwarning?
			LogWarningFromResources (messageResourceName, messageArgs);
		}

		public void LogWarningWithCodeFromResources (string subcategoryResourceName,
							     string file,
							     int lineNumber,
							     int columnNumber,
							     int endLineNumber,
							     int endColumnNumber,
							     string messageResourceName,
							     params object[] messageArgs)
		{
			LogWarningFromResources (subcategoryResourceName, file,
				lineNumber, columnNumber, endLineNumber,
				endColumnNumber, messageResourceName,
				messageArgs);
		}
		
		[MonoTODO]
		public void LogExternalProjectFinished (string message,
							string helpKeyword,
							string projectFile,
							bool succeeded)
		{
		}
		
		[MonoTODO]
		public void LogExternalProjectStarted (string message,
						       string helpKeyword,
						       string projectFile,
						       string targetNames)
		{
		}

		protected IBuildEngine BuildEngine {
			get {
				return buildEngine;
			}
		}

		public bool HasLoggedErrors {
			get {
				return hasLoggedErrors;
			}
		}

		public string HelpKeywordPrefix {
			get {
				return helpKeywordPrefix;
			}
			set {
				helpKeywordPrefix = value;
			}
		}

		protected string TaskName {
			get {
				return taskName;
			}
		}

		public ResourceManager TaskResources {
			get {
				return taskResources;
			}
			set {
				taskResources = value;
			}
		}
	}
}

#endif