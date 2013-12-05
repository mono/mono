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

#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace System.Reflection
{
#if NET_4_0
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_ParameterInfo))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
	class MonoParameterInfo : ParameterInfo {
#else
	public partial class ParameterInfo {
#endif

#if !FULL_AOT_RUNTIME
#if NET_4_0
		internal MonoParameterInfo (ParameterBuilder pb, Type type, MemberInfo member, int position) {
#else
		internal ParameterInfo (ParameterBuilder pb, Type type, MemberInfo member, int position) {
#endif
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
#endif

		/*FIXME this constructor looks very broken in the position parameter*/
#if NET_4_0
		internal MonoParameterInfo (ParameterInfo pinfo, Type type, MemberInfo member, int position) {
#else
		internal ParameterInfo (ParameterInfo pinfo, Type type, MemberInfo member, int position) {
#endif
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

#if NET_4_0
		internal MonoParameterInfo (ParameterInfo pinfo, MemberInfo member) {
#else
		internal ParameterInfo (ParameterInfo pinfo, MemberInfo member) {
#endif
			this.ClassImpl = pinfo.ParameterType;
			this.MemberImpl = member;
			this.NameImpl = pinfo.Name;
			this.PositionImpl = pinfo.Position;
			this.AttrsImpl = pinfo.Attributes;
			this.DefaultValueImpl = pinfo.GetDefaultValueImpl ();
			//this.parent = pinfo;
		}

		/* to build a ParameterInfo for the return type of a method */
#if NET_4_0
		internal MonoParameterInfo (Type type, MemberInfo member, MarshalAsAttribute marshalAs) {
#else
		internal ParameterInfo (Type type, MemberInfo member, MarshalAsAttribute marshalAs) {
#endif
			this.ClassImpl = type;
			this.MemberImpl = member;
			this.NameImpl = "";
			this.PositionImpl = -1;	// since parameter positions are zero-based, return type pos is -1
			this.AttrsImpl = ParameterAttributes.Retval;
			this.marshalAs = marshalAs;
		}

#if NET_4_0
		public override
#else
		public virtual
#endif
		object DefaultValue {
			get {
				if (ClassImpl == typeof (Decimal)) {
					/* default values for decimals are encoded using a custom attribute */
					DecimalConstantAttribute[] attrs = (DecimalConstantAttribute[])GetCustomAttributes (typeof (DecimalConstantAttribute), false);
					if (attrs.Length > 0)
						return attrs [0].Value;
				} else if (ClassImpl == typeof (DateTime)) {
					/* default values for DateTime are encoded using a custom attribute */
					DateTimeConstantAttribute[] attrs = (DateTimeConstantAttribute[])GetCustomAttributes (typeof (DateTimeConstantAttribute), false);
					if (attrs.Length > 0)
						return new DateTime (attrs [0].Ticks);
				}
				return DefaultValueImpl;
			}
		}

#if NET_4_0
		public override
#else
		public
#endif
		object RawDefaultValue {
			get {
				/*FIXME right now DefaultValue doesn't throw for reflection-only assemblies. Change this once the former is fixed.*/
				return DefaultValue;
			}
		}

		public
#if NET_4_0
		override
#endif
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
#if NET_4_0
		override
#else
		virtual
#endif
		object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public
#if NET_4_0
		override
#else
		virtual
#endif
		object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}


		public
#if NET_4_0
		override
#else
		virtual
#endif
		bool IsDefined( Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

#if NET_4_0
		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}
#endif


		public
#if NET_4_0
		override
#else
		virtual
#endif
		Type[] GetOptionalCustomModifiers () {
			Type[] types = GetTypeModifiers (true);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public
#if NET_4_0
		override
#else
		virtual
#endif
		Type[] GetRequiredCustomModifiers () {
			Type[] types = GetTypeModifiers (false);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}
	}
}
