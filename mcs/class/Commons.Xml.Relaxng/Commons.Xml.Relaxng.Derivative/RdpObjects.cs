//
// Commons.Xml.Relaxng.Derivative.RdpObjects.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace Commons.Xml.Relaxng.Derivative
{
	///
	/// Context Class
	///
	public class RdpContext : ICloneable
	{
		public RdpContext ()
		{
		}

		public object Clone ()
		{
			return new RdpContext ();
		}
	}

	///
	/// Datatype Related Classes
	///
	public class RdpParamList : ArrayList
	{
		public RdpParamList () : base ()
		{
		}
	}

	public class RdpParam : ICloneable
	{
		public RdpParam (string localName, string value)
		{
			this.localName = localName;
			this.value = value;
		}

		string value;
		public string Value {
			get { return this.value; }
		}

		string localName;
		public string LocalName {
			get { return localName; }
		}

		public object Clone ()
		{
			return new RdpParam (this.localName, this.value);
		}
	}

	public class RdpDatatype : ICloneable
	{
		public RdpDatatype (string ns, string localName)
		{
			this.ns = ns;
			this.localName = localName;
		}

		string ns;
		public string NamespaceURI {
			get { return ns; }
		}

		string localName;
		public string LocalName {
			get { return localName; }
		}

		public virtual bool IsAllowed (RdpParamList pl, string value)
		{
			if (ns == String.Empty && localName == "string")
				return true;
			else if (ns == String.Empty && localName == "token")
				return true;
			else
				return true;
//				throw new NotSupportedException ("non-supported datatype validation.");
		}

		public virtual bool IsTypeEqual (string s1, RdpContext ctx1, string s2)
		{
			if (ns == String.Empty && localName == "string")
				return s1 == s2;
			else if (ns == String.Empty && localName == "token")
				return NormalizeWhitespace (s1) ==
					NormalizeWhitespace (s2);
			else
				throw new NotSupportedException ("non-supported datatype validation.");
		}

		public string NormalizeWhitespace (string s)
		{
			return String.Join (" ", s.Split (RdpUtil.WhitespaceChars));
		}

		public object Clone ()
		{
			return new RdpDatatype (ns, localName);
		}
	}

	///
	/// ChildNode Classes
	///
	public abstract class RdpChildNode
	{
		// Strip
		public virtual bool IsNonWhitespaceText {
			get { return false; }
		}
	}

	public class RdpTextChild : RdpChildNode
	{
		public RdpTextChild (string text)
		{
			this.text = text;
		}

		string text;
		public string Text {
			get { return text; }
		}

		public override bool IsNonWhitespaceText {
			get { return RdpUtil.Whitespace (text); }
		}
	}

	public class RdpElementChild : RdpChildNode
	{
		public RdpElementChild (string name, string ns, RdpContext ctx, RdpAttributes attributes, RdpChildNodes childNodes)
		{
			this.name = name;
			this.ns = ns;
			this.ctx = ctx;
			this.attributes = attributes;
			this.childNodes = childNodes;
		}

		string name;
		public string LocalName {
			get { return name; }
		}

		string ns;
		public string NamespaceURI {
			get { return ns; }
		}

		RdpContext ctx;
		public RdpContext Context {
			get { return ctx; }
		}

		RdpAttributes attributes;
		public RdpAttributes Attributes {
			get { return attributes; }
		}

		RdpChildNodes childNodes;
		public RdpChildNodes ChildNodes {
			get { return childNodes; }
		}
	}

	public class RdpChildNodes : ArrayList
	{
		public RdpChildNodes () : base ()
		{
		}
	}

	public class RdpAttributes : ArrayList
	{
		public RdpAttributes () : base ()
		{
		}
	}

	public class RdpAttributeNode : RdpChildNode
	{
		public RdpAttributeNode (string name, string ns, string value) : base ()
		{
			this.name = name;
			this.ns = ns;
			this.value = value;
		}

		string value;
		public string Value {
			get { return value; }
		}

		string name;
		public string LocalName {
			get { return name; }
		}

		string ns;
		public string NamespaceURI {
			get { return ns; }
		}
	}
}

