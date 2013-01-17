//
// System.ComponentModel.MemberDescriptor.cs
//
// Author:
//  Miguel de Icaza (miguel@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//  Ivan N. Zlatev <contact@i-nz.net>
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2003 Andreas Nahr
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

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Globalization;

namespace System.ComponentModel
{
	[ComVisible (true)]
	public abstract class MemberDescriptor
	{
		private string name;
		private Attribute [] attrs; // only the attributes supplied in the ctor
		private AttributeCollection attrCollection;
		private static IComparer default_comparer;

		protected MemberDescriptor (string name, Attribute [] attrs)
		{
			this.name = name;
			this.attrs = attrs;
		}

		protected MemberDescriptor (MemberDescriptor reference, Attribute [] attrs)
		{
			name = reference.name;
			this.attrs = attrs;
		}

		protected MemberDescriptor (string name)
		{
			this.name = name;
		}

		protected MemberDescriptor (MemberDescriptor reference)
		{
			name = reference.name;
			attrs = reference.AttributeArray;
		}

		protected virtual Attribute [] AttributeArray {
			get {
				ArrayList list = new ArrayList ();
				if (attrs != null)
					list.AddRange (attrs);
				FillAttributes (list);
				// For duplicate attributes, the last one added to the list is kept.
				Hashtable attributes = new Hashtable ();
				foreach (Attribute attribute in list)
					attributes[attribute.TypeId] = attribute;
				Attribute[] attributesArray = new Attribute[attributes.Values.Count];
				attributes.Values.CopyTo (attributesArray, 0);
				return attributesArray;
			}
			set {
				attrs = value;
			}
		}

		protected virtual void FillAttributes(System.Collections.IList attributeList)
		{
			// to be overriden
		}

		public virtual AttributeCollection Attributes {
			get {
				if (attrCollection == null)
					attrCollection = CreateAttributeCollection ();
				return attrCollection;
			}
		}

		protected virtual AttributeCollection CreateAttributeCollection ()
		{
			return new AttributeCollection (AttributeArray);
		}

		public virtual string Category {
			get {
				return ((CategoryAttribute) Attributes [typeof (CategoryAttribute)]).Category;
			}
		}

		public virtual string Description {
			get {
				foreach (Attribute attr in AttributeArray)
					if (attr is DescriptionAttribute)
						return ((DescriptionAttribute) attr).Description;
				return string.Empty;
			}
		}

		public virtual bool DesignTimeOnly {
			get {
				foreach (Attribute attr in AttributeArray)
					if (attr is DesignOnlyAttribute)
						return ((DesignOnlyAttribute) attr).IsDesignOnly;
				return false;
			}
		}

		public virtual string DisplayName {
			get {
				foreach (Attribute attr in AttributeArray)
					if (attr is DisplayNameAttribute)
						return ((DisplayNameAttribute) attr).DisplayName;
				return name;
			}
		}

		public virtual string Name {
			get {
				return name;
			}
		}

		public virtual bool IsBrowsable {
			get {
				foreach (Attribute attr in AttributeArray)
					if (attr is BrowsableAttribute)
						return ((BrowsableAttribute) attr).Browsable;
				return true;
			}
		}

		protected virtual int NameHashCode {
			get {
				return name.GetHashCode ();
			}
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			MemberDescriptor other = obj as MemberDescriptor;
			if (other == null)
				return false;
			return other.name == name;
		}

		protected static ISite GetSite (object component)
		{
			if (component is Component)
				return ((Component) component).Site;
			else
				return null;
		}

		[Obsolete ("Use GetInvocationTarget")]
		protected static object GetInvokee (Type componentClass, object component)
		{
			if (component is IComponent) {
				ISite site = ((IComponent) component).Site;
				if (site != null && site.DesignMode) {
					IDesignerHost host = site.GetService (typeof (IDesignerHost)) as IDesignerHost;
					if (host != null) {
						IDesigner designer = host.GetDesigner ((IComponent) component);
						if (designer != null && componentClass.IsInstanceOfType (designer)) {
							component = designer;
						}
					}
				}
			}
			return component;
		}

		protected virtual object GetInvocationTarget (Type type, object instance)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (instance == null)
				throw new ArgumentNullException ("instance");

			return GetInvokee (type, instance);
		}

		protected static MethodInfo FindMethod(Type componentClass, string name, Type[] args, Type returnType)
		{
			return FindMethod (componentClass, name, args, returnType, true);
		}

		protected static MethodInfo FindMethod(Type componentClass, string name, Type[] args, Type returnType, bool publicOnly)
		{
			BindingFlags bf;
			if (publicOnly == true)
				bf = BindingFlags.Public;
			else
				bf = BindingFlags.NonPublic | BindingFlags.Public;
			// FIXME returnType is not taken into account. AFAIK methods are not allowed to only
			// differ by return type anyway
			return componentClass.GetMethod (name, bf, null, CallingConventions.Any, args, null);
		}

		internal static IComparer DefaultComparer {
			get {
				if (default_comparer == null)
					default_comparer = new MemberDescriptorComparer ();
				return default_comparer;
			}
		}

		private class MemberDescriptorComparer : IComparer
		{
			public int Compare (object x, object y)
			{
				return String.Compare (((MemberDescriptor)x).Name, ((MemberDescriptor)y).Name, 
						       false, CultureInfo.InvariantCulture);
			}
		}
	}
}
