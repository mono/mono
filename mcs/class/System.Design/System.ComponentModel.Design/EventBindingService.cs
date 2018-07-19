//
// System.ComponentModel.Design.EventBindingService
// 
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;

namespace System.ComponentModel.Design
{
	public abstract class EventBindingService : IEventBindingService
	{

		private IServiceProvider _provider;

		protected EventBindingService (IServiceProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			_provider = provider;
		}
					
		protected abstract bool ShowCode (IComponent component, EventDescriptor e, string methodName);
		protected abstract bool ShowCode (int lineNumber);
		protected abstract bool ShowCode ();
		protected abstract string CreateUniqueMethodName (IComponent component, EventDescriptor e);
		protected abstract ICollection GetCompatibleMethods (EventDescriptor e);

		protected virtual void FreeMethod (IComponent component, EventDescriptor e, string methodName)
		{
		}

		protected virtual void UseMethod (IComponent component, EventDescriptor e, string methodName)
		{
		}


		protected virtual void ValidateMethodName (string methodName)
		{
		}
 

		protected object GetService (Type serviceType)
		{
			if (_provider != null)
				return _provider.GetService (serviceType);
			return null;
		}

#region IEventBindingService implementation

		string IEventBindingService.CreateUniqueMethodName (IComponent component, EventDescriptor eventDescriptor)
		{
			if (eventDescriptor == null)
				throw new ArgumentNullException ("eventDescriptor");
			if (component == null)
				throw new ArgumentNullException ("component");

			return this.CreateUniqueMethodName (component, eventDescriptor);
		}

		ICollection IEventBindingService.GetCompatibleMethods (EventDescriptor eventDescriptor)
		{
			if (eventDescriptor == null)
				throw new ArgumentNullException ("eventDescriptor");

			return this.GetCompatibleMethods (eventDescriptor);
		}

		EventDescriptor IEventBindingService.GetEvent (PropertyDescriptor property)
		{
			if (property == null)
				throw new ArgumentNullException ("property");

			EventPropertyDescriptor eventPropDescriptor = property as EventPropertyDescriptor;
			if (eventPropDescriptor == null)
				return null;
			
			return eventPropDescriptor.InternalEventDescriptor;
		}

		PropertyDescriptorCollection IEventBindingService.GetEventProperties (EventDescriptorCollection events)
		{
			if (events == null)
				throw new ArgumentNullException ("events");

			List<PropertyDescriptor> properties = new List <PropertyDescriptor>();
			foreach (EventDescriptor eventDescriptor in events)
				properties.Add (((IEventBindingService)this).GetEventProperty (eventDescriptor));
				
			return new PropertyDescriptorCollection (properties.ToArray ());
		}

		PropertyDescriptor IEventBindingService.GetEventProperty (EventDescriptor eventDescriptor)
		{
			if (eventDescriptor == null) 
				throw new ArgumentNullException ("eventDescriptor");

			return new EventPropertyDescriptor (eventDescriptor);
		}

		bool IEventBindingService.ShowCode (IComponent component, EventDescriptor eventDescriptor)
		{
			if (component == null)
				throw new ArgumentNullException ("component");
			if (eventDescriptor == null)
				throw new ArgumentNullException ("eventDescriptor");

			return this.ShowCode (component, eventDescriptor, (string) ((IEventBindingService)this).GetEventProperty (eventDescriptor).GetValue (component));
		}

		bool IEventBindingService.ShowCode (int lineNumber)
		{
			return this.ShowCode (lineNumber);
		}

		bool IEventBindingService.ShowCode ()
		{
			return this.ShowCode ();
		}
#endregion

	}

	internal class EventPropertyDescriptor : PropertyDescriptor
	{
		private EventDescriptor _eventDescriptor;
	
		public EventPropertyDescriptor (EventDescriptor eventDescriptor)
			: base (eventDescriptor)
		{
			if (eventDescriptor == null)
				throw new ArgumentNullException ("eventDescriptor");
			_eventDescriptor = eventDescriptor;
		}
		
		public override bool CanResetValue (object component)
		{
			return true;
		}

		public override Type ComponentType {
			get { return _eventDescriptor.ComponentType; }
		}

		public override bool IsReadOnly {
			get { return false; }
		}

		public override Type PropertyType {
			get { return _eventDescriptor.EventType; }
		}

		public override void ResetValue (object component)
		{
			this.SetValue (component, null);
		}

		public override object GetValue (object component)
		{
			if (component is IComponent && ((IComponent)component).Site != null) {
				IDictionaryService dictionary = ((IComponent)component).Site.GetService (typeof (IDictionaryService)) as IDictionaryService;
				if (dictionary != null)
					return dictionary.GetValue (base.Name);
			}
			return null;
		}

		public override void SetValue (object component, object value)
		{
			if (component is IComponent && ((IComponent)component).Site != null) {
				IDictionaryService dictionary = ((IComponent)component).Site.GetService (typeof (IDictionaryService)) as IDictionaryService;
				if (dictionary != null)
					dictionary.SetValue (base.Name, value);
			}
		}

		public override bool ShouldSerializeValue (object component)
		{
			if (this.GetValue (component) != null)
				return true;
			return false;
		}
		
		public override TypeConverter Converter {
			get { return TypeDescriptor.GetConverter (String.Empty); }
		}
		
		internal EventDescriptor InternalEventDescriptor {
			get { return _eventDescriptor; }
		}
	}
}
