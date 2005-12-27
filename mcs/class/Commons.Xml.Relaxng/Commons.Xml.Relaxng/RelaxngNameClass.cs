//
// Commons.Xml.Relaxng.RelaxngNameClass.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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
using System.IO;
using System.Net;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;
using Commons.Xml.Relaxng.Rnc;

namespace Commons.Xml.Relaxng
{
	public class RelaxngNameClassList : CollectionBase
	{
		public RelaxngNameClassList ()
		{
		}

		public void Add (RelaxngNameClass p)
		{
			List.Add (p);
		}

		public RelaxngNameClass this [int i] {
			get { return this.List [i] as RelaxngNameClass; }
			set { this.List [i] = value; }
		}

		public void Insert (int pos, RelaxngNameClass p)
		{
			List.Insert (pos, p);
		}

		public void Remove (RelaxngNameClass p)
		{
			List.Remove (p);
		}
	}

	public abstract class RelaxngNameClass : RelaxngElementBase
	{
		protected RelaxngNameClass ()
		{
		}

		internal abstract RdpNameClass Compile (RelaxngGrammar g);

		internal abstract void CheckConstraints (bool rejectAnyName, bool rejectNsName);

		internal bool FindInvalidType (RdpNameClass nc, bool allowNsName)
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

	public class RelaxngAnyName : RelaxngNameClass
	{
		RelaxngExceptNameClass except;
		public RelaxngAnyName ()
		{
		}

		public RelaxngExceptNameClass Except {
			get { return except; }
			set { except = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "anyName", RelaxngGrammar.NamespaceURI);
			if (except != null)
				except.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteAnyName (this);
		}

		internal override RdpNameClass Compile (RelaxngGrammar g)
		{
			if (except != null) {
				RdpNameClass exc = except.Compile (g);
				if (FindInvalidType (exc, true))
					throw new RelaxngException (except, "anyName except cannot have anyName children.");
				return new RdpAnyNameExcept (exc);
			} else
				return RdpAnyName.Instance;
		}

		internal override void CheckConstraints (bool rejectAnyName, bool rejectNsName) 
		{
			if (rejectAnyName)
				throw new RelaxngException (this, "Not allowed anyName was found.");
			if (except != null)
				foreach (RelaxngNameClass nc in except.Names)
					nc.CheckConstraints (true, rejectNsName);
		}

	}

	public class RelaxngNsName : RelaxngNameClass
	{
		string ns;
		RelaxngExceptNameClass except;
		public RelaxngNsName ()
		{
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public RelaxngExceptNameClass Except {
			get { return except; }
			set { except = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "nsName", RelaxngGrammar.NamespaceURI);
			if (except != null)
				except.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteNsName (this);
		}

		internal override RdpNameClass Compile (RelaxngGrammar g)
		{
			if (except != null) {
				RdpNameClass exc = except.Compile (g);
				if (FindInvalidType (exc, false))
					throw new RelaxngException (except, "nsName except cannot have anyName nor nsName children.");
				return new RdpNsNameExcept (ns, exc);
			} else {
				return new RdpNsName (ns);
			}
		}

		internal override void CheckConstraints (bool rejectAnyName, bool rejectNsName) 
		{
			if (rejectNsName)
				throw new RelaxngException (this, "Not allowed nsName was found.");
			if (except != null)
				foreach (RelaxngNameClass nc in except.Names)
					nc.CheckConstraints (true, true);
		}

	}

	public class RelaxngName : RelaxngNameClass
	{
		string ns;
		string ncname;

		public RelaxngName ()
		{
		}

		public RelaxngName (string ncname, string ns)
		{
			XmlConvert.VerifyNCName (ncname);
			this.ncname = ncname;
			this.ns = ns;
		}

		public string LocalName {
			get { return ncname; }
			set {
				XmlConvert.VerifyNCName (value);
				ncname = value;
			}
		}

		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "name", RelaxngGrammar.NamespaceURI);
			writer.WriteAttributeString ("ns", ns);
			// Here we just skip qname
			writer.WriteString (ncname);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteName (this);
		}

		internal override RdpNameClass Compile (RelaxngGrammar g)
		{
			return new RdpName (ncname, ns);
		}

		internal override void CheckConstraints (bool rejectAnyName, bool rejectNsName) 
		{
			// no error
		}
	}

	public class RelaxngNameChoice : RelaxngNameClass
	{
		RelaxngNameClassList names = new RelaxngNameClassList ();

		public RelaxngNameChoice ()
		{
		}

		public RelaxngNameClassList Children {
			get { return names; }
			set { names = value; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "choice", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngNameClass nc in Children)
				nc.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteNameChoice (this);
		}

		internal override RdpNameClass Compile (RelaxngGrammar g)
		{
			// Flatten names into RdpChoice. See 4.12.
			if (names.Count == 0)
				return null;
			RdpNameClass p = ((RelaxngNameClass) names [0]).Compile (g);
			if (names.Count == 1)
				return p;

			for (int i=1; i<names.Count; i++)
				p = new RdpNameClassChoice (p, ((RelaxngNameClass) names [i]).Compile (g));
			return p;
		}

		internal override void CheckConstraints (bool rejectAnyName, bool rejectNsName) 
		{
			foreach (RelaxngNameClass nc in names)
				nc.CheckConstraints (rejectAnyName, rejectNsName);
		}
	}

	public class RelaxngExceptNameClass : RelaxngElementBase
	{
		RelaxngNameClassList names = new RelaxngNameClassList ();

		public RelaxngExceptNameClass ()
		{
		}

		public RelaxngNameClassList Names {
			get { return names; }
		}

		public override void Write (XmlWriter writer)
		{
			writer.WriteStartElement ("", "except", RelaxngGrammar.NamespaceURI);
			foreach (RelaxngNameClass nc in Names)
				nc.Write (writer);
			writer.WriteEndElement ();
		}

		internal override void WriteRnc (RncWriter writer)
		{
			writer.WriteNameExcept (this);
		}

		internal RdpNameClass Compile (RelaxngGrammar g)
		{
			// Flatten names into RdpGroup. See 4.12.
			if (names.Count == 0)
				return null;
			RdpNameClass p = ((RelaxngNameClass) names [0]).Compile (g);
			for (int i=1; i<names.Count; i++) {
				p = new RdpNameClassChoice (
					((RelaxngNameClass) names [i]).Compile (g),
					p);
			}
			return p;
		}
	}
}
