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
		unsafe static extern bool CategoryDelete (char* category, int category_length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe static extern string CategoryHelpInternal (char* category, int category_length);

		/* this icall allows a null counter and it will just search for the category */
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe static extern bool CounterCategoryExists (char* counter, int counter_length,
			char* category, int category_length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe static extern bool Create (char* categoryName, int categoryName_length,
			char* categoryHelp, int categoryHelp_length,
			PerformanceCounterCategoryType categoryType, CounterCreationData[] items);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe static extern bool InstanceExistsInternal (char* instance, int instance_length,
			char* category, int category_length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string[] GetCategoryNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe static extern string[] GetCounterNames (char* category, int category_length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		unsafe static extern string[] GetInstanceNames (char* category, int category_length);

		static int StringLength (string a)
		{
			return a?.Length ?? 0;
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
				if (IsValidMachine (machineName)) {
					unsafe {
						fixed (char* fixed_categoryName = categoryName)
							res = CategoryHelpInternal(fixed_categoryName, categoryName.Length);
					}
				}
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
			if (!IsValidMachine (machineName))
				return false;
			unsafe {
				fixed (char* fixed_counterName = counterName,
					     fixed_categoryName = categoryName) {
					return CounterCategoryExists (fixed_counterName, counterName.Length,
								      fixed_categoryName, categoryName.Length);
				}
			}
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

		static PerformanceCounterCategory Create (
			string categoryName,
			string categoryHelp,
			PerformanceCounterCategoryType categoryType,
			CounterCreationData[] counterData)
		{
			unsafe {
				fixed (char* fixed_categoryName = categoryName,
					     fixed_categoryHelp = (categoryHelp != null) ? categoryHelp : null) {
					if (!Create (fixed_categoryName, categoryName.Length,
						     fixed_categoryHelp, StringLength (categoryHelp),
						     categoryType, counterData)) {
						throw new InvalidOperationException ();
					}
				}
			}
			return new PerformanceCounterCategory (categoryName, categoryHelp);
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
			return Create (categoryName, categoryHelp, categoryType, items);
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
			return Create (categoryName, categoryHelp, categoryType, items);
		}

		public static void Delete (string categoryName)
		{
			CheckCategory (categoryName);
			unsafe {
				fixed (char* fixed_categoryName = categoryName) {
					if (!CategoryDelete (fixed_categoryName, categoryName.Length))
						throw new InvalidOperationException ();
				}
			}
		}

		public static bool Exists (string categoryName)
		{
			return Exists (categoryName, ".");
		}

		public static bool Exists (string categoryName, string machineName)
		{
			CheckCategory (categoryName);
			if (!IsValidMachine (machineName))
				return false;
			unsafe {
				fixed (char* fixed_categoryName = categoryName)
					return CounterCategoryExists (null, 0, fixed_categoryName, categoryName.Length);
			}
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
			string[] countnames;
			unsafe {
				fixed (char* fixed_categoryName = (categoryName != null) ? categoryName : null)
					countnames = GetCounterNames (fixed_categoryName, StringLength (categoryName));
			}

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
			unsafe {
				fixed (char* fixed_categoryName = (categoryName != null) ? categoryName : null)
					return GetInstanceNames (fixed_categoryName, StringLength (categoryName));
			}
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

			//FIXME: machine appears to be wrong
			//if (!IsValidMachine (machineName))
			//return false;

			unsafe {
				fixed (char* fixed_instanceName = instanceName,
					     fixed_categoryName = categoryName) {
					return InstanceExistsInternal (fixed_instanceName, instanceName.Length,
								       fixed_categoryName, categoryName.Length);
				}
			}
		}

		[MonoTODO]
		public InstanceDataCollectionCollection ReadCategory ()
		{
			throw new NotImplementedException ();
		}
	}
}

