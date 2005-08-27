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
using System.Diagnostics;
using System.Windows;
using Mono.Windows.Serialization;
using System.Xml;

namespace System.Windows.Serialization {
	public class Parser {
		private object instance;
		ArrayList objects = new ArrayList();

		public static object LoadXml(Stream s)
		{
			return LoadXml(new XmlTextReader(s));
		}
		// TODO: this should take a XmlReader in order to be same as MSFT
		public static object LoadXml(XmlTextReader reader)
		{
			Parser r = new Parser(reader);
			return r.instance;
		}
		private Parser(XmlTextReader reader)
		{
			XamlParser p = new XamlParser(reader);
			XamlNode n;
			while (true) {
				n = p.GetNextNode();
				if (n == null)
					break;
				Debug.WriteLine("ParserToCode: INCOMING " + n.GetType());
				if (n is XamlDocumentStartNode) {
					Debug.WriteLine("ParserToCode: document begins");
					// do nothing
				} else if (n is XamlElementStartNode && n.Depth == 0) {
					Debug.WriteLine("ParserToCode: element begins as top-level");
					CreateTopLevel(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name);
				} else if (n is XamlElementStartNode && ((XamlElementStartNode)n).propertyObject) {
					Debug.WriteLine("ParserToCode: element begins as property value");
					CreatePropertyObject(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name);
				} else if (n is XamlElementStartNode && ((XamlElementStartNode)n).depPropertyObject) {
					Debug.WriteLine("ParserToCode: element begins as dependency property value");
					CreateDependencyPropertyObject(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name);

				} else if (n is XamlElementStartNode) {
					Debug.WriteLine("ParserToCode: element begins");
					CreateObject(((XamlElementStartNode)n).ElementType, ((XamlElementStartNode)n).name);
				} else if (n is XamlPropertyNode && ((XamlPropertyNode)n).PropInfo != null) {
					Debug.WriteLine("ParserToCode: normal property begins");
					CreateProperty(((XamlPropertyNode)n).PropInfo);
				} else if (n is XamlPropertyNode && ((XamlPropertyNode)n).DP != null) {
					Debug.WriteLine("ParserToCode: dependency property begins");
					DependencyProperty dp = ((XamlPropertyNode)n).DP;
					Type typeAttachedTo = dp.OwnerType;
					string propertyName = ((XamlPropertyNode)n).PropertyName;
					
					CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
				} else if (n is XamlClrEventNode && !(((XamlClrEventNode)n).EventMember is EventInfo)) {
					Debug.WriteLine("ParserToCode: delegate property");
					CreatePropertyDelegate(((XamlClrEventNode)n).Value, ((PropertyInfo)((XamlClrEventNode)n).EventMember).PropertyType);
					EndProperty();


				} else if (n is XamlClrEventNode) {
					Debug.WriteLine("ParserToCode: event");
					CreateEvent((EventInfo)((XamlClrEventNode)n).EventMember);
					CreateEventDelegate(((XamlClrEventNode)n).Value, ((EventInfo)((XamlClrEventNode)n).EventMember).EventHandlerType);
					EndEvent();

				} else if (n is XamlTextNode && ((XamlTextNode)n).mode == XamlParseMode.Object){
					Debug.WriteLine("ParserToCode: text for object");
					CreateObjectText(((XamlTextNode)n).TextContent);
				} else if (n is XamlTextNode && ((XamlTextNode)n).mode == XamlParseMode.Property){
					Debug.WriteLine("ParserToCode: text for property");
					CreatePropertyText(((XamlTextNode)n).TextContent, ((XamlTextNode)n).finalType);
					EndProperty();
				} else if (n is XamlTextNode && ((XamlTextNode)n).mode == XamlParseMode.DependencyProperty){
					Debug.WriteLine("ParserToCode: text for dependency property");
					CreateDependencyPropertyText(((XamlTextNode)n).TextContent, ((XamlTextNode)n).finalType);
					EndDependencyProperty();
				} else if (n is XamlPropertyComplexEndNode) {
					Debug.WriteLine("ParserToCode: end complex property");
					EndProperty();
				} else if (n is XamlLiteralContentNode) {
					Debug.WriteLine("ParserToCode: literal content");
					CreateCode(((XamlLiteralContentNode)n).Content);
				} else if (n is XamlElementEndNode) {
					Debug.WriteLine("ParserToCode: end element");
					Type ft = ((XamlElementEndNode)n).finalType;
					if (((XamlElementEndNode)n).propertyObject)
						EndPropertyObject(ft);
					else if (((XamlElementEndNode)n).depPropertyObject)
						EndDependencyPropertyObject(ft);
					else
						EndObject();
				} else if (n is XamlDocumentEndNode) {
					Debug.WriteLine("ParserToCode: end document");
					Finish();
				} else {
					throw new Exception("Unknown node " + n.GetType());
				}

			}
		}

	
		public void CreateTopLevel(Type parent, string className)
		{
			instance = Activator.CreateInstance(parent);
			push(instance);
		}

