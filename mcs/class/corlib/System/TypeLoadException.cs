//
// System.TypeLoadException.cs
//
// Authors:
//   Sean MacIsaac (macisaac@ximian.com)
//   Duncan Mak  (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class TypeLoadException : SystemException
	{
		const int Result = unchecked ((int)0x80131522);
		
		// Fields
		private string msg;
		private string type;

		// Constructors
		public TypeLoadException ()
			: base (Locale.GetText ("A type load exception has occurred."))
		{
			HResult = Result;
		}

		public TypeLoadException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public TypeLoadException (string message, Exception inner)
			: base (message, inner)
		{
			HResult = Result;
		}

		protected TypeLoadException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			type = info.GetString ("TypeLoadClassName");
		}

		// Properties
		public override string Message {
			get {
				if (type == null)
					return base.Message;

				if (msg == null)
					msg = "Cannot load type '" + type + "'";

				return msg;
			}
		}

		public string TypeName {
			get { 
				if (type == null)
					return "";

				return type;
			}
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			base.GetObjectData (info, context);
			info.AddValue ("TypeLoadClassName", type, typeof (string)); 
			info.AddValue ("TypeLoadAssemblyName", "", typeof (string)); 
			info.AddValue ("TypeLoadMessageArg", "", typeof (string)); 
			info.AddValue ("TypeLoadResourceID", 0, typeof (int)); 
		}
	}
}
