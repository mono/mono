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
		private XmlTextReader reader;
		private ArrayList nodeQueue = new ArrayList();

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

		private ParserState currentState() {
			if (oldStates.Count == 0) return null;
			return (ParserState)oldStates[oldStates.Count - 1];
		}
		private ArrayList oldStates = new ArrayList();

		int tempStateCount = 0;
		private int getDepth() {
			return oldStates.Count - tempStateCount;
		}
	
		public XamlParser(string filename) : this(
				new XmlTextReader(filename))
		{
		}
		
		public XamlParser(TextReader reader) : this(
				new XmlTextReader(reader))
		{
		}
		
		public XamlParser(XmlTextReader reader)
		{
			this.reader = reader;
		}

		private XamlNode topNode()
		{
			return (XamlNode)nodeQueue[nodeQueue.Count - 1];
		}
		
		public XamlNode GetNextNode()
		{
			if (nodeQueue.Count != 0) {
				XamlNode x = (XamlNode)nodeQueue[0];
				nodeQueue.RemoveAt(0);
				return x;
			}
			while (reader.Read()) {
				Debug.WriteLine("XamlParser: NOW PARSING: " + reader.NodeType + "; " + reader.Name + "; " + reader.Value);
				if (goneTooFar())
					throw new Exception("Too far: " + reader.NodeType + ", " + reader.Name);
				if (currentState() != null && currentState().type == CurrentType.Code)
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
					throw new Exception("Unknown element type " + reader.NodeType);
				}
				if (nodeQueue.Count != 0) {
					XamlNode x = (XamlNode)nodeQueue[0];
					nodeQueue.RemoveAt(0);
					return x;
				}
			}
			return null;
		}
		void processElementInCodeState()
		{
			if (reader.NodeType == XmlNodeType.EndElement &&
					reader.LocalName == "Code" && 
					reader.NamespaceURI == XAML_NAMESPACE) {
				parseEndElement();
			} else if (reader.NodeType != XmlNodeType.CDATA && reader.NodeType != XmlNodeType.Text) {
				throw new Exception("Code element children must be either text or CDATA nodes.");
			} else {
				currentState().obj = (string)currentState().obj + reader.Value;
			}
		}
		bool goneTooFar()
		{

			if (begun && 
					currentState() == null && 
					reader.NodeType != XmlNodeType.Whitespace && 
					reader.NodeType != XmlNodeType.Comment)
				return true;
			else
				return false;
		}

		void parsePI()
		{
			if (reader.Name != "Mapping")
				throw new Exception("Unknown processing instruction.");
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
					currentState().type == CurrentType.Property) {
				parseObjectElement();
				return;
			}
			string beforeDot = reader.LocalName.Substring(0, dotPosition);
			string afterDot = reader.LocalName.Substring(dotPosition + 1);
			// If we've got this far, then currentState().Type == Object
			if (isNameOfAncestorClass(beforeDot, (Type)currentState().obj))
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
			nodeQueue.Add(new XamlTextNode(reader.LineNumber, reader.LinePosition, getDepth(), reader.Value));
			switch (currentState().type) {
			case CurrentType.Object:
			case CurrentType.PropertyObject:
				abortIfNotAddChild("text");
				((XamlTextNode)topNode()).setmode(XamlParseMode.Object);
//				writer.CreateObjectText(reader.Value);
				break;
			case CurrentType.DependencyProperty:
				DependencyProperty dp = (DependencyProperty)currentState().obj;
//				writer.CreateDependencyPropertyText(reader.Value, dp.PropertyType);
				((XamlTextNode)topNode()).setmode(XamlParseMode.DependencyProperty);
				((XamlTextNode)topNode()).setfinalType(dp.PropertyType);
				break;
			case CurrentType.Property:
				PropertyInfo prop = (PropertyInfo)currentState().obj;
//				writer.CreatePropertyText(reader.Value, prop.PropertyType);
				((XamlTextNode)topNode()).setmode(XamlParseMode.Property);
				((XamlTextNode)topNode()).setfinalType(prop.PropertyType);
				break;
			default:
				throw new NotImplementedException();
			}
		}

		void abortIfNotAddChild(string thing)
		{
			if (!isAddChild((Type)currentState().obj))
				throw new Exception("Cannot add " + thing +
						" to instance of '" + 
						((Type)currentState().obj) + 
						"'.");
		}
		
		void parseNormalPropertyElement(string propertyName)
		{
			// preconditions: currentState().Type == Object
			Type currentType = (Type)currentState().obj;
			PropertyInfo prop = currentType.GetProperty(propertyName);

			if (prop == null) {
				throw new Exception("Property '" + propertyName + "' not found on '" + currentType.Name + "'.");
			}


//			writer.CreateProperty(prop);
			nodeQueue.Add(new XamlPropertyNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					null,
					currentType.Assembly.FullName,
					currentType.AssemblyQualifiedName,
					propertyName,
					reader.Value,
					reader.NamespaceURI,
					BamlAttributeUsage.Default,
					false));
			((XamlPropertyNode)topNode()).setPropInfo(prop);
			push(CurrentType.Property, prop);

			if (reader.HasAttributes) {
				throw new Exception("Property node should not have attributes.");
			}
		}


		void parseDependencyPropertyElement(string attachedTo, string propertyName)
		{
			Type currentType = (Type)currentState().obj;
			ensureDependencyObject(currentType);
			Type typeAttachedTo = findTypeToAttachTo(attachedTo, propertyName);
			DependencyProperty dp = getDependencyProperty(typeAttachedTo, propertyName);
			

//			writer.CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
			nodeQueue.Add(new XamlPropertyNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					null,
					currentType.Assembly.FullName,
					currentType.AssemblyQualifiedName,
					propertyName,
					reader.Value,
					reader.NamespaceURI,
					BamlAttributeUsage.Default,
					false));
			((XamlPropertyNode)topNode()).setDP(dp);

			push(CurrentType.DependencyProperty, dp);
	
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
			if (currentState() == null) {
				parseTopLevelObjectElement(parent);
			} else {
				parseChildObjectElement(parent);
			}
			
			if (isEmpty)
				tempStateCount ++;
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
			if (reader.GetAttribute("Class", XAML_NAMESPACE) != null)
				throw new Exception("The XAML Class attribute can not be applied to child elements\n"+
						"Do you mean the Name attribute?");
			string name = reader.GetAttribute("Name", XAML_NAMESPACE);
			if (name == null)
				name = reader.GetAttribute("Name", reader.NamespaceURI);

			Debug.WriteLine("XamlParser: parent is " + parent);
			if (currentState().type == CurrentType.Object) {
				abortIfNotAddChild("object");
				addChild(parent, name);
			} else if (currentState().type == CurrentType.Property) {
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
			if (currentState().type == CurrentType.Object) {
				nodeQueue.Add(new XamlElementEndNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth()));
//				writer.EndObject();
			} else if (currentState().type == CurrentType.PropertyObject) {
				nodeQueue.Add(new XamlElementEndNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth()));
				((XamlElementEndNode)topNode()).setpropertyObject(true);
				nodeQueue.Add(new XamlPropertyComplexEndNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth()));
				((XamlPropertyComplexEndNode)topNode()).setfinalType((Type)currentState().obj);
