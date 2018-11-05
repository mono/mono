// System.Reflection.ParameterInfo
//
// Authors:
//   Sean MacIsaac (macisaac@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) 2001 Ximian, Inc.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2013 Xamarin, Inc (http://www.xamarin.com)
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

#if MONO_FEATURE_SRE
using System.Reflection.Emit;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
	abstract class RuntimeParameterInfo : ParameterInfo
	{

	}

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_ParameterInfo))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
	class MonoParameterInfo : RuntimeParameterInfo {		
		internal MarshalAsAttribute marshalAs;

		internal static void FormatParameters (StringBuilder sb, ParameterInfo[] p, CallingConventions callingConvention, bool serialization)
		{
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					sb.Append (", ");

				Type t = p[i].ParameterType;

				string typeName = t.FormatTypeName (serialization);

				// Legacy: Why use "ByRef" for by ref parameters? What language is this?
				// VB uses "ByRef" but it should precede (not follow) the parameter name.
				// Why don't we just use "&"?
				if (t.IsByRef && !serialization) {
					sb.Append (typeName.TrimEnd (new char[] { '&' }));
					sb.Append (" ByRef");
				} else {
					sb.Append (typeName);
				}
			}

			if ((callingConvention & CallingConventions.VarArgs) != 0) {
				if (p.Length > 0)
					sb.Append (", ");
				sb.Append ("...");
			}
		}

#if MONO_FEATURE_SRE
		internal MonoParameterInfo (ParameterBuilder pb, Type type, MemberInfo member, int position) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			if (pb != null) {
				this.NameImpl = pb.Name;
				this.PositionImpl = pb.Position - 1;	// ParameterInfo.Position is zero-based
				this.AttrsImpl = (ParameterAttributes) pb.Attributes;
			} else {
				this.NameImpl = null;
				this.PositionImpl = position - 1;
				this.AttrsImpl = ParameterAttributes.None;
			}
		}

		internal static ParameterInfo New (ParameterBuilder pb, Type type, MemberInfo member, int position)
		{
			return new MonoParameterInfo (pb, type, member, position);
		}		
