//
// TestDisplayer.cs: Base class for parsing Type objects.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any           
// person obtaining a copy of this software and associated        
// documentation files (the "Software"), to deal in the           
// Software without restriction, including without limitation     
// the rights to use, copy, modify, merge, publish,               
// distribute, sublicense, and/or sell copies of the Software,    
// and to permit persons to whom the Software is furnished to     
// do so, subject to the following conditions:                    
//                                                                 
// The above copyright notice and this permission notice          
// shall be included in all copies or substantial portions        
// of the Software.                                               
//                                                                 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY      
// KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO         
// THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A               
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL      
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,      
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,  
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION       
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public delegate void BaseTypeEventHandler (object sender, BaseTypeEventArgs e);
	public delegate void TypeEventHandler (object sender, TypeEventArgs e);
	public delegate void InterfacesEventHandler (object sender, InterfacesEventArgs e);
	public delegate void FieldsEventHandler (object sender, FieldsEventArgs e);
	public delegate void PropertiesEventHandler (object sender, PropertiesEventArgs e);
	public delegate void EventsEventHandler (object sender, EventsEventArgs e);
	public delegate void ConstructorsEventHandler (object sender, ConstructorsEventArgs e);
	public delegate void MethodsEventHandler (object sender, MethodsEventArgs e);

	public class BaseTypeEventArgs : EventArgs {

		private Type _base;

		internal BaseTypeEventArgs (Type type)
		{
			_base = type;
		}

		public Type BaseType {
			get {return _base;}
		}
	}

	public class TypeEventArgs : EventArgs {

		private Type _type;

		internal TypeEventArgs (Type type)
		{
			_type = type;
		}

		public Type Type {
			get {return _type;}
		}
	}

	public class InterfacesEventArgs : EventArgs {

		private Type[] _interfaces;

		internal InterfacesEventArgs (Type[] interfaces)
		{
			_interfaces = interfaces ;
		}

		public Type[] Interfaces {
			get {return _interfaces;}
		}
	}

	public class FieldsEventArgs : EventArgs {
		private FieldInfo[] _fields;

		internal FieldsEventArgs (FieldInfo[] fields)
		{
			_fields = fields;
		}

		public FieldInfo[] Fields {
			get {return _fields;}
		}
	}

	public class PropertiesEventArgs : EventArgs {

		private PropertyInfo[] _props;

		internal PropertiesEventArgs (PropertyInfo[] properties)
		{
			_props = properties;
		}

		public PropertyInfo[] Properties {
			get {return _props;}
		}
	}

	public class EventsEventArgs : EventArgs {

		private EventInfo[] _events;

		internal EventsEventArgs (EventInfo[] events)
		{
			_events = events;
		}

		public EventInfo[] Events {
			get {return _events;}
		}
	}

	public class ConstructorsEventArgs : EventArgs {

		private ConstructorInfo[] _ctors;

		internal ConstructorsEventArgs (ConstructorInfo[] ctors)
		{
			_ctors = ctors;
		}

		public ConstructorInfo[] Constructors {
			get {return _ctors;}
		}
	}

	public class MethodsEventArgs : EventArgs {

		private MethodInfo[] _methods;

		internal MethodsEventArgs (MethodInfo[] methods)
		{
			_methods = methods;
		}

		public MethodInfo[] Methods {
			get {return _methods;}
		}
	}

	public class TypeDisplayer {

		private bool showBase = false;
		private bool showConstructors = false;
		private bool showEvents = false;
		private bool showFields = false;
		private bool showInterfaces = false;
		private bool showMethods = false;
		private bool showProperties = false;
		private bool showTypeProperties = false;
		private bool showInheritedMembers = false;
		private bool verboseOutput = false;
		private bool flattenHierarchy = false;
		private bool showNonPublic = false;
		private bool showMonoBroken = false;

		// `PrintTypeProperties' is recursive, but refrains from printing
		// duplicates.  Despite duplicate removal, the output for printing the
		// Properties of System.Type is > 800K of text.
		//
		// 3 levels permits viewing Attribute values, but not the attributes of
		// those attribute values.
		//
		// For example, 3 levels permits:
		// 		class		System.Type                           {depth 0}
		// 			Properties:                                 {depth 1}
		// 				System.Reflection.MemberTypes MemberType  {depth 2}
		// 					- CanRead=True                          {depth 3}
		// 					- CanWrite=False                        {depth 3}
		// 					...
		private int maxDepth = 3;

    public TypeDisplayer ()
    {
    }

		public int MaxDepth {
			get {return maxDepth;}
			set {maxDepth = value;}
		}

		public bool ShowBase {
			get {return showBase;}
			set {showBase = value;}
		}

		public bool ShowConstructors {
			get {return showConstructors;}
			set {showConstructors = value;}
		}

		public bool ShowEvents {
			get {return showEvents;}
			set {showEvents = value;}
		}

		public bool ShowFields {
			get {return showFields;}
			set {showFields = value;}
		}

		public bool ShowInterfaces {
			get {return showInterfaces;}
			set {showInterfaces = value;}
		}

		public bool ShowMethods {
			get {return showMethods;}
			set {showMethods = value;}
		}

		public bool ShowProperties {
			get {return showProperties;}
			set {showProperties = value;}
		}

		public bool ShowTypeProperties {
			get {return showTypeProperties;}
			set {showTypeProperties = value;}
		}

		public bool ShowInheritedMembers {
			get {return showInheritedMembers;}
			set {showInheritedMembers = value;}
		}

		public bool ShowNonPublic {
			get {return showNonPublic;}
			set {showNonPublic = value;}
		}

		public bool ShowMonoBroken {
			get {return showMonoBroken;}
			set {showMonoBroken = value;}
		}

		public bool FlattenHierarchy {
			get {return flattenHierarchy;}
			set {flattenHierarchy = value;}
		}

		public bool VerboseOutput {
			get {return verboseOutput;}
			set {verboseOutput = value;}
		}

		private static BindingFlags bindingFlags = 
			BindingFlags.DeclaredOnly | 
			BindingFlags.Public | 
			BindingFlags.Instance | 
			BindingFlags.Static;

		public void Parse (Type type)
		{
			BindingFlags bf = bindingFlags;

			if (FlattenHierarchy)
				bf |= BindingFlags.FlattenHierarchy;
			if (ShowInheritedMembers)
				bf &= ~BindingFlags.DeclaredOnly;
			if (ShowNonPublic)
				bf |= BindingFlags.NonPublic;

			OnTypeDeclaration (type);
			OnTypeBody (type, bf);
		}

		public static string FieldValue (FieldInfo f)
		{
			string s = null;
			if (f.DeclaringType.IsEnum)
				s = String.Format ("0x{0}",
						Enum.Format (f.DeclaringType, f.GetValue (null), "x"));
			else
				s = f.GetValue(null).ToString();
			return s;
		}

		protected virtual void OnTypeDeclaration (Type type)
		{
			Type (type);

			BaseType (type.BaseType);
			Interfaces (type.GetInterfaces ());
		}

		protected virtual void OnTypeBody (Type type, BindingFlags bf)
		{
			Fields (type.GetFields(bf));
			Constructors (type.GetConstructors(bf));
			Properties (type.GetProperties(bf));
			Events (type.GetEvents(bf));
			Methods (type.GetMethods(bf));
		}

		private void Type (Type t)
		{
			TypeEventArgs ea = new TypeEventArgs (t);
			try {
				OnType (ea);
			} finally {
				if (ReceiveTypes != null)
					ReceiveTypes (this, ea);
			}
		}

		protected virtual void OnType (TypeEventArgs e) {}

		private void BaseType (Type t)
		{
			if (ShowBase) {
				BaseTypeEventArgs ea = new BaseTypeEventArgs (t);
				try {
					OnBaseType (ea);
				} finally {
					if (ReceiveBaseType != null)
						ReceiveBaseType (this, ea);
				}
			}
		}

		protected virtual void OnBaseType (BaseTypeEventArgs e) {}

		private void Interfaces (Type[] i)
		{
			if (ShowInterfaces) {
				InterfacesEventArgs ea = new InterfacesEventArgs (i);
				try {
					OnInterfaces (ea);
				} finally {
					if (ReceiveInterfaces != null)
						ReceiveInterfaces (this, ea);
				}
			}
		}

		protected virtual void OnInterfaces (InterfacesEventArgs e) {}

		private void Fields (FieldInfo[] f)
		{
			if (ShowFields) {
				FieldsEventArgs ea = new FieldsEventArgs (f);
				try {
					OnFields (ea);
				} finally {
					if (ReceiveFields != null)
						ReceiveFields (this, ea);
				}
			}
		}

		protected virtual void OnFields (FieldsEventArgs e) {}

		private void Properties (PropertyInfo[] p)
		{
			if (ShowProperties) {
				PropertiesEventArgs ea = new PropertiesEventArgs (p);
				try {
					OnProperties (ea);
				} finally {
					if (ReceiveProperties != null)
						ReceiveProperties (this, ea);
				}
			}
		}

		protected virtual void OnProperties (PropertiesEventArgs e) {}

		private void Events (EventInfo[] e)
		{
			if (ShowEvents) {
				EventsEventArgs ea = new EventsEventArgs (e);
				try {
					OnEvents (ea);
				} finally {
					if (ReceiveEvents != null)
						ReceiveEvents (this, ea);
				}
			}
		}

		protected virtual void OnEvents (EventsEventArgs e) {}

		private void Constructors (ConstructorInfo[] c)
		{
			if (ShowConstructors) {
				ConstructorsEventArgs ea = new ConstructorsEventArgs (c);
				try {
					OnConstructors (ea);
				} finally {
					if (ReceiveConstructors != null)
						ReceiveConstructors (this, ea);
				}
			}
		}

		protected virtual void OnConstructors (ConstructorsEventArgs e)
		{
		}

		private void Methods (MethodInfo[] m)
		{
			if (ShowMethods) {
				MethodsEventArgs ea = new MethodsEventArgs (m);
				try {
					OnMethods (ea);
				} finally {
					if (ReceiveMethods != null)
						ReceiveMethods (this, ea);
				}
			}
		}

		protected virtual void OnMethods (MethodsEventArgs e) {}

		protected static string GetEnumDescription (Type enumType, object value)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (Enum.Format(enumType, value, "f"));
			sb.Append (" (");
			try {
				sb.Append (String.Format ("0x{0}", Enum.Format (enumType, value, "x")));
			}
			catch {
				sb.Append ("<unable to determine enumeration value>");
			}
			sb.Append (")");
			return sb.ToString ();
		}

    protected static string GetTypeKeyword (Type type)
    {
			string t = null;

			if (type.IsClass)
				t = "class";
			else if (type.IsEnum)
				t = "enum";
			else if (type.IsValueType)
				t = "struct";
			else if (type.IsInterface)
				t = "interface";
			else
				t = "type";

      return t;
    }

    protected static string GetTypeHeader (Type type)
    {
      return String.Format ("{0,-11}{1}", GetTypeKeyword (type), type.ToString());
    }

		public event TypeEventHandler         ReceiveTypes;
		public event BaseTypeEventHandler     ReceiveBaseType;
		public event InterfacesEventHandler   ReceiveInterfaces;
		public event FieldsEventHandler       ReceiveFields;
		public event PropertiesEventHandler   ReceiveProperties;
		public event EventsEventHandler       ReceiveEvents;
		public event ConstructorsEventHandler ReceiveConstructors;
		public event MethodsEventHandler      ReceiveMethods;
	}
}

