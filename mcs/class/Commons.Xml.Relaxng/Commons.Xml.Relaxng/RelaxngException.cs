//
// Commons.Xml.Relaxng.General.cs
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
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	public class RelaxngException : Exception
	{
//		string debugXml;

		public RelaxngException () : base () {}
		public RelaxngException (string message) : base (message) {}
		public RelaxngException (string message, Exception innerException)
			: base (message, innerException) {}
		internal RelaxngException (string message, RdpPattern invalidatedPattern)
			: base (message)
		{
//			debugXml = RdpUtil.DebugRdpPattern (invalidatedPattern, new Hashtable ());
		}
	}

}

