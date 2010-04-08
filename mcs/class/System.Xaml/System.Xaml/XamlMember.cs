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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml
{
	public class XamlMember : IEquatable<XamlMember>
	{
		public XamlMember (EventInfo eventInfo, XamlSchemaContext schemaContext)
			: this (eventInfo, schemaContext, null)
		{
		}

		public XamlMember (EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
			: this (schemaContext, invoker)
		{
			throw new NotImplementedException ();
		}

		public XamlMember (PropertyInfo propertyInfo, XamlSchemaContext schemaContext)
			: this (propertyInfo, schemaContext, null)
		{
		}

		public XamlMember (PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
			: this (schemaContext, invoker)
		{
			throw new NotImplementedException ();
		}

		public XamlMember (string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext)
			: this (attachableEventName, adder, schemaContext, null)
		{
		}

		public XamlMember (string name, XamlType declaringType, bool isAttachable)
		{
			Name = name;
			DeclaringType = declaringType;
			throw new NotImplementedException ();
		}

		public XamlMember (string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext)
			: this (attachablePropertyName, getter, setter, schemaContext, null)
		{
		}

		public XamlMember (string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
			: this (schemaContext, invoker)
		{
			throw new NotImplementedException ();
		}

		public XamlMember (string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
			: this (schemaContext, invoker)
		{
			Name = attachablePropertyName;
			underlying_getter = getter;
			underlying_setter = setter;
		}

		XamlMember (XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		{
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			context = schemaContext;
			invoker = invoker;
		}

		MemberInfo underlying_member;
		MethodInfo underlying_getter, underlying_setter;
		XamlSchemaContext context;
		XamlMemberInvoker invoker;

		public XamlType DeclaringType { get; private set; }
		public string Name { get; private set; }

		public string PreferredXamlNamespace {
			get { throw new NotImplementedException (); }
		}
		public DesignerSerializationVisibility SerializationVisibility {
			get { throw new NotImplementedException (); }
		}

		public bool IsAttachable {
			get { throw new NotImplementedException (); }
		}
		public bool IsDirective {
			get { throw new NotImplementedException (); }
		}
		public bool IsNameValid {
			get { throw new NotImplementedException (); }
		}

		public XamlValueConverter<XamlDeferringLoader> DeferringLoader {
			get { return LookupDeferringLoader (); }
		}
		public IList<XamlMember> DependsOn {
			get { return LookupDependsOn (); }
		}
		public XamlMemberInvoker Invoker {
			get { return LookupInvoker (); }
		}
		public bool IsAmbient {
			get { return LookupIsAmbient (); }
		}
		public bool IsEvent {
			get { return LookupIsEvent (); }
		}
		public bool IsReadOnly {
			get { return LookupIsReadOnly (); }
		}
		public bool IsReadPublic {
			get { return LookupIsReadPublic (); }
		}
		public bool IsUnknown {
			get { return LookupIsUnknown (); }
		}
		public bool IsWriteOnly {
			get { return LookupIsWriteOnly (); }
		}
		public bool IsWritePublic {
			get { return LookupIsWritePublic (); }
		}
		public XamlType TargetType {
			get { return LookupTargetType (); }
		}
		public XamlType Type {
			get { return LookupType (); }
		}
		public XamlValueConverter<TypeConverter> TypeConverter {
			get { return LookupTypeConverter (); }
		}
		public MemberInfo UnderlyingMember {
			get { return LookupUnderlyingMember (); }
		}
		public XamlValueConverter<ValueSerializer> ValueSerializer {
			get { return LookupValueSerializer (); }
		}

		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}
		
		public bool Equals (XamlMember other)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public virtual IList<string> GetXamlNamespaces ()
		{
			throw new NotImplementedException ();
		}

		// lookups

		protected virtual ICustomAttributeProvider LookupCustomAttributeProvider ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader ()
		{
			throw new NotImplementedException ();
		}
		protected virtual IList<XamlMember> LookupDependsOn ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlMemberInvoker LookupInvoker ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsAmbient ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsEvent ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsReadOnly ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsReadPublic ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsUnknown ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsWriteOnly ()
		{
			throw new NotImplementedException ();
		}
		protected virtual bool LookupIsWritePublic ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlType LookupTargetType ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlType LookupType ()
		{
			throw new NotImplementedException ();
		}
		protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter ()
		{
			throw new NotImplementedException ();
		}

		protected virtual MethodInfo LookupUnderlyingGetter ()
		{
			return underlying_getter;
		}

		protected virtual MemberInfo LookupUnderlyingMember ()
		{
			return underlying_member;
		}

		protected virtual MethodInfo LookupUnderlyingSetter ()
		{
			return underlying_setter;
		}

		protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer ()
		{
			throw new NotImplementedException ();
		}
	}
}
