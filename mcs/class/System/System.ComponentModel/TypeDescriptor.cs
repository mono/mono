//
// System.ComponentModel.TypeDescriptor.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

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
using System.Reflection;
using System.Globalization;
using System.ComponentModel.Design;

namespace System.ComponentModel
{

public sealed class TypeDescriptor
{
	private static readonly string creatingDefaultConverters = "creatingDefaultConverters";
	private static Hashtable defaultConverters;
	private static IComNativeDescriptorHandler descriptorHandler;
	private static Hashtable componentTable = new Hashtable ();
	private static Hashtable typeTable = new Hashtable ();

	private TypeDescriptor ()
	{
	}

	[MonoTODO]
	public static void AddEditorTable (Type editorBaseType, Hashtable table)
	{
		throw new NotImplementedException ();
	}

	public static IDesigner CreateDesigner(IComponent component, Type designerBaseType)
	{
		string tn = designerBaseType.AssemblyQualifiedName;
		AttributeCollection col = GetAttributes (component);
		
		foreach (Attribute at in col) {
			DesignerAttribute dat = at as DesignerAttribute;
			if (dat != null && tn == dat.DesignerBaseTypeName) {
				return (IDesigner) Activator.CreateInstance (GetTypeFromName (component, dat.DesignerTypeName));
			}
		}
				
		return null;
	}

	public static EventDescriptor CreateEvent (Type componentType,
						   string name,
						   Type type,
						   Attribute [] attributes)
	{
		return new ReflectionEventDescriptor (componentType, name, type, attributes);
	}

	public static EventDescriptor CreateEvent (Type componentType,
						   EventDescriptor oldEventDescriptor,
						   Attribute [] attributes)
	{
		return new ReflectionEventDescriptor (componentType, oldEventDescriptor, attributes);
	}

	public static PropertyDescriptor CreateProperty (Type componentType,
							 string name,
							 Type type,
							 Attribute [] attributes)
	{
		return new ReflectionPropertyDescriptor (componentType, name, type, attributes);
	}

	public static PropertyDescriptor CreateProperty (Type componentType,
							 PropertyDescriptor oldPropertyDescriptor,
							 Attribute [] attributes)
	{
		return new ReflectionPropertyDescriptor (componentType, oldPropertyDescriptor, attributes);
	}

	public static AttributeCollection GetAttributes (Type componentType)
	{
		if (componentType == null)
			return AttributeCollection.Empty;

		return GetTypeInfo (componentType).GetAttributes ();
	}

	public static AttributeCollection GetAttributes (object component)
	{
		return GetAttributes (component, false);
	}

	public static AttributeCollection GetAttributes (object component, bool noCustomTypeDesc)
	{
		if (component == null)
		    return AttributeCollection.Empty;

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
		    return ((ICustomTypeDescriptor) component).GetAttributes ();
		} else {
			IComponent com = component as IComponent;
			if (com != null)
				return GetComponentInfo (com).GetAttributes ();
			else
				return GetTypeInfo (component.GetType()).GetAttributes ();
		}
	}

	public static string GetClassName (object component)
	{
		return GetClassName (component, false);
	}

