//
// TypeLoader.cs: Loads types from a list of Assemblies
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class TypeLoader {

		private static TraceSwitch info = 
			new TraceSwitch ("type-loader", "TypeLoader messages");

		// String collection
		private ICollection assemblies = null;

		private bool matchFullName = true;
		private bool matchName = false;
		private bool matchBase = false;
		private bool matchMethodReturnType = false;
		private bool matchNamespace = false;

		public bool MatchFullName {
			get {return matchFullName;}
			set {matchFullName = value;}
		}

		public bool MatchClassName {
			get {return matchName;}
			set {matchName = value;}
		}

		public bool MatchBase {
			get {return matchBase;}
			set {matchBase = value;}
		}

		public bool MatchMethodReturnType {
			get {return matchMethodReturnType;}
			set {matchMethodReturnType = value;}
		}

		public bool MatchNamespace {
			get {return matchNamespace;}
			set {matchNamespace = value;}
		}

		public TypeLoader ()
		{
		}

		public TypeLoader (ICollection assemblies)
		{
			Console.WriteLine ("type-loader: " + info.Level);
			this.assemblies = assemblies;
		}

		public ICollection Assemblies {
			get {return assemblies;}
			set {assemblies = value;}
		}

		public ICollection LoadTypes (string match)
		{
			if (assemblies == null)
				throw new ArgumentNullException ("Assemblies");

			IList found = new ArrayList ();

			foreach (string a in assemblies) {
				LoadMatchingTypesFrom (a, match, found);
			}

			return found;
		}

		private void LoadMatchingTypesFrom (string where, string match, IList types)
		{
			Regex re = new Regex (match);
			try {
				Assembly a = Assembly.LoadFrom (where);
				Type[] _types = a.GetTypes();
				foreach (Type t in _types) {
					if (Matches (re, t))
						types.Add (t);
				}
			} catch (Exception e) {
				Trace.WriteLineIf (info.TraceError, String.Format (
					"Unable to load type regex `{0}' from `{1}'.",
					match, where));
				Trace.WriteLineIf (info.TraceError, e.ToString());
			}
		}

		private bool Matches (Regex r, Type t)
		{
			bool f, c, b, rt, n;
			f = c = b = rt = n = false;
			if (MatchFullName)
				f = r.Match(t.FullName).Success;
			if (MatchClassName)
				c = r.Match(t.Name).Success;
			if (MatchNamespace)
				n = r.Match(t.Namespace).Success;
			if (MatchBase) {
				b = (!MatchFullName ? false : r.Match (t.BaseType.FullName).Success) ||
				    (!MatchClassName ? false : r.Match (t.BaseType.Name).Success) ||
				    (!MatchNamespace ? false : r.Match (t.BaseType.Namespace).Success);
			}
			// TODO: MatchMethodReturnType
			Trace.WriteLineIf (info.TraceInfo, String.Format("TypeLoader.Matches: c={0}, b={1}, rt={2}, n={3}", c, b, rt, n));
			return f || c || b || rt || n;
		}
	}
}

