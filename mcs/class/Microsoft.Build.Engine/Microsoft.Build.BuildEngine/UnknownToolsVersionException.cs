//
// UnknownToolsVersionException.cs
//
// Author:
// 	Ankit Jain (jankit@novell.com)
// 
// Copyright 2010 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Build.BuildEngine {
	[Serializable]
	internal class UnknownToolsVersionException : Exception {
		string message;

		public UnknownToolsVersionException (string toolsVersion)
		{
			this.message = GetErrorMessage (toolsVersion);
		}

		public UnknownToolsVersionException (string toolsVersion, string message)
		{
			this.message = String.Format ("{0}. {1}", message, GetErrorMessage (toolsVersion));
		}

		public UnknownToolsVersionException (string message,
					Exception innerException)
			: base (message, innerException)
		{
		}

		protected UnknownToolsVersionException (SerializationInfo info,
					   StreamingContext context)
			: base (info, context)
		{
		}

		public override string Message {
			get { return message; }
		}

		string GetErrorMessage (string toolsVersion)
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("Unknown tools version: '{0}' . Known versions:", toolsVersion);

			foreach (var ts in Engine.GlobalEngine.Toolsets)
				sb.AppendFormat (" '{0}'", ts.ToolsVersion);

			return sb.ToString ();
		}
	}
}

#endif
