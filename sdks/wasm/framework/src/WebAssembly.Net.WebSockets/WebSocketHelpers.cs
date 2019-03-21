using System;
using System.Globalization;
using System.Net.WebSockets;

namespace WebAssembly.Net.WebSockets {
	internal static class WebSocketHelpers {

		private const int CloseStatusCodeAbort = 1006;
		private const int CloseStatusCodeFailedTLSHandshake = 1015;
		private const int InvalidCloseStatusCodesFrom = 0;
		private const int InvalidCloseStatusCodesTo = 999;

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


		internal static void ValidateCloseStatus (WebSocketCloseStatus closeStatus, string statusDescription)
		{
			if (closeStatus == WebSocketCloseStatus.Empty && !string.IsNullOrEmpty (statusDescription)) {
				throw new ArgumentException ($"The close status description '{statusDescription}' is invalid. When using close status code '{WebSocketCloseStatus.Empty}' the description must be null.",
				    nameof (statusDescription));
			}

			int closeStatusCode = (int)closeStatus;

			if ((closeStatusCode >= InvalidCloseStatusCodesFrom &&
			    closeStatusCode <= InvalidCloseStatusCodesTo) ||
			    closeStatusCode == CloseStatusCodeAbort ||
			    closeStatusCode == CloseStatusCodeFailedTLSHandshake) {
				// CloseStatus 1006 means Aborted - this will never appear on the wire and is reflected by calling WebSocket.Abort
				throw new ArgumentException ($"The close status code '{closeStatusCode}' is reserved for system use only and cannot be specified when calling this method.",
				    nameof (closeStatus));
			}

		}


	}
}
