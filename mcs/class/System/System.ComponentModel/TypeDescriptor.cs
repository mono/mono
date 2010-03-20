//
// System.ComponentModel.TypeDescriptor.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ivan N. Zlatev (contact@i-nz.net)
// 
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
// (C) 2008 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Reflection;
using System.Globalization;
using System.ComponentModel.Design;
using System.Security.Permissions;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.ComponentModel
{

public sealed class TypeDescriptor
{
	private static readonly object creatingDefaultConverters = new object ();
	private static ArrayList defaultConverters;
	private static IComNativeDescriptorHandler descriptorHandler;
	private static Hashtable componentTable = new Hashtable ();
	private static Hashtable typeTable = new Hashtable ();
	private static Hashtable editors;

#if NET_2_0
	static object typeDescriptionProvidersLock = new object ();
	static Dictionary <Type, LinkedList <TypeDescriptionProvider>> typeDescriptionProviders;

	static object componentDescriptionProvidersLock = new object ();
	static Dictionary <WeakObjectWrapper, LinkedList <TypeDescriptionProvider>> componentDescriptionProviders;

	static TypeDescriptor ()
	{
		typeDescriptionProviders = new Dictionary <Type, LinkedList <TypeDescriptionProvider>> ();
		componentDescriptionProviders = new Dictionary <WeakObjectWrapper, LinkedList <TypeDescriptionProvider>> (new WeakObjectWrapperComparer ());
	}
#endif
	private TypeDescriptor ()
	{
	}

#if NET_2_0
	[MonoNotSupported ("Mono does not support COM")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static Type ComObjectType {
		get { throw new NotImplementedException (); }
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static TypeDescriptionProvider AddAttributes (object instance, params Attribute [] attributes)
	{
		if (instance == null)
			throw new ArgumentNullException ("instance");
		if (attributes == null)
			throw new ArgumentNullException ("attributes");

		var ret = new AttributeProvider (attributes, GetProvider (instance));
		AddProvider (ret, instance);

		return ret;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static TypeDescriptionProvider AddAttributes (Type type, params Attribute [] attributes)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		if (attributes == null)
			throw new ArgumentNullException ("attributes");

		var ret = new AttributeProvider (attributes, GetProvider (type));
		AddProvider (ret, type);
		
		return ret;
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void AddProvider (TypeDescriptionProvider provider, object instance)
	{
		if (provider == null)
			throw new ArgumentNullException ("provider");
		if (instance == null)
			throw new ArgumentNullException ("instance");

		lock (componentDescriptionProvidersLock) {
			LinkedList <TypeDescriptionProvider> plist;
			WeakObjectWrapper instanceWrapper = new WeakObjectWrapper (instance);
			
			if (!componentDescriptionProviders.TryGetValue (instanceWrapper, out plist)) {
				plist = new LinkedList <TypeDescriptionProvider> ();
				componentDescriptionProviders.Add (new WeakObjectWrapper (instance), plist);
			}

			plist.AddLast (provider);
			instanceWrapper = null;
			Refresh (instance);
		}
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void AddProvider (TypeDescriptionProvider provider, Type type)
	{
		if (provider == null)
			throw new ArgumentNullException ("provider");
		if (type == null)
			throw new ArgumentNullException ("type");

		lock (typeDescriptionProvidersLock) {
			LinkedList <TypeDescriptionProvider> plist;

			if (!typeDescriptionProviders.TryGetValue (type, out plist)) {
				plist = new LinkedList <TypeDescriptionProvider> ();
				typeDescriptionProviders.Add (type, plist);
			}

			plist.AddLast (provider);
			Refresh (type);
		}
	}

	[MonoTODO]
	public static object CreateInstance (IServiceProvider provider, Type objectType, Type [] argTypes, object [] args)
	{
		if (objectType == null)
			throw new ArgumentNullException ("objectType");

		object instance = null;

		if (provider != null) {
			TypeDescriptionProvider typeDescrProvider = provider.GetService (typeof (TypeDescriptionProvider)) as TypeDescriptionProvider;
			if (typeDescrProvider != null)
				instance = typeDescrProvider.CreateInstance (provider, objectType, argTypes, args);
		}

		// TODO: also search and use the internal providers table once Add/RemoveProvider have been implemented

		if (instance == null)
			instance = Activator.CreateInstance (objectType, args);

		return instance;
	}
#endif

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
#if !NET_2_1
	public
#else
	internal
#endif
	static void AddEditorTable (Type editorBaseType, Hashtable table)
	{
		if (editorBaseType == null)
			throw new ArgumentNullException ("editorBaseType");

		if (editors == null)
			editors = new Hashtable ();

		if (!editors.ContainsKey (editorBaseType))
			editors [editorBaseType] = table;
	}

	public static IDesigner CreateDesigner(IComponent component, Type designerBaseType)
	{
		string tn = designerBaseType.AssemblyQualifiedName;
		AttributeCollection col = GetAttributes (component);
		
		foreach (Attribute at in col) {
			DesignerAttribute dat = at as DesignerAttribute;
			if (dat != null && tn == dat.DesignerBaseTypeName) {
				Type designerType = GetTypeFromName (component, dat.DesignerTypeName);
				if (designerType != null)
					return (IDesigner) Activator.CreateInstance (designerType);
			}
		}
				
		return null;
	}

	[ReflectionPermission (SecurityAction.LinkDemand, TypeInformation = true, MemberAccess = true)]
	public static EventDescriptor CreateEvent (Type componentType,
						   string name,
						   Type type,
						   params Attribute [] attributes)
	{
		return new ReflectionEventDescriptor (componentType, name, type, attributes);
	}

	[ReflectionPermission (SecurityAction.LinkDemand, TypeInformation = true, MemberAccess = true)]
	public static EventDescriptor CreateEvent (Type componentType,
						   EventDescriptor oldEventDescriptor,
						   params Attribute [] attributes)
	{
		return new ReflectionEventDescriptor (componentType, oldEventDescriptor, attributes);
	}

	[ReflectionPermission (SecurityAction.LinkDemand, TypeInformation = true, MemberAccess = true)]
	public static PropertyDescriptor CreateProperty (Type componentType,
							 string name,
							 Type type,
							 params Attribute [] attributes)
	{
		return new ReflectionPropertyDescriptor (componentType, name, type, attributes);
	}

	[ReflectionPermission (SecurityAction.LinkDemand, TypeInformation = true, MemberAccess = true)]
	public static PropertyDescriptor CreateProperty (Type componentType,
							 PropertyDescriptor oldPropertyDescriptor,
							 params Attribute [] attributes)
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

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static AttributeCollection GetAttributes (object component, bool noCustomTypeDesc)
	{
		if (component == null)
			return AttributeCollection.Empty;

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
			return ((ICustomTypeDescriptor) component).GetAttributes ();
		} else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
				return GetComponentInfo (com).GetAttributes ();
			else
				return GetTypeInfo (component.GetType()).GetAttributes ();
		}
	}

	public static string GetClassName (object component)
	{
		return GetClassName (component, false);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static string GetClassName (object component, bool noCustomTypeDesc)
	{
		if (component == null)
			throw new ArgumentNullException ("component", "component cannot be null");

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
			String res = ((ICustomTypeDescriptor) component).GetClassName ();
			if (res == null)
				res = ((ICustomTypeDescriptor) component).GetComponentName ();
			if (res == null)
				res = component.GetType ().FullName;
			return res;
		} else {
			return component.GetType ().FullName;
		}
	}

	public static string GetComponentName (object component)
	{
		return GetComponentName (component, false);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
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
#if NET_2_0
			return null;
#else
			return component.GetType().Name;
#endif
		}
	}

#if NET_2_0
	[MonoNotSupported("")]
	public static string GetFullComponentName (object component)
	{
		throw new NotImplementedException ();
	}

	[MonoNotSupported("")]
	public static string GetClassName (Type componentType)
	{
		throw new NotImplementedException ();
	}
#endif

	public static TypeConverter GetConverter (object component)
	{
		return GetConverter (component, false);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static TypeConverter GetConverter (object component, bool noCustomTypeDesc)
	{
		if (component == null)
			throw new ArgumentNullException ("component", "component cannot be null");

		if (noCustomTypeDesc == false && component is ICustomTypeDescriptor) {
			return ((ICustomTypeDescriptor) component).GetConverter ();
		} 
		else {
			Type converterType = null;
			AttributeCollection atts = GetAttributes (component, false);
			TypeConverterAttribute tca = (TypeConverterAttribute) atts[typeof(TypeConverterAttribute)];
			if (tca != null && tca.ConverterTypeName.Length > 0)
				converterType = GetTypeFromName (component as IComponent, tca.ConverterTypeName);
			if (converterType == null)
				converterType = FindDefaultConverterType (component.GetType ());
			
			if (converterType != null) {
				ConstructorInfo ci = converterType.GetConstructor (new Type[] { typeof(Type) });
				if (ci != null)
					return (TypeConverter) ci.Invoke (new object[] { component.GetType () });
				else
					return (TypeConverter) Activator.CreateInstance (converterType);
			} else
				return null;
		}
	}

	private static ArrayList DefaultConverters
	{
		get {
			lock (creatingDefaultConverters) {
				if (defaultConverters != null)
					return defaultConverters;
				
				defaultConverters = new ArrayList ();
				defaultConverters.Add (new DictionaryEntry (typeof (bool), typeof (BooleanConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (byte), typeof (ByteConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (sbyte), typeof (SByteConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (string), typeof (StringConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (char), typeof (CharConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (short), typeof (Int16Converter)));
				defaultConverters.Add (new DictionaryEntry (typeof (int), typeof (Int32Converter)));
				defaultConverters.Add (new DictionaryEntry (typeof (long), typeof (Int64Converter)));
				defaultConverters.Add (new DictionaryEntry (typeof (ushort), typeof (UInt16Converter)));
				defaultConverters.Add (new DictionaryEntry (typeof (uint), typeof (UInt32Converter)));
				defaultConverters.Add (new DictionaryEntry (typeof (ulong), typeof (UInt64Converter)));
				defaultConverters.Add (new DictionaryEntry (typeof (float), typeof (SingleConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (double), typeof (DoubleConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (decimal), typeof (DecimalConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (void), typeof (TypeConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (Array), typeof (ArrayConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (CultureInfo), typeof (CultureInfoConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (DateTime), typeof (DateTimeConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (Guid), typeof (GuidConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (TimeSpan), typeof (TimeSpanConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (ICollection), typeof (CollectionConverter)));
				defaultConverters.Add (new DictionaryEntry (typeof (Enum), typeof (EnumConverter)));
			}
			return defaultConverters;
		}
	}
	
	public static TypeConverter GetConverter (Type type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		
		Type converterType = null;
		AttributeCollection atts = GetAttributes (type);
		TypeConverterAttribute tca = (TypeConverterAttribute) atts[typeof(TypeConverterAttribute)];
		if (tca != null && tca.ConverterTypeName.Length > 0)
			converterType = GetTypeFromName (null, tca.ConverterTypeName);
		if (converterType == null)
			converterType = FindDefaultConverterType (type);

		if (converterType != null) {
			ConstructorInfo ci = converterType.GetConstructor (new Type[] { typeof(Type) });
			if (ci != null)
				return (TypeConverter) ci.Invoke (new object[] { type });
			else
				return (TypeConverter) Activator.CreateInstance (converterType);
		}
		else
			return null;
	}

	private static Type FindDefaultConverterType (Type type)
	{
		Type converterType = null;
		if (type != null) {
#if NET_2_0
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return typeof(NullableConverter);
#endif
			// Is there a default converter
			foreach (DictionaryEntry entry in DefaultConverters) {
				if ((Type)entry.Key == type)
					return (Type)entry.Value;
			}
		}
		
		// Find default converter with a baseType this baseType is assignable to
		Type baseType = type;
		while (baseType != null && baseType != typeof (object)) {
			foreach (DictionaryEntry entry in DefaultConverters) {
				Type defType = (Type)entry.Key;
				if (defType.IsAssignableFrom (baseType)) {
					converterType = (Type)entry.Value;
					break;
				}
			}
			baseType = baseType.BaseType;
		}

		if (converterType == null) {
			if (type != null && type.IsInterface)
				converterType = typeof (ReferenceConverter);
			else
				converterType = typeof (TypeConverter);
		}

		return converterType;
	}

	public static EventDescriptor GetDefaultEvent (Type componentType)
	{
		return GetTypeInfo (componentType).GetDefaultEvent ();
	}

	public static EventDescriptor GetDefaultEvent (object component)
	{
		return GetDefaultEvent (component, false);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static EventDescriptor GetDefaultEvent (object component, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetDefaultEvent ();
		else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
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

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static PropertyDescriptor GetDefaultProperty (object component, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetDefaultProperty ();
		else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
				return GetComponentInfo (com).GetDefaultProperty ();
			else
				return GetTypeInfo (component.GetType()).GetDefaultProperty ();
		}
	}
		
	internal static object CreateEditor (Type t, Type componentType)
	{
		if (t == null) 
			return null;

		try {
			return Activator.CreateInstance (t);
		} catch {}

		try {
			return Activator.CreateInstance (t, new object [] {componentType});
		} catch {}

		return null;
	}
		
	private static object FindEditorInTable (Type componentType, Type editorBaseType, Hashtable table)
	{
		object editorReference = null;
		object editor = null;
		
		if (componentType == null || editorBaseType == null || table == null)
			return null;
			
		Type ct = componentType;
		while (ct != null) {						
			editorReference = table [ct];
			if (editorReference != null)
				break;			
			ct = ct.BaseType;
		}
		
		if (editorReference == null) {
			foreach (Type iface in componentType.GetInterfaces ()) {
				editorReference = table [iface];
				if (editorReference != null) 
					break;
			}
		}
				
		if (editorReference == null)
			return null;
				
		if (editorReference is string)
			editor = CreateEditor (Type.GetType ((string) editorReference), componentType);
		else if (editorReference is Type)
			editor = CreateEditor ((Type) editorReference, componentType);
		else if (editorReference.GetType ().IsSubclassOf (editorBaseType))
			editor = editorReference;
		
		if (editor != null) 
			table [componentType] = editor;
		
		return editor;
	}

	public static object GetEditor (Type componentType, Type editorBaseType)
	{
		Type editorType = null;
		object editor = null;
		object [] atts = componentType.GetCustomAttributes (typeof(EditorAttribute), true);
		
		if (atts != null && atts.Length != 0) {
			foreach (EditorAttribute ea in atts) {
				editorType = GetTypeFromName (null, ea.EditorTypeName);
				if (editorType != null && editorType.IsSubclassOf(editorBaseType))
					break;
			}
		}
		
		if (editorType != null)
			editor = CreateEditor (editorType, componentType);
			
		if (editorType == null || editor == null) {
#if !TARGET_JVM
			// Make sure the editorBaseType's static constructor has been called,
			// since that's where we're putting the initialization of its editor table.
			
			System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor (editorBaseType.TypeHandle);
#endif				
			if (editors != null)
				editor = FindEditorInTable (componentType, editorBaseType, editors [editorBaseType] as Hashtable);
		}

		return editor;
	}

	public static object GetEditor (object component, Type editorBaseType)
	{
		return GetEditor (component, editorBaseType, false);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static object GetEditor (object component, Type editorBaseType, bool noCustomTypeDesc)
	{
		if (component == null)
			throw new ArgumentNullException ("component");
		if (editorBaseType == null)
			throw new ArgumentNullException ("editorBaseType");
		
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetEditor (editorBaseType);

		object [] atts = component.GetType ().GetCustomAttributes (typeof (EditorAttribute), true);
		if (atts.Length == 0)
			return null;
		string target = editorBaseType.AssemblyQualifiedName;
		
		foreach (EditorAttribute ea in atts){
			if (ea.EditorBaseTypeName == target){
				Type t = Type.GetType (ea.EditorTypeName, true);

				return Activator.CreateInstance (t);
			}
		}
		return null;
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

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static EventDescriptorCollection GetEvents (object component, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetEvents ();
		else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
				return GetComponentInfo (com).GetEvents ();
			else
				return GetTypeInfo (component.GetType()).GetEvents ();
		}
	}

	public static EventDescriptorCollection GetEvents (Type componentType, Attribute [] attributes)
	{
		return GetTypeInfo (componentType).GetEvents (attributes);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static EventDescriptorCollection GetEvents (object component, Attribute [] attributes, bool noCustomTypeDesc)
	{
		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetEvents (attributes);
		else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
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
		if (component == null)
			return PropertyDescriptorCollection.Empty;

		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetProperties (attributes);
		else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
				return GetComponentInfo (com).GetProperties (attributes);
			else
				return GetTypeInfo (component.GetType()).GetProperties (attributes);
		}
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
	public static PropertyDescriptorCollection GetProperties (object component, bool noCustomTypeDesc)
	{
		if (component == null)
			return PropertyDescriptorCollection.Empty;

		if (!noCustomTypeDesc && (component is ICustomTypeDescriptor))
			return ((ICustomTypeDescriptor) component).GetProperties ();
		else {
			IComponent com = component as IComponent;
			if (com != null && com.Site != null)
				return GetComponentInfo (com).GetProperties ();
			else
				return GetTypeInfo (component.GetType()).GetProperties ();
		}
	}

	public static PropertyDescriptorCollection GetProperties (Type componentType, Attribute [] attributes)
	{
		return GetTypeInfo (componentType).GetProperties (attributes);
	}

#if NET_2_0
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static TypeDescriptionProvider GetProvider (object instance)
	{
		if (instance == null)
			throw new ArgumentNullException ("instance");

		TypeDescriptionProvider ret = null;
		lock (componentDescriptionProvidersLock) {
			LinkedList <TypeDescriptionProvider> plist;
			WeakObjectWrapper instanceWrapper = new WeakObjectWrapper (instance);
			
			if (componentDescriptionProviders.TryGetValue (instanceWrapper, out plist) && plist.Count > 0)
				ret = plist.Last.Value;
			
			instanceWrapper = null;
		}

		if (ret == null)
			ret = GetProvider (instance.GetType ());

		if (ret == null)
			return new DefaultTypeDescriptionProvider ();
		else
			return new WrappedTypeDescriptionProvider (ret);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static TypeDescriptionProvider GetProvider (Type type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		
		TypeDescriptionProvider ret = null;
		lock (typeDescriptionProvidersLock) {
			LinkedList <TypeDescriptionProvider> plist;
			
			while (!typeDescriptionProviders.TryGetValue (type, out plist)) {
				plist = null;
				type = type.BaseType;
				if (type == null)
					break;
			}

			if (plist != null && plist.Count > 0)
				ret = plist.Last.Value;
		}

		if (ret == null)
			return new DefaultTypeDescriptionProvider ();
		else
			return new WrappedTypeDescriptionProvider (ret);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static Type GetReflectionType (object instance)
	{
		if (instance == null)
			throw new ArgumentNullException ("instance");
		
		return instance.GetType ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static Type GetReflectionType (Type type)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
		
		return type;
	}

	[MonoNotSupported("Associations not supported")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void CreateAssociation (object primary, object secondary)
	{
		throw new NotImplementedException ();
	}

	[MonoNotSupported ("Associations not supported")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static object GetAssociation (Type type, object primary)
	{
		throw new NotImplementedException ();
	}

	[MonoNotSupported ("Associations not supported")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void RemoveAssociation (object primary, object secondary)
	{
		throw new NotImplementedException ();
	}

	[MonoNotSupported ("Associations not supported")]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void RemoveAssociations (object primary)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void RemoveProvider (TypeDescriptionProvider provider, object instance)
	{
		if (provider == null)
			throw new ArgumentNullException ("provider");
		if (instance == null)
			throw new ArgumentNullException ("instance");

		//bool removed = false;
		lock (componentDescriptionProvidersLock) {
			LinkedList <TypeDescriptionProvider> plist;
			WeakObjectWrapper instanceWrapper = new WeakObjectWrapper (instance);

			if (componentDescriptionProviders.TryGetValue (instanceWrapper, out plist) && plist.Count > 0) {
				RemoveProvider (provider, plist);
				//removed = true;
			}
			
			instanceWrapper = null;
		}

		var refreshed = Refreshed;
		if (refreshed != null)
			refreshed (new RefreshEventArgs (instance));
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public static void RemoveProvider (TypeDescriptionProvider provider, Type type)
	{
		if (provider == null)
			throw new ArgumentNullException ("provider");
		if (type == null)
			throw new ArgumentNullException ("type");

		lock (typeDescriptionProvidersLock) {
			LinkedList <TypeDescriptionProvider> plist;

			if (typeDescriptionProviders.TryGetValue (type, out plist) && plist.Count > 0) {
				RemoveProvider (provider, plist);
			}
		}

		var refreshed = Refreshed;
		if (refreshed != null)
			refreshed (new RefreshEventArgs (type));
	}

	static void RemoveProvider (TypeDescriptionProvider provider, LinkedList <TypeDescriptionProvider> plist)
	{
		LinkedListNode <TypeDescriptionProvider> node = plist.Last;
		LinkedListNode <TypeDescriptionProvider> first = plist.First;
		TypeDescriptionProvider p;
				
		do {
			p = node.Value;
			if (p == provider) {
				plist.Remove (node);
				break;
			}
			if (node == first)
				break;
					
			node = node.Previous;
					
		} while (true);
	}
#endif

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

#if NET_2_0
	// well, ComObjectType is not implemented, but we don't support COM anyways ...
	[Obsolete ("Use ComObjectType")]
#endif
	public static IComNativeDescriptorHandler ComNativeDescriptorHandler {
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
		get { return descriptorHandler; }
		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
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
	
	private static Type GetTypeFromName (IComponent component, string typeName)
	{
		Type type = null;
		if (component != null && component.Site != null) {
			ITypeResolutionService resver = (ITypeResolutionService) component.Site.GetService (typeof(ITypeResolutionService));
			if (resver != null)
				type = resver.GetType (typeName);
		}
		if (type == null)
			type = Type.GetType (typeName);
		return type;
	}

#if NET_2_0
	sealed class AttributeProvider : TypeDescriptionProvider
	{
		Attribute[] attributes;
		
		public AttributeProvider (Attribute[] attributes, TypeDescriptionProvider parent)
			: base (parent)
		{
			this.attributes = attributes;
		}

		public override ICustomTypeDescriptor GetTypeDescriptor (Type type, object instance)
		{
			return new AttributeTypeDescriptor (base.GetTypeDescriptor (type, instance), attributes);
		}
		
		sealed class AttributeTypeDescriptor : CustomTypeDescriptor
		{
			Attribute[] attributes;
			
			public AttributeTypeDescriptor (ICustomTypeDescriptor parent, Attribute[] attributes)
				: base (parent)
			{
				this.attributes = attributes;
			}

			public override AttributeCollection GetAttributes ()
			{
				AttributeCollection attrs = base.GetAttributes ();

				if (attrs != null && attrs.Count > 0)
					return AttributeCollection.FromExisting (attrs, attributes);
				else
					return new AttributeCollection (attributes);
			}
		}
	}

	sealed class WrappedTypeDescriptionProvider : TypeDescriptionProvider
	{
		public TypeDescriptionProvider Wrapped { get; private set; }
		
		public WrappedTypeDescriptionProvider (TypeDescriptionProvider wrapped)
		{
			Wrapped = wrapped;
		}

		public override object CreateInstance (IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
		{
			TypeDescriptionProvider wrapped = Wrapped;

			if (wrapped == null)
				return base.CreateInstance (provider, objectType, argTypes, args);
			
			return wrapped.CreateInstance (provider, objectType, argTypes, args);
		}

		public override IDictionary GetCache (object instance)
		{
			TypeDescriptionProvider wrapped = Wrapped;

			if (wrapped == null)
				return base.GetCache (instance);

			return wrapped.GetCache (instance);
		}

		public override ICustomTypeDescriptor GetExtendedTypeDescriptor (object instance)
		{
			return new DefaultTypeDescriptor (this, null, instance);
		}

		public override string GetFullComponentName (object component)
		{
			TypeDescriptionProvider wrapped = Wrapped;

			if (wrapped == null)
				return base.GetFullComponentName (component);

			return wrapped.GetFullComponentName (component);
		}

		public override Type GetReflectionType (Type type, object instance)
		{
			TypeDescriptionProvider wrapped = Wrapped;

			if (wrapped == null)
				return base.GetReflectionType (type, instance);

			return wrapped.GetReflectionType (type, instance);
		}

		public override ICustomTypeDescriptor GetTypeDescriptor (Type objectType, object instance)
		{
			TypeDescriptionProvider wrapped = Wrapped;

			if (wrapped == null)
				return new DefaultTypeDescriptor (this, objectType, instance);

			return wrapped.GetTypeDescriptor (objectType, instance);
		}
	}

	// TODO: this needs more work
	sealed class DefaultTypeDescriptor : CustomTypeDescriptor
	{
		TypeDescriptionProvider owner;
		Type objectType;
		object instance;

		public DefaultTypeDescriptor (TypeDescriptionProvider owner, Type objectType, object instance)
		{
			this.owner = owner;
			this.objectType = objectType;
			this.instance = instance;
		}

		public override AttributeCollection GetAttributes ()
		{
			var wrapped = owner as WrappedTypeDescriptionProvider;

			if (wrapped != null)
				return wrapped.Wrapped.GetTypeDescriptor (objectType, instance).GetAttributes ();

			if (instance != null)
				return TypeDescriptor.GetAttributes (instance, false);

			if (objectType != null)
				return TypeDescriptor.GetTypeInfo (objectType).GetAttributes ();
			
			return base.GetAttributes ();
		}
		
		public override string GetClassName ()
		{
			var wrapped = owner as WrappedTypeDescriptionProvider;

			if (wrapped != null)
				return wrapped.Wrapped.GetTypeDescriptor (objectType, instance).GetClassName ();

			return base.GetClassName ();
		}

		public override PropertyDescriptor GetDefaultProperty ()
		{
			var wrapped = owner as WrappedTypeDescriptionProvider;

			if (wrapped != null)
				return wrapped.Wrapped.GetTypeDescriptor (objectType, instance).GetDefaultProperty ();

			PropertyDescriptor ret;
			if (objectType != null)
				ret = TypeDescriptor.GetTypeInfo (objectType).GetDefaultProperty ();
			else if (instance != null)
				ret = TypeDescriptor.GetTypeInfo (instance.GetType ()).GetDefaultProperty ();
			else
				ret = base.GetDefaultProperty ();

			return ret;
		}

		public override PropertyDescriptorCollection GetProperties ()
		{
			var wrapped = owner as WrappedTypeDescriptionProvider;

			if (wrapped != null)
				return wrapped.Wrapped.GetTypeDescriptor (objectType, instance).GetProperties ();

			if (instance != null)
				return TypeDescriptor.GetProperties (instance, null, false);

			if (objectType != null)
				return TypeDescriptor.GetTypeInfo (objectType).GetProperties (null);

			return base.GetProperties ();
		}		
	}

	sealed class DefaultTypeDescriptionProvider : TypeDescriptionProvider
	{
		public DefaultTypeDescriptionProvider ()
		{
		}

		public override ICustomTypeDescriptor GetExtendedTypeDescriptor (object instance)
		{
			return new DefaultTypeDescriptor (this, null, instance);
		}

		public override ICustomTypeDescriptor GetTypeDescriptor (Type objectType, object instance)
		{
			return new DefaultTypeDescriptor (this, objectType, instance);
		}
	}
#endif
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
			if (attributes == null)
				return evs;
			else
				return evs.Filter (attributes);
		}
		
		public PropertyDescriptorCollection GetProperties (Attribute[] attributes)
		{
			PropertyDescriptorCollection props = GetProperties ();
			if (attributes == null)
				return props;
			else
				return props.Filter (attributes);
		}
		
		public EventDescriptor GetDefaultEvent ()
		{
			if (_gotDefaultEvent)
				return _defaultEvent;
			
			DefaultEventAttribute attr = (DefaultEventAttribute) GetAttributes()[typeof(DefaultEventAttribute)];
			if (attr == null || attr.Name == null) 
				_defaultEvent = null;
			else {
				EventDescriptorCollection events = GetEvents ();
				_defaultEvent = events [attr.Name];
#if !NET_2_0
				// In our test case (TypeDescriptorTest.TestGetDefaultEvent), we have
				// a scenario where a custom filter adds the DefaultEventAttribute,
				// but its FilterEvents method removes the event the
				// DefaultEventAttribute applied to.  .NET 1.x accepts this and returns
				// the *other* event defined in the class.
				//
				// Consequently, we know we have a DefaultEvent, but we need to check
				// and ensure that the requested event is unfiltered.  If it is, just
				// grab the first element in the collection.
				if (_defaultEvent == null && events.Count > 0)
					_defaultEvent = events [0];
#endif
			}
			_gotDefaultEvent = true;
			return _defaultEvent;
		}
		
		public PropertyDescriptor GetDefaultProperty ()
		{
			if (_gotDefaultProperty)
				return _defaultProperty;
			
			DefaultPropertyAttribute attr = (DefaultPropertyAttribute) GetAttributes()[typeof(DefaultPropertyAttribute)];
			if (attr == null || attr.Name == null) 
				_defaultProperty = null;
			else {
				PropertyDescriptorCollection properties = GetProperties ();
				_defaultProperty = properties[attr.Name];
			}
			_gotDefaultProperty = true;
			return _defaultProperty;
		}
		
		protected AttributeCollection GetAttributes (IComponent comp)
		{
			if (_attributes != null)
				return _attributes;
			
			bool cache = true;
			ArrayList attributesList = new ArrayList ();

			// 1) Attributes of the type
			foreach (Attribute attribute in _infoType.GetCustomAttributes (false))
				attributesList.Add (attribute);

			// 2) Attributes of the base types
			Type baseType = _infoType.BaseType;
			while (baseType != null && baseType != typeof (object)) {
				foreach (Attribute attribute in baseType.GetCustomAttributes (false))
					attributesList.Add (attribute);
				baseType = baseType.BaseType;
			}

			// 3) Attributes of the type's implemented interfaces and their interfaces as well
			foreach (Type inface in _infoType.GetInterfaces ())
				foreach (Attribute attribute in TypeDescriptor.GetAttributes (inface))
					attributesList.Add (attribute);

			// Filter out duplicate attributes, so that the base types have higher precedence 
			// than the interfaces and the type higher than both.
			Hashtable attributesTable = new Hashtable ();
			for (int i = attributesList.Count - 1; i >= 0; i--) {
				Attribute attribute = (Attribute)attributesList[i];
				attributesTable[attribute.TypeId] = attribute;
			}

			if (comp != null && comp.Site != null) 
			{
				ITypeDescriptorFilterService filter = (ITypeDescriptorFilterService) comp.Site.GetService (typeof(ITypeDescriptorFilterService));
				if (filter != null)
					cache = filter.FilterAttributes (comp, attributesTable);
			}

			Attribute[] attributes = new Attribute[attributesTable.Values.Count];
			attributesTable.Values.CopyTo (attributes, 0);
			AttributeCollection attCol = new AttributeCollection (attributes);
			if (cache)
				_attributes = attCol;
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
			if (_events != null)
				return _events;
			
			bool cache = true;
			EventInfo[] events = _component.GetType().GetEvents ();
			Hashtable t = new Hashtable ();
			foreach (EventInfo ev in events)
				t [ev.Name] = new ReflectionEventDescriptor (ev);
					
			if (_component.Site != null) 
			{
				ITypeDescriptorFilterService filter = (ITypeDescriptorFilterService) _component.Site.GetService (typeof(ITypeDescriptorFilterService));
				if (filter != null)
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
			if (_properties != null)
				return _properties;
			
			bool cache = true;
			PropertyInfo[] props = _component.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public);
			Hashtable t = new Hashtable ();
			for (int i = props.Length-1; i >= 0; i--)
				t [props[i].Name] = new ReflectionPropertyDescriptor (props[i]);
					
			if (_component.Site != null) 
			{
				ITypeDescriptorFilterService filter = (ITypeDescriptorFilterService) _component.Site.GetService (typeof(ITypeDescriptorFilterService));
				if (filter != null)
					cache = filter.FilterProperties (_component, t);
			}

			PropertyDescriptor[] descriptors = new PropertyDescriptor[t.Values.Count];
			t.Values.CopyTo (descriptors, 0);
			PropertyDescriptorCollection attCol = new PropertyDescriptorCollection (descriptors, true);
			if (cache)
				_properties = attCol;
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
			if (_events != null)
				return _events;
			
			EventInfo[] events = InfoType.GetEvents ();
			EventDescriptor[] descs = new EventDescriptor [events.Length];
			for (int n=0; n<events.Length; n++)
				descs [n] = new ReflectionEventDescriptor (events[n]);

			_events = new EventDescriptorCollection (descs);
			return _events;
		}
		
		public override PropertyDescriptorCollection GetProperties ()
		{
			if (_properties != null)
				return _properties;

			Hashtable propertiesHash = new Hashtable (); // name - null
			ArrayList propertiesList = new ArrayList (); // propertydescriptors
			Type currentType = InfoType;
			// Getting properties type by type, because in the case of a property in the child type, where
			// the "new" keyword is used and also the return type is changed Type.GetProperties returns 
			// also the parent property. 
			// 
			// Note that we also have to preserve the properties order here.
			// 
			while (currentType != null && currentType != typeof (object)) {
				PropertyInfo[] props = currentType.GetProperties (BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
				foreach (PropertyInfo property in props) {
					if (property.GetIndexParameters ().Length == 0 &&
					    property.CanRead &&
					    !propertiesHash.ContainsKey (property.Name)) {
						propertiesList.Add (new ReflectionPropertyDescriptor (property));
						propertiesHash.Add (property.Name, null);
					}
				}
				currentType = currentType.BaseType;
			}

			_properties = new PropertyDescriptorCollection ((PropertyDescriptor[]) propertiesList.ToArray (typeof (PropertyDescriptor)), true);
			return _properties;
		}
	}
}
