//
// System.Diagnostics.PerformanceCounterCategory.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics 
{

	public sealed class PerformanceCounterCategory 
	{
		private string categoryName;
		private string machineName;

		public PerformanceCounterCategory ()
			: this ("", ".")
		{
		}

		// may throw ArgumentException (""), ArgumentNullException
		public PerformanceCounterCategory (string categoryName)
			: this (categoryName, ".")
		{
		}

		// may throw ArgumentException (""), ArgumentNullException
		[MonoTODO]
		public PerformanceCounterCategory (string categoryName,
			string machineName)
		{
			// TODO checks and whatever else is needed
			this.categoryName = categoryName;
			this.machineName = machineName;
			throw new NotImplementedException ();
		}

		// may throw InvalidOperationException, Win32Exception
		[MonoTODO]
		public string CategoryHelp {
			get {throw new NotImplementedException ();}
		}

		// may throw ArgumentException (""), ArgumentNullException
		[MonoTODO]
		public string CategoryName {
			get {return categoryName;}
			set {
				// TODO needs validity checks
				categoryName = value;
			}
		}

		// may throw ArgumentException
		[MonoTODO]
		public string MachineName {
			get {return machineName;}
			set {
				// TODO needs validity checks
				machineName = value;
			}
		}

		// may throw ArgumentNullException, InvalidOperationException
		// (categoryName isn't set), Win32Exception
		[MonoTODO]
		public bool CounterExists (string counterName)
		{
			throw new NotImplementedException ();
		}

		// may throw ArgumentNullException, InvalidOperationException
		// (categoryName is ""), Win32Exception
		[MonoTODO]
		public static bool CounterExists (string counterName, 
			string categoryName)
		{
			throw new NotImplementedException ();
		}

		// may throw ArgumentNullException, InvalidOperationException
		// (categoryName is "", machine name is bad), Win32Exception
		[MonoTODO]
		public static bool CounterExists (string counterName, 
			string categoryName,
			string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			CounterCreationDataCollection counterData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			string counterName,
			string counterHelp)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Delete (string categoryName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool Exists (string categoryName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool Exists (string categoryName, 
			string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PerformanceCounterCategory[] GetCategories ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PerformanceCounterCategory[] GetCategories (
			string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PerformanceCounter[] GetCounters ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PerformanceCounter[] GetCounters (string instanceName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string[] GetInstanceNames ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool InstanceExists (string instanceName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool InstanceExists (string instanceName, 
			string categoryName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool InstanceExists (string instanceName, 
			string categoryName,
			string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public InstanceDataCollectionCollection ReadCategory ()
		{
			throw new NotImplementedException ();
		}
	}
}

