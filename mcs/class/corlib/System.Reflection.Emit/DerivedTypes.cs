//
// System.Reflection.Emit.DerivedTypes.cs
//
// Authors:
// 	Rodrigo Kumpera <rkumpera@novell.com>
//
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;


namespace System.Reflection.Emit
{
	internal enum TypeKind : int {
		SZARRAY = 0x1d,
		ARRAY = 0x14
	}

	[StructLayout (LayoutKind.Sequential)]
	internal abstract class DerivedType : Type
	{
		internal Type elementType;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void create_unmanaged_type (Type type);

		internal DerivedType (Type elementType)
		{
			this.elementType = elementType;
		}

		internal abstract String FormatName (string elementName);

		public override Type GetInterface (string name, bool ignoreCase)
		{
			throw new NotSupportedException ();
		}

		public override Type[] GetInterfaces ()
		{
			throw new NotSupportedException ();
		}

		public override Type GetElementType ()
		{
			return elementType;
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                             CallingConventions callConvention, Type[] types,
		                                             ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                                 Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}


		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			/*LAMEIMPL MS just return the elementType.Attributes*/
			return elementType.Attributes; 
		}

		protected override bool HasElementTypeImpl ()
		{
			return true;
		}

		protected override bool IsArrayImpl ()
		{
			return false;
		}

		protected override bool IsByRefImpl ()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl ()
		{
			return false;
		}

		protected override bool IsPointerImpl ()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl ()
		{
			return false;
		}


		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			throw new NotSupportedException ();
		}

		public override InterfaceMapping GetInterfaceMap (Type interfaceType)
		{
			throw new NotSupportedException ();
		}

		public override bool IsInstanceOfType (object o)
		{
			return false;
		}

		public override bool IsAssignableFrom (Type c)
		{
			return false;
		}

		public override bool ContainsGenericParameters {
			get { return elementType.ContainsGenericParameters; }
		}

		//FIXME this should be handled by System.Type
		public override Type MakeGenericType (params Type[] typeArguments)
		{
			throw new NotSupportedException ();
		}

		public override Type MakeArrayType ()
		{
			return new ArrayType (this, 0);
		}

		public override Type MakeArrayType (int rank)
		{
			if (rank < 1)
				throw new IndexOutOfRangeException ();
			return new ArrayType (this, rank);
		}

		public override Type MakeByRefType ()
		{
			return new ByRefType (this);
		}

		public override Type MakePointerType ()
		{
			return new PointerType (this);
		}

		public override string ToString ()
		{
			return FormatName (elementType.ToString ());
		}

		public override GenericParameterAttributes GenericParameterAttributes {
			get { throw new NotSupportedException (); }
		}

		public override StructLayoutAttribute StructLayoutAttribute {
			get { throw new NotSupportedException (); }
		}

		public override Assembly Assembly {
			get { return elementType.Assembly; }
		}

		public override string AssemblyQualifiedName {
			get {
				string fullName = FormatName (elementType.FullName);
				if (fullName == null)
					return null;
				return fullName + ", " + elementType.Assembly.FullName;
			}
		}


		public override string FullName {
			get {
				return FormatName (elementType.FullName);
			}
		}

		public override string Name {
			get {
				return FormatName (elementType.Name);
			}
		}

		public override Guid GUID {
			get { throw new NotSupportedException (); }
		}

		public override Module Module {
			get { return elementType.Module; }
		}
	
		public override string Namespace {
			get { return elementType.Namespace; }
		}

		public override RuntimeTypeHandle TypeHandle {
			get { throw new NotSupportedException (); }
		}

		public override Type UnderlyingSystemType {
			get {
				create_unmanaged_type (this);
				return this;
			}
		}

		//MemberInfo
		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			throw new NotSupportedException ();
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotSupportedException ();
		}

		internal override bool IsUserType {
			get {
				return elementType.IsUserType;
			}
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal class ArrayType : DerivedType
	{
		int rank;

		internal ArrayType (Type elementType, int rank) : base (elementType)
		{
			this.rank = rank;
		}

		internal int GetEffectiveRank ()
		{
			return rank;
		}

		internal override Type InternalResolve ()
		{
			Type et = elementType.InternalResolve (); 
			if (rank == 0)
				return et.MakeArrayType ();			
			return et.MakeArrayType (rank);
		}

		protected override bool IsArrayImpl ()
		{
			return true;
		}

		public override int GetArrayRank ()
		{
			return (rank == 0) ? 1 : rank;
		}

		public override Type BaseType {
			get { return typeof (System.Array); }
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return elementType.Attributes;
		}

		internal override String FormatName (string elementName)
		{
			if (elementName == null)
				return null;
			StringBuilder sb = new StringBuilder (elementName);
			sb.Append ("[");
			for (int i = 1; i < rank; ++i)
				sb.Append (",");
			if (rank == 1)
				sb.Append ("*");
			sb.Append ("]");
			return sb.ToString ();
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal class ByRefType : DerivedType
	{
		internal ByRefType (Type elementType) : base (elementType)
		{
		}

		internal override Type InternalResolve ()
		{
			return elementType.InternalResolve ().MakeByRefType (); 
		}

		protected override bool IsByRefImpl ()
		{
			return true;
		}

		public override Type BaseType {
			get { return typeof (Array); }
		}

		internal override String FormatName (string elementName)
		{
			if (elementName == null)
				return null;
			return elementName + "&";
		}

		public override Type MakeArrayType ()
		{
			throw new ArgumentException ("Cannot create an array type of a byref type");
		}

		public override Type MakeArrayType (int rank)
		{
			throw new ArgumentException ("Cannot create an array type of a byref type");
		}

		public override Type MakeByRefType ()
		{
			throw new ArgumentException ("Cannot create a byref type of an already byref type");
		}

		public override Type MakePointerType ()
		{
			throw new ArgumentException ("Cannot create a pointer type of a byref type");
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	internal class PointerType : DerivedType
	{
		internal PointerType (Type elementType) : base (elementType)
		{
		}

		internal override Type InternalResolve ()
		{
			return elementType.InternalResolve ().MakePointerType (); 
		}

		protected override bool IsPointerImpl ()
		{
			return true;
		}

		public override Type BaseType {
			get { return typeof(Array); }
		}

		internal override String FormatName (string elementName)
		{
			if (elementName == null)
				return null;
			return elementName + "*";
		}
	}

}
#endif
