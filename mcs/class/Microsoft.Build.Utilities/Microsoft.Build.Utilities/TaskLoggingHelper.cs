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
		ITask		taskInstance;
	
		public TaskLoggingHelper (ITask taskInstance)
		{
			if (taskInstance == null)
				throw new ArgumentNullException ("taskInstance");

			this.taskInstance = taskInstance;
			taskName = null;
		}

		[MonoTODO]
		public string ExtractMessageCode (string message,
						  out string messageWithoutCodePrefix)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
				
			messageWithoutCodePrefix = String.Empty;
			return String.Empty;
		}

		[MonoTODO]
		public virtual string FormatResourceString (string resourceName,
							    params object[] args)
		{
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");

			if (taskResources == null)
				throw new InvalidOperationException ("Task did not register any task resources");

			string resourceString = taskResources.GetString (resourceName);
			if (resourceString == null)
				throw new ArgumentException ($"No resource string found for resource named {resourceName}");

			return FormatString (resourceString, args);
		}

		[MonoTODO]
		public virtual string FormatString (string unformatted,
						   params object[] args)
		{
			if (unformatted == null)
				throw new ArgumentNullException ("unformatted");
		
			if (args == null || args.Length == 0)
				return unformatted;
			else
				return String.Format (unformatted, args);
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
			if (message == null)
				throw new ArgumentNullException ("message");

			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");
				
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, BuildEngine.ProjectFileOfTaskNode, 0, 0, 0, 0, FormatString (message, messageArgs),
				helpKeywordPrefix, null);
			BuildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogError (string subcategory, string errorCode,
				      string helpKeyword, string file,
				      int lineNumber, int columnNumber,
				      int endLineNumber, int endColumnNumber,
				      string message,
				      params object[] messageArgs)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			
			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory, errorCode, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				FormatString (message, messageArgs), helpKeyword /*it's helpKeyword*/,
				null /*it's senderName*/);
			BuildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogErrorFromException (Exception exception)
		{
			LogErrorFromException (exception, true);
		}

		public void LogErrorFromException (Exception exception,
						   bool showStackTrace)
		{
			LogErrorFromException (exception, showStackTrace, true, String.Empty);
		}

		[MonoTODO ("Arguments @showDetail and @file are not honored")]
		public void LogErrorFromException (Exception exception,
						   bool showStackTrace, bool showDetail, string file)
		{
			if (exception == null)
				throw new ArgumentNullException ("exception");
		
			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");

			StringBuilder sb = new StringBuilder ();
			sb.Append (exception.Message);
			if (showStackTrace == true)
				sb.Append (exception.StackTrace);
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, BuildEngine.ProjectFileOfTaskNode, 0, 0, 0, 0, sb.ToString (),
				exception.HelpLink, exception.Source);
			BuildEngine.LogErrorEvent (beea);
			hasLoggedErrors = true;
		}

		public void LogErrorFromResources (string messageResourceName,
						   params object[] messageArgs)
		{
			LogErrorFromResources (null, null, null, null, 0, 0, 0,
				0, messageResourceName, messageArgs);
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
			if (messageResourceName == null)
				throw new ArgumentNullException ("messageResourceName");

			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");

			string subcategory = null;
			if (!String.IsNullOrEmpty (subcategoryResourceName))
				subcategory = taskResources.GetString (subcategoryResourceName);

			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				subcategory,
				errorCode, file, lineNumber, columnNumber,
				endLineNumber, endColumnNumber,
				FormatResourceString (messageResourceName, messageArgs),
				helpKeyword, null );
			BuildEngine.LogErrorEvent (beea);
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
			LogMessage (MessageImportance.Normal, message, messageArgs); 
		}

		public void LogMessage (MessageImportance importance,
					string message,
					params object[] messageArgs)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
		
			LogMessageFromText (FormatString (message, messageArgs), importance);
		}

		public void LogMessageFromResources (string messageResourceName,
						     params object[] messageArgs)
		{
			LogMessageFromResources (MessageImportance.Normal, messageResourceName, messageArgs);
		}

		public void LogMessageFromResources (MessageImportance importance,
						     string messageResourceName,
						     params object[] messageArgs)
		{
			if (messageResourceName == null)
				throw new ArgumentNullException ("messageResourceName");

			LogMessage (importance, FormatResourceString (messageResourceName, messageArgs));
		}

		public bool LogMessagesFromFile (string fileName)
		{
			return LogMessagesFromFile (fileName, MessageImportance.Normal);
		}

		public bool LogMessagesFromFile (string fileName,
						 MessageImportance messageImportance)
		{
			try {
				StreamReader sr = new StreamReader (fileName);
				LogMessage (messageImportance, sr.ReadToEnd (),
					null);
				sr.Close ();
				return true;
			}
			catch (Exception) {
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
			catch (Exception) {
				return false;
			}
			finally {
				// FIXME: should it be done here?
				stream.Close ();
			}
		}

		public bool LogMessageFromText (string lineOfText,
						MessageImportance messageImportance)
		{
			if (lineOfText == null)
				throw new ArgumentNullException ("lineOfText");

			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");

			BuildMessageEventArgs bmea = new BuildMessageEventArgs (
				lineOfText, helpKeywordPrefix,
				null, messageImportance);
			BuildEngine.LogMessageEvent (bmea);

			return true;
		}

		public void LogWarning (string message,
				       params object[] messageArgs)
		{
			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");

			// FIXME: what about all the parameters?
			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				null, null, BuildEngine.ProjectFileOfTaskNode, 0, 0, 0, 0, FormatString (message, messageArgs),
				helpKeywordPrefix, null);
			BuildEngine.LogWarningEvent (bwea);
		}

		public void LogWarning (string subcategory, string warningCode,
					string helpKeyword, string file,
					int lineNumber, int columnNumber,
					int endLineNumber, int endColumnNumber,
					string message,
					params object[] messageArgs)
		{
			ThrowInvalidOperationIf (BuildEngine == null, "Task is attempting to log before it was initialized");

			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				subcategory, warningCode, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				FormatString (message, messageArgs), helpKeyword, null);
			BuildEngine.LogWarningEvent (bwea);
		}

		public void LogWarningFromException (Exception exception)
		{
			LogWarningFromException (exception, false);
		}

		public void LogWarningFromException (Exception exception,
						     bool showStackTrace)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (exception.Message);
			if (showStackTrace)
				sb.Append (exception.StackTrace);
			LogWarning (null, null, null, null, 0, 0, 0, 0,
				sb.ToString (), null);
		}

		public void LogWarningFromResources (string messageResourceName,
						     params object[] messageArgs)
		{
			if (messageResourceName == null)
				throw new ArgumentNullException ("messageResourceName");

			LogWarningFromResources (null, null, null, null, 0, 0, 0, 0, messageResourceName, messageArgs);
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
			if (messageResourceName == null)
				throw new ArgumentNullException ("messageResourceName");

			string subcategory = null;
			if (!String.IsNullOrEmpty (subcategoryResourceName))
				subcategory = taskResources.GetString (subcategoryResourceName);

			LogWarning (subcategory,
				warningCode, helpKeyword, file, lineNumber,
				columnNumber, endLineNumber, endColumnNumber,
				FormatResourceString (messageResourceName, messageArgs));
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

		void ThrowInvalidOperationIf (bool condition, string message)
		{
			if (condition)
				throw new InvalidOperationException (message);
		}

		protected IBuildEngine BuildEngine {
			get {
				return taskInstance?.BuildEngine;
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

