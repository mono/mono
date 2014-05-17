//
// MemberInvocationReference.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using Microsoft.Build.Framework;
using System.Text;

namespace Microsoft.Build.BuildEngine
{
	class MemberInvocationReference : IReference
	{
		Type type;
		readonly string name;

		static readonly char[] ArgumentTrimChars = new char[] { '\"', '\'', '`' };
		static readonly object ConversionFailed = new object ();

		public MemberInvocationReference (Type type, string name)
		{
			this.type = type;
			this.name = name;
		}

		public List<string> Arguments { get; set; }

		public IReference Instance { get; set; }

		public string ConvertToString (Project project, ExpressionOptions options)
		{
			return ConvertResult (Invoke (project, options));
		}

		object Invoke (Project project, ExpressionOptions options)
		{
			var flags = BindingFlags.IgnoreCase | BindingFlags.Public;
			object target;

			if (Instance == null) {
				target = null;
				flags |= BindingFlags.Static;
			} else {
				var mir = Instance as MemberInvocationReference;
				if (mir != null) {
					target = mir.Invoke (project, options);
					if (target == null) {
						throw new NotImplementedException ("Instance method on null value");
					}

					type = target.GetType ();
				} else {
					target = Instance.ConvertToString (project, options);
					type = typeof (string);
				}

				flags |= BindingFlags.Instance;
			}

			object[] args;
			if (Arguments == null) {
				flags |= BindingFlags.GetProperty;
				args = null;
			} else {
				flags |= BindingFlags.InvokeMethod;
				ExpandArguments (project, options);
				args = PrepareMethodArguments (flags);
				if (args == null)
					throw new InvalidProjectFileException (string.Format ("Method '{0}({1})' arguments cannot be evaluated'", name, string.Join (", ", Arguments.ToArray ())));
			}

			object value;
			try {
				value = type.InvokeMember (name, flags, null, target, args, CultureInfo.InvariantCulture);
			} catch (MissingFieldException) {
				//
				// It can be field/constant instead of a property
				//
				if (args == null && Instance == null) {
					flags &= ~BindingFlags.GetProperty;
					flags |= BindingFlags.GetField;
					value = type.InvokeMember (name, flags, null, null, null, CultureInfo.InvariantCulture);
				} else {
					throw;
				}
			}

			return value;
		}

		void ExpandArguments (Project project, ExpressionOptions options)
		{
			for (int i = 0; i < Arguments.Count; ++i) {
				string arg = Arguments [i].Trim ();
				if (string.Equals (arg, "null", StringComparison.OrdinalIgnoreCase)) {
					arg = null;
				} else {
					arg = Expression.ParseAs<string> (arg, ParseOptions.None,
						project, options);

					arg = arg.Trim (ArgumentTrimChars);
				}

				Arguments [i] = arg;
			}
		}

		object[] PrepareMethodArguments (BindingFlags flags)
		{
			var candidates = type.GetMember (name, MemberTypes.Method, flags);
			object[] args = null;
			ParameterInfo[] best = null;
			foreach (MethodBase candidate in candidates) {
				var parameters = candidate.GetParameters ();
				if (parameters.Length != Arguments.Count)
					continue;

				if (parameters.Length == 0)
					return new object [0];

				object[] cand_args = null;
				for (int i = 0; i < parameters.Length; ++i) {
					var target = ConvertArgument (Arguments [i], parameters [i]);
					if (target == ConversionFailed) {
						cand_args = null;
						break;
					}

					if (cand_args == null)
						cand_args = new object[parameters.Length];

					cand_args [i] = target;
				}

				if (cand_args == null)
					continue;

				if (args == null) {
					args = cand_args;
					best = parameters;
					continue;
				}

				if (BetterCandidate (best, parameters) > 1) {
					args = cand_args;
					best = parameters;
				}
			}

			return args;
		}

		static object ConvertArgument (object value, ParameterInfo target)
		{
			var ptype = target.ParameterType;
			if (ptype.IsEnum) {
				var s = value as string;
				if (s != null)
					return ConvertToEnum (s, ptype);
			} else if (ptype == typeof (char[])) {
				var s = value as string;
				if (s != null)
					return s.ToCharArray ();
			}

			try {
				return Convert.ChangeType (value, ptype, CultureInfo.InvariantCulture);
			} catch {
				return ConversionFailed;
			}
		}

		static object ConvertToEnum (string s, Type type)
		{
			var dot = s.IndexOf ('.');
			if (dot < 0)
				return ConversionFailed;

			var fn = type.FullName + ".";
			if (s.StartsWith (fn, StringComparison.Ordinal)) {
				s = s.Substring (fn.Length);
			} else if (s.StartsWith (type.Name, StringComparison.Ordinal) && s [type.Name.Length] == '.') {
				s = s.Substring (type.Name.Length + 1);
			}

			try {
				return Enum.Parse (type, s);
			} catch {
				return ConversionFailed;
			}
		}

		static string ConvertResult (object value)
		{
			if (value is string)
				return (string)value;

			var e = value as IEnumerable;
			if (e != null) {
				var sb = new StringBuilder ();
				foreach (var entry in e) {
					if (sb.Length > 0)
						sb.Append (";");

					sb.Append (ConvertResult (entry));
				}

				return sb.ToString ();
			}

			return value == null ? "" : value.ToString ();
		}

		//
		// Returns better candidate for untyped string values. We can really do only
		// preference for string over any other types
		//
		// 1: a is better
		// 2: b is better
		// 0: neither is better
		//
		static int BetterCandidate (ParameterInfo[] a, ParameterInfo[] b)
		{
			int res = 0;
			for (int i = 0; i < a.Length; ++i) {
				var atype = a [i].ParameterType;
				var btype = b [i].ParameterType;

				if (atype == typeof (string) && btype != atype) {
					if (res < 2)
						res = 1;
					continue;
				}

				if (btype == typeof (string) && btype != atype) {
					if (res != 1)
						res = 2;

					continue;
				}
			}

			return res;
		}

		public ITaskItem[] ConvertToITaskItemArray (Project project, ExpressionOptions options)
		{
			throw new NotImplementedException ();
		}
	}
}