	public static string GetClassName (object component, bool noCustomTypeDesc)
	{
		if (component == null)
		    throw new ArgumentNullException ("component", "component cannot be null");

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
		    return ((ICustomTypeDescriptor) component).GetClassName ();
		} else {
		    return component.GetType ().FullName;
		}
	}

	public static string GetComponentName (object component)
	{
		return GetComponentName (component, false);
	}

	public static string GetComponentName (object component, bool noCustomTypeDesc)
	{
		if (component == null)
		    throw new ArgumentNullException ("component", "component cannot be null");

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
		    return ((ICustomTypeDescriptor) component).GetComponentName ();
		} else {
			IComponent c = component as IComponent;
			if (c != null && c.Site != null)
				return c.Site.Name;
			return component.GetType().Name;
		}
	}

	public static TypeConverter GetConverter (object component)
	{
		return GetConverter (component.GetType ());
	}

	public static TypeConverter GetConverter (object component, bool noCustomTypeDesc)
	{
		if (component == null)
			throw new ArgumentNullException ("component", "component cannot be null");

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
			return ((ICustomTypeDescriptor) component).GetConverter ();
		} 
		else {
			Type t = null;
			AttributeCollection atts = GetAttributes (component, false);
			TypeConverterAttribute tca = (TypeConverterAttribute) atts[typeof(TypeConverterAttribute)];
			if (tca != null && tca.ConverterTypeName.Length > 0) {
				t = GetTypeFromName (component as IComponent, tca.ConverterTypeName);
			}
			
			if (t != null)
				return (TypeConverter) Activator.CreateInstance (t);
			else
				return GetConverter (component.GetType ());
		}
	}

	private static Hashtable DefaultConverters
	{
		get {
			if (defaultConverters != null)
				return defaultConverters;

			lock (creatingDefaultConverters) {
				if (defaultConverters != null)
					return defaultConverters;
				
				defaultConverters = new Hashtable ();
				defaultConverters.Add (typeof (bool), typeof (BooleanConverter));
				defaultConverters.Add (typeof (byte), typeof (ByteConverter));
				defaultConverters.Add (typeof (sbyte), typeof (SByteConverter));
				defaultConverters.Add (typeof (string), typeof (StringConverter));
				defaultConverters.Add (typeof (char), typeof (CharConverter));
				defaultConverters.Add (typeof (short), typeof (Int16Converter));
				defaultConverters.Add (typeof (int), typeof (Int32Converter));
				defaultConverters.Add (typeof (long), typeof (Int64Converter));
				defaultConverters.Add (typeof (ushort), typeof (UInt16Converter));
				defaultConverters.Add (typeof (uint), typeof (UInt32Converter));
				defaultConverters.Add (typeof (ulong), typeof (UInt64Converter));
				defaultConverters.Add (typeof (float), typeof (SingleConverter));
				defaultConverters.Add (typeof (double), typeof (DoubleConverter));
				defaultConverters.Add (typeof (decimal), typeof (DecimalConverter));
				defaultConverters.Add (typeof (object), typeof (TypeConverter));
				defaultConverters.Add (typeof (void), typeof (TypeConverter));
				defaultConverters.Add (typeof (Array), typeof (ArrayConverter));
				defaultConverters.Add (typeof (CultureInfo), typeof (CultureInfoConverter));
				defaultConverters.Add (typeof (DateTime), typeof (DateTimeConverter));
				defaultConverters.Add (typeof (Guid), typeof (GuidConverter));
				defaultConverters.Add (typeof (TimeSpan), typeof (TimeSpanConverter));
				defaultConverters.Add (typeof (ICollection), typeof (CollectionConverter));
			}
			return defaultConverters;
		}
	}
	
	public static TypeConverter GetConverter (Type type)
	{
		TypeConverterAttribute tca = null;
		Type t = null;
		object [] atts = type.GetCustomAttributes (typeof(TypeConverterAttribute), true);
		
		if (atts.Length > 0)
			tca = (TypeConverterAttribute)atts[0];
		
		if (tca != null) {
			t = GetTypeFromName (null, tca.ConverterTypeName);
		}
		
		if (t == null) {
			if (type.IsEnum) {
				// EnumConverter needs to know the enum type
				return new EnumConverter(type);
			} else if (type.IsArray) {
				return new ArrayConverter ();
			}
		}
		
		if (t == null)
			t = FindConverterType (type);

		if (t != null) {
			Exception exc = null;
			try {
				return (TypeConverter) Activator.CreateInstance (t);
			} catch (MissingMethodException e) {
				exc = e;
			}

			try {
				return (TypeConverter) Activator.CreateInstance (t, new object [] {type});
			} catch (MissingMethodException e) {
				throw exc;
			}
		}

		return new ReferenceConverter (type);    // Default?
	}

	private static Type FindConverterType (Type type)
	{
		Type t = null;
		
		// Is there a default converter
		t = (Type) DefaultConverters [type];
		if (t != null)
			return t;
		
		// Find default converter with a type this type is assignable to
		foreach (Type defType in DefaultConverters.Keys) {
			if (defType.IsInterface && defType.IsAssignableFrom (type)) {
				return (Type) DefaultConverters [defType];
			}
		}
		
		// Nothing found, try the same with our base type
		if (type.BaseType != null)
			return FindConverterType (type.BaseType);
		else
			return null;
	}

	public static EventDescriptor GetDefaultEvent (Type componentType)
	{
		return GetTypeInfo (componentType).GetDefaultEvent ();
	}

	public static EventDescriptor GetDefaultEvent (object component)
	{
		return GetDefaultEvent (component, false);
	}

	public static EventDescriptor GetDefaultEvent (object component, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetDefaultEvent ();
		else {
			IComponent com = component as IComponent;
			if (com != null)
				return GetComponentInfo (com).GetDefaultEvent ();
			else
				return GetTypeInfo (component.GetType()).GetDefaultEvent ();
		}
	}

	public static PropertyDescriptor GetDefaultProperty (Type componentType)
	{
		return GetTypeInfo (componentType).GetDefaultProperty ();
	}

	public static PropertyDescriptor GetDefaultProperty (object component)
	{
		return GetDefaultProperty (component, false);
	}

	public static PropertyDescriptor GetDefaultProperty (object component, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetDefaultProperty ();
		else {
			IComponent com = component as IComponent;
			if (com != null)
				return GetComponentInfo (com).GetDefaultProperty ();
			else
				return GetTypeInfo (component.GetType()).GetDefaultProperty ();
		}
	}

	[MonoTODO]
	public static object GetEditor (Type componentType, Type editorBaseType)
	{
		throw new NotImplementedException ();
	}

	public static object GetEditor (object component, Type editorBaseType)
	{
		return GetEditor (component, editorBaseType, false);
	}

	[MonoTODO]
	public static object GetEditor (object component, Type editorBaseType, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	public static EventDescriptorCollection GetEvents (object component)
	{
		return GetEvents (component, false);
	}

	public static EventDescriptorCollection GetEvents (Type componentType)
	{
		return GetEvents (componentType, null);
	}

	public static EventDescriptorCollection GetEvents (object component, Attribute [] attributes)
	{
		return GetEvents (component, attributes, false);
	}

	public static EventDescriptorCollection GetEvents (object component, bool noCustomTypeDesc)
	{
		return GetEvents (component, null, noCustomTypeDesc);
	}

	public static EventDescriptorCollection GetEvents (Type componentType, Attribute [] attributes)
	{
		return GetTypeInfo (componentType).GetEvents (attributes);
	}

	public static EventDescriptorCollection GetEvents (object component, Attribute [] attributes, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetEvents (attributes);
		else {
			IComponent com = component as IComponent;
			if (com != null)
				return GetComponentInfo (com).GetEvents (attributes);
			else
				return GetTypeInfo (component.GetType()).GetEvents (attributes);
		}
	}

	public static PropertyDescriptorCollection GetProperties (object component)
	{
		return GetProperties (component, false);
	}

	public static PropertyDescriptorCollection GetProperties (Type componentType)
	{
		return GetProperties (componentType, null);
	}

	public static PropertyDescriptorCollection GetProperties (object component, Attribute [] attributes)
	{
		return GetProperties (component, attributes, false);
	}

	public static PropertyDescriptorCollection GetProperties (object component, Attribute [] attributes, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetProperties (attributes);
		else {
			IComponent com = component as IComponent;
			if (com != null)
				return GetComponentInfo (com).GetProperties (attributes);
			else
				return GetTypeInfo (component.GetType()).GetProperties (attributes);
		}
	}

	public static PropertyDescriptorCollection GetProperties (object component, bool noCustomTypeDesc)
	{
		return GetProperties (component, null, noCustomTypeDesc);
	}

	public static PropertyDescriptorCollection GetProperties (Type componentType, Attribute [] attributes)
	{
		return GetTypeInfo (componentType).GetProperties (attributes);
	}

	public static void SortDescriptorArray (IList infos)
	{
		string[] names = new string [infos.Count];
		object[] values = new object [infos.Count];
		for (int n=0; n<names.Length; n++) {
			names[n] = ((MemberDescriptor)infos[n]).Name;
			values[n] = infos[n];
		}
		Array.Sort (names, values);
		infos.Clear();
		foreach (object ob in values)
			infos.Add (ob);
	}

	public static IComNativeDescriptorHandler ComNativeDescriptorHandler {
		get { return descriptorHandler; }
		set { descriptorHandler = value; }
	}

	public static void Refresh (Assembly assembly)
	{
		foreach (Type type in assembly.GetTypes())
			Refresh (type);
	}

	public static void Refresh (Module module)
	{
		foreach (Type type in module.GetTypes())
			Refresh (type);
	}

	public static void Refresh (object component)
	{
		lock (componentTable)
		{
			componentTable.Remove (component);
		}
		if (Refreshed != null) Refreshed (new RefreshEventArgs (component));
	}

	public static void Refresh (Type type)
	{
		lock (typeTable)
		{
			typeTable.Remove (type);
		}
		if (Refreshed != null) Refreshed (new RefreshEventArgs (type));
	}

	static EventHandler onDispose;

	static void OnComponentDisposed (object sender, EventArgs args)
	{
		lock (componentTable) {
			componentTable.Remove (sender);
		}
	}

	public static event RefreshEventHandler Refreshed;
	
	internal static ComponentInfo GetComponentInfo (IComponent com)
	{
		lock (componentTable)
		{
			ComponentInfo ci = (ComponentInfo) componentTable [com];
			if (ci == null) {
				if (onDispose == null)
					onDispose = new EventHandler (OnComponentDisposed);

				com.Disposed += onDispose;
				ci = new ComponentInfo (com);
				componentTable [com] = ci;
			}
			return ci;
		}
	}
	
	internal static TypeInfo GetTypeInfo (Type type)
	{
		lock (typeTable)
		{
			TypeInfo ci = (TypeInfo) typeTable [type];
			if (ci == null) {
				ci = new TypeInfo (type);
				typeTable [type] = ci;
			}
			return ci;
		}
	}
	
	static Type GetTypeFromName (IComponent component, string typeName)
	{
		if (component != null && component.Site != null) {
			ITypeResolutionService resver = (ITypeResolutionService) component.Site.GetService (typeof(ITypeResolutionService));
			if (resver != null) return resver.GetType (typeName, true, false);
		}
		
		Type t = Type.GetType (typeName);
		if (t == null) throw new ArgumentException ("Type '" + typeName + "' not found");
		return t;
	}
}

	internal abstract class Info
	{
		Type _infoType;
		EventDescriptor _defaultEvent;
		bool _gotDefaultEvent;
		PropertyDescriptor _defaultProperty;
		bool _gotDefaultProperty;
		AttributeCollection _attributes;
		
		public Info (Type infoType)
		{
			_infoType = infoType;
		}
		
		public abstract AttributeCollection GetAttributes ();
		public abstract EventDescriptorCollection GetEvents ();
		public abstract PropertyDescriptorCollection GetProperties ();
		
		public Type InfoType
		{
			get { return _infoType; }
		}
		
		public EventDescriptorCollection GetEvents (Attribute[] attributes)
		{
			EventDescriptorCollection evs = GetEvents ();
			if (attributes == null) return evs;
			else return evs.Filter (attributes);
		}
		
		public PropertyDescriptorCollection GetProperties (Attribute[] attributes)
		{
			PropertyDescriptorCollection props = GetProperties ();
			if (attributes == null) return props;
			else return props.Filter (attributes);
		}
		
		public EventDescriptor GetDefaultEvent ()
		{
			if (_gotDefaultEvent) return _defaultEvent;
			
			DefaultEventAttribute attr = (DefaultEventAttribute) GetAttributes()[typeof(DefaultEventAttribute)];
			if (attr == null || attr.Name == null) 
				_defaultEvent = null;
			else {
				// WTF? 
				// In our test case (TypeDescriptorTest.TestGetDefaultEvent), we have
				// a scenario where a custom filter adds the DefaultEventAttribute,
				// but its FilterEvents method removes the event the
				// DefaultEventAttribute applied to.  .NET accepts this and returns
				// the *other* event defined in the class.
				//
				// Consequently, we know we have a DefaultEvent, but we need to check
				// and ensure that the requested event is unfiltered.  If it is, just
				// grab the first element in the collection.
				EventDescriptorCollection events = GetEvents ();
				_defaultEvent = events [attr.Name];
				if (_defaultEvent == null && events.Count > 0)
					_defaultEvent = events [0];
			}
			_gotDefaultEvent = true;
			return _defaultEvent;
		}
		
		public PropertyDescriptor GetDefaultProperty ()
		{
			if (_gotDefaultProperty) return _defaultProperty;
			
			DefaultPropertyAttribute attr = (DefaultPropertyAttribute) GetAttributes()[typeof(DefaultPropertyAttribute)];
			if (attr == null || attr.Name == null) 
				_defaultProperty = null;
			else {
				PropertyInfo ei = _infoType.GetProperty (attr.Name);
				if (ei == null)
					throw new ArgumentException ("Property '" + attr.Name + "' not found in class " + _infoType);
				_defaultProperty = new ReflectionPropertyDescriptor (ei);
			}
			_gotDefaultProperty = true;
			return _defaultProperty;
		}
		
		protected AttributeCollection GetAttributes (IComponent comp)
		{
			if (_attributes != null) return _attributes;
			
			bool cache = true;
			object[] ats = _infoType.GetCustomAttributes (true);
			Hashtable t = new Hashtable ();
			foreach (Attribute at in ats)
				t [at.TypeId] = at;
					
			if (comp != null && comp.Site != null) 
			{
				ITypeDescriptorFilterService filter = (ITypeDescriptorFilterService) comp.Site.GetService (typeof(ITypeDescriptorFilterService));
				cache = filter.FilterAttributes (comp, t);
			}
			
			ArrayList atts = new ArrayList ();
			atts.AddRange (t.Values);
			AttributeCollection attCol = new AttributeCollection (atts);
			if (cache) _attributes = attCol;
			return attCol;
		}
	}

	internal class ComponentInfo : Info
	{
		IComponent _component;
		EventDescriptorCollection _events;
		PropertyDescriptorCollection _properties;
		
		public ComponentInfo (IComponent component): base (component.GetType())
		{
			_component = component;
		}
		
		public override AttributeCollection GetAttributes ()
		{
			return base.GetAttributes (_component);
		}
		
		public override EventDescriptorCollection GetEvents ()
		{
			if (_events != null) return _events;
			
			bool cache = true;
			EventInfo[] events = _component.GetType().GetEvents ();
			Hashtable t = new Hashtable ();
			foreach (EventInfo ev in events)
				t [ev.Name] = new ReflectionEventDescriptor (ev);
					
			if (_component.Site != null) 
			{
				Console.WriteLine ("filtering events...");
				ITypeDescriptorFilterService filter = (ITypeDescriptorFilterService) _component.Site.GetService (typeof(ITypeDescriptorFilterService));
				cache = filter.FilterEvents (_component, t);
			}
			
			ArrayList atts = new ArrayList ();
			atts.AddRange (t.Values);
			EventDescriptorCollection attCol = new EventDescriptorCollection (atts);
			if (cache) _events = attCol;
			return attCol;
		}
		
		public override PropertyDescriptorCollection GetProperties ()
		{
			if (_properties != null) return _properties;
			
			bool cache = true;
			PropertyInfo[] props = _component.GetType().GetProperties ();
			Hashtable t = new Hashtable ();
			foreach (PropertyInfo pr in props)
				t [pr.Name] = new ReflectionPropertyDescriptor (pr);
					
			if (_component.Site != null) 
			{
				ITypeDescriptorFilterService filter = (ITypeDescriptorFilterService) _component.Site.GetService (typeof(ITypeDescriptorFilterService));
				cache = filter.FilterProperties (_component, t);
			}
			
			ArrayList atts = new ArrayList ();
			atts.AddRange (t.Values);
			PropertyDescriptorCollection attCol = new PropertyDescriptorCollection (atts);
			if (cache) _properties = attCol;
			return attCol;
		}
	}
	
	internal class TypeInfo : Info
	{
		EventDescriptorCollection _events;
		PropertyDescriptorCollection _properties;
		
		public TypeInfo (Type t): base (t)
		{
		}
		
		public override AttributeCollection GetAttributes ()
		{
			return base.GetAttributes (null);
		}
		
		public override EventDescriptorCollection GetEvents ()
		{
			if (_events != null) return _events;
			
			EventInfo[] events = InfoType.GetEvents ();
			EventDescriptor[] descs = new EventDescriptor [events.Length];
			for (int n=0; n<events.Length; n++)
				descs [n] = new ReflectionEventDescriptor (events[n]);

			_events = new EventDescriptorCollection (descs);
			return _events;
		}
		
		public override PropertyDescriptorCollection GetProperties ()
		{
			if (_properties != null) return _properties;
			
			PropertyInfo[] props = InfoType.GetProperties ();
			PropertyDescriptor[] descs = new PropertyDescriptor [props.Length];
			for (int n=0; n<props.Length; n++)
				descs [n] = new ReflectionPropertyDescriptor (props[n]);

			_properties = new PropertyDescriptorCollection (descs);
			return _properties;
		}
	}
}

