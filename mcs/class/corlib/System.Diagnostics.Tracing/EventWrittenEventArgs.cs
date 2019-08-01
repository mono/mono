//
// EventWrittenEventArgs.cs
//
// Authors:
//	Frederik Carlier  <frederik.carlier@quamotion.mobi>
//
// Copyright (C) 2015 Quamotion (http://quamotion.mobi)
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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Diagnostics.Tracing
{
	public class EventWrittenEventArgs : EventArgs
	{
		internal EventWrittenEventArgs (EventSource eventSource)
		{
			this.EventSource = eventSource;
		}

		public Guid ActivityId
		{
			get { return EventSource.CurrentThreadActivityId; }
		}

		public EventChannel Channel
		{
			get { return EventChannel.None; }
		}

		public int EventId
		{
			get;
			internal set;
		}

		public long OSThreadId
		{
			get;
			internal set;
		}

		public DateTime TimeStamp
		{
			get;
			internal set;
		}

		public string EventName
		{
			get;
			internal set;
		}

		public EventSource EventSource
		{
			get;
			private set;
		}

		public EventKeywords Keywords
		{
			get { return EventKeywords.None; }
		}

		public EventLevel Level
		{
			get { return EventLevel.LogAlways; }
		}

		public string Message
		{
			get;
			internal set;
		}

		public EventOpcode Opcode
		{
			get { return EventOpcode.Info; }
		}

		public ReadOnlyCollection<object> Payload
		{
			get;
			internal set;
		}

		public ReadOnlyCollection<string> PayloadNames
		{
			get;
			internal set;
		}

		public Guid RelatedActivityId
		{
			get;
			internal set;
		}

		public EventTags Tags
		{
			get { return EventTags.None; }
		}

		public EventTask Task
		{
			get { return EventTask.None; }
		}

		public byte Version
		{
			get { return 0; }
		}
	}
}

