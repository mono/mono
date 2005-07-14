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
using System.Windows.Serialization;

namespace Mono.Windows.Serialization {
	public class XamlParser {
		public const string XAML_NAMESPACE = "http://schemas.microsoft.com/winfx/xaml/2005";
		private Mapper mapper = new Mapper(new string[] { });
		private XmlReader reader;
		private XamlWriter writer;

		private enum CurrentType { Object, 
			Property, 
			DependencyProperty,
	       		Code }

		private class ParserState {
			public object obj;
			public CurrentType type;
		}
	
		private bool begun = false;

		private ParserState currentState = null;
		private ArrayList oldStates = new ArrayList();
	
		public XamlParser(string filename, XamlWriter writer) : this(
				new XmlTextReader(filename), writer)
		{
		}
		
		public XamlParser(TextReader reader, XamlWriter writer) : this(
				new XmlTextReader(reader), writer)
		{
		}
		
		public XamlParser(XmlReader reader, XamlWriter writer)
		{
			this.reader = reader;
			this.writer = writer;
		}
		
		public void Parse()
		{
			while (reader.Read()) {
				if (begun && currentState == null && reader.NodeType != XmlNodeType.Whitespace)
					throw new Exception("Too far: " + reader.NodeType + ", " + reader.Name);
				if (currentState != null && currentState.type == CurrentType.Code)
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
				case XmlNodeType.Comment:
					// skip whitespace and comments
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
			mapper.AddMappingProcessingInstruction(reader.Value);
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
				parseDependencyPropertyElement(beforeDot, afterDot);
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
			} else if (currentState.type == CurrentType.DependencyProperty) {
				DependencyProperty dp = (DependencyProperty)currentState.obj;
				writer.CreateDependencyPropertyText(reader.Value, dp.PropertyType);
			} else {
				PropertyInfo prop = (PropertyInfo)currentState.obj;
				writer.CreatePropertyText(reader.Value, prop.PropertyType);
			}
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


		void parseDependencyPropertyElement(string attachedTo, string propertyName)
		{
			Type currentType = (Type)currentState.obj;
			ensureDependencyObject(currentType);
			Type typeAttachedTo = findTypeToAttachTo(attachedTo, propertyName);
			DependencyProperty dp = getDependencyProperty(typeAttachedTo, propertyName);
			
			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.obj = dp;
			currentState.type = CurrentType.DependencyProperty;

			writer.CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
		}

		void parseObjectElement()
		{
			Type parent;
			bool isEmpty = reader.IsEmptyElement;
			
			parent = mapper.GetType(reader.NamespaceURI, reader.Name);
			if (parent.GetInterface("System.Windows.Serialization.IAddChild") == null)
				{} //TODO: throw exception
			if (currentState == null) {
				if (reader.GetAttribute("Name", XAML_NAMESPACE) != null)
					throw new Exception("The XAML Name attribute can not be applied to top level elements\n"+
							"Do you mean the Class attribute?");
				begun = true;
				createTopLevel(parent.AssemblyQualifiedName, reader.GetAttribute("Class", XAML_NAMESPACE));
			} else {
				string name = reader.GetAttribute("Name", XAML_NAMESPACE);
				if (name == null)
					name = reader.GetAttribute("Name", reader.NamespaceURI);

				if (currentState.type == CurrentType.Object)
					addChild(parent, name);
				else if (currentState.type == CurrentType.Property)
					addPropertyChild(parent, name);
				else
					throw new NotImplementedException();
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
						parseDependencyPropertyAttribute();
				} while (reader.MoveToNextAttribute());
			}
			

			if (isEmpty) {
				writer.EndObject();
				pop();
			}
		}

		void createTopLevel(string parentName, string className)
		{
			Type t = Type.GetType(parentName);

			writer.CreateTopLevel(t, className);
			currentState = new ParserState();
			currentState.type = CurrentType.Object;
			currentState.obj = t;
		}

		void addChild(Type type, string objectName)
		{
			writer.CreateObject(type, objectName);
			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.type = CurrentType.Object;
			currentState.obj = type;
		}
		
		void addPropertyChild(Type type, string objectName)
		{
//			writer.CreatePropertyObject(type, objectName);
			writer.CreatePropertyObject(((PropertyInfo)currentState.obj).PropertyType, objectName);

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
				writer.CreatePropertyText(reader.Value, prop.PropertyType);
			
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

		void ensureDependencyObject(Type currentType)
		{
			if (!currentType.IsSubclassOf(typeof(System.Windows.DependencyObject)))
					throw new Exception("Dependency properties can only be set on "+
							"DependencyObjects (not " + currentType.Name + ")");
		}
		Type findTypeToAttachTo(string attachedTo, string propertyName)
		{
			Type typeAttachedTo = null;
			foreach (ParserState state in oldStates) {
				if (state.type == CurrentType.Object &&
						((Type)state.obj).Name == attachedTo) {
					typeAttachedTo = (Type)state.obj;
					break;
				}
			}
			if (typeAttachedTo == null)
				throw new Exception("Nothing to attach to: " + attachedTo + "." + propertyName);
			return typeAttachedTo;
		}

		DependencyProperty getDependencyProperty(Type typeAttachedTo, string propertyName)
		{
			FieldInfo propField = typeAttachedTo.GetField(propertyName + "Property");
			if (propField == null)
				throw new Exception("Property " + propertyName + " does not exist on " + typeAttachedTo.Name);
			return (DependencyProperty)propField.GetValue(null);
		}

		void parseDependencyPropertyAttribute()
		{
			int index = reader.LocalName.LastIndexOf('.');
			string attachedTo = reader.LocalName.Substring(0, index);
			string propertyName = reader.LocalName.Substring(index + 1);
			
			Type currentType = (Type)currentState.obj;
			ensureDependencyObject(currentType);
			Type typeAttachedTo = findTypeToAttachTo(attachedTo, propertyName);
			DependencyProperty dp = getDependencyProperty(typeAttachedTo, propertyName);
			
			writer.CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
			writer.CreateDependencyPropertyText(reader.Value, dp.PropertyType);
			writer.EndDependencyProperty();
		}

		void parseEndElement()
		{
			if (currentState.type == CurrentType.Code) {
				writer.CreateCode((string)currentState.obj);
			} else if (currentState.type == CurrentType.Object) {
				ParserState prev = null;
				if (oldStates.Count > 1)
					prev = (ParserState)oldStates[oldStates.Count - 1];
				
				if (prev != null && prev.type == CurrentType.Property)
					writer.EndPropertyObject((Type)currentState.obj);
				else
					writer.EndObject();
			} else if (currentState.type == CurrentType.Property) {
				writer.EndProperty();
			} else if (currentState.type == CurrentType.DependencyProperty) {
				writer.EndDependencyProperty();
			}
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
