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
	abstract class RuntimeParameterInfo : ParameterInfo
	{

	}

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_ParameterInfo))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
	class MonoParameterInfo : RuntimeParameterInfo {

#if !FULL_AOT_RUNTIME
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
			this.DefaultValueImpl = pinfo.GetDefaultValueImpl ();
			//this.parent = pinfo;
		}

		/* to build a ParameterInfo for the return type of a method */
		internal MonoParameterInfo (Type type, MemberInfo member, MarshalAsAttribute marshalAs) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			this.NameImpl = "";
			this.PositionImpl = -1;	// since parameter positions are zero-based, return type pos is -1
			this.AttrsImpl = ParameterAttributes.Retval;
			this.marshalAs = marshalAs;
		}

		public override
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
						return attrs [0].Value;
				}
				return DefaultValueImpl;
			}
		}

		public override
		object RawDefaultValue {
			get {
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


		public
		override
		bool IsDefined( Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}


		public
		override
		Type[] GetOptionalCustomModifiers () {
			Type[] types = GetTypeModifiers (true);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public
		override
		Type[] GetRequiredCustomModifiers () {
			Type[] types = GetTypeModifiers (false);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

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
	}
}
