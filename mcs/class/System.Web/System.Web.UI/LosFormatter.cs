//
// System.Web.UI.LosFormatter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class LosFormatter {

		ObjectStateFormatter osf = new ObjectStateFormatter ();
		bool disable_mac = true;
		HashAlgorithm algo;
		
		public LosFormatter ()
		{
		}

#if NET_1_1
		public LosFormatter (bool enableMac, string macKeyModifier)
			: this (enableMac, Convert.FromBase64String (macKeyModifier))
		{
		}
#endif
		[MonoTODO]
#if NET_2_0
		public
#else
		internal
#endif
		LosFormatter (bool enableMac, byte[] macKeyModifier)
		{
			this.disable_mac = !enableMac;
			if (enableMac)
				algo = new HMACSHA1 (macKeyModifier);
		}

		void ValidateInput (byte [] data, int offset, int size)
		{
			int hash_size = algo.HashSize / 8;
			if (size != 0 && size < hash_size)
				throw new HttpException ("Unable to validate data.");

			int data_length = size - hash_size;
			MemoryStream data_stream = new MemoryStream (data, offset, data_length, false, false);
			byte [] hash = algo.ComputeHash (data_stream);
			for (int i = 0; i < hash_size; i++) {
				if (hash [i] != data [data_length + i])
					throw new HttpException ("Unable to validate data.");
			}
		}

		public object Deserialize (Stream stream)
		{
			if (disable_mac)
				return osf.Deserialize (stream);

			byte [] bytes = new byte [stream.Length >= 0 ? stream.Length : 2048];
			MemoryStream ms = null;
			if ((stream is MemoryStream) && stream.Position == 0) {
				// We save allocating a new stream and reading in this case.
				ms = (MemoryStream) stream;
			} else {
				ms = new MemoryStream ();
				int n;
				while ((n = stream.Read (bytes, 0, bytes.Length)) > 0)
					ms.Write (bytes, 0, n);
			}

			byte [] buffer = ms.GetBuffer ();
			int length = (int) ms.Length;
			ValidateInput (buffer, 0, length);
			return osf.Deserialize (new MemoryStream (buffer, 0, length, false, false));
		}

		public object Deserialize (TextReader input)
		{
			if (input == null)
				throw new ArgumentNullException ("input");

			return Deserialize (input.ReadToEnd ());
		}

		public object Deserialize (string input)
		{
			if (disable_mac)
				return osf.Deserialize (input);

			byte [] input_bytes = Convert.FromBase64String (input);
			ValidateInput (input_bytes, 0, input_bytes.Length);
			return osf.Deserialize (new MemoryStream (input_bytes, 0, input_bytes.Length, false, false));
		}

		void SerializeAndHash (MemoryStream ms, object value)
		{
			osf.Serialize (ms, value);
			if (ms.Length == 0)
				return;

			byte [] hash = algo.ComputeHash (ms.GetBuffer (), 0, (int) ms.Length);
			ms.Write (hash, 0, hash.Length);
		}

		public void Serialize (Stream stream, object value)
		{
			if (disable_mac) {
				osf.Serialize (stream, value);
				return;
			}

			MemoryStream ms = null;
			if ((stream is MemoryStream) && stream.Position == 0) {
				// We save allocating a new stream and reading in this case.
				ms = (MemoryStream) stream;
			} else {
				ms = new MemoryStream ();
			}

			SerializeAndHash (ms, value);
			if (ms != stream)
				ms.WriteTo (stream);
		}

		public void Serialize (TextWriter output, object value)
		{
			if (output == null)
				throw new ArgumentNullException ("output");
			
			if (disable_mac) {
				output.Write (osf.Serialize (value));
				return;
			}

			MemoryStream ms = new MemoryStream ();
			SerializeAndHash (ms, value);
			output.Write (Convert.ToBase64String (ms.GetBuffer (), 0, (int) ms.Length));
		}	
	}
}

