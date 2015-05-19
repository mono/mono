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
using System.Runtime.Serialization;

namespace System.Reflection
{
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_ParameterInfo))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
	public partial class ParameterInfo : ICustomAttributeProvider

#if !MOBILE
	, _ParameterInfo
#endif

	, IObjectReference
 	{
		protected Type ClassImpl;
		protected object DefaultValueImpl;
		protected MemberInfo MemberImpl;
		protected string NameImpl;
		protected int PositionImpl;
		protected ParameterAttributes AttrsImpl;
		internal MarshalAsAttribute marshalAs;

		protected ParameterInfo () {
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

		public virtual Type ParameterType {
			get {return ClassImpl;}
		}
		public virtual ParameterAttributes Attributes {
			get {return AttrsImpl;}
		}

		public bool IsIn {
			get {
				return (Attributes & ParameterAttributes.In) != 0;
			}
		}
#if FEATURE_USE_LCID
		public bool IsLcid {
			get {
				return (Attributes & ParameterAttributes.Lcid) != 0;
			}
		}
#endif
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
		internal extern int GetMetadataToken ();

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
		internal extern Type[] GetTypeModifiers (bool optional);

		internal object GetDefaultValueImpl ()
		{
			return DefaultValueImpl;
		}

		public virtual IEnumerable<CustomAttributeData> CustomAttributes {
			get { return GetCustomAttributesData (); }
		}
		
		public virtual bool HasDefaultValue {
			get { throw new NotImplementedException (); }
		}

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

		public virtual object DefaultValue {
			get { throw new NotImplementedException (); }
		}

		public virtual object RawDefaultValue {
			get { throw new NotImplementedException (); }
		}

		public virtual int MetadataToken {
			get { return 0x8000000; }
		}

		public virtual object[] GetCustomAttributes (bool inherit)
		{
			return new object [0];
		}

		public virtual object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return new object [0];
		}

		public object GetRealObject (StreamingContext context)
		{
			throw new NotImplementedException ();
		}		

		public virtual bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}

		public virtual Type[] GetRequiredCustomModifiers () {
			return new Type [0];
		}

		public virtual Type[] GetOptionalCustomModifiers () {
			return new Type [0];
		}

		public virtual IList<CustomAttributeData> GetCustomAttributesData () {
			throw new NotImplementedException ();
		}

#if !FULL_AOT_RUNTIME
		internal static ParameterInfo New (ParameterBuilder pb, Type type, MemberInfo member, int position)
		{
			return new MonoParameterInfo (pb, type, member, position);
		}
#endif

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
	}
}