		public void CreateObject(Type type, string varName)
		{
			Object o = Activator.CreateInstance(type);
			((IAddChild)peek()).AddChild(o);
			push(o);
		}

		public void CreateProperty(PropertyInfo property)
		{
			push(property);
		}

		// top of stack is a reference to an object
		// pushes a reference to the event
		public void CreateEvent(EventInfo evt)
		{
			push(evt);
		}

		public void CreateDependencyProperty(Type attachedTo, string propertyName, Type propertyType)
		{
			push(attachedTo);
			push(propertyName);
		}

		public void EndDependencyProperty()
		{
			object value = pop();
			string propertyName = (string)pop();
			Type attachedTo = (Type)pop();

			MethodInfo setter = attachedTo.GetMethod("Set" + propertyName);
			setter.Invoke(null, new object[] { peek(), value});
		}

		public void CreateObjectText(string text)
		{
			((IAddChild)peek()).AddText(text);
		}

		// top of stack is reference to an event
		public void CreateEventDelegate(string functionName, Type eventDelegateType)
		{
			EventInfo e = (EventInfo)peek();
			object o = peek(1);
			e.AddEventHandler(o, Delegate.CreateDelegate(o.GetType(), o, functionName));
		}
		// top of stack is reference to a property
		public void CreatePropertyDelegate(string functionName, Type propertyType)
		{
			PropertyInfo p = (PropertyInfo)peek();
			object o = peek(1);
			p.SetValue(o, Delegate.CreateDelegate(o.GetType(), o, functionName), null);
		}

		public void CreatePropertyText(string text, Type propertyType)
		{
			object value = text;
			if (propertyType != typeof(string)) {
				TypeConverter tc = TypeDescriptor.GetConverter(propertyType);
				value = tc.ConvertFromString(text);
			}
			PropertyInfo p = (PropertyInfo)peek();
			object o = peek(1);
			p.SetValue(o, value, null);
		}
		
		public void CreatePropertyObject(Type type, string name)
		{
			object value = Activator.CreateInstance(type);
			Debug.WriteLine("ObjectWriter CREATING PROPERTY OBJECT of type" + type);
			push(value);
		}
		public void EndPropertyObject(Type destType)
		{
			object value = pop();
			Type sourceType = value.GetType();
			Debug.WriteLine("ObjectWriter: EndPropertyObject has a " + value + value.GetType() + ", needs a " + destType);
			if (destType != sourceType && !sourceType.IsSubclassOf(destType)) {
				TypeConverter tc = TypeDescriptor.GetConverter(destType);
				value = tc.ConvertFrom(value);
			}
			PropertyInfo p = (PropertyInfo)peek();
			object o = peek(1);
			p.SetValue(o, value, null);
		}

		public void CreateDependencyPropertyObject(Type type, string name)
		{
			throw new NotImplementedException();
		}
		public void EndDependencyPropertyObject(Type finalType)
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
			push(value);
		}
		
		public void EndObject()
		{
			pop();
		}

		public void EndProperty()
		{
			pop();
		}
		
		public void EndEvent()
		{
			pop();
		}

		public void Finish()
		{
		}

		public void CreateCode(string code)
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
			Debug.WriteLine("ObjectWriter POPPING");
			return v;
		}
		private void push(object v)
		{
			Debug.WriteLine("ObjectWriter PUSHING " + v);
			objects.Add(v);
		}
	}
}
