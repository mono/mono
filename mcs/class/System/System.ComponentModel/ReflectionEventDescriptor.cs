//
// System.ComponentModel.EventDescriptor.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) Novell, Inc. 2004, 2005
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
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;

namespace System.ComponentModel
{
	internal class ReflectionEventDescriptor: EventDescriptor
	{
		Type _eventType;
		Type _componentType;
		EventInfo _eventInfo;

		MethodInfo add_method;
		MethodInfo remove_method;

		public ReflectionEventDescriptor (EventInfo eventInfo) : base (eventInfo.Name, (Attribute[]) eventInfo.GetCustomAttributes (true))
		{
			_eventInfo = eventInfo;
			_componentType = eventInfo.DeclaringType;
			_eventType = eventInfo.EventHandlerType;

			add_method = eventInfo.GetAddMethod ();
			remove_method = eventInfo.GetRemoveMethod ();
		}

		public ReflectionEventDescriptor (Type componentType, EventDescriptor oldEventDescriptor, Attribute[] attrs) : base (oldEventDescriptor, attrs)
		{
			_componentType = componentType;
			_eventType = oldEventDescriptor.EventType;

			EventInfo event_info = componentType.GetEvent (oldEventDescriptor.Name);
			add_method = event_info.GetAddMethod ();
			remove_method = event_info.GetRemoveMethod ();
		}

		public ReflectionEventDescriptor (Type componentType, string name, Type type, Attribute[] attrs) : base (name, attrs)
		{
			_componentType = componentType;
			_eventType = type;

			EventInfo event_info = componentType.GetEvent (name);
			add_method = event_info.GetAddMethod ();
			remove_method = event_info.GetRemoveMethod ();
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
			add_method.Invoke (component, new object [] { value });
		}

		public override void RemoveEventHandler (object component, System.Delegate value)
		{
			remove_method.Invoke (component, new object [] { value });
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
