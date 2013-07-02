// System.Reflection.ParameterInfo
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

namespace System.Reflection
{
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_ParameterInfo))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
#if MOBILE
	public class ParameterInfo : ICustomAttributeProvider {
#else
	public class ParameterInfo : ICustomAttributeProvider, _ParameterInfo {
#endif

		protected Type ClassImpl;
		protected object DefaultValueImpl;
		protected MemberInfo MemberImpl;
		protected string NameImpl;
		protected int PositionImpl;
		protected ParameterAttributes AttrsImpl;
		private MarshalAsAttribute marshalAs;

		protected ParameterInfo () {
		}

#if !FULL_AOT_RUNTIME
		internal ParameterInfo (ParameterBuilder pb, Type type, MemberInfo member, int position) {
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
		internal ParameterInfo (ParameterInfo pinfo, Type type, MemberInfo member, int position) {
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

		internal ParameterInfo (ParameterInfo pinfo, MemberInfo member) {
			this.ClassImpl = pinfo.ParameterType;
			this.MemberImpl = member;
			this.NameImpl = pinfo.Name;
			this.PositionImpl = pinfo.Position;
			this.AttrsImpl = pinfo.Attributes;
			//this.parent = pinfo;
		}

		/* to build a ParameterInfo for the return type of a method */
		internal ParameterInfo (Type type, MemberInfo member, MarshalAsAttribute marshalAs) {
			this.ClassImpl = type;
			this.MemberImpl = member;
			this.NameImpl = "";
			this.PositionImpl = -1;	// since parameter positions are zero-based, return type pos is -1
			this.AttrsImpl = ParameterAttributes.Retval;
			this.marshalAs = marshalAs;
		}

		public override string ToString() {
			Type elementType = ClassImpl;
			while (elementType.HasElementType) {
					elementType = elementType.GetElementType();
			}
			bool useShort = elementType.IsPrimitive || ClassImpl == typeof(void)
				|| ClassImpl.Namespace == MemberImpl.DeclaringType.Namespace;
			string result = useShort
				? ClassImpl.Name
				: ClassImpl.FullName;
			// MS.NET seems to skip this check and produce an extra space for return types
			if (!IsRetval) {
				result += ' ';
				result += NameImpl;
			}
			return result;
		}

		public virtual Type ParameterType {
			get {return ClassImpl;}
		}
		public virtual ParameterAttributes Attributes {
			get {return AttrsImpl;}
		}
		public virtual object DefaultValue {
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

		public bool IsIn {
			get {
				return (Attributes & ParameterAttributes.In) != 0;
			}
		}

		public bool IsLcid {
			get {
				return (Attributes & ParameterAttributes.Lcid) != 0;
			}
		}

		public bool IsOptional {
			get {
				return (Attributes & ParameterAttributes.Optional) != 0;
			}
		}

		public bool IsOut {
			get {
				return (Attributes & ParameterAttributes.Out) != 0;
			}
		}

		public bool IsRetval {
			get {
				return (Attributes & ParameterAttributes.Retval) != 0;
			}
		}

		public virtual MemberInfo Member {
			get {return MemberImpl;}
		}

		public virtual string Name {
			get {return NameImpl;}
		}

		public virtual int Position {
			get {return PositionImpl;}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern int GetMetadataToken ();

		public
#if NET_4_0
		virtual
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

		public virtual object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public virtual object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public virtual bool IsDefined( Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		internal object[] GetPseudoCustomAttributes () {
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
			if (IsOptional)
				attrs [count ++] = new OptionalAttribute ();
			if (IsOut)
				attrs [count ++] = new OutAttribute ();

			if (marshalAs != null)
				attrs [count ++] = marshalAs.Copy ();

			return attrs;
		}			

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern Type[] GetTypeModifiers (bool optional);

		public virtual Type[] GetOptionalCustomModifiers () {
			Type[] types = GetTypeModifiers (true);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public virtual Type[] GetRequiredCustomModifiers () {
			Type[] types = GetTypeModifiers (false);
			if (types == null)
				return Type.EmptyTypes;
			return types;
		}

		public virtual object RawDefaultValue {
			get {
				/*FIXME right now DefaultValue doesn't throw for reflection-only assemblies. Change this once the former is fixed.*/
				return DefaultValue;
			}
		}

#if NET_4_0
		public virtual IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}
#endif

#if NET_4_5
		public virtual IEnumerable<CustomAttributeData> CustomAttributes {
			get { return GetCustomAttributesData (); }
		}

		public virtual bool HasDefaultValue {
			get { throw new NotImplementedException (); }
		}
#endif

#if !MOBILE
		void _ParameterInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _ParameterInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ParameterInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _ParameterInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
#endif

	}
}
