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
using System.IO;
using System.Net;

namespace System.Web.Services.Protocols {
	public class TextReturnReader : MimeReturnReader {

		PatternMatcher _matcher;
		
		#region Constructors

		public TextReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			return new PatternMatcher (methodInfo.ReturnType);
		}

		public override void Initialize (object o)
		{
			_matcher = (PatternMatcher) o;
		}

		public override object Read (WebResponse response, Stream responseStream)
		{
			StreamReader sr = new StreamReader (responseStream);
			string text = sr.ReadToEnd ();
			return _matcher.Match (text);
		}
		
		#endregion // Methods
	}	
}

