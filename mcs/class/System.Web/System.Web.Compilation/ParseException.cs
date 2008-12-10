//
// System.Web.Compilation.ParseException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Web.Compilation
{
	[Serializable]
	internal class ParseException : HtmlizedException
	{
		ILocation location;
		string fileText;

		ParseException (SerializationInfo info, StreamingContext context)
			: base (info, context)
                {
                }
		
		public ParseException (ILocation location, string message)
			: this (location, message, null)
		{
			location = new Location (location);
		}


		public ParseException (ILocation location, string message, Exception inner)
			: base (message, inner)
		{
			this.location = location;
		}

		public override string Title {
			get { return "Parser Error"; }
		}

		public override string Description {
			get {
				return "Error parsing a resource required to service this request. " +
				       "Review your source file and modify it to fix this error.";
			}
		}

		public override string ErrorMessage {
			get { return Message; }
		}

		public override string SourceFile {
			get { return FileName; }
		}
		
		public override string FileName {
			get {
				if (location == null)
					return null;

				return location.Filename;
			}
		}

		public override string FileText {
			get {
				if (fileText != null)
					return fileText;

				string text = location != null ? location.FileText : null;
				if (text != null && text.Length > 0)
					return text;
				
				if (FileName == null)
					return null;

				//FIXME: encoding
				using (TextReader reader = new StreamReader (FileName))
					fileText = reader.ReadToEnd ();
				return fileText;
			}
		}

		public override int [] ErrorLines {
			get {
				if (location == null)
					return null;

				return new int [] {location.BeginLine, location.EndLine};
			}
		}

		public override bool ErrorLinesPaired {
			get { return true; }
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext ctx)
		{
			base.GetObjectData (info, ctx);
		}
	}
}

