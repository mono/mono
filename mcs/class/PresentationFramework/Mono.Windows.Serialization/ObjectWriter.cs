//
// CodeWriter.cs
//
// Author:
//   Iain McCoy (iain@mccoy.id.au)
//
// (C) 2005 Iain McCoy
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
using System.Reflection;
using System.IO;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Windows.Serialization;

namespace Mono.Windows.Serialization {
	public class ObjectWriter : XamlWriter {
		public object instance;
		ArrayList objects = new ArrayList();
	
		public void CreateTopLevel(Type parent, string className)
		{
			instance = Activator.CreateInstance(parent);
			objects.Add(instance);
		}

		public void CreateObject(Type type, string varName)
		{
			Object o = Activator.CreateInstance(type);
			((IAddChild)objects[objects.Count - 1]).AddChild(o);
			objects.Add(o);
		}

		public void CreateProperty(PropertyInfo property)
		{
			objects.Add(property);
		}

		// top of stack is a reference to an object
		// pushes a reference to the event
		public void CreateEvent(EventInfo evt)
		{
			objects.Add(evt);
		}

		public void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType)
		{
			objects.Add(attachedTo);
			objects.Add(propertyName);
		}

		public void EndDependencyProperty()
		{
			object value = pop();
			string propertyName = (string)pop();
			Type attachedTo = (Type)pop();

			MethodInfo setter = attachedTo.GetMethod("Set" + propertyName);
			setter.Invoke(null, new object[] { objects[objects.Count - 1], value});
		}

		public void CreateElementText(string text)
		{
			((IAddChild)objects[objects.Count - 1]).AddText(text);
		}

		// top of stack is reference to an event
		public void CreateEventDelegate(string functionName, Type eventDelegateType)
		{
			EventInfo e = (EventInfo)objects[objects.Count-1];
			object o = objects[objects.Count-2];
			e.AddEventHandler(o, Delegate.CreateDelegate(o.GetType(), o, functionName));
		}
		// top of stack is reference to a property
		public void CreatePropertyDelegate(string functionName, Type propertyType)
		{
			PropertyInfo p = (PropertyInfo)objects[objects.Count-1];
			object o = objects[objects.Count-2];
			p.SetValue(o, Delegate.CreateDelegate(o.GetType(), o, functionName), null);
		}

		public void CreatePropertyText(string text, Type propertyType)
		{
			object value = text;
			if (propertyType != typeof(string)) {
				TypeConverter tc = TypeDescriptor.GetConverter(propertyType);
				value = tc.ConvertFromString(text);
			}
			PropertyInfo p = (PropertyInfo)objects[objects.Count-1];
			object o = objects[objects.Count-2];
			p.SetValue(o, value, null);
		}
		
		public void CreatePropertyObject(Type type, string name)
		{
			throw new NotImplementedException();
		}
		public void EndPropertyObject(Type sourceType)
		{
			throw new NotImplementedException();
		}

		// top of stack is reference to an attached property
		public void CreateDependencyPropertyText(string text, Type propertyType)
		{
			object value = text;
			if (propertyType != typeof(string)) {
				TypeConverter tc = TypeDescriptor.GetConverter(propertyType);
				value = tc.ConvertFromString(text);
			}
			objects.Add(value);
		}
		
		public void EndObject()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void EndProperty()
		{
			objects.RemoveAt(objects.Count - 1);
		}
		
		public void EndEvent()
		{
			objects.RemoveAt(objects.Count - 1);
		}

		public void Finish()
		{
		}

		public void CreateCode(string code)
		{
			throw new NotImplementedException();
		}

		private object pop()
		{
			object v = objects[objects.Count - 1];
			objects.RemoveAt(objects.Count - 1);
			return v;
		}
	}
}
