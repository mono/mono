//
// VsaException.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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