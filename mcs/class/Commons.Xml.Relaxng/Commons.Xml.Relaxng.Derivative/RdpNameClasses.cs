//
// Commons.Xml.Relaxng.Derivative.RdpNameClasses.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Xml;

namespace Commons.Xml.Relaxng.Derivative
{
	public enum RdpNameClassType
	{
		None = 0,
		AnyName = 1,
		AnyNameExcept = 2,
		NsName = 3,
		NsNameExcept = 4,
		Name = 5,
		NameClassChoice = 6
	}

	public abstract class RdpNameClass : ICloneable
	{
		public abstract RdpNameClassType NameClassType { get; }
		public abstract bool Contains (string name, string ns);
		public abstract object Clone ();
	}

	public class RdpAnyName : RdpNameClass
	{
		static RdpAnyName instance;
		static RdpAnyName ()
		{
			instance = new RdpAnyName ();
		}

		public static RdpAnyName Instance {
			get { return instance; }
		}

		private RdpAnyName () {}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.AnyName; }
		}

		public override bool Contains (string name, string ns)
		{
			return true;
		}

		public override object Clone ()
		{
			return instance;
		}
	}

	public class RdpAnyNameExcept : RdpNameClass
	{
		RdpNameClass except;

		public RdpAnyNameExcept (RdpNameClass except)
		{
			this.except = except;
		}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.AnyNameExcept; }
		}

		public RdpNameClass ExceptNameClass {
			get { return except; }
		}

		public override bool Contains (string name, string ns)
		{
			return (except == null) || !except.Contains (name, ns);
		}

		public override object Clone ()
		{
			return new RdpAnyNameExcept (
				(except != null) ?
					this.except.Clone () as RdpNameClass :
					null);
		}
	}

	public class RdpNsName : RdpNameClass
	{
		string ns;

		public RdpNsName (string ns)
		{
			this.ns = ns;
		}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.NsName; }
		}

		public string NamespaceURI {
			get { return ns; }
		}

		public override bool Contains (string name, string ns)
		{
			return NamespaceURI == ns;
		}

		public override object Clone ()
		{
			return new RdpNsName (this.ns);
		}
	}

	public class RdpNsNameExcept : RdpNsName
	{
		string ns;
		RdpNameClass except;

		public RdpNsNameExcept (string ns, RdpNameClass except)
			: base (ns)
		{
			this.ns = ns;
			this.except = except;
		}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.NsNameExcept; }
		}

		public RdpNameClass ExceptNameClass {
			get { return except; }
		}

		public override bool Contains (string name, string ns)
		{
			return this.ns == ns &&
				(except == null || !except.Contains (name, ns));
		}

		public override object Clone ()
		{
			return new RdpNsNameExcept (this.ns,
				(except != null) ?
					this.except.Clone () as RdpNameClass :
					null);
		}
	}

	public class RdpName : RdpNameClass
	{
		string local;
		string ns;

		public RdpName (string local, string ns)
		{
			this.ns = ns;
			this.local = local;
		}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.Name; }
		}

		public string NamespaceURI {
			get { return ns; }
		}

		public string LocalName {
			get { return local; }
		}

		public override bool Contains (string name, string ns)
		{
			return this.ns == ns && this.local == name;
		}

		public override object Clone ()
		{
			return new RdpName (this.local, this.ns);
		}
	}

	public class RdpNameClassChoice : RdpNameClass
	{
		RdpNameClass l;
		RdpNameClass r;

		public RdpNameClassChoice (RdpNameClass l, RdpNameClass r)
		{
			this.l = l;
			this.r = r;
		}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.NameClassChoice; }
		}

		public RdpNameClass LValue {
			get { return l; }
		}

		public RdpNameClass RValue {
			get { return r; }
		}

		public override bool Contains (string name, string ns)
		{
			return l.Contains (name, ns) || r.Contains (name, ns);
		}

		public override object Clone ()
		{
			return new RdpNameClassChoice (l.Clone () as RdpNameClass,
				r.Clone () as RdpNameClass);
		}
	}

}

