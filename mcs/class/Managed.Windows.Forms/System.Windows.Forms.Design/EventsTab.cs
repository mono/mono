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
// Copyright (c) 2004-2008 Novell, Inc.
//
// Authors:
//	Dennis Hayes (dennish@raytek.com)
//	Rafael Teixeira (rafaelteixeirabr@hotmail.com)
// 	Ivan N. Zlatev (contact@i-nz.net)
//

// COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms.Design
{
	public class EventsTab : PropertyTab
	{
		private EventsTab()
		{
		}
		
		private IServiceProvider serviceProvider;

		public EventsTab(IServiceProvider sp)
		{
			this.serviceProvider = sp;
		}

		public override string HelpKeyword {
			get { return TabName; }
		}
		
		public override string TabName {
			get { return Locale.GetText ("Events"); }
		}
		
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, 
									   Attribute[] attributes)
		{
			IEventBindingService eventPropertySvc = null;
			EventDescriptorCollection events = null;
			
			if (serviceProvider != null)
				eventPropertySvc = (IEventBindingService) serviceProvider.GetService(typeof(IEventBindingService));

			if (eventPropertySvc == null)			 
				return new PropertyDescriptorCollection(null);

			if (attributes != null)			
				events = TypeDescriptor.GetEvents(component, attributes);
			else
				events = TypeDescriptor.GetEvents(component);
	 
			// Return event properties for the event descriptors.
			return eventPropertySvc.GetEventProperties(events);
		}
		
		public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
		{
			return this.GetProperties(null, component, attributes);
		}
		
		public override bool CanExtend(object extendee)
		{
			return false;
		}
		
		public override PropertyDescriptor GetDefaultProperty(object obj)
		{
			if (serviceProvider == null)
				return null;

			EventDescriptor defaultEvent = TypeDescriptor.GetDefaultEvent (obj);
			IEventBindingService eventPropertySvc = (IEventBindingService) serviceProvider.GetService(typeof(IEventBindingService));
			if (defaultEvent != null && eventPropertySvc != null)
				return eventPropertySvc.GetEventProperty (defaultEvent);
			return null;
		}

	}
}
