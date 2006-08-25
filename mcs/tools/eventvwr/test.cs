using System;
using System.Diagnostics;

public class Test
{
	public static void Main ()
	{
		if (!EventLog.SourceExists ("pipeline"))
			EventLog.CreateEventSource ("pipeline", "XSP");

		using (EventLog log = new EventLog ("XSP", ".", "pipeline")) {
			log.WriteEntry ("A test message.", 
				EventLogEntryType.Information, 5);
			log.WriteEntry ("Some test message.",
				EventLogEntryType.Warning, 4);
			log.WriteEntry ("Another test message.",
				EventLogEntryType.Error, 1300);
		}

		if (!EventLog.SourceExists ("XPlatUI"))
			EventLog.CreateEventSource ("XPlatUI", "MWF");

		using (EventLog log = new EventLog ("MWF", ".", "XPlatUI")) {
			log.WriteEntry ("Bug #56093 fixed.",
				EventLogEntryType.Information);
			log.WriteEntry ("New bug reported.",
				EventLogEntryType.Warning);
			log.WriteEntry ("Build failure.",
				EventLogEntryType.Error);
		}
	}
}
