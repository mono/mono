//
// System.ComponentModel.TypeDescriptor
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections;
using System.Reflection;
using System.ComponentModel.Design;

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
	public static IDesigner CreateDesigner(IComponent component, Type designerBaseType)
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
		return GetAttributes (component, false);
	}

	[MonoTODO]
	public static AttributeCollection GetAttributes (object component, bool noCustomTypeDesc)
	{
		if (component == null)
		    return AttributeCollection.Empty;

		// FIXME: implementation correct?
		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
		    return ((ICustomTypeDescriptor) component).GetAttributes ();
		} else {
		    // FIXME: wrong implementation (we need to check the Attributes of the real instance?
		    // not of the type?
		    object [] atts = component.GetType ().GetCustomAttributes (false);
		    return new AttributeCollection ((Attribute []) atts);
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

		// FIXME: implementation correct?
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

		// FIXME: implementation correct?
		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
		    return ((ICustomTypeDescriptor) component).GetComponentName ();
		} else {
		    if (((IComponent) component).Site == null)
			return null;
		    else
			return ((IComponent) component).Site.Name;
		}
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
	public static EventDescriptor GetDefaultEvent (Type componentType)
	{
		throw new NotImplementedException ();
	}

	public static EventDescriptor GetDefaultEvent (object component)
	{
		return GetDefaultEvent (component, false);
	}

	[MonoTODO]
	public static EventDescriptor GetDefaultEvent (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static PropertyDescriptor GetDefaultProperty (Type componentType)
	{
		throw new NotImplementedException ();
	}

	public static PropertyDescriptor GetDefaultProperty (object component)
	{
		return GetDefaultProperty (component, false);
	}

	[MonoTODO]
	public static PropertyDescriptor GetDefaultProperty (object component, bool noCustomTypeDesc)
	{
		throw new NotImplementedException ();
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

	[MonoTODO]
	public static PropertyDescriptorCollection GetProperties (object component, Attribute [] attributes, bool noCustomTypeDesc)
	{
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
	public static void SortDescriptorArray(IList infos)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public static IComNativeDescriptorHandler ComNativeDescriptorHandler {
		get{ throw new NotImplementedException (); }
		set{ throw new NotImplementedException (); }
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

