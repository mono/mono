#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using XPI = System.Xml.XLinq.XProcessingInstruction;


namespace System.Xml.XLinq
{
	public class XAttribute
	{
		static IEnumerable <XAttribute> emptySequence =
			new List <XAttribute> ();

		public static IEnumerable <XAttribute> EmptySequence {
			get { return emptySequence; }
		}

		XName name;
		object value;
		XElement parent;

		public XAttribute (XAttribute source)
		{
			name = source.name;
			value = source.value;
		}

		public XAttribute (XName name, object value)
		{
			this.name = name;
			this.value = XUtil.ToString (value);
		}

		public XName Name {
			get { return name; }
		}

		public XElement Parent {
			get { return parent; }
			internal set {
				parent = value;
				value.InternalAppendAttribute (this);
			}
		}

		public string Value {
			get { return XUtil.ToString (value); }
			set { this.value = value; }
		}

		public override bool Equals (object obj)
		{
			XAttribute a = obj as XAttribute;
			if (a == null)
				return false;
			return a.Name == name && a.value == value;
		}

		public override int GetHashCode ()
		{
			return name.GetHashCode () ^ value.GetHashCode ();
		}

		public static explicit operator bool (XAttribute a)
		{
			return XUtil.ToBoolean (a.value);
		}

		public static explicit operator Nullable <bool> (XAttribute a)
		{
			return a.value == null || String.Empty == a.value as string ?
				null : XUtil.ToNullableBoolean (a.value);
		}

		// FIXME: similar conversion methods follow.

		public void Remove ()
		{
			if (parent != null) {
				parent.InternalRemoveAttribute (this);
				parent = null;
			}
		}
	}
}

#endif
