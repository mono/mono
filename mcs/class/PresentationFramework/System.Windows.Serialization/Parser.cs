//
// Parser.cs - instantiate an object according to a Xaml file
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
using System.Diagnostics;
using System.Windows;
using Mono.Windows.Serialization;
using System.Xml;

namespace System.Windows.Serialization {
	public class Parser {

		public static object LoadXml(Stream s)
		{
			return LoadXml(new XmlTextReader(s));
		}
		// TODO: this should take a XmlReader in order to be same as MSFT
		public static object LoadXml(XmlTextReader reader)
		{
			Consumer r = new Consumer();
			r.crunch(reader);
			return r.instance;
		}
		private class Consumer : ParserConsumerBase {
		
			public object instance;
			ArrayList objects = new ArrayList();

			Hashtable keys = new Hashtable();

			public override void CreateTopLevel(Type parent, string className)
			{
				instance = Activator.CreateInstance(parent);
				push(instance);
			}

			public override void CreateObject(Type type, string varName, string key)
			{
				Object o = Activator.CreateInstance(type);
				((IAddChild)peek()).AddChild(o);

				if (key != null)
					keys[key] = o;
				push(o);
			}

			public override void CreateProperty(PropertyInfo property)
			{
				push(property);
			}

			// top of stack is a reference to an object
			// pushes a reference to the event
			public override void CreateEvent(EventInfo evt)
			{
				push(evt);
			}

			public override void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType)
			{
				push(attachedTo);
				push(propertyName);
			}

			public override void EndDependencyProperty()
			{
				object value = pop();
				string propertyName = (string)pop();
				Type attachedTo = (Type)pop();


				MethodInfo setter = attachedTo.GetMethod("Set" + propertyName);
				setter.Invoke(null, new object[] { peek(), value});
			}

			public override void CreateObjectText(string text)
			{
				((IAddChild)peek()).AddText(text);
			}

			// top of stack is reference to an event
			public override void CreateEventDelegate(string functionName, Type eventDelegateType)
			{
				EventInfo e = (EventInfo)peek();
				object o = peek(1);
				e.AddEventHandler(o, Delegate.CreateDelegate(o.GetType(), o, functionName));
			}
			// top of stack is reference to a property
			public override void CreatePropertyDelegate(string functionName, Type propertyType)
			{
				PropertyInfo p = (PropertyInfo)peek();
				object o = peek(1);
				p.SetValue(o, Delegate.CreateDelegate(o.GetType(), o, functionName), null);
			}

			public override void CreatePropertyText(string text, Type propertyType)
			{
				object value = convertText(propertyType, text);
				storeToProperty(value);
			}
			
			public override void CreatePropertyObject(Type type, string name, string key)
			{
				object value = Activator.CreateInstance(type);
				Debug.WriteLine("ObjectWriter CREATING PROPERTY OBJECT of type" + type);
				if (key != null)
					keys[key] = value;
				push(value);
			}

			public override void CreatePropertyReference(string key)
			{
				push(keys[key]);
			}
			public override void CreateDependencyPropertyReference(string key)
			{
				push(keys[key]);
			}
			public override void EndPropertyObject(Type destType)
			{
				object value = convertPropertyObjectValue(destType, pop());
				storeToProperty(value);
			}
			private void storeToProperty(object value)
			{
				PropertyInfo p = (PropertyInfo)peek();
				object o = peek(1);
				p.SetValue(o, value, null);
			}
			private object convertPropertyObjectValue(Type destType, object value)
			{
				Type sourceType = value.GetType();
				if (destType != sourceType && !sourceType.IsSubclassOf(destType)) {
					TypeConverter tc = TypeDescriptor.GetConverter(sourceType);
					value = tc.ConvertTo(value, destType);
				}
				return value;
			}
			private object convertText(Type propertyType, string text)
			{
				if (propertyType != typeof(string)) {
					TypeConverter tc = TypeDescriptor.GetConverter(propertyType);
					return tc.ConvertFromString(text);
				} else {
					return text;
				}
			}

			public override void CreateDependencyPropertyObject(Type type, string name, string key)
			{
				CreatePropertyObject(type, name, key);
			}
			public override void EndDependencyPropertyObject(Type finalType)
			{
				push(convertPropertyObjectValue(finalType, pop()));
			}

			// top of stack is reference to an attached property
			public override void CreateDependencyPropertyText(string text, Type propertyType)
			{
				object value = convertText(propertyType, text);
				push(value);
			}
			
			public override void EndObject()
			{
				pop();
			}

			public override void EndProperty()
			{
				pop();
			}
			
			public override void EndEvent()
			{
				pop();
			}

			public override void Finish()
			{
			}

			public override void CreateCode(string code)
			{
				throw new NotImplementedException();
			}

			private object peek()
			{
				return peek(0);
			}
			private object peek(int i)
			{
				return objects[objects.Count - 1 - i];
			}
			private object pop()
			{
				object v = objects[objects.Count - 1];
				objects.RemoveAt(objects.Count - 1);
				Debug.WriteLine("ObjectWriter: POPPING");
				return v;
			}
			private void push(object v)
			{
				Debug.WriteLine("ObjectWriter: PUSHING " + v);
				objects.Add(v);
			}
		}
	}
}
