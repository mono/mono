/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

#if NET_2_0

namespace System.IO.Ports
{
	public enum SerialErrors
	{
		Frame,    /* The hardware detected a framing error */
		Overrun,  /* A character-buffer overrun has occurred. The next character will be lost. */
		RxOver,   /* An input buffer overflow has occurred.  There is
			   * either no room in the input buffer, or a character
			   * was received after the end-of-file (EOF)
			   * character. */
		RxParity, /* The hardware detected a parity error. */
		TxFull    /* The application tried to transmit a character, but
			   * the output buffer was full. */
	}

	public class SerialErrorEventArgs : EventArgs
	{

		internal SerialErrorEventArgs (SerialErrors event_type)
		{
			this.event_type = event_type;
		}

		// properties

		public SerialErrors EventType
		{
			get {
				return event_type;
			}
		}

		SerialErrors event_type;
	}
}

#endif
