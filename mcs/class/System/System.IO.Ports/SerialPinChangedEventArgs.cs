/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

#if NET_2_0

namespace System.IO.Ports
{
	public enum SerialPinChanges
	{
		Break,      /* A break was detected on input */
		CDChanged,  /* The receive Line Signal Detect (RLSD) signal changed state. */
		CtsChanged, /* The Clear to Send (CTS) signal changed state. */
		DsrChanged, /* The Data Set Ready (DSR) signal changed state. */
		Ring        /* A ring indicator was detected. */
	}

	public class SerialPinChangedEventArgs : EventArgs
	{
		internal SerialPinChangedEventArgs (SerialPinChanges event_type)
		{
			this.event_type = event_type;
		}

		// properties

		public SerialPinChanges EventType
		{
			get {
				return event_type;
			}
		}

		SerialPinChanges event_type;
	}
}

#endif
