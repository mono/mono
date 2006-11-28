// 
// System.Web.Services.Protocols.PatternMatcher.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Web.Services;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections;

namespace System.Web.Services.Protocols 
{
	public sealed class PatternMatcher 
	{
		Type _returnType;
		MatchInfo[] _matchInfos;
		
		public PatternMatcher (Type type) 
		{
			_returnType = type;
			
			FieldInfo[] fields = type.GetFields ();
			ArrayList matchInfos = new ArrayList ();
			
			foreach (FieldInfo field in fields)
			{
				object[] ats = field.GetCustomAttributes (typeof(MatchAttribute), true);
				if (ats.Length == 0) continue;
				
				MatchInfo mi = new MatchInfo ();
				mi.Field = field;
				mi.Match = (MatchAttribute) ats[0];
				
				RegexOptions opts = RegexOptions.Multiline;
				if (mi.Match.IgnoreCase) opts |= RegexOptions.IgnoreCase;
				mi.Regex = new Regex (mi.Match.Pattern, opts);
				
				matchInfos.Add (mi);
			}
			_matchInfos = (MatchInfo[]) matchInfos.ToArray (typeof(MatchInfo));
		}
		
		public object Match (string text)
		{
			object ob = Activator.CreateInstance (_returnType);
			
			foreach (MatchInfo mi in _matchInfos)
			{
				MatchCollection matches = mi.Regex.Matches (text);
				
				object res = null;
				
				if (mi.Field.FieldType.IsArray)
				{
					int max = mi.Match.MaxRepeats;
					if (max == -1) max = matches.Count;
					
					Type elemType = mi.Field.FieldType.GetElementType();
					Array array = Array.CreateInstance (elemType, max);
					for (int n=0; n<max; n++)
						array.SetValue (mi.GetMatchValue (matches[n], elemType), n);
					res = array;
				}
				else if (matches.Count > 0)
					res = mi.GetMatchValue (matches[0], mi.Field.FieldType);
					
				mi.Field.SetValue (ob, res);
			}
			return ob;
		}

	}

	class MatchInfo
	{
		public FieldInfo Field;
		public MatchAttribute Match;
		public Regex Regex;
		
		const string GroupError = "{0} is not a valid group index for match '{1}'. The highest valid group index for this match is {2}";
		const string CaptureError = "{0} is not a valid capture index for match '{1}'. The highest valid capture index for this match is {2}";
		
		public object GetMatchValue (Match match, Type castType)
		{
			if (Match.Group >= match.Groups.Count)
				throw new Exception (string.Format (GroupError, Match.Group, Field.Name, match.Groups.Count-1));
				
			Group group = match.Groups [Match.Group];
			if (Match.Capture >= group.Captures.Count)
				throw new Exception (string.Format (CaptureError, Match.Capture, Field.Name, group.Captures.Count-1));
				
			string val = group.Captures [Match.Capture].Value;
			return Convert.ChangeType (val, castType);
		}
	}

}