//				ParserState state = (ParserState)oldStates[oldStates.Count - 1];
//				writer.EndPropertyObject(((PropertyInfo)state.obj).PropertyType);
			}
			tempStateCount --;
			pop();
		}

		void createTopLevel(string parentName, string className)
		{
			Type t = Type.GetType(parentName);
			nodeQueue.Add(new XamlDocumentStartNode(reader.LineNumber, reader.LinePosition, getDepth()));
			nodeQueue.Add(new XamlElementStartNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					t.Assembly.FullName,
					t.AssemblyQualifiedName,
					t,
					null));

			((XamlElementStartNode)topNode()).setname(className);

//			writer.CreateTopLevel(t, className);
			push(CurrentType.Object, t);
		}

		void addChild(Type type, string objectName)
		{
			nodeQueue.Add(new XamlElementStartNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					type.Assembly.FullName,
					type.AssemblyQualifiedName,
					type,
					null));
			((XamlElementStartNode)topNode()).setname(objectName);

//			writer.CreateObject(type, objectName);
			push(CurrentType.Object, type);
		}
		
		void addPropertyChild(Type type, string objectName)
		{
//			writer.CreatePropertyObject(type, objectName);
			nodeQueue.Add(new XamlElementStartNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					type.Assembly.FullName,
					type.AssemblyQualifiedName,
					type,
					null));
			((XamlElementStartNode)topNode()).setname(objectName);
			((XamlElementStartNode)topNode()).setpropertyObject(true);


			push(CurrentType.PropertyObject, type);
		}


		
		void parseLocalPropertyAttribute()
		{
			string propertyName = reader.LocalName;
			Type currentType = (Type)currentState().obj;
			PropertyInfo prop = currentType.GetProperty(propertyName);
			if (parsedAsEventProperty(currentType, propertyName))
				return;
			if (prop == null)
				throw new Exception ("Property '" + propertyName + "' not found on '" + currentType.Name + "'.");
			nodeQueue.Add(new XamlPropertyNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					null,
					currentType.Assembly.FullName,
					currentType.AssemblyQualifiedName,
					propertyName,
					reader.Value,
					reader.NamespaceURI,
					BamlAttributeUsage.Default,
					false));
			((XamlPropertyNode)nodeQueue[nodeQueue.Count - 1]).setPropInfo(prop);

			if (!prop.PropertyType.IsSubclassOf(typeof(Delegate))) {

				nodeQueue.Add(new XamlTextNode(
						reader.LineNumber,
						reader.LinePosition,
						getDepth(),
						reader.Value));

				((XamlTextNode)topNode()).setmode(XamlParseMode.Property);
//				writer.CreatePropertyText(reader.Value, prop.PropertyType);
							
//				writer.EndProperty();
				((XamlTextNode)topNode()).setfinalType(prop.PropertyType);
			} else {
//				writer.CreatePropertyDelegate(reader.Value, prop.PropertyType);
				nodeQueue.Add(new XamlClrEventNode(
						reader.LineNumber,
						reader.LinePosition, 
						getDepth(),
						propertyName,
						prop,
						reader.Value));
			}
		}
		
		bool parsedAsEventProperty(Type currentType, string eventName)
		{
			EventInfo evt = currentType.GetEvent(eventName);
			if (evt == null)
				return false;
			nodeQueue.Add(new XamlClrEventNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					eventName, 
					evt, 
					reader.Value));
