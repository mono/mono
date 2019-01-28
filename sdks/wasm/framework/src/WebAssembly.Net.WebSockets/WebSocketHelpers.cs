using System;
using System.Globalization;
using System.Threading;

namespace WebAssembly.Net.WebSockets {
	internal static class WebSocketHelpers {

		private const string Separators = "()<>@,;:\\\"/[]?={} ";

		private static readonly ArraySegment<byte> s_EmptyPayload = new ArraySegment<byte> (new byte [] { }, 0, 0);

		internal static ArraySegment<byte> EmptyPayload {
			get { return s_EmptyPayload; }
		}



		internal static void ValidateSubprotocol (string subProtocol)
		{
			if (string.IsNullOrWhiteSpace (subProtocol)) {
				throw new ArgumentException ("Protocol string can not be null or empty", nameof (subProtocol));
			}

			char [] chars = subProtocol.ToCharArray ();
			string invalidChar = null;
			int i = 0;
			while (i < chars.Length) {
				char ch = chars [i];
				if (ch < 0x21 || ch > 0x7e) {
					invalidChar = string.Format (CultureInfo.InvariantCulture, "[{0}]", (int)ch);
					break;
				}

				if (Separators.IndexOf (ch) >= 0) {
					invalidChar = ch.ToString ();
					break;
				}

				i++;
			}

			if (invalidChar != null) {
				throw new ArgumentException ($"Invalid char: {invalidChar} in Protocol string");
			}
		}

		internal static void ValidateOptions (string subProtocol)
		{
			// We allow the subProtocol to be null. Validate if it is not null.
			if (subProtocol != null) {
				ValidateSubprotocol (subProtocol);
			}

		}

	}
}
