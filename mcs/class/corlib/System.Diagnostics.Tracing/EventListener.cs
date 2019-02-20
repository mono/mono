//
// EventListener.cs
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

namespace System.Diagnostics.Tracing
{
	public class EventListener : IDisposable
	{
		public EventListener ()
		{
		}

		//public event System.EventHandler<EventSourceCreatedEventArgs> EventSourceCreated;
		//public event System.EventHandler<EventWrittenEventArgs> EventWritten;

		public static int EventSourceIndex(EventSource eventSource)
		{
			return 0;
		}

		public void EnableEvents (EventSource eventSource, EventLevel level)
		{
		}

		public void EnableEvents (EventSource eventSource, EventLevel level, EventKeywords matchAnyKeyword)
		{
		}

		public void EnableEvents (EventSource eventSource, EventLevel level, EventKeywords matchAnyKeyword, IDictionary<string, string> arguments)
		{
		}

		public void DisableEvents (EventSource eventSource)
		{
		}

		protected internal virtual void OnEventSourceCreated (EventSource eventSource)
		{
		}

		protected internal virtual void OnEventWritten (EventWrittenEventArgs eventData)
		{
		}

		public virtual void Dispose()
		{
		}

#pragma warning disable CS0067
		public event EventHandler<EventSourceCreatedEventArgs> EventSourceCreated;
		public event EventHandler<EventWrittenEventArgs> EventWritten;
#pragma warning restore CS0067
	}
}