//			writer.CreateEvent(evt);
//			writer.CreateEventDelegate(reader.Value, evt.EventHandlerType);
//			writer.EndEvent();
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
			
			Type currentType = (Type)currentState().obj;
			ensureDependencyObject(currentType);
			Type typeAttachedTo = findTypeToAttachTo(attachedTo, propertyName);
			DependencyProperty dp = getDependencyProperty(typeAttachedTo, propertyName);
			
			nodeQueue.Add(new XamlPropertyNode(
					reader.LineNumber,
					reader.LinePosition,
					getDepth(),
					null,
					currentType.Assembly.FullName,
					currentType.AssemblyQualifiedName,
					propertyName,
					reader.Value,
					reader.NamespaceURI,
					BamlAttributeUsage.Default,
					false));
			((XamlPropertyNode)topNode()).setDP(dp);

			nodeQueue.Add(new XamlTextNode(reader.LineNumber, reader.LinePosition, getDepth(), reader.Value));
			((XamlTextNode)topNode()).setmode(XamlParseMode.DependencyProperty);
			((XamlTextNode)topNode()).setfinalType(dp.PropertyType);

//			writer.CreateDependencyProperty(typeAttachedTo, propertyName, dp.PropertyType);
//			writer.CreateDependencyPropertyText(reader.Value, dp.PropertyType);
//			writer.EndDependencyProperty();
		}

		void parseEndElement()
		{
			Debug.WriteLine("XamlParser: IN ENDELEMENT, SWITCHING ON " + currentState().type);
			switch (currentState().type) {
			case CurrentType.Code:
				nodeQueue.Add(new XamlLiteralContentNode(
						reader.LineNumber,
						reader.LinePosition,
						getDepth(),
						(string)currentState().obj));
//				writer.CreateCode((string)currentState().obj);
				break;
			case CurrentType.Object:
				nodeQueue.Add(new XamlElementEndNode(
						reader.LineNumber,
						reader.LinePosition,
						getDepth()));
//				writer.EndObject();
				break;
			case CurrentType.PropertyObject:
				nodeQueue.Add(new XamlElementEndNode(
						reader.LineNumber,
						reader.LinePosition,
						getDepth()));
				((XamlElementEndNode)topNode()).setpropertyObject(true);
				nodeQueue.Add(new XamlPropertyComplexEndNode(
						reader.LineNumber,
						reader.LinePosition,
						getDepth()));
				Debug.WriteLine("XamlParser: XXXXXXXX" + currentState().obj);
				Debug.WriteLine("XamlParser: XXXXXXXX" + (currentState().obj is Type));
				((XamlPropertyComplexEndNode)topNode()).setfinalType((Type)currentState().obj);
				Debug.WriteLine("XamlParser: XXXXXXXX" + ((XamlPropertyComplexEndNode)topNode()).finalType);
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 1]).obj.GetType());
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 1]).type);
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 2]).obj.GetType());
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 2]).type);
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 3]).obj.GetType());
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 3]).type);
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 4]).obj.GetType());
				Debug.WriteLine("TTTTTTTTT " + ((ParserState)oldStates[oldStates.Count - 4]).type);
//				writer.EndPropertyObject((Type)currentState().obj);
//				return;
				break;
			// these next two happen automatically in the new model
			case CurrentType.Property:
//				writer.EndProperty();
				break;
			case CurrentType.DependencyProperty:
//				writer.EndDependencyProperty();
				break;
			}
			pop();
		}

		void pop()
		{
			Debug.WriteLine("XamlParser: POPPING: " + currentState().type);
			// we are popping the last element
			if (oldStates.Count == 1) {
//				writer.Finish();
				nodeQueue.Add(new XamlDocumentEndNode(
						reader.LineNumber,
						reader.LinePosition,
						getDepth()));
				return;
			}
			int lastIndex = oldStates.Count - 1;
			oldStates.RemoveAt(lastIndex);
		}
		void push(CurrentType type, Object obj)
		{
			Debug.WriteLine("XamlParser: PUSHING: " + oldStates.Count + " " + type);
			ParserState currentState = new ParserState();
			currentState.type = type;
			currentState.obj = obj;
			oldStates.Add(currentState);
		}
	}
}
