//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml
{
	static class TypeExtensionMethods
	{
		#region inheritance search and custom attribute provision

		public static T GetCustomAttribute<T> (this ICustomAttributeProvider type, bool inherit) where T : Attribute
		{
			foreach (var a in type.GetCustomAttributes (typeof (T), inherit))
				return (T) (object) a;
			return null;
		}

		public static T GetCustomAttribute<T> (this XamlType type) where T : Attribute
		{
			if (type.UnderlyingType == null)
				return null;

			T ret = type.GetCustomAttributeProvider ().GetCustomAttribute<T> (true);
			if (ret != null)
				return ret;
			if (type.BaseType != null)
				return type.BaseType.GetCustomAttribute<T> ();
			return null;
		}

		public static bool ImplementsAnyInterfacesOf (this Type type, params Type [] definitions)
		{
			return definitions.Any (t => ImplementsInterface (type, t));
		}

		public static bool ImplementsInterface (this Type type, Type definition)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (definition == null)
				throw new ArgumentNullException ("definition");
			if (type == definition)
				return true;

			foreach (var iface in type.GetInterfaces ())
				if (iface == definition || (iface.IsGenericType && iface.GetGenericTypeDefinition () == definition))
					return true;
			return false;
		}
		
		#endregion
		
		#region type conversion and member value retrieval
		
		static readonly NullExtension null_value = new NullExtension ();

		public static object GetExtensionWrapped (object o)
		{
			// FIXME: should this manually checked, or is there any way to automate it?
			// Also XamlSchemaContext might be involved but this method signature does not take it consideration.
			if (o == null)
				return null_value;
			if (o is Array)
				return new ArrayExtension ((Array) o);
			if (o is Type)
				return new TypeExtension ((Type) o);
			return o;
		}
		
		public static string GetStringValue (XamlType xt, XamlMember xm, object obj, IValueSerializerContext vsctx)
		{
			if (obj == null)
				return String.Empty;
			if (obj is Type)
				return new XamlTypeName (xt.SchemaContext.GetXamlType ((Type) obj)).ToString (vsctx != null ? vsctx.GetService (typeof (INamespacePrefixLookup)) as INamespacePrefixLookup : null);

			var vs = (xm != null ? xm.ValueSerializer : null) ?? xt.ValueSerializer;
			if (vs != null)
				return vs.ConverterInstance.ConvertToString (obj, vsctx);

			// FIXME: does this make sense?
			var vc = (xm != null ? xm.TypeConverter : null) ?? xt.TypeConverter;
			var tc = vc != null ? vc.ConverterInstance : null;
			if (tc != null && typeof (string) != null && tc.CanConvertTo (vsctx, typeof (string)))
				return (string) tc.ConvertTo (vsctx, CultureInfo.InvariantCulture, obj, typeof (string));
			if (obj is string || obj == null)
				return (string) obj;
			throw new InvalidCastException (String.Format ("Cannot cast object '{0}' to string", obj.GetType ()));
		}
		
		public static TypeConverter GetTypeConverter (this Type type)
		{
#if MOONLIGHT
			if (typeof (IConvertible).IsAssignableFrom (type))
				return (TypeConverter) Activator.CreateInstance (typeof (ConvertibleTypeConverter<>).MakeGenericType (new Type [] {type}));
			var name = type.GetCustomAttribute<TypeConverterAttribute> (true).ConverterTypeName;
			return (TypeConverter) Activator.CreateInstance (type.Assembly.GetType (name) ?? Type.GetType (name));
#else
			return TypeDescriptor.GetConverter (type);
#endif
		}
		
		// FIXME: I want this to cover all the existing types and make it valid in both NET_2_1 and !NET_2_1.
		class ConvertibleTypeConverter<T> : TypeConverter
		{
			Type type;
			public ConvertibleTypeConverter ()
			{
				this.type = typeof (T);
			}
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof (string);
			}
			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				return destinationType == typeof (string);
			}
			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (type == typeof (DateTime))
					return System.Xml.XmlConvert.ToDateTime ((string) value, System.Xml.XmlDateTimeSerializationMode.Unspecified);
				return ((IConvertible) value).ToType (type, CultureInfo.InvariantCulture);
			}
			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (value is DateTime)
					return System.Xml.XmlConvert.ToString ((DateTime) value);
				return ((IConvertible) value).ToType (destinationType, CultureInfo.InvariantCulture);
			}
		}

		#endregion

		public static bool IsContentValue (this XamlMember member, IValueSerializerContext vsctx)
		{
			if (member == XamlLanguage.Initialization)
				return true;
			if (member == XamlLanguage.PositionalParameters || member == XamlLanguage.Arguments)
				return false; // it's up to the argument (no need to check them though, as IList<object> is not of value)
			if (member.TypeConverter != null && member.TypeConverter.ConverterInstance != null && member.TypeConverter.ConverterInstance.CanConvertTo (vsctx, typeof (string)))
				return true;
			return IsContentValue (member.Type,vsctx);
		}

		public static bool IsContentValue (this XamlType type, IValueSerializerContext vsctx)
		{
			if (type.TypeConverter != null && type.TypeConverter.ConverterInstance != null && type.TypeConverter.ConverterInstance.CanConvertTo (vsctx, typeof (string)))
				return true;
			return false;
		}

		public static bool ListEquals (this IList<XamlType> a1, IList<XamlType> a2)
		{
			if (a1 == null || a1.Count == 0)
				return a2 == null || a2.Count == 0;
			if (a2 == null || a2.Count == 0)
				return false;
			if (a1.Count != a2.Count)
				return false;
			for (int i = 0; i < a1.Count; i++)
				if (a1 [i] != a2 [i])
					return false;
			return true;
		}

		public static bool HasPositionalParameters (this XamlType type, IValueSerializerContext vsctx)
		{
			// FIXME: find out why only TypeExtension and StaticExtension yield this directive. Seealso XamlObjectReaderTest.Read_CustomMarkupExtension*()
			return  type == XamlLanguage.Type ||
				type == XamlLanguage.Static ||
				ExaminePositionalParametersApplicable (type, vsctx) && type.ConstructionRequiresArguments;
		}
		
		static bool ExaminePositionalParametersApplicable (this XamlType type, IValueSerializerContext vsctx)
		{
			if (!type.IsMarkupExtension || type.UnderlyingType == null)
				return false;

			var args = type.GetSortedConstructorArguments ();
			if (args == null)
				return false;

			foreach (var arg in args)
				if (arg.Type != null && !arg.Type.IsContentValue (vsctx))
					return false;

			Type [] argTypes = (from arg in args select arg.Type.UnderlyingType).ToArray ();
			if (argTypes.Any (at => at == null))
				return false;
			var ci = type.UnderlyingType.GetConstructor (argTypes);
			return ci != null;
		}
		
		public static IEnumerable<XamlMember> GetConstructorArguments (this XamlType type)
		{
			return type.GetAllMembers ().Where (m => m.UnderlyingMember != null && m.GetCustomAttributeProvider ().GetCustomAttribute<ConstructorArgumentAttribute> (false) != null);
		}

		public static IEnumerable<XamlMember> GetSortedConstructorArguments (this XamlType type)
		{
			var args = type.GetConstructorArguments ().ToArray ();
			foreach (var ci in type.UnderlyingType.GetConstructors ().Where (c => c.GetParameters ().Length == args.Length)) {
				var pis = ci.GetParameters ();
				if (args.Length != pis.Length)
					continue;
				bool mismatch = false;
				foreach (var pi in pis)
				for (int i = 0; i < args.Length; i++)
					if (!args.Any (a => a.ConstructorArgumentName () == pi.Name))
						mismatch = true;
				if (mismatch)
					continue;
				return args.OrderBy (c => pis.FindParameterWithName (c.ConstructorArgumentName ()).Position);
			}
			return null;
		}

		static ParameterInfo FindParameterWithName (this IEnumerable<ParameterInfo> pis, string name)
		{
			return pis.FirstOrDefault (pi => pi.Name == name);
		}

		public static string ConstructorArgumentName (this XamlMember xm)
		{
			var caa = xm.GetCustomAttributeProvider ().GetCustomAttribute<ConstructorArgumentAttribute> (false);
			return caa.ArgumentName;
		}
		

		internal static int CompareMembers (XamlMember m1, XamlMember m2)
		{
			// ConstructorArguments and PositionalParameters go first.
			if (m1 == XamlLanguage.PositionalParameters)
				return -1;
			if (m2 == XamlLanguage.PositionalParameters)
				return 1;
			if (m1.IsConstructorArgument ()) {
				if (!m2.IsConstructorArgument ())
					return -1;
			}
			else if (m2.IsConstructorArgument ())
				return 1;

			// ContentProperty is returned at last.
			if (m1.DeclaringType != null && m1.DeclaringType.ContentProperty == m1)
				return 1;
			if (m2.DeclaringType != null && m2.DeclaringType.ContentProperty == m2)
				return -1;

			// then, compare names.
			return String.CompareOrdinal (m1.Name, m2.Name);
		}

		internal static bool IsConstructorArgument (this XamlMember xm)
		{
			var ap = xm.GetCustomAttributeProvider ();
			return ap != null && ap.GetCustomAttributes (typeof (ConstructorArgumentAttribute), false).Length > 0;
		}

		internal static string GetInternalXmlName (this XamlMember xm)
		{
			return xm.IsAttachable ? String.Concat (xm.DeclaringType.GetInternalXmlName (), ".", xm.Name) : xm.Name;
		}

#if DOTNET
		internal static ICustomAttributeProvider GetCustomAttributeProvider (this XamlType type)
		{
			return type.UnderlyingType;
		}
		
		internal static ICustomAttributeProvider GetCustomAttributeProvider (this XamlMember member)
		{
			return member.UnderlyingMember;
		}
#endif
	}
}
