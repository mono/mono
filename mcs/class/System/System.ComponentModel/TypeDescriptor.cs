//
// System.ComponentModel.TypeDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Reflection;

namespace System.ComponentModel
{

public sealed class TypeDescriptor
{
	private static readonly string creatingDefaultConverters = "creatingDefaultConverters";
	private static Hashtable defaultConverters;

	private TypeDescriptor ()
	{
	}

	[MonoTODO]
	public static void AddEditorTable (Type editorBaseType, Hashtable table)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptor CreateEvent (Type componentType,
						   string name,
						   Type type,
						   Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptor CreateEvent (Type componentType,
						   EventDescriptor oldEventDescriptor,
						   Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptor CreateProperty (Type componentType,
							 string name,
							 Type type,
							 Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptor CreateProperty (Type componentType,
							 PropertyDescriptor oldPropertyDescriptor,
							 Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	public static AttributeCollection GetAttributes (Type componentType)
	{
		if (componentType == null)
			return AttributeCollection.Empty;

		object [] atts = componentType.GetCustomAttributes (false);
		return new AttributeCollection ((Attribute []) atts);
	}

	public static AttributeCollection GetAttributes (object component)
	{
		return GetAttributes (component.GetType ());
	}

	[MonoTODO]
	public static AttributeCollection GetAttributes (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static string GetClassName (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static string GetClassName (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static string GetComponentName (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static string GetComponentName (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	public static TypeConverter GetConverter (object component)
	{
		return GetConverter (component.GetType ());
	}

	[MonoTODO]
	public static TypeConverter GetConverter (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	private static Hashtable DefaultConverters
	{
		get {
			if (defaultConverters != null)
				return defaultConverters;

			lock (creatingDefaultConverters) {
				if (defaultConverters != null)
					return defaultConverters;
				
				//FIXME: add more converters as we implement them
				defaultConverters = new Hashtable ();
				defaultConverters.Add (typeof (Enum), typeof (EnumConverter));
				defaultConverters.Add (typeof (string), typeof (StringConverter));
			}
			return defaultConverters;
		}
	}
	
	public static TypeConverter GetConverter (Type type)
	{
		Type t = DefaultConverters [type] as Type;
		if (t == null && type.IsEnum)
			t = (Type) DefaultConverters [typeof (Enum)];

		string converter_name = null;
		if (t == null) {
			object [] attrs = type.GetCustomAttributes (false);
			foreach (object o in attrs){
				if (o is TypeConverterAttribute){
					TypeConverterAttribute tc = (TypeConverterAttribute) o;
					converter_name = tc.ConverterTypeName;
					break;
				}
			}
		} else {
			converter_name = t.FullName;
		}
		

		if (converter_name == null)
			return null;

		object converter = null;
		try {
			converter = Activator.CreateInstance (Type.GetType (converter_name));
		} catch (Exception){
		}
	
		return converter as TypeConverter;
	}

	[MonoTODO]
	public static EventDescriptor GetDefaultEvent (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptor GetDefaultEvent (Type componentType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptor GetDefaultEvent (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptor GetDefaultProperty (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptor GetDefaultProperty (Type componentType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptor GetDefaultProperty (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static object GetEditor (object component, Type editorBaseType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static object GetEditor (Type componentType, Type editorBaseType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static object GetEditor (object component, Type editorBaseType, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptorCollection GetEvents (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptorCollection GetEvents (Type componentType)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptorCollection GetEvents (object component, Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptorCollection GetEvents (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptorCollection GetEvents (Type componentType, Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static EventDescriptorCollection GetEvents (object component,
							   Attribute [] attributes,
							   bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	public static PropertyDescriptorCollection GetProperties (object component)
	{
		return GetProperties (component, false);
	}

	public static PropertyDescriptorCollection GetProperties (Type componentType)
	{
		PropertyInfo [] props = componentType.GetProperties ();
		DerivedPropertyDescriptor [] propsDescriptor = new DerivedPropertyDescriptor [props.Length];
		int i = 0;
		foreach (PropertyInfo prop in props) {
			DerivedPropertyDescriptor propDescriptor = new DerivedPropertyDescriptor (prop.Name,
												  null, 0);
			propDescriptor.SetReadOnly (!prop.CanWrite);
			propDescriptor.SetComponentType (componentType);
			propDescriptor.SetPropertyType (prop.PropertyType);
			propsDescriptor [i++] = propDescriptor;
		}
		
		return new PropertyDescriptorCollection (propsDescriptor);
	}

	[MonoTODO]
	public static PropertyDescriptorCollection GetProperties (object component, Attribute [] attributes)
	{
		Type type = component.GetType ();
		if (typeof (ICustomTypeDescriptor).IsAssignableFrom (type))
			return ((ICustomTypeDescriptor) component).GetProperties (attributes);

		throw new NotImplementedException ();
	}

	[MonoTODO("noCustomTypeDesc")]
	public static PropertyDescriptorCollection GetProperties (object component, bool noCustomTypeDesc)
	{
		Type type = component.GetType ();
		if (typeof (ICustomTypeDescriptor).IsAssignableFrom (type))
			return ((ICustomTypeDescriptor) component).GetProperties ();

		return GetProperties (type);
	}

	[MonoTODO]
	public static PropertyDescriptorCollection GetProperties (Type componentType,
								  Attribute [] attributes)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptorCollection GetProperties (Type componentType,
								  Attribute [] attributes,
								  bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static void Refresh (Assembly assembly)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static void Refresh (Module module)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static void Refresh (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static void Refresh (Type type)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static event RefreshEventHandler Refreshed;
}
}

