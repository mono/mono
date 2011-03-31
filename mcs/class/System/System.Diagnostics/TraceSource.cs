//
// TraceSource.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;

namespace System.Diagnostics
{
	public class TraceSource
	{
		SourceSwitch source_switch;
		TraceListenerCollection listeners;

		public TraceSource (string name)
			: this (name, SourceLevels.Off)
		{
		}

		public TraceSource (string name, SourceLevels sourceLevels)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			Hashtable sources = DiagnosticsConfiguration.Settings ["sources"] as Hashtable;
			TraceSourceInfo info = sources != null ? sources [name] as TraceSourceInfo : null;
			source_switch = new SourceSwitch (name);

			if (info == null)
				listeners = new TraceListenerCollection ();
			else {
				source_switch.Level = info.Levels;
				listeners = info.Listeners;
			}
		}

		public StringDictionary Attributes {
			get { return source_switch.Attributes; }
		}

		public TraceListenerCollection Listeners {
			get { return listeners; }
		}

		public string Name {
			get { return source_switch.DisplayName; }
		}

		public SourceSwitch Switch {
			get { return source_switch; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				source_switch = value;
			}
		}

		public void Close ()
		{
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.Close ();
			}
		}

		public void Flush ()
		{
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.Flush ();
			}
		}

		[Conditional ("TRACE")]
		public void TraceData (
			TraceEventType eventType, int id, object data)
		{
			if (!source_switch.ShouldTrace (eventType))
				return;
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.TraceData (new TraceEventCache(), Name, eventType, id, data);
			}
		}

		[Conditional ("TRACE")]
		public void TraceData (
			TraceEventType eventType, int id, params object [] data)
		{
			if (!source_switch.ShouldTrace (eventType))
				return;
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.TraceData (new TraceEventCache(), Name, eventType, id, data);
			}
		}

		[Conditional ("TRACE")]
		public void TraceEvent (TraceEventType eventType, int id)
		{
			if (!source_switch.ShouldTrace (eventType))
				return;
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.TraceEvent (new TraceEventCache(), Name, eventType, id);
			}
		}

		[Conditional ("TRACE")]
		public void TraceEvent (TraceEventType eventType,
			int id, string message)
		{
			if (!source_switch.ShouldTrace (eventType))
				return;
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.TraceEvent (new TraceEventCache(), Name, eventType, id, message);
			}
		}

		[Conditional ("TRACE")]
		public void TraceEvent (TraceEventType eventType,
			int id, string format, params object [] args)
		{
			if (!source_switch.ShouldTrace (eventType))
				return;
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.TraceEvent (new TraceEventCache(), Name, eventType, id, format, args);
			}
		}

		[Conditional ("TRACE")]
		public void TraceInformation (string format)
		{
			TraceEvent (TraceEventType.Information, 0, format);
		}

		[Conditional ("TRACE")]
		public void TraceInformation (
			string format, params object [] args)
		{
			TraceEvent (TraceEventType.Information, 0, format, args);
		}

		[Conditional ("TRACE")]
		public void TraceTransfer (int id, string message, Guid relatedActivityId)
		{
			if (!source_switch.ShouldTrace (TraceEventType.Transfer ))
				return;
			lock (((ICollection) listeners).SyncRoot) {
				foreach (TraceListener tl in listeners)
					tl.TraceTransfer (new TraceEventCache(), Name, id, message, relatedActivityId);
			}
		}

		protected virtual string [] GetSupportedAttributes ()
		{
			return null;
		}
	}
}

#endif
