//
// System.Web.UI.LosFormatter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Configuration;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Web.Util;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class LosFormatter
	{
		ObjectStateFormatter osf;
		
		public LosFormatter ()
		{
			osf = new ObjectStateFormatter ();
		}

		public LosFormatter (bool enableMac, string macKeyModifier) : this (enableMac, String.IsNullOrEmpty (macKeyModifier) ? null : Encoding.ASCII.GetBytes (macKeyModifier))
		{
		}

		public LosFormatter (bool enableMac, byte[] macKeyModifier)
		{
			osf = new ObjectStateFormatter ();
			if (enableMac && (macKeyModifier != null)) {
				SetMacKey (macKeyModifier);
			}
		}

		private void SetMacKey (byte[] macKeyModifier)
		{
			try {
				osf.Section.ValidationKey = MachineKeySectionUtils.GetHexString (macKeyModifier);
			}
			catch (ArgumentException) {
			}
			catch (ConfigurationErrorsException) {
				// bad key (e.g. size), default key will be used
			}
		}

		public object Deserialize (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
#if NET_4_0
			using (StreamReader sr = new StreamReader (stream)) {
				return Deserialize (sr.ReadToEnd ());
			}
#else
			long streamLength = -1;
			if (stream.CanSeek)
				streamLength = stream.Length;
			MemoryStream ms = null;
			if (streamLength  != -1 && (stream is MemoryStream) && stream.Position == 0) {
				// We save allocating a new stream and reading in this case.
				ms = (MemoryStream) stream;
			} else {
				byte [] bytes = new byte [streamLength >= 0 ? streamLength : 2048];
				ms = new MemoryStream ();
				int n;
				while ((n = stream.Read (bytes, 0, bytes.Length)) > 0)
					ms.Write (bytes, 0, n);
				streamLength = ms.Length;
			}
			string b64 = Encoding.ASCII.GetString (ms.GetBuffer (),
				0, (int) streamLength);
			return Deserialize (b64);
#endif
		}

		public object Deserialize (TextReader input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return Deserialize (input.ReadToEnd ());
		}

		public object Deserialize (string input)
		{
			if (input == null)
				return null;

			return osf.Deserialize (input);
		}

		internal string SerializeToBase64 (object value)
		{
			return osf.Serialize (value);
		}

		public void Serialize (Stream stream, object value)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
#if NET_4_0
			if (!stream.CanSeek)
				throw new NotSupportedException ();
#endif
			string b64 = SerializeToBase64 (value);
			byte [] bytes = Encoding.ASCII.GetBytes (b64);
			stream.Write (bytes, 0, bytes.Length);
		}

		public void Serialize (TextWriter output, object value)
		{
			if (output == null)
				throw new ArgumentNullException ("output");

			output.Write (SerializeToBase64 (value));
		}	
	}
}

