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
using System.Runtime.CompilerServices;

namespace System.Diagnostics 
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class PerformanceCounterCategory 
	{
		private string categoryName;
		private string machineName;
#if NET_2_0
		private PerformanceCounterCategoryType type = PerformanceCounterCategoryType.Unknown;
#endif		

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool CategoryDelete (string name);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string CategoryHelpInternal (string category, string machine);

		/* this icall allows a null counter and it will just search for the category */
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool CounterCategoryExists (string counter, string category, string machine);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern bool Create (string categoryName, string categoryHelp,
			PerformanceCounterCategoryType categoryType, CounterCreationData[] items);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern int InstanceExistsInternal (string instance, string category, string machine);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string[] GetCategoryNames (string machine);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string[] GetCounterNames (string category, string machine);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string[] GetInstanceNames (string category, string machine);

		static void CheckCategory (string categoryName) {
			if (categoryName == null)
				throw new ArgumentNullException ("categoryName");
			if (categoryName == "")
				throw new ArgumentException ("categoryName");
		}

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
		public PerformanceCounterCategory (string categoryName, string machineName)
		{
			CheckCategory (categoryName);
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			// TODO checks and whatever else is needed
			this.categoryName = categoryName;
			this.machineName = machineName;
		}

		// may throw InvalidOperationException, Win32Exception
		public string CategoryHelp {
			get {
				string res = CategoryHelpInternal (categoryName, machineName);
				if (res != null)
					return res;
				throw new InvalidOperationException ();
			}
		}

		// may throw ArgumentException (""), ArgumentNullException
		public string CategoryName {
			get {return categoryName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value == "")
					throw new ArgumentException ("value");
				categoryName = value;
			}
		}

		// may throw ArgumentException
		public string MachineName {
			get {return machineName;}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value == "")
					throw new ArgumentException ("value");
				machineName = value;
			}
		}

#if NET_2_0
		public PerformanceCounterCategoryType CategoryType {
			get {
				return type;
			}
		}
#endif

		public bool CounterExists (string counterName)
		{
			return CounterExists (counterName, categoryName, machineName);
		}

		public static bool CounterExists (string counterName, string categoryName)
		{
			return CounterExists (counterName, categoryName, ".");
		}

		// may throw ArgumentNullException, InvalidOperationException
		// (categoryName is "", machine name is bad), Win32Exception
		public static bool CounterExists (string counterName, string categoryName, string machineName)
		{
			if (counterName == null)
				throw new ArgumentNullException ("counterName");
			CheckCategory (categoryName);
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			return CounterCategoryExists (counterName, categoryName, machineName);
		}

#if NET_2_0
		[Obsolete ("Use another overload that uses PerformanceCounterCategoryType instead")]
#endif
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			CounterCreationDataCollection counterData)
		{
			return Create (categoryName, categoryHelp,
				PerformanceCounterCategoryType.Unknown, counterData);
		}

#if NET_2_0
		[Obsolete ("Use another overload that uses PerformanceCounterCategoryType instead")]
#endif
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			string counterName,
			string counterHelp)
		{
			return Create (categoryName, categoryHelp,
				PerformanceCounterCategoryType.Unknown, counterName, counterHelp);
		}

#if NET_2_0
		public
#endif
		static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			PerformanceCounterCategoryType categoryType,
			CounterCreationDataCollection counterData)
		{
			CheckCategory (categoryName);
			if (counterData == null)
				throw new ArgumentNullException ("counterData");
			if (counterData.Count == 0)
				throw new ArgumentException ("counterData");
			CounterCreationData[] items = new CounterCreationData [counterData.Count];
			counterData.CopyTo (items, 0);
			if (!Create (categoryName, categoryHelp, categoryType, items))
				throw new InvalidOperationException ();
			return new PerformanceCounterCategory (categoryName, categoryHelp);
		}

#if NET_2_0
		public
#endif
		static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			PerformanceCounterCategoryType categoryType,
			string counterName,
			string counterHelp)
		{
			CheckCategory (categoryName);
			CounterCreationData[] items = new CounterCreationData [1];
			// we use PerformanceCounterType.NumberOfItems32 as the default type
			items [0] = new CounterCreationData (counterName, counterHelp, PerformanceCounterType.NumberOfItems32);
			if (!Create (categoryName, categoryHelp, categoryType, items))
				throw new InvalidOperationException ();
			return new PerformanceCounterCategory (categoryName, categoryHelp);
		}

		public static void Delete (string categoryName)
		{
			CheckCategory (categoryName);
			if (!CategoryDelete (categoryName))
				throw new InvalidOperationException ();
		}

		public static bool Exists (string categoryName)
		{
			return Exists (categoryName, ".");
		}

		public static bool Exists (string categoryName, string machineName)
		{
			CheckCategory (categoryName);
			return CounterCategoryExists (null, categoryName, machineName);
		}

		public static PerformanceCounterCategory[] GetCategories ()
		{
			return GetCategories (".");
		}

		public static PerformanceCounterCategory[] GetCategories (string machineName)
		{
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			string[] catnames = GetCategoryNames (machineName);
			PerformanceCounterCategory[] cats = new PerformanceCounterCategory [catnames.Length];
			for (int i = 0; i < catnames.Length; ++i)
				cats [i] = new PerformanceCounterCategory (catnames [i], machineName);
			return cats;
		}

		public PerformanceCounter[] GetCounters ()
		{
			return GetCounters ("");
		}

		public PerformanceCounter[] GetCounters (string instanceName)
		{
			string[] countnames = GetCounterNames (categoryName, machineName);
			PerformanceCounter[] counters = new PerformanceCounter [countnames.Length];
			for (int i = 0; i < countnames.Length; ++i) {
				counters [i] = new PerformanceCounter (categoryName, countnames [i], instanceName, machineName);
			}
			return counters;
		}

		public string[] GetInstanceNames ()
		{
			return GetInstanceNames (categoryName, machineName);
		}

		public bool InstanceExists (string instanceName)
		{
			return InstanceExists (instanceName, categoryName, machineName);
		}

		public static bool InstanceExists (string instanceName, string categoryName)
		{
			return InstanceExists (instanceName, categoryName, ".");
		}

		public static bool InstanceExists (string instanceName, string categoryName, string machineName)
		{
			if (instanceName == null)
				throw new ArgumentNullException ("instanceName");
			CheckCategory (categoryName);
			if (machineName == null)
				throw new ArgumentNullException ("machineName");
			int val = InstanceExistsInternal (instanceName, categoryName, machineName);
			if (val == 0)
				return false;
			if (val == 1)
				return true;
			throw new InvalidOperationException ();
		}

		[MonoTODO]
		public InstanceDataCollectionCollection ReadCategory ()
		{
			throw new NotImplementedException ();
		}
	}
}

