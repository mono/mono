//
// SourceSwitch.cs
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


using System;

namespace System.Diagnostics
{
	public class SourceSwitch : Switch
	{
		// FIXME: better explanation.
		const string description = "Source switch.";

		public SourceSwitch (string displayName)
			: this (displayName, null)
		{
		}

		public SourceSwitch (string displayName, string defaultSwitchValue)
			: base (displayName, description, defaultSwitchValue)
		{
		}
		
		public SourceLevels Level {
			get { return (SourceLevels) SwitchSetting; }
			set {
				SwitchSetting = (int) value;
			}
		}

		public bool ShouldTrace (TraceEventType eventType)
		{
			switch (eventType) {
			case TraceEventType.Critical:
				return (Level & SourceLevels.Critical) != 0;
			case TraceEventType.Error:
				return (Level & SourceLevels.Error) != 0;
			case TraceEventType.Warning:
				return (Level & SourceLevels.Warning) != 0;
			case TraceEventType.Information:
				return (Level & SourceLevels.Information) != 0;
			case TraceEventType.Verbose:
				return (Level & SourceLevels.Verbose) != 0;
			case TraceEventType.Start:
			case TraceEventType.Stop:
			case TraceEventType.Suspend:
			case TraceEventType.Resume:
			case TraceEventType.Transfer:
			default:
				return (Level & SourceLevels.ActivityTracing) != 0;
			}
		}

		protected override void OnValueChanged ()
		{
			SwitchSetting = (int) Enum.Parse (typeof (SourceLevels),
				Value, true);
		}
	}
}

