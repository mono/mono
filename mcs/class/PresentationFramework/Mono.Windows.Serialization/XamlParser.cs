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
using System.Diagnostics;
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
		private IXamlWriter writer;

		private enum CurrentType { Object, 
			Property, 
			PropertyObject,
			DependencyProperty,
	       		Code }

		private class ParserState {
			public object obj;
			public CurrentType type;
		}
	
		private bool begun = false;

		private ParserState currentState = null;
		private ArrayList oldStates = new ArrayList();
	
		public XamlParser(string filename, IXamlWriter writer) : this(
				new XmlTextReader(filename), writer)
		{
		}
		
		public XamlParser(TextReader reader, IXamlWriter writer) : this(
				new XmlTextReader(reader), writer)
		{
		}
		
		public XamlParser(XmlReader reader, IXamlWriter writer)
		{
			this.reader = reader;
			this.writer = writer;
		}
		
		public void Parse()
		{
			while (reader.Read()) {
				Debug.WriteLine("NOW PARSING: " + reader.NodeType + "; " + reader.Name + "; " + reader.Value);
				if (goneTooFar())
					throw new Exception("Too far: " + reader.NodeType + ", " + reader.Name);
				if (currentState != null && currentState.type == CurrentType.Code)
				{
					processElementInCodeState();
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
		void processElementInCodeState()
		{
			if (reader.NodeType == XmlNodeType.EndElement &&
					reader.LocalName == "Code" && 
					reader.NamespaceURI == XAML_NAMESPACE) {
				parseEndElement();
			} else {
				currentState.obj = (string)currentState.obj + reader.Value;
			}
		}
		bool goneTooFar()
		{

			if (begun && 
					currentState == null && 
					reader.NodeType != XmlNodeType.Whitespace && 
					reader.NodeType != XmlNodeType.Comment)
				return true;
			else
				return false;
		}

		void parsePI()
		{
			if (reader.Name != "Mapping")
				throw new Exception("Unknown processing instruction");
			mapper.AddMappingProcessingInstruction(reader.Value);
		}

		void parseElement()
		{
			if (reader.NamespaceURI == "")
				throw new Exception("No xml namespace specified.");
			if (reader.LocalName == "Code" && reader.NamespaceURI == XAML_NAMESPACE) {
				parseCodeElement();
				return;
			}
			// This element must be an object if:
			//  - It's a direct child of a property element
			//  - It's a direct child of an IAddChild element
			//    and does not have a dot in its name
			//    
			//  We just check that it doesn't have a dot in it here
			//  since parseObjectElement will confirm that it is
			//  a direct child of an IAddChild.
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

		// handle an x:Code element. Most of the handling for this is
		// at the start of the main parsing loop, in the 
		// processElementInCodeState() function
		void parseCodeElement()
		{
			push(CurrentType.Code, "");
		}

		void parseText()
		{
			switch (currentState.type) {
			case CurrentType.Object:
			case CurrentType.PropertyObject:
				abortIfNotAddChild("text");
				writer.CreateObjectText(reader.Value);
				break;
			case CurrentType.DependencyProperty:
				DependencyProperty dp = (DependencyProperty)currentState.obj;
				writer.CreateDependencyPropertyText(reader.Value, dp.PropertyType);
				break;
			case CurrentType.Property:
				PropertyInfo prop = (PropertyInfo)currentState.obj;
				writer.CreatePropertyText(reader.Value, prop.PropertyType);
				break;
			default:
				throw new NotImplementedException();
			}
		}

		void abortIfNotAddChild(string thing)
		{
			if (!isAddChild((Type)currentState.obj))
				throw new Exception("Cannot add " + thing +
						" to instance of '" + 
						((Type)currentState.obj) + 
						"'.");
		}
		
		void parseNormalPropertyElement(string propertyName)
		{
			// preconditions: currentState.Type == Object
			Type currentType = (Type)currentState.obj;
			PropertyInfo prop = currentType.GetProperty(propertyName);

			if (prop == null) {
				throw new Exception("Property '" + propertyName + "' not found on '" + currentType.Name + "'.");
			}

			push(CurrentType.Property, prop);

			writer.CreateProperty(prop);

			if (reader.HasAttributes) {
				throw new Exception("Property node should not have attributes.");
			}
		}


		void parseDependencyPropertyElement(string attachedTo, string propertyName)
		{
			Type currentType = (Type)currentState.obj;
			ensureDependencyObject(currentType);
			Type typeAttachedTo = findTypeToAttachTo(attachedTo, propertyName);
			DependencyProperty dp = getDependencyProperty(typeAttachedTo, propertyName);
			
			push(CurrentType.DependencyProperty, dp);

			writer.CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
		}
		bool isAddChild(Type t)
		{
			return (t.GetInterface("System.Windows.Serialization.IAddChild") != null);
		}
		void parseObjectElement()
		{
			Type parent;
			bool isEmpty = reader.IsEmptyElement;
			
			parent = mapper.GetType(reader.NamespaceURI, reader.Name);
			if (parent == null)
				throw new Exception("Class '" + reader.Name + "' not found.");
		
			// whichever of these functions runs will push something
			if (currentState == null) {
				parseTopLevelObjectElement(parent);
			} else {
				parseChildObjectElement(parent);
			}
			
			processObjectAttributes();

			if (isEmpty) {
				closeEmptyObjectElement();
			}
		}
		void parseTopLevelObjectElement(Type parent)
		{
			if (reader.GetAttribute("Name", XAML_NAMESPACE) != null)
				throw new Exception("The XAML Name attribute can not be applied to top level elements\n"+
						"Do you mean the Class attribute?");
			begun = true;
			createTopLevel(parent.AssemblyQualifiedName, reader.GetAttribute("Class", XAML_NAMESPACE));
		}

		void parseChildObjectElement(Type parent)
		{
			string name = reader.GetAttribute("Name", XAML_NAMESPACE);
			if (name == null)
				name = reader.GetAttribute("Name", reader.NamespaceURI);

			if (currentState.type == CurrentType.Object) {
				abortIfNotAddChild("object");
				addChild(parent, name);
			} else if (currentState.type == CurrentType.Property) {
				addPropertyChild(parent, name);
			} else {
				throw new NotImplementedException();
			}
		}
		void processObjectAttributes()
		{
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
		}

		void closeEmptyObjectElement()
		{
			if (currentState.type == CurrentType.Object) {
				writer.EndObject();
			} else if (currentState.type == CurrentType.PropertyObject) {
				ParserState state = (ParserState)oldStates[oldStates.Count - 1];
				writer.EndPropertyObject(((PropertyInfo)state.obj).PropertyType);
			}
			pop();
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
			push(CurrentType.Object, type);
		}
		
		void addPropertyChild(Type type, string objectName)
		{
			writer.CreatePropertyObject(type, objectName);

			push(CurrentType.PropertyObject, type);
		}


		
		void parseLocalPropertyAttribute()
		{
			string propertyName = reader.LocalName;
			Type currentType = (Type)currentState.obj;
			PropertyInfo prop = currentType.GetProperty(propertyName);
			if (parsedAsEventProperty(currentType, propertyName))
				return;
			if (prop == null)
				throw new Exception ("Property '" + propertyName + "' not found on '" + currentType.Name + "'.");

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
				if ((state.type == CurrentType.Object || 
						state.type == CurrentType.PropertyObject) &&
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
				throw new Exception("Property '" + propertyName + "' does not exist on '" + typeAttachedTo.Name + "'.");
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
			Debug.WriteLine("IN ENDELEMENT, SWITCHING ON " + currentState.type);
			switch (currentState.type) {
			case CurrentType.Code:
				writer.CreateCode((string)currentState.obj);
				break;
			case CurrentType.Object:
				writer.EndObject();
				break;
			case CurrentType.PropertyObject:
				writer.EndPropertyObject((Type)currentState.obj);
				break;
			case CurrentType.Property:
				writer.EndProperty();
				break;
			case CurrentType.DependencyProperty:
				writer.EndDependencyProperty();
				break;
			}
			pop();
		}

		void pop()
		{
			Debug.WriteLine("POPPING: " + currentState.type);
			if (oldStates.Count == 0) {
				currentState = null;
				writer.Finish();
				return;
			}
			int lastIndex = oldStates.Count - 1;
			currentState = (ParserState)oldStates[lastIndex];
			oldStates.RemoveAt(lastIndex);
		}
		void push(CurrentType type, Object obj)
		{
			Debug.WriteLine("PUSHING: " + type);
			oldStates.Add(currentState);
			currentState = new ParserState();
			currentState.type = type;
			currentState.obj = obj;
		}
	}
}
