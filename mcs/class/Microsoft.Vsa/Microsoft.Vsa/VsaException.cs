//
// VsaException.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Vsa {

	[Serializable]
	public class VsaException : ExternalException {

		public new VsaError ErrorCode {
			get { return (VsaError) HResult; }
		}

		public VsaException (VsaError error)
		{
			HResult = (int) error;
		}

		public VsaException (SerializationInfo info, StreamingContext context)
		{
			HResult = info.GetInt32 ("VsaException_HResult");
			HelpLink = info.GetString ("VsaException_HelpLink");
			Source = info.GetString ("VsaException_Source");
		}

		public VsaException (VsaError error, string message)
			: base (message, (int) error)
		{
		}

		public VsaException (VsaError error, string message, Exception innerexception)
			: base (message, innerexception)
		{
			HResult = (int) error;			
		}

		public override void GetObjectData (SerializationInfo info, 
						    StreamingContext context)
		{
			info.AddValue ("VsaException_HResult", HResult);
			info.AddValue ("VsaException_HelpLink", HelpLink);
			info.AddValue ("VsaException_Source", Source);			
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("Microsoft.Vsa.VsaException: ");
			sb.Append (System.Enum.GetName (typeof (VsaError), (VsaError) HResult));
			sb.Append (" (0x" + String.Format ("{0,8:X}", HResult) + "): ");
			sb.Append (Message);

			return sb.ToString ();
		}
	}
}