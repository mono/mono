//
// NullEventLog.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//   Gert Driesen  <drieseng@users.sourceforge.net>
//
// (C) 2006 Novell, Inc.
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
using System.Diagnostics;

namespace System.Diagnostics
{
	// Empty implementation that does not need any specific platform
	// but should be enough to get applications to run that WRITE to eventlog
	internal class NullEventLog : EventLogImpl
	{
		public NullEventLog (EventLog coreEventLog)
			: base (coreEventLog)
		{
		}

		public override void BeginInit ()
		{
		}

		public override void Clear ()
		{
		}

		public override void Close ()
		{
		}

		public override void CreateEventSource (EventSourceCreationData sourceData)
		{
		}

		public override void Delete (string logName, string machineName)
		{
		}

		public override void DeleteEventSource (string source, string machineName)
		{
		}

		public override void Dispose (bool disposing)
		{
		}

		public override void DisableNotification ()
		{
		}

		public override void EnableNotification ()
		{
		}

		public override void EndInit ()
		{
		}

		public override bool Exists (string logName, string machineName)
		{
			return true;
		}

		protected override string FormatMessage (string source, uint messageID, string [] replacementStrings)
		{
			return string.Join (", ", replacementStrings);
		}

		protected override int GetEntryCount ()
		{
			return 0;
		}

		protected override EventLogEntry GetEntry (int index)
		{
			return null;
		}

		protected override string GetLogDisplayName ()
		{
			return CoreEventLog.Log;
		}

		protected override string [] GetLogNames (string machineName)
		{
			return new string [0];
		}

		public override string LogNameFromSourceName (string source, string machineName)
		{
			return null;
		}

		public override bool SourceExists (string source, string machineName)
		{
			return false;
		}

		public override void WriteEntry (string [] replacementStrings, EventLogEntryType type, uint instanceID, short category, byte [] rawData)
		{
		}

		public override OverflowAction OverflowAction {
			get { return OverflowAction.DoNotOverwrite; }
		}

		public override int MinimumRetentionDays {
			get { return int.MaxValue; }
		}

		public override long MaximumKilobytes {
			get { return long.MaxValue; }
			set { throw new NotSupportedException ("This EventLog implementation does not support setting max kilobytes policy"); }
		}

		public override void ModifyOverflowPolicy (OverflowAction action, int retentionDays)
		{
			throw new NotSupportedException ("This EventLog implementation does not support modifying overflow policy");
		}

		public override void RegisterDisplayName (string resourceFile, long resourceId)
		{
			throw new NotSupportedException ("This EventLog implementation does not support registering display name");
		}
	}
}
