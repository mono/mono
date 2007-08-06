//
// System.Diagnostics.PerformanceCounterCategory.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

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

using System.Security.Permissions;

namespace System.Diagnostics 
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class PerformanceCounterCategory 
	{
		private string categoryName;
		private string machineName;
#if NET_2_0
		private PerformanceCounterCategoryType type;
#endif

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

#if NET_2_0
		[MonoTODO]
		public PerformanceCounterCategoryType CategoryType {
			get { return type; }
		}
#endif

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
#if NET_2_0
		[Obsolete ("Use another overload that uses PerformanceCounterCategoryType instead")]
#endif
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			CounterCreationDataCollection counterData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("Use another overload that uses PerformanceCounterCategoryType instead")]
#endif
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			string counterName,
			string counterHelp)
		{
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			PerformanceCounterCategoryType categoryType,
			CounterCreationDataCollection counterData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			PerformanceCounterCategoryType categoryType,
			string counterName,
			string counterHelp)
		{
			throw new NotImplementedException ();
		}
#endif

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

