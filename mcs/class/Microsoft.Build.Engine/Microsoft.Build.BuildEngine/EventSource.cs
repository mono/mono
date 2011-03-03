//
// EventSource.cs: Implements IEventSource.
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

using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class EventSource : IEventSource {
		
		AnyEventHandler			anyEventRaised;
		BuildFinishedEventHandler	buildFinished;
		BuildStartedEventHandler	buildStarted;
		CustomBuildEventHandler		customEventRaised;
		BuildErrorEventHandler		errorRaised;
		BuildMessageEventHandler	messageRaised;
		ProjectFinishedEventHandler	projectFinished;
		ProjectStartedEventHandler	projectStarted;
		BuildStatusEventHandler		statusEventRaised;
		TargetFinishedEventHandler	targetFinished;
		TargetStartedEventHandler	targetStarted;
		TaskFinishedEventHandler	taskFinished;
		TaskStartedEventHandler		taskStarted;
		BuildWarningEventHandler	warningRaised;
		bool				onlyLogCriticalEvents;

		public EventSource ()
		{
			this.onlyLogCriticalEvents = false;
		}
		
		public void FireCustomEventRaised (object sender, CustomBuildEventArgs cbea)
		{
			if (customEventRaised != null)
				customEventRaised (sender, cbea);
			FireAnyEvent (sender, cbea);
		}
		public void FireErrorRaised (object sender, BuildErrorEventArgs beea)
		{
			if (errorRaised != null)
				errorRaised (sender, beea);
			FireAnyEvent (sender, beea);
		}
		public void FireMessageRaised (object sender, BuildMessageEventArgs bmea)
		{
			if (messageRaised != null)
				messageRaised (sender, bmea);
			FireAnyEvent (sender, bmea);
		}
		public void FireWarningRaised (object sender, BuildWarningEventArgs bwea)
		{
			if (warningRaised != null)
				warningRaised (sender, bwea);
			FireAnyEvent (sender, bwea);
		}
		
		public void FireTargetStarted (object sender, TargetStartedEventArgs tsea)
		{
			if (targetStarted != null)
				targetStarted (sender, tsea);
			FireAnyEvent (sender, tsea);
		}
		
		public void FireTargetFinished (object sender, TargetFinishedEventArgs tfea)
		{
			if (targetFinished != null)
				targetFinished (sender, tfea);
			FireAnyEvent (sender, tfea);
		}
		
		public void FireBuildStarted (object sender, BuildStartedEventArgs bsea)
		{
			if (buildStarted != null)
				buildStarted (sender, bsea);
			FireAnyEvent (sender, bsea);
		}
		
		public void FireBuildFinished (object sender, BuildFinishedEventArgs bfea)
		{
			if (buildFinished != null)
				buildFinished (sender, bfea);
			FireAnyEvent (sender, bfea);
		}
		
		public void FireProjectStarted (object sender, ProjectStartedEventArgs psea)
		{
			if (projectStarted != null)
				projectStarted (sender, psea);
			FireAnyEvent (sender, psea);
		}
		
		public void FireProjectFinished (object sender, ProjectFinishedEventArgs pfea)
		{
			if (projectFinished != null)
				projectFinished (sender, pfea);
			FireAnyEvent (sender, pfea);
		}
		
		public void FireTaskStarted (object sender, TaskStartedEventArgs tsea)
		{
			if (taskStarted != null)
				taskStarted (sender, tsea);
			FireAnyEvent (sender, tsea);
		}
		
		public void FireTaskFinished (object sender, TaskFinishedEventArgs tfea)
		{
			if (taskFinished != null)
				taskFinished (sender, tfea);
			FireAnyEvent (sender, tfea);
		}

		public void FireAnyEvent (object sender, BuildEventArgs bea)
		{
			if (anyEventRaised != null)
				anyEventRaised (sender, bea);
		}

		public event AnyEventHandler AnyEventRaised {
			add {
				lock (this)
					anyEventRaised += value;
			}
			remove {
				lock (this)
					anyEventRaised -= value;
			}
		}
		
		public event BuildFinishedEventHandler BuildFinished {
			add {
				lock (this)
					buildFinished += value;
			}
			remove {
				lock (this)
					buildFinished -= value;
			}
		}
		
		public event BuildStartedEventHandler BuildStarted {
			add {
				lock (this)
					buildStarted += value;
			}
			remove {
				lock (this)
					buildStarted -= value;
			}
		}
		
		public event CustomBuildEventHandler CustomEventRaised {
			add {
				lock (this)
					customEventRaised += value;
			}
			remove {
				lock (this)
					customEventRaised -= value;
			}
		}
		
		public event BuildErrorEventHandler ErrorRaised {
			add {
				lock (this)
					errorRaised += value;
			}
			remove {
				lock (this)
					errorRaised -= value;
			}
		}
		
		public event BuildMessageEventHandler MessageRaised {
			add {
				lock (this)
					messageRaised += value;
			}
			remove {
				lock (this)
					messageRaised -= value;
			}
		}
		
		public event ProjectFinishedEventHandler ProjectFinished {
			add {
				lock (this)
					projectFinished += value;
			}
			remove {
				lock (this)
					projectFinished -= value;
			}
		}
		
		public event ProjectStartedEventHandler ProjectStarted {
			add {
				lock (this)
					projectStarted += value;
			}
			remove {
				lock (this)
					projectStarted -= value;
			}
		}
		
		public event BuildStatusEventHandler StatusEventRaised {
			add {
				lock (this)
					statusEventRaised += value;
			}
			remove {
				lock (this)
					statusEventRaised -= value;
			}
		}
		
		public event TargetFinishedEventHandler TargetFinished {
			add {
				lock (this)
					targetFinished += value;
			}
			remove {
				lock (this)
					targetFinished -= value;
			}
		}
		
		public event TargetStartedEventHandler TargetStarted {
			add {
				lock (this)
					targetStarted += value;
			}
			remove {
				lock (this)
					targetStarted -= value;
			}
		}
		
		public event TaskFinishedEventHandler TaskFinished {
			add {
				lock (this)
					taskFinished += value;
			}
			remove {
				lock (this)
					taskFinished -= value;
			}
		}
		
		public event TaskStartedEventHandler TaskStarted {
			add {
				lock (this)
					taskStarted += value;
			}
			remove {
				lock (this)
					taskStarted -= value;
			}
		}
		
		public event BuildWarningEventHandler WarningRaised {
			add {
				lock (this)
					warningRaised += value;
			}
			remove {
				lock (this)
					warningRaised -= value;
			}
		}
		
		public bool OnlyLogCriticalEvents {
			get { return onlyLogCriticalEvents; }
			set { onlyLogCriticalEvents = value; }
		}
	}
}

#endif
