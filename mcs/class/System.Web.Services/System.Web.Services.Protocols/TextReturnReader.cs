// 
// System.Web.Services.Protocols.TextReturnReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Net;

namespace System.Web.Services.Protocols {
	public class TextReturnReader : MimeReturnReader {

		ReturnInfo info;
		
		#region Constructors

		public TextReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			Type rt = methodInfo.ReturnType;
			FieldInfo[] fields = rt.GetFields ();
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
			ReturnInfo info = new ReturnInfo ();
			info.ReturnType = rt;
			info.MatchInfos = (MatchInfo[]) matchInfos.ToArray (typeof(MatchInfo));
				
			return info;
		}

		public override void Initialize (object o)
		{
			info = (ReturnInfo) o;
		}

		public override object Read (WebResponse response, Stream responseStream)
		{
			StreamReader sr = new StreamReader (responseStream);
			string text = sr.ReadToEnd ();
			
			object ob = Activator.CreateInstance (info.ReturnType);
			
			foreach (MatchInfo mi in info.MatchInfos)
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
		
		#endregion // Methods
	}
	
	class ReturnInfo
	{
		public Type ReturnType;
		public MatchInfo[] MatchInfos;
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

