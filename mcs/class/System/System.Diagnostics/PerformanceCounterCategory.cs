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
		private PerformanceCounterCategoryType type = PerformanceCounterCategoryType.Unknown;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static unsafe extern bool CategoryDelete_icall (char* name, int name_length);

		static unsafe bool CategoryDelete (string name)
		{
			fixed (char* fixed_name = name)
				return CategoryDelete_icall (fixed_name, name?.Length ?? 0);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private unsafe static extern string CategoryHelp_icall (char* category, int category_length);

		static unsafe string CategoryHelpInternal (string category)
		{
			fixed (char* fixed_category = category)
				return CategoryHelp_icall (fixed_category, category?.Length ?? 0);
		}

		/* this icall allows a null counter and it will just search for the category */
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static unsafe extern bool CounterCategoryExists_icall (char* counter, int counter_length,
			char* category, int category_length);

		static unsafe bool CounterCategoryExists (string counter, string category)
		{
			fixed (char* fixed_counter = counter,
				     fixed_category = category)
				return CounterCategoryExists_icall (fixed_counter, counter?.Length ?? 0,
					fixed_category, category?.Length ?? 0);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static private unsafe extern bool Create_icall (char* categoryName, int categoryName_length,
			char* categoryHelp, int categoryHelp_length,
			PerformanceCounterCategoryType categoryType, CounterCreationData[] items);

		static unsafe bool Create (string categoryName, string categoryHelp,
			PerformanceCounterCategoryType categoryType, CounterCreationData[] items)
		{
			fixed (char* fixed_categoryName = categoryName,
				     fixed_categoryHelp = categoryHelp)
				return Create_icall (fixed_categoryName, categoryName?.Length ?? 0,
					fixed_categoryHelp, categoryHelp?.Length ?? 0, categoryType, items);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static private unsafe extern bool InstanceExistsInternal_icall (char* instance, int instance_length,
			char* category, int category_length);

		static unsafe bool InstanceExistsInternal (string instance, string category)
		{
			fixed (char* fixed_instance = instance,
				     fixed_category = category)
				return InstanceExistsInternal_icall (fixed_instance, instance?.Length ?? 0,
					fixed_category, category?.Length ?? 0);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string[] GetCategoryNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static private unsafe extern string[] GetCounterNames_icall (char* category, int category_length);

		static unsafe string[] GetCounterNames (string category)
		{
			fixed (char* fixed_category = category)
				return GetCounterNames_icall (fixed_category, category?.Length ?? 0);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static private unsafe extern string[] GetInstanceNames_icall (char* category, int category_length);

		static unsafe string[] GetInstanceNames (string category)
		{
			fixed (char* fixed_category = category)
				return GetInstanceNames_icall (fixed_category, category?.Length ?? 0);
		}

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

		static bool IsValidMachine (string machine)
		{ // no support for counters on other machines
			return machine == ".";
		}

		// may throw InvalidOperationException, Win32Exception
		public string CategoryHelp {
			get {
				string res = null;
				if (IsValidMachine (machineName))
					res = CategoryHelpInternal (categoryName);
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

		public PerformanceCounterCategoryType CategoryType {
			get {
				return type;
			}
		}

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
			return IsValidMachine (machineName)
				&& CounterCategoryExists (counterName, categoryName);
		}

		[Obsolete ("Use another overload that uses PerformanceCounterCategoryType instead")]
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			CounterCreationDataCollection counterData)
		{
			return Create (categoryName, categoryHelp,
				PerformanceCounterCategoryType.Unknown, counterData);
		}

		[Obsolete ("Use another overload that uses PerformanceCounterCategoryType instead")]
		public static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			string counterName,
			string counterHelp)
		{
			return Create (categoryName, categoryHelp,
				PerformanceCounterCategoryType.Unknown, counterName, counterHelp);
		}

		public static PerformanceCounterCategory Create (
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

		public static PerformanceCounterCategory Create (
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
			return IsValidMachine (machineName) &&
				CounterCategoryExists (null, categoryName);
		}

		public static PerformanceCounterCategory[] GetCategories ()
		{
			return GetCategories (".");
		}

		public static PerformanceCounterCategory[] GetCategories (string machineName)
		{
			if (machineName == null)
				throw new ArgumentNullException ("machineName");

			if (!IsValidMachine (machineName))
				return Array.Empty<PerformanceCounterCategory>();

			string[] catnames = GetCategoryNames ();
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
			if (!IsValidMachine (machineName))
				return Array.Empty<PerformanceCounter>();
			string[] countnames = GetCounterNames (categoryName);
			PerformanceCounter[] counters = new PerformanceCounter [countnames.Length];
			for (int i = 0; i < countnames.Length; ++i) {
				counters [i] = new PerformanceCounter (categoryName, countnames [i], instanceName, machineName);
			}
			return counters;
		}

		public string[] GetInstanceNames ()
		{
			if (!IsValidMachine (machineName))
				return Array.Empty<string>();
			return GetInstanceNames (categoryName);
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

			//?FIXME: machine appears to be wrong
			//if (!IsValidMachine (machineName))
			//return false;

			return InstanceExistsInternal (instanceName, categoryName);
		}

		[MonoTODO]
		public InstanceDataCollectionCollection ReadCategory ()
		{
			throw new NotImplementedException ();
		}
	}
}

