﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mono.Debugger.Soft
{
	// Represents a temporary type of a lambda expression.
	// In order to determine the type of a lambda based on
	// its context, type evaluation for it must be delayed.
	public class DelayedLambdaType : Mirror
	{
		class ParsedType
		{
			public string nameSpace;
			public string typeName;
			public string [] argTypeNames;
		}

		string expression;
		Tuple<string, Value>[] locals;
		string uniqueName;

		public DelayedLambdaType (VirtualMachine vm, Tuple<string, Value>[] locals, string expression) : base (vm, 0)
		{
			this.expression = expression;
			this.locals = locals;
			this.uniqueName = "Lambda" + Guid.NewGuid ().ToString ("N");
		}

		public string Expression {
			get { return expression; }
		}

		public Tuple<string, Value>[] Locals {
			get {
				if (locals == null)
					return new Tuple<string, Value> [0];
				return locals;
			}
		}

		public string Name {
			get { return uniqueName; }
		}

		public Value[] GetLocalValues ()
		{
			var vals = new Value [Locals.Length];
			for (int i = 0; i < vals.Length; i++)
				vals [i] = Locals [i].Item2;
			return vals;
		}

		public bool IsAcceptableType (TypeMirror t)
		{
			return IsAcceptable (ParseFullName (t));
		}

		private bool IsAcceptable (ParsedType t)
		{
			return t != null;
		}

		public string GetLiteralType (TypeMirror typ)
		{
			ParsedType t = ParseFullName (typ);
			if (t == null || !IsAcceptable (t))
				return null;
			var ns = t.nameSpace != null ? t.nameSpace + "." : "";
			var args = t.argTypeNames != null ? "<" + String.Join (",", t.argTypeNames) + ">" : "";

			return ns + t.typeName + args;
		}

		private static ParsedType ParseFullName (TypeMirror t)
		{
			try {
				return OnParseFullName (t);
			} catch (Exception) {
				return null;
			}
		}

		private static ParsedType OnParseFullName (TypeMirror typ)
		{
			// Parse fullName of type to ParsedType that is defined above. Examples for the parsing
			// are following.
			// 1) System.Action`1[[System.Int32, mscorlib, Version=xxx, Culture=xxx,
			//    PublicKeyToken=xxx]]
			//  => { nameSpace: "System", typeName: "Action", argTypeNames: ["System.Int32"] }
			// 2) System.Func`2[[System.Int32, mscorlib, Version=xxx, Culture=xxx,
			//    PublicKeyToken=xxx],[System.Single, mscorlib, Version=xxx, Culture=xxx,
			//    PublicKeyToken=xxx]]
			//  => { nameSpace: "System", typeName: "Func",
			//       argTypeNames: ["System.Int32", "System.Single"] }

			if (typ == null)
				return null;

			string fullName = typ.FullName.Replace ('+', '.');
			string nmespace = typ.Namespace == "" ? null : typ.Namespace;
			string typeName = "";
			string [] argTypeNames = null;

			string rest = fullName;

			if (nmespace != null) {
				var omit = nmespace + ".";
				if (!rest.StartsWith (omit, StringComparison.Ordinal)) {
					throw new ArgumentException ("should starts with namespace");
				}
				rest = rest.Substring (omit.Length, rest.Length - omit.Length);
			}

			string pattern = @"(.*?)`\d+(.*)";
			Match m = Regex.Match (rest, pattern);
			if (m.Success) {
				typeName = m.Groups [1].Value;
				rest = m.Groups [2].Value;

				int len = rest.Length;
				if (rest [0] != '[' || rest [len - 1] != ']')
					throw new ArgumentException ("Failed to skip braces");
				rest = rest.Substring (1, len - 2);
				argTypeNames = ReadArgTypeNames (rest);
			} else {
				if (!rest.EndsWith (typ.Name, StringComparison.Ordinal))
					throw new ArgumentException ("invalid");
				typeName = rest;
			}

			ParsedType t = new ParsedType ();
			t.nameSpace = nmespace;
			t.typeName = typeName;
			t.argTypeNames = argTypeNames;

			return t;
		}

		private static string [] ReadArgTypeNames (string s)
		{
			List<string> args = new List<string> ();
			int nest = 0;
			int start = 0;
			for (int i = 0; i < s.Length; i++) {
				char c = s [i];
				if (c == '[') {
					if (nest == 0)
						start = i;
					nest++;
				} else if (c == ']') {
					if (nest == 1)
						args.Add (s.Substring (start, i - start + 1));
					nest--;
				}
			}
			string [] result = new string [args.Count];
			int n = 0;
			foreach (string arg in args) {
				string pattern = @"\[(.*?)[,\]]";
				Match m = Regex.Match (arg, pattern);
				if (!m.Success)
					throw new ArgumentException ("Failed to parse arg's names");
				result [n] = m.Groups [1].Value;
				n++;
			}
			return result;
		}
	}
}