//
// System.Diagnostics.PerformanceCounter.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Diagnostics {

	// must be safe for multithreaded operations
	public class PerformanceCounter : Component, ISupportInitialize {

		private string categoryName;
		private string counterName;
		private string instanceName;
		private string machineName;
		private bool readOnly;

		[MonoTODO("Find the actual value")]
		public static int DefaultFileMappingSize = 0x80000;

		// set catname, countname, instname to "", machname to "."
		public PerformanceCounter ()
		{
			categoryName = counterName = instanceName = "";
			machineName = ".";
		}

		// throws: InvalidOperationException (if catName or countName
		// is ""); ArgumentNullException if either is null
		// sets instName to "", machname to "."
		public PerformanceCounter (String categoryName, 
			string counterName)
			: this (categoryName, counterName, false)
		{
		}

		public PerformanceCounter (string categoryName, 
			string counterName,
			bool readOnly)
			: this (categoryName, counterName, "", readOnly)
		{
		}

		public PerformanceCounter (string categoryName,
			string counterName,
			string instanceName)
			: this (categoryName, counterName, instanceName, false)
		{
		}

		public PerformanceCounter (string categoryName,
			string counterName,
			string instanceName,
			bool readOnly)
		{

			CategoryName = categoryName;
			CounterName = counterName;

			if (categoryName == "" || counterName == "")
				throw new InvalidOperationException ();

			InstanceName = instanceName;
			this.instanceName = instanceName;
			this.machineName = ".";
			this.readOnly = readOnly;
		}

		public PerformanceCounter (string categoryName,
			string counterName,
			string instanceName,
			string machineName)
			: this (categoryName, counterName, instanceName, false)
		{
			this.machineName = machineName;
		}

		// may throw ArgumentNullException
		public string CategoryName {
			get {return categoryName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("categoryName");
				categoryName = value;
			}
		}

//		// may throw InvalidOperationException
//		[MonoTODO]
//		public string CounterHelp {
//			get {return "";}
//		}
//
		// may throw ArgumentNullException
		public string CounterName {
			get {return counterName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("counterName");
				counterName = value;
			}
		}

//		// may throw InvalidOperationException
//		[MonoTODO]
//		public PerformanceCounterType CounterType {
//			get {return 0;}
//		}
//
		public string InstanceName {
			get {return instanceName;}
			set {instanceName = value;}
		}

//		// may throw ArgumentException if machine name format is wrong
//		[MonoTODO("What's the machine name format?")]
//		public string MachineName {
//			get {return machineName;}
//			set {machineName = value;}
//		}
//
//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public long RawValue {
//			get {return 0;}
//			set {
//				throw new NotImplementedException ();
//			}
//		}
//
//		public bool ReadOnly {
//			get {return readOnly;}
//			set {readOnly = value;}
//		}
//
		[MonoTODO]
		public void BeginInit ()
		{
			throw new NotImplementedException ();
		}

//		[MonoTODO]
//		public void Close ()
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		public static void CloseSharedResources ()
//		{
//			throw new NotImplementedException ();
//		}
//
//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public long Decrement ()
//		{
//			throw new NotImplementedException ();
//		}
//
//		[MonoTODO]
//		protected override void Dispose (bool disposing)
//		{
//			throw new NotImplementedException ();
//		}
//
		[MonoTODO]
		public void EndInit ()
		{
			throw new NotImplementedException ();
		}

//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public long Increment ()
//		{
//			throw new NotImplementedException ();
//		}
//
//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public long IncrementBy (long value)
//		{
//			throw new NotImplementedException ();
//		}
//
//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public CounterSample NextSample ()
//		{
//			throw new NotImplementedException ();
//		}
//
//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public float NextValue ()
//		{
//			throw new NotImplementedException ();
//		}
//
//		// may throw InvalidOperationException, Win32Exception
//		[MonoTODO]
//		public void RemoveInstance ()
//		{
//			throw new NotImplementedException ();
//		}
	}
}

