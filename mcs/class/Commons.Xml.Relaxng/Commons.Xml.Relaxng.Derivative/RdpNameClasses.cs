//
// Commons.Xml.Relaxng.Derivative.RdpNameClasses.cs
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

	public abstract class RdpNameClass
	{
		public abstract bool HasInfiniteName { get; }
		public abstract RdpNameClassType NameClassType { get; }
		public abstract bool Contains (string name, string ns);
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

		public override bool HasInfiniteName {
			get { return true; }
		}

		public override RdpNameClassType NameClassType {
			get { return RdpNameClassType.AnyName; }
		}

		public override bool Contains (string name, string ns)
		{
			return true;
		}
	}

	public class RdpAnyNameExcept : RdpNameClass
	{
		RdpNameClass except;

		public RdpAnyNameExcept (RdpNameClass except)
		{
			this.except = except;
		}

		public override bool HasInfiniteName {
			get { return true; }
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
	}

	public class RdpNsName : RdpNameClass
	{
		string ns;

		public RdpNsName (string ns)
		{
			this.ns = ns;
		}

		public override bool HasInfiniteName {
			get { return true; }
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

		public override bool HasInfiniteName {
			get { return true; }
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

		public override bool HasInfiniteName {
			get { return false; }
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

		public override bool HasInfiniteName {
			get { return l.HasInfiniteName || r.HasInfiniteName; }
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
	}

}

