//
// System.ComponentModel.EventDescriptor.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Novell, Inc. 2004
//

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;

namespace System.ComponentModel
{
	internal class ReflectionEventDescriptor: EventDescriptor
	{
		Hashtable handlers;
		Type _eventType;
		Type _componentType;
		EventInfo _eventInfo;
		
		public ReflectionEventDescriptor (EventInfo eventInfo) : base (eventInfo.Name, (Attribute[]) eventInfo.GetCustomAttributes (true))
		{
			_eventInfo = eventInfo;
			_componentType = eventInfo.DeclaringType;
			_eventType = eventInfo.EventHandlerType;
		}

		public ReflectionEventDescriptor (Type componentType, EventDescriptor oldEventDescriptor, Attribute[] attrs) : base (oldEventDescriptor, attrs)
		{
			_componentType = componentType;
			_eventType = oldEventDescriptor.EventType;
		}

		public ReflectionEventDescriptor (Type componentType, string name, Type type, Attribute[] attrs) : base (name, attrs)
		{
			_componentType = componentType;
			_eventType = type;
		}
		
		EventInfo GetEventInfo ()
		{
			if (_eventInfo == null) {
				_eventInfo = _componentType.GetEvent (Name);
				if (_eventInfo == null)
					throw new ArgumentException ("Accessor methods for the " + Name + " event are missing");
			}
			return _eventInfo;
		}

		public override void AddEventHandler (object component, System.Delegate value)
		{
			if (handlers == null)
				handlers = new Hashtable ();
			
			ArrayList delegates = (ArrayList) handlers [component];
			if (delegates == null) {
				delegates = new ArrayList ();
				handlers [component] = delegates;
			}
			
			if (!delegates.Contains (value))
				delegates.Add (value);
		}

		public override void RemoveEventHandler (object component, System.Delegate value)
		{
			if (handlers == null) return;
			
			ArrayList delegates = (ArrayList) handlers [component];
			if (delegates == null) return;
			
			delegates.Remove (value);
		}

		public override System.Type ComponentType
		{ 
			get { return _componentType; }
		}

		public override System.Type EventType
		{ 
			get { return _eventType; }
		}

		public override bool IsMulticast 
		{
			get { return GetEventInfo().IsMulticast; }
		}
	}
}
