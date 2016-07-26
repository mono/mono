//
// EventSource.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//	Frederik Carlier <frederik.carlier@quamotion.mobi>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
// Copyrithg (C) 2015 Quamotion (http://quamotion.mobi)
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
	public class EventSource : IDisposable
	{
		protected internal struct EventData
		{
			public IntPtr DataPointer { get; set; }
			public int Size { get; set; }
		}

		protected EventSource ()
		{
			this.Name = this.GetType().Name;
		}

		protected EventSource (bool throwOnEventWriteErrors)
			: this ()
		{
		}

		protected EventSource (EventSourceSettings settings)
			: this ()
		{
			this.Settings = settings;
		}

		protected EventSource (EventSourceSettings settings, params string[] traits)
			: this (settings)
		{
		}

		public EventSource (string eventSourceName)
		{
			this.Name = eventSourceName;
		}

		public EventSource (string eventSourceName, EventSourceSettings config)
			: this (eventSourceName)
		{
			this.Settings = config;
		}

		public EventSource (string eventSourceName, EventSourceSettings config, params string[] traits)
			: this (eventSourceName, config)
		{
		}

		~EventSource ()
		{
			Dispose (false);
		}

		public Exception ConstructionException
		{
			get { return null; }
		}

		public static Guid CurrentThreadActivityId
		{
			get { return Guid.Empty; }
		}

		public Guid Guid
		{
			get { return Guid.Empty; }
		}

		public string Name
		{
			get;
			private set;
		}

		public EventSourceSettings Settings
		{
			get;
			private set;
		}

		public bool IsEnabled ()
		{
			return false;
		}

		public bool IsEnabled (EventLevel level, EventKeywords keywords)
		{
			return false;
		}

		public bool IsEnabled (EventLevel level, EventKeywords keywords, EventChannel channel)
		{
			return false;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public string GetTrait (string key)
		{
			return null;
		}

		public void Write (string eventName)
		{
		}

		public void Write (string eventName, EventSourceOptions options)
		{
		}

		public void Write<T> (string eventName, T data)
		{
		}

		public void Write<T> (string eventName, EventSourceOptions options, T data)
		{
		}

		public void Write<T> (string eventName, ref EventSourceOptions options, ref T data)
		{
		}

		public void Write<T> (string eventName, ref EventSourceOptions options, ref Guid activityId, ref Guid relatedActivityId, ref T data)
		{
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		protected virtual void OnEventCommand (EventCommandEventArgs command)
		{
		}

		protected void WriteEvent (int eventId)
		{
			WriteEvent (eventId, new object[] { } );
		}

		protected void WriteEvent (int eventId, byte[] arg1)
		{
			WriteEvent (eventId, new object[] { arg1 } );
		}

		protected void WriteEvent (int eventId, int arg1)
		{
			WriteEvent (eventId, new object[] { arg1 } );
		}

		protected void WriteEvent (int eventId, string arg1)
		{
			WriteEvent (eventId, new object[] { arg1 } );
		}

		protected void WriteEvent (int eventId, int arg1, int arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, int arg1, int arg2, int arg3)
		{
			WriteEvent (eventId, new object[] { arg1, arg2, arg3 } );
		}

		protected void WriteEvent (int eventId, int arg1, string arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, long arg1)
		{
			WriteEvent (eventId, new object[] { arg1 } );
		}

		protected void WriteEvent (int eventId, long arg1, byte[] arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, long arg1, long arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, long arg1, long arg2, long arg3)
		{
			WriteEvent (eventId, new object[] { arg1, arg2, arg3 } );
		}

		protected void WriteEvent (int eventId, long arg1, string arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, params object[] args)
		{
		}

		protected void WriteEvent (int eventId, string arg1, int arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, string arg1, int arg2, int arg3)
		{
			WriteEvent (eventId, new object[] { arg1, arg2, arg3 } );
		}

		protected void WriteEvent (int eventId, string arg1, long arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, string arg1, string arg2)
		{
			WriteEvent (eventId, new object[] { arg1, arg2 } );
		}

		protected void WriteEvent (int eventId, string arg1, string arg2, string arg3)
		{
			WriteEvent (eventId, new object[] { arg1, arg2, arg3 } );
		}

		protected unsafe void WriteEventCore (int eventId, int eventDataCount, EventData* data)
		{
		}

		protected unsafe void WriteEventWithRelatedActivityId (int eventId, Guid relatedActivityId, params object[] args)
		{
		}

		protected unsafe void WriteEventWithRelatedActivityIdCore (int eventId, Guid* relatedActivityId, int eventDataCount, EventSource.EventData* data)
		{
		}

#if NETSTANDARD
		[MonoTODO]
		public event EventHandler<EventCommandEventArgs> EventCommandExecuted
		{
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
#endif

		[MonoTODO]
		public static string GenerateManifest (Type eventSourceType, string assemblyPathToIncludeInManifest)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GenerateManifest (Type eventSourceType, string assemblyPathToIncludeInManifest, EventManifestOptions flags)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Guid GetGuid (Type eventSourceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetName (Type eventSourceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable<EventSource> GetSources ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SendCommand (EventSource eventSource, EventCommand command, IDictionary<string, string> commandArguments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetCurrentThreadActivityId (Guid activityId)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetCurrentThreadActivityId (Guid activityId, out Guid oldActivityThatWillContinue)
		{
			throw new NotImplementedException ();
		}
	}
}

