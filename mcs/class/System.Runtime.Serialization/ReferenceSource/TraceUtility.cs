//
// bare-bones based implementation based on the references
// from the Microsoft reference source code to get things to build
//
namespace System.Runtime.Serialization.Diagnostics {
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

	class TraceRecord {
	}
	
	static class TraceUtility {
		internal static void Trace(TraceEventType severity, int traceCode, string traceDescription)
		{
			Console.WriteLine ("Serialization/Trace: {0} {1} {2}", severity, traceCode, traceDescription);
		}
		
		internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record)
		{
			Console.WriteLine ("Serialization/Trace: {0} {1} {2} {3}", severity, traceCode, traceDescription, record.ToString ());
		}

		internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record, Exception exception)
		{
			Console.WriteLine ("Serialization/Trace: {0} {1} {2} {3} {4}", severity, traceCode, traceDescription, record.ToString (), exception);
		}
		
	}
}
