//
// TypeDisplayer.cs: 
//   Common ITypeDisplayer operations
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

// #define TRACE

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Mono.TypeReflector.Finders;
using Mono.TypeReflector.Formatters;

namespace Mono.TypeReflector.Displayers
{
	sealed class DisplayerComparer : IComparer
	{
		public static readonly DisplayerComparer Default;

		static DisplayerComparer ()
		{
			Default = new DisplayerComparer ();
		}

		// we store either Assembly's or strings; handle both.
		public int Compare (object a, object b)
		{
			if (a is Assembly) {
				Assembly aa = (Assembly) a;
				Assembly ab = (Assembly) b;
				return aa.FullName.CompareTo (ab.FullName);
			}
      if (a is Type) {
        Type ta = (Type) a;
        Type tb = (Type) b;
        return ta.FullName.CompareTo (tb.FullName);
      }
			return Comparer.Default.Compare (a, b);
		}
	}

	public abstract class TypeDisplayer : ITypeDisplayer
	{
		private INodeFormatter formatter;
		private INodeFinder finder;
		private TypeReflectorOptions options;

		// type: map<Assembly, map<Namespace, list<Type> > >
		private IDictionary types = CreateDictionary ();

		public TypeDisplayer ()
		{
		}

		public INodeFormatter Formatter {
			get {return formatter;}
			set {formatter = value;}
		}

		public INodeFinder Finder {
			get {return finder;}
			set {finder = value;}
		}

		public TypeReflectorOptions Options {
			get {return options;}
			set {options = value;}
		}

		public abstract int MaxDepth {set;}
		public abstract bool RequireTypes {get;}

		protected ICollection Assemblies {
			get {return types.Keys;}
		}

		private static IDictionary CreateDictionary ()
		{
			return new SortedList (DisplayerComparer.Default);
		}

		protected ICollection Namespaces (Assembly a)
		{
			return _Namespaces(a).Keys;
		}

		protected ICollection Types (Assembly a, string ns)
		{
			// return (ICollection) _Namespaces(a)[ns];
			return _Types (_Namespaces(a), ns).Keys;
		}

		private IDictionary _Namespaces (Assembly a)
		{
			IDictionary d = (IDictionary) types[a];
			if (d == null) {
				d = CreateDictionary ();
				types[a] = d;
			}
			return d;
		}

		private IDictionary _Types (IDictionary namespaces, string ns)
		{
			if (ns == null)
				ns = string.Empty;
			IDictionary list = (IDictionary) namespaces[ns];
			if (list == null) {
				list = CreateDictionary ();
				namespaces[ns] = list;
			}
			return list;
		}

		public virtual void AddType (Type type)
		{
			_Types(_Namespaces(type.Assembly), type.Namespace)[type] = null;
		}

		public abstract void Run ();

		public virtual void ShowError (string message)
		{
			Console.WriteLine (message);
		}
	}
}

