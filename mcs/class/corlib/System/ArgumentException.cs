//
// System.ArgumentException.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ComVisible (true)]
	[StructLayout (LayoutKind.Sequential)]
	public class ArgumentException : SystemException
	{
		const int Result = unchecked ((int)0x80070057);

		private string param_name;

		// Constructors
		public ArgumentException ()
			: base (Locale.GetText ("Value does not fall within the expected range."))
		{
			HResult = Result;
		}

		public ArgumentException (string message)
			: base (message)
		{
			HResult = Result;
		}

		public ArgumentException (string message, Exception innerException)
			: base (message, innerException)
		{
			HResult = Result;
		}

		public ArgumentException (string message, string paramName)
			: base (message)
		{
			this.param_name = paramName;
			HResult = Result;
		}

		public ArgumentException (string message, string paramName, Exception innerException)
			: base (message, innerException)
		{
			this.param_name = paramName;
			HResult = Result;
		}

		protected ArgumentException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			param_name = info.GetString ("ParamName");
		}

		// Properties
		public virtual string ParamName {
			get {
				return param_name;
			}
		}

		public override string Message {
			get {
				if (ParamName != null && ParamName.Length != 0)
					return base.Message + Environment.NewLine
						+ Locale.GetText ("Parameter name: ")
						+ ParamName;
				return base.Message;
			}
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("ParamName", ParamName);
		}
	}
}
