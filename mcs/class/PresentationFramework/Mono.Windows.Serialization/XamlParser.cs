//
// XamlParser.cs
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
using System.Collections;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows;

namespace Mono.Windows.Serialization {
	public class XamlParser {
		public const string XAML_NAMESPACE = "http://schemas.microsoft.com/winfx/xaml/2005";
		private Mapper mapper = new Mapper();
		private XmlReader reader;
		private XamlWriter writer;

		private enum CurrentType { Object, 
			Property, 
			AttachedProperty,
	       		Code }

		private class ParserState {
			public object obj;
			public CurrentType type;
		}
	
		private ParserState currentState = null;
		private ArrayList oldStates = new ArrayList();
	
		public XamlParser(string filename, XamlWriter writer)
		{
			reader = new XmlTextReader(filename);
			this.writer = writer;
		}
		
		public void Parse()
		{
			while (reader.Read()) {
				if (currentState != null &&
						currentState.type == CurrentType.Code)
				{
					if (reader.NodeType == XmlNodeType.EndElement &&
							reader.LocalName == "Code" && 
							reader.NamespaceURI == XAML_NAMESPACE) {
						parseEndElement();
					} else {
						currentState.obj = (string)currentState.obj + reader.Value;
					}
					continue;
				}
				switch (reader.NodeType) {
				case XmlNodeType.ProcessingInstruction:
					parsePI();
					break;
				case XmlNodeType.Element:
					parseElement();
					break;
				case XmlNodeType.EndElement:
					parseEndElement();
					break;
				case XmlNodeType.Text:
					parseText();
					break;
				case XmlNodeType.Whitespace:
					// skip whitespace
					break;
				default:
					Console.Out.WriteLine("Unknown element type " + reader.NodeType);
					break;
				}
			}
		}
		void parsePI()
		{
			if (reader.Name != "Mapping")
				Console.WriteLine("Unknown processing instruction");
			Mapping mapping = new Mapping(reader.Value);
			mapper.AddMapping(mapping);
		}

		void parseElement()
		{
			if (reader.LocalName == "Code" && reader.NamespaceURI == XAML_NAMESPACE) {
				parseCodeElement();
				return;
			}
			// This element must be an object if:
			//  - It's a direct child of a property element
			//  - It's a direct child of an IAddChild element
			//    and does not have a dot in its name
			//    
			//  parseObjectElement will verify the second case
			//
			//  If it's a dotted name, then it is a property.
			//  What it is a property of depends on the bit of the
			//  name before the dot.
			int dotPosition = reader.LocalName.IndexOf('.');
			if (dotPosition < 0 ||
					currentState.type == CurrentType.Property) {
				parseObjectElement();
				return;
			}
			string beforeDot = reader.LocalName.Substring(0, dotPosition);
			string afterDot = reader.LocalName.Substring(dotPosition + 1);
			// If we've got this far, then currentState.Type == Object
			if (isNameOfAncestorClass(beforeDot, (Type)currentState.obj))
				parseNormalPropertyElement(afterDot);
			else
				parseAttachedPropertyElement(beforeDot, afterDot);
		}

		// check if the given name is the name of an ancestor of 
		// the given type
		bool isNameOfAncestorClass(string name, Type t)
		{
			if (name == "object")
				return true;
			while (t.BaseType != null) {
				if (t.Name == name)
					return true;
				t = t.BaseType;
			}
			return false;
		}

		void parseCodeElement()
		{
			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.type = CurrentType.Code;
			currentState.obj = "";
		}

		void parseText()
		{
			if (currentState.type == CurrentType.Object) {
				writer.CreateElementText(reader.Value);
			} else if (currentState.type == CurrentType.AttachedProperty) {
				DependencyProperty dp = (DependencyProperty)currentState.obj;
				writer.CreateAttachedPropertyText(reader.Value, dp.PropertyType, 
						getTypeConverter(dp.PropertyType));
			} else {
				PropertyInfo prop = (PropertyInfo)currentState.obj;
				writer.CreatePropertyText(reader.Value, prop.PropertyType,
						getTypeConverter(prop.PropertyType));
			}
		}
		
		Type getTypeConverter(Type fromType)
		{
			// TODO: this business setting assembly is frankly
			// grotesque. It should just be something along the
			// lines of Assembly.Load("System.dll")
			Assembly assembly = null;
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				if (a.CodeBase.EndsWith("System.dll")) {
					assembly = a;
					break;
				}
			}

			
			if (fromType.Namespace == "System" && 
					fromType.Name == "String")
				return null;
			string converterName = "System.ComponentModel." + fromType.Name + "Converter,System.dll";
			Type converter = assembly.GetType(converterName);
			return converter;
			// TODO: check if converter == null and do something cool
		}

		
		void parseNormalPropertyElement(string propertyName)
		{
			// preconditions: currentState.Type == Object
			Type currentType = (Type)currentState.obj;
			PropertyInfo prop = currentType.GetProperty(propertyName);

			if (prop == null) {
				Console.WriteLine("Property " + propertyName + " not found on " + currentType.Name);
				return;
				// TODO: exception
			}

			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.type = CurrentType.Property;
			currentState.obj = prop;

			writer.CreateProperty(prop);

			if (reader.HasAttributes) {
				Console.WriteLine("Property node should not have attributes");
				return;
				// TODO: exception
			}
		}

