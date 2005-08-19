//
// RegExpConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) Copyright 2005, Novell Inc (http://novell.com)
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
using System.Text.RegularExpressions;

namespace Microsoft.JScript {

	public class RegExpConstructor : ScriptFunction {

		internal static RegExpConstructor Ctr = new RegExpConstructor ();

		internal RegExpConstructor ()
		{
			_prototype = RegExpPrototype.Proto;
			_length = 2;
			name = "RegExp";
			AddField ("$_");
			AddField ("$&");
			AddField ("$+");
			AddField ("$`");
			AddField ("$'");
			AddField ("$*", false);
		}

		static internal void UpdateLastMatch (Match md, string input)
		{
			GroupCollection groups = md.Groups;
			int n = groups.Count - 1;
			string left_context = Convert.ToString (input).Substring (0, md.Index);
			string right_context = Convert.ToString (input).Substring (md.Index + md.Length);

			Ctr._lastmatch = md;
			Ctr.GetField ("$_").SetValue ("$_", input);
			Ctr.GetField ("$&").SetValue ("$&", md.Value);
			Ctr.GetField ("$+").SetValue ("$+", n > 0 ? groups [n].Value : "");
			Ctr.GetField ("$`").SetValue ("$`", left_context);
			Ctr.GetField ("$'").SetValue ("$'", right_context);
		}

		public Object Construct (string pattern, bool ignoreCase, bool global, bool multiLine)
		{
			RegExpObject re = new RegExpObject (pattern, ignoreCase, global, multiLine);
			return re;
		}

		//
		// Invoked when we do: new RegExp (...)
		//
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new RegExpObject CreateInstance (params object [] args)
		{
			return Invoke (args);
		}

		//
		// Invoked when we do: x = RegExp (...)
		//
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public RegExpObject Invoke (params object [] args)
		{
			if (args != null) {
				int length = args.Length;
				if (length == 0)
					return new RegExpObject ("", false, false, false);
				else if (length > 0) {
					object o = args [0];
					if (o is RegExpObject)
						return (RegExpObject) o;

					string flags = "";
					if (length > 1)
						flags += Convert.ToString (args [1]);
					return new RegExpObject (Convert.ToString (args [0]),
								 flags.IndexOfAny (new char [] {'i'}) > -1,
								 flags.IndexOfAny (new char [] {'g'}) > -1,
								 flags.IndexOfAny (new char [] {'m'}) > -1);
				}
			}
			throw new NotImplementedException ();
		}

		#region Properties $1 .. $9
		public Object dollar_1 {
			get { return _lastmatch.Groups [1].Value; }
		}

		public Object dollar_2 {
			get { return _lastmatch.Groups [2].Value; }
		}

		public Object dollar_3 {
			get { return _lastmatch.Groups [3].Value; }
		}

		public Object dollar_4 {
			get { return _lastmatch.Groups [4].Value; }
		}

		public Object dollar_5 {
			get { return _lastmatch.Groups [5].Value; }
		}

		public Object dollar_6 {
			get { return _lastmatch.Groups [6].Value; }
		}

		public Object dollar_7 {
			get { return _lastmatch.Groups [7].Value; }
		}

		public Object dollar_8 {
			get { return _lastmatch.Groups [8].Value; }
		}

		public Object dollar_9
		{
			get { return _lastmatch.Groups [9].Value; }
		}
		#endregion

		public Object index {
			get { throw new NotImplementedException (); }
		}

		public Object input {
			get {
				return RegExpConstructor.Ctr.GetField ("$_").GetValue ("$_");
			}

			set {
				RegExpConstructor.Ctr.GetField ("$_").SetValue ("$_", value);
			}
		}

		public Object lastIndex {
			get { throw new NotImplementedException (); }
		}

		internal Match _lastmatch = null;

		public Object lastMatch {
			get { return _lastmatch.Value; }
		}

		public Object lastParen {
			get {
				GroupCollection groups = _lastmatch.Groups;
				int n = groups.Count - 1;
				if (n > 0)
					return groups [n].Value;
				else
					return "";
			}
		}

		public Object leftContext {
			get {
				return RegExpConstructor.Ctr.GetField ("$`").GetValue ("$`");
			}
		}

		public Object rightContext {
			get {
				return RegExpConstructor.Ctr.GetField ("$'").GetValue ("$'");
			}
		}

		public Object multiline {
			get {
				return Convert.ToBoolean (RegExpConstructor.Ctr.GetField ("$*").GetValue ("$*"));
			}
			set {
				RegExpConstructor.Ctr.GetField ("$*").SetValue ("$*", value);
			}
		}
	}
}