#endif

		/*FIXME this constructor looks very broken in the position parameter*/
		internal MonoParameterInfo (ParameterInfo pinfo, Type type, MemberInfo member, int position) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			if (pinfo != null) {
				this.NameImpl = pinfo.Name;
				this.PositionImpl = pinfo.Position - 1;	// ParameterInfo.Position is zero-based
				this.AttrsImpl = (ParameterAttributes) pinfo.Attributes;
			} else {
				this.NameImpl = null;
				this.PositionImpl = position - 1;
				this.AttrsImpl = ParameterAttributes.None;
			}
		}

		internal MonoParameterInfo (ParameterInfo pinfo, MemberInfo member) {
			this.ClassImpl = pinfo.ParameterType;
			this.MemberImpl = member;
			this.NameImpl = pinfo.Name;
			this.PositionImpl = pinfo.Position;
			this.AttrsImpl = pinfo.Attributes;
			this.DefaultValueImpl = GetDefaultValueImpl (pinfo);
		}

		/* to build a ParameterInfo for the return type of a method */
		internal MonoParameterInfo (Type type, MemberInfo member, MarshalAsAttribute marshalAs) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			this.NameImpl = null;
			this.PositionImpl = -1;	// since parameter positions are zero-based, return type pos is -1
			this.AttrsImpl = ParameterAttributes.Retval;
			this.marshalAs = marshalAs;
		}

		public override
		object DefaultValue {
			get {
				if (ClassImpl == typeof (Decimal) || ClassImpl == typeof (Decimal?)) {
					/* default values for decimals are encoded using a custom attribute */
					DecimalConstantAttribute[] attrs = (DecimalConstantAttribute[])GetCustomAttributes (typeof (DecimalConstantAttribute), false);
					if (attrs.Length > 0)
						return attrs [0].Value;
				} else if (ClassImpl == typeof (DateTime) || ClassImpl == typeof (DateTime?)) {
					/* default values for DateTime are encoded using a custom attribute */
					DateTimeConstantAttribute[] attrs = (DateTimeConstantAttribute[])GetCustomAttributes (typeof (DateTimeConstantAttribute), false);
					if (attrs.Length > 0)
						return attrs [0].Value;
				}
				return DefaultValueImpl;
			}
		}

		public override
		object RawDefaultValue {
			get {
				if (DefaultValue != null && DefaultValue.GetType ().IsEnum)
					return ((Enum)DefaultValue).GetValue ();
				/*FIXME right now DefaultValue doesn't throw for reflection-only assemblies. Change this once the former is fixed.*/
				return DefaultValue;
			}
		}

		public
		override
		int MetadataToken {
			get {
				if (MemberImpl is PropertyInfo) {
					PropertyInfo prop = (PropertyInfo)MemberImpl;
					MethodInfo mi = prop.GetGetMethod (true);
					if (mi == null)
						mi = prop.GetSetMethod (true);

					return mi.GetParametersInternal () [PositionImpl].MetadataToken;
				} else if (MemberImpl is MethodBase) {
					return GetMetadataToken ();
				}
				throw new ArgumentException ("Can't produce MetadataToken for member of type " + MemberImpl.GetType ());
			}
		}


		public
		override
		object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public
		override
		object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		internal object GetDefaultValueImpl (ParameterInfo pinfo)
		{
			FieldInfo field = typeof (ParameterInfo).GetField ("DefaultValueImpl", BindingFlags.Instance | BindingFlags.NonPublic);
			return field.GetValue (pinfo);
		}

		public
		override
		bool IsDefined( Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern int GetMetadataToken ();

		public override Type[] GetOptionalCustomModifiers () => GetCustomModifiers (true);

		internal object[] GetPseudoCustomAttributes () 
		{
			int count = 0;

			if (IsIn)
				count ++;
			if (IsOut)
				count ++;
			if (IsOptional)
				count ++;
			if (marshalAs != null)
				count ++;

			if (count == 0)
				return null;
			object[] attrs = new object [count];
			count = 0;

			if (IsIn)
				attrs [count ++] = new InAttribute ();
			if (IsOut)
				attrs [count ++] = new OutAttribute ();
			if (IsOptional)
				attrs [count ++] = new OptionalAttribute ();

			if (marshalAs != null)
				attrs [count ++] = marshalAs.Copy ();

			return attrs;
		}

		internal CustomAttributeData[] GetPseudoCustomAttributesData ()
		{
			int count = 0;

			if (IsIn)
				count++;
			if (IsOut)
				count++;
			if (IsOptional)
				count++;
			if (marshalAs != null)
				count++;

			if (count == 0)
				return null;
			CustomAttributeData[] attrsData = new CustomAttributeData [count];
			count = 0;

			if (IsIn)
				attrsData [count++] = new CustomAttributeData ((typeof (InAttribute)).GetConstructor (Type.EmptyTypes));
			if (IsOut)
				attrsData [count++] = new CustomAttributeData ((typeof (OutAttribute)).GetConstructor (Type.EmptyTypes));
			if (IsOptional)
				attrsData [count++] = new CustomAttributeData ((typeof (OptionalAttribute)).GetConstructor (Type.EmptyTypes));				
			if (marshalAs != null) {
				var ctorArgs = new CustomAttributeTypedArgument[] { new CustomAttributeTypedArgument (typeof (UnmanagedType), marshalAs.Value) };
				attrsData [count++] = new CustomAttributeData (
					(typeof (MarshalAsAttribute)).GetConstructor (new[] { typeof (UnmanagedType) }),
					ctorArgs,
					EmptyArray<CustomAttributeNamedArgument>.Value);//FIXME Get named params
			}

			return attrsData;
		}

		public override Type[] GetRequiredCustomModifiers () => GetCustomModifiers (false);

		public override bool HasDefaultValue {
			get { 
				object defaultValue = DefaultValue;
				if (defaultValue == null)
					return true;

				if (defaultValue.GetType () == typeof(DBNull) || defaultValue.GetType () == typeof(Missing))
					return false;

				return true;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Type[] GetTypeModifiers (bool optional);		

		internal static ParameterInfo New (ParameterInfo pinfo, Type type, MemberInfo member, int position)
		{
			return new MonoParameterInfo (pinfo, type, member, position);
		}

		internal static ParameterInfo New (ParameterInfo pinfo, MemberInfo member)
		{
			return new MonoParameterInfo (pinfo, member);
		}

		internal static ParameterInfo New (Type type, MemberInfo member, MarshalAsAttribute marshalAs)
		{
			return new MonoParameterInfo (type, member, marshalAs);
		}

		private Type[] GetCustomModifiers (bool optional) => GetTypeModifiers (optional) ?? Type.EmptyTypes;
	}
}