		void parseAttachedPropertyElement(string attachedTo, string propertyName)
		{
			Type typeAttachedTo = null;
			FieldInfo propField;
			DependencyProperty dp;
			Type currentType = (Type)currentState.obj;
			if (!currentType.IsSubclassOf(typeof(System.Windows.DependencyObject)))
					throw new Exception("Attached properties can only be set on "+
							"DependencyObjects (not " + currentType.Name + ")");
			foreach (ParserState state in oldStates) {
				if (state.type == CurrentType.Object &&
						((Type)state.obj).Name == attachedTo) {
					typeAttachedTo = (Type)state.obj;
					break;
				}
			}
			if (typeAttachedTo == null)
				throw new Exception("Nothing to attach to: " + attachedTo + "." + propertyName);
			propField = typeAttachedTo.GetField(propertyName + "Property");
			if (propField == null)
				throw new Exception("Property " + propertyName + " does not exist on " + attachedTo);
			dp = (DependencyProperty)propField.GetValue(null);
			
			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.obj = dp;
			currentState.type = CurrentType.AttachedProperty;

			writer.CreateAttachedProperty(typeAttachedTo, propertyName, dp.PropertyType);
		}

		void parseObjectElement()
		{
			Type parent;
			string objectName = null;
			bool isEmpty = reader.IsEmptyElement;
			
			parent = mapper.Resolve(reader.NamespaceURI, reader.Name);
			objectName = reader.GetAttribute("Class", XAML_NAMESPACE);
			if (parent.GetInterface("System.Windows.Serialization.IAddChild") == null)
				{} //TODO: throw exception
			if (currentState == null) {
				createTopLevel(parent.AssemblyQualifiedName, objectName);
			} else {
				addChild(parent);
			}
			
			if (reader.MoveToFirstAttribute()) {
				do {
					if (reader.Name.StartsWith("xmlns"))
						continue;
					if (reader.NamespaceURI == XAML_NAMESPACE)
						continue;
					if (reader.LocalName.IndexOf(".") < 0)
						parseLocalPropertyAttribute();
					else
						parseContextPropertyAttribute();
				} while (reader.MoveToNextAttribute());
			}
			

			if (isEmpty) {
				writer.EndObject();
				pop();
			}
		}

		void createTopLevel(string parentName, string objectName)
		{
			Type t = Type.GetType(parentName);
			currentState = new ParserState();
			currentState.type = CurrentType.Object;
			currentState.obj = t;
			if (objectName == null) {
				objectName = "derived" + t.Name;
			}
			writer.CreateTopLevel(t, objectName);
		}

		void addChild(Type type)
		{
			writer.CreateObject(type);
			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.type = CurrentType.Object;
			currentState.obj = type;
		}

		
		void parseLocalPropertyAttribute()
		{
			string propertyName = reader.LocalName;
			Type currentType = (Type)currentState.obj;
			PropertyInfo prop = currentType.GetProperty(propertyName);
			if (parsedAsEventProperty(currentType, propertyName))
				return;
			if (prop == null) {
				Console.WriteLine("Property " + propertyName + " not found on " + currentType.Name);
				return;
				// TODO: throw exception
			}

			writer.CreateProperty(prop);

			if (prop.PropertyType.IsSubclassOf(typeof(Delegate)))
				writer.CreatePropertyDelegate(reader.Value, prop.PropertyType);
			else
				writer.CreatePropertyText(reader.Value, prop.PropertyType, getTypeConverter(prop.PropertyType));
			
			writer.EndProperty();
		}
		
		bool parsedAsEventProperty(Type currentType, string eventName)
		{
			EventInfo evt = currentType.GetEvent(eventName);
			if (evt == null)
				return false;
			writer.CreateEvent(evt);
			writer.CreateEventDelegate(reader.Value, evt.EventHandlerType);
			writer.EndEvent();
			return true;
		}



		void parseContextPropertyAttribute()
		{
			throw new NotImplementedException("parseContextPropertyAttribute");
		}

		void parseEndElement()
		{
			if (currentState.type == CurrentType.Code)
				writer.CreateCode((string)currentState.obj);
			if (currentState.type == CurrentType.Object)
				writer.EndObject();
			else if (currentState.type == CurrentType.Property)
				writer.EndProperty();
			else if (currentState.type == CurrentType.AttachedProperty)
				writer.EndAttachedProperty();
				
			pop();
		}

		void pop()
		{
			if (oldStates.Count == 0) {
				currentState = null;
				writer.Finish();
				return;
			}
			int lastIndex = oldStates.Count - 1;
			currentState = (ParserState)oldStates[lastIndex];
			oldStates.RemoveAt(lastIndex);

		}

	}
}
