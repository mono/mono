/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

#if NET_2_0

namespace System.IO.Ports {

	public enum SerialReceived
	{
		EofReceived,  /* The event character was received and placed in the input buffer */
		ReceivedChars /* A character was received and placed in the input buffer */
	}

	public class SerialReceivedEventArgs : EventArgs
	{
		internal SerialReceivedEventArgs (SerialReceived event_type)
		{
			this.event_type = event_type;
		}

		// properties

		public SerialReceived EventType
		{
			get {
				return event_type;
			}
		}

		SerialReceived event_type;
	}
}

#endif
