//
// Commons.Xml.Relaxng.RngNameClass.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	public abstract class RngNameClass : RngElementBase
	{
		internal protected RngNameClass ()
		{
		}

		public abstract RdpNameClass Compile (RngGrammar g);

		internal protected bool FindInvalidType (RdpNameClass nc, bool allowNsName)
		{
			RdpNameClassChoice choice = nc as RdpNameClassChoice;
			if (choice != null)
				return FindInvalidType (choice.LValue, allowNsName)
					|| FindInvalidType (choice.RValue, allowNsName);
			else if (nc is RdpAnyName)
				return true;
			else if (nc is RdpNsName && !allowNsName)
				return true;
			else
				return false;
		}
	}

	public abstract class RngNameContainerClass : RngNameClass
	{
	}

	public class RngAnyName : RngNameClass
	{
		RngExceptNameClass except;
		public RngAnyName ()
		{
		}

		public override RdpNameClass Compile (RngGrammar g)
		{
			if (except != null) {
				RdpNameClass exc = except.Compile (g);
				if (FindInvalidType (exc, true))
					throw new RngException ("anyName except cannot have anyName children.");
				return new RdpAnyNameExcept (exc);
			} else
				return RdpAnyName.Instance;
		}

		public RngExceptNameClass Except {
			get { return except; }
			set { except = value; }
		}
	}

	public class RngNsName : RngNameClass
	{
		string ns;
		RngExceptNameClass except;
		public RngNsName ()
		{
		}

		public override RdpNameClass Compile (RngGrammar g)
		{
			if (except != null) {
				RdpNameClass exc = except.Compile (g);
				if (FindInvalidType (exc, false))
					throw new RngException ("nsName except cannot have anyName nor nsName children.");
				return new RdpNsNameExcept (ns, exc);
			} else {
				return new RdpNsName (ns);
			}
		}

		public RngExceptNameClass Except {
			get { return except; }
			set { except = value; }
		}
	}

	public class RngName : RngNameClass
	{
		string ns;
		string ncname;

		public RngName ()
		{
		}

		public RngName (string ncname, string ns)
		{
			XmlConvert.VerifyNCName (ncname);
			this.ncname = ncname;
			this.ns = ns;
		}

		public string NCName {
			get { return ncname; }
			set {
				XmlConvert.VerifyNCName (value);
				ncname = value;
			}
		}

		public string NS {
			get { return ns; }
			set { ns = value; }
		}

		public override RdpNameClass Compile (RngGrammar g)
		{
			return new RdpName (ncname, ns);
		}
	}

	public class RngNameChoice : RngNameClass
	{
		ArrayList names = new ArrayList ();

		public RngNameChoice ()
		{
		}

		public override RdpNameClass Compile (RngGrammar g)
		{
			// Flatten names into RdpChoice. See 4.12.
			if (names.Count == 0)
				return null;
			RdpNameClass p = ((RngNameClass) names [0]).Compile (g);
			if (names.Count == 1)
				return p;

			for (int i=1; i<names.Count; i++)
				p = new RdpNameClassChoice (p, ((RngNameClass) names [i]).Compile (g));
			return p;
		}

		public ArrayList Children {
			get { return names; }
			set { names = value; }
		}
	}

	public class RngExceptNameClass : RngElementBase
	{
		ArrayList names = new ArrayList ();

		public RngExceptNameClass ()
		{
		}

		public RdpNameClass Compile (RngGrammar g)
		{
			// Flatten names into RdpGroup. See 4.12.
			if (names.Count == 0)
				return null;
			RdpNameClass p = ((RngNameClass) names [0]).Compile (g);
			for (int i=1; i<names.Count; i++) {
				p = new RdpNameClassChoice (
					((RngNameClass) names [i]).Compile (g),
					p);
			}
			return p;
		}

		public ArrayList Names {
			get { return names; }
			set { names = value; }
		}
	}
}
