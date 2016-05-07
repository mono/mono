//------------------------------------------------------------------------------
// <copyright file="PerformanceCounterCategory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Runtime.Serialization.Formatters;
    using System.ComponentModel;
    using System.Diagnostics;
    using System;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using Microsoft.Win32;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///     A Performance counter category object.
    /// </devdoc>
    [
    HostProtection(Synchronization=true, SharedState=true)
    ]
    public sealed class PerformanceCounterCategory {
        private string categoryName;
        private string categoryHelp;
        private string machineName;
        internal const int MaxCategoryNameLength = 80; 
        internal const int MaxCounterNameLength = 32767;
        internal const int MaxHelpLength = 32767;
        private const string perfMutexName = "netfxperf.1.0";

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PerformanceCounterCategory() {
            machineName = ".";
        }

        /// <devdoc>
        ///     Creates a PerformanceCounterCategory object for given category.
        ///     Uses the local machine.
        /// </devdoc>
        public PerformanceCounterCategory(string categoryName)
            : this(categoryName, ".") {
        }

        /// <devdoc>
        ///     Creates a PerformanceCounterCategory object for given category.
        ///     Uses the given machine name.
        /// </devdoc>
        public PerformanceCounterCategory(string categoryName, string machineName) {
            if (categoryName == null)
                throw new ArgumentNullException("categoryName");

            if (categoryName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "categoryName", categoryName));

            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));

            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read, machineName, categoryName);
            permission.Demand();

            this.categoryName = categoryName;
            this.machineName = machineName;
         }

        /// <devdoc>
        ///     Gets/sets the Category name
        /// </devdoc>
        public string CategoryName {
            get {
                return categoryName;
            }

            set {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidProperty, "CategoryName", value));

                // there lock prevents a ---- between setting CategoryName and MachineName, since this permission 
                // checks depend on both pieces of info. 
                lock (this) {
                    PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read, machineName, value);
                    permission.Demand();

                    this.categoryName = value;
                }
            }
        }

        /// <devdoc>
        ///     Gets/sets the Category help
        /// </devdoc>
        public string CategoryHelp {
            get {
                if (this.categoryName == null)
                    throw new InvalidOperationException(SR.GetString(SR.CategoryNameNotSet));

                if (categoryHelp == null)
                    categoryHelp = PerformanceCounterLib.GetCategoryHelp(this.machineName, this.categoryName);

                return categoryHelp;
            }
        }

        public PerformanceCounterCategoryType CategoryType{
            get {
                CategorySample categorySample = PerformanceCounterLib.GetCategorySample(machineName, categoryName);

                // If we get MultiInstance, we can be confident it is correct.  If it is single instance, though
                // we need to check if is a custom category and if the IsMultiInstance value is set in the registry.
                // If not we return Unknown
                if (categorySample.IsMultiInstance)
                    return PerformanceCounterCategoryType.MultiInstance;
                else {
                    if (PerformanceCounterLib.IsCustomCategory(".", categoryName))
                        return PerformanceCounterLib.GetCategoryType(".", categoryName);
                    else
                        return PerformanceCounterCategoryType.SingleInstance;
                }
            }
        }
        

        /// <devdoc>
        ///     Gets/sets the Machine name
        /// </devdoc>
        public string MachineName {
            get {
                return machineName;
            }
            set {
                if (!SyntaxCheck.CheckMachineName(value))
                    throw new ArgumentException(SR.GetString(SR.InvalidProperty, "MachineName", value));

                // there lock prevents a ---- between setting CategoryName and MachineName, since this permission 
                // checks depend on both pieces of info. 
                lock (this) {
                    if (categoryName != null) {
                        PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read, value, categoryName);
                        permission.Demand();
                    }

                    machineName = value;
                }
            }
        }

        /// <devdoc>
        ///     Returns true if the counter is registered for this category
        /// </devdoc>
        public bool CounterExists(string counterName) {
            if (counterName == null)
                throw new ArgumentNullException("counterName");

            if (this.categoryName == null)
                    throw new InvalidOperationException(SR.GetString(SR.CategoryNameNotSet));

            return PerformanceCounterLib.CounterExists(machineName, categoryName, counterName);
        }

        /// <devdoc>
        ///     Returns true if the counter is registered for this category on the current machine.
        /// </devdoc>
        public static bool CounterExists(string counterName, string categoryName) {
            return CounterExists(counterName, categoryName, ".");
        }

        /// <devdoc>
        ///     Returns true if the counter is registered for this category on a particular machine.
        /// </devdoc>
        public static bool CounterExists(string counterName, string categoryName, string machineName) {
            if (counterName == null)
                throw new ArgumentNullException("counterName");

            if (categoryName == null)
                throw new ArgumentNullException("categoryName");

            if (categoryName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "categoryName", categoryName));

            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));

            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read,  machineName, categoryName);
            permission.Demand();

            return PerformanceCounterLib.CounterExists(machineName, categoryName, counterName);
         }

        /// <devdoc>
        ///     Registers one extensible performance category of type NumberOfItems32 with the system
        /// </devdoc>
        [Obsolete("This method has been deprecated.  Please use System.Diagnostics.PerformanceCounterCategory.Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, string counterName, string counterHelp) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, string counterName, string counterHelp) {
            CounterCreationData customData = new CounterCreationData(counterName, counterHelp, PerformanceCounterType.NumberOfItems32);
            return Create(categoryName, categoryHelp, PerformanceCounterCategoryType.Unknown, new CounterCreationDataCollection(new CounterCreationData [] {customData}));
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, string counterName, string counterHelp) {
            CounterCreationData customData = new CounterCreationData(counterName, counterHelp, PerformanceCounterType.NumberOfItems32);
            return Create(categoryName, categoryHelp, categoryType, new CounterCreationDataCollection(new CounterCreationData [] {customData}));
        }

        /// <devdoc>
        ///     Registers the extensible performance category with the system on the local machine
        /// </devdoc>
        [Obsolete("This method has been deprecated.  Please use System.Diagnostics.PerformanceCounterCategory.Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, CounterCreationDataCollection counterData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, CounterCreationDataCollection counterData) {
            return Create(categoryName, categoryHelp, PerformanceCounterCategoryType.Unknown, counterData);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, CounterCreationDataCollection counterData) {
            if (categoryType < PerformanceCounterCategoryType.Unknown || categoryType > PerformanceCounterCategoryType.MultiInstance)
                throw new ArgumentOutOfRangeException("categoryType");
            if (counterData == null)
                throw new ArgumentNullException("counterData");

            CheckValidCategory(categoryName);
            if (categoryHelp != null) {
                // null categoryHelp is a valid option - it gets set to "Help Not Available" later on.
                CheckValidHelp(categoryHelp);
            }
            string machineName = ".";

            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Administer,  machineName, categoryName);
            permission.Demand();

            SharedUtils.CheckNtEnvironment();

            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                SharedUtils.EnterMutex(perfMutexName, ref mutex);
                if (PerformanceCounterLib.IsCustomCategory(machineName, categoryName) || PerformanceCounterLib.CategoryExists(machineName , categoryName))
                    throw new InvalidOperationException(SR.GetString(SR.PerformanceCategoryExists, categoryName));

                CheckValidCounterLayout(counterData);
                PerformanceCounterLib.RegisterCategory(categoryName, categoryType, categoryHelp, counterData);
                return new PerformanceCounterCategory(categoryName, machineName);
            }
            finally {
                if (mutex != null) {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
        }

        // there is an idential copy of CheckValidCategory in PerformnaceCounterInstaller
        internal static void CheckValidCategory(string categoryName) {
            if (categoryName == null)
                throw new ArgumentNullException("categoryName");

            if (!CheckValidId(categoryName, MaxCategoryNameLength))
                throw new ArgumentException(SR.GetString(SR.PerfInvalidCategoryName, 1, MaxCategoryNameLength));

            // 1026 chars is the size of the buffer used in perfcounter.dll to get this name.  
            // If the categoryname plus prefix is too long, we won't be able to read the category properly. 
            if (categoryName.Length > (1024 - SharedPerformanceCounter.DefaultFileMappingName.Length))
                throw new ArgumentException(SR.GetString(SR.CategoryNameTooLong));
        }

        internal static void CheckValidCounter(string counterName) {
            if (counterName == null)
                throw new ArgumentNullException("counterName");

            if (!CheckValidId(counterName, MaxCounterNameLength))
                throw new ArgumentException(SR.GetString(SR.PerfInvalidCounterName, 1, MaxCounterNameLength));
        }

        // there is an idential copy of CheckValidId in PerformnaceCounterInstaller
        internal static bool CheckValidId(string id, int maxLength) {
            if (id.Length == 0 || id.Length > maxLength)
                return false;

            for (int index = 0; index < id.Length; ++index) {
                char current = id[index];

                if ((index == 0 || index == (id.Length -1)) && current == ' ')
                    return false;

                if (current == '\"')
                    return false;

                if (char.IsControl(current))
                    return false;
            }

            return true;
        }

        internal static void CheckValidHelp(string help) {
            if (help == null)
                throw new ArgumentNullException("help");
            if (help.Length > MaxHelpLength)
                throw new ArgumentException(SR.GetString(SR.PerfInvalidHelp, 0, MaxHelpLength));
        }

        internal static void CheckValidCounterLayout(CounterCreationDataCollection counterData) {
            // Ensure that there are no duplicate counter names being created
            Hashtable h = new Hashtable();
            for (int i = 0; i < counterData.Count; i++) {
                if (counterData[i].CounterName == null || counterData[i].CounterName.Length == 0) {
                    throw new ArgumentException(SR.GetString(SR.InvalidCounterName));
                }
            
                int currentSampleType = (int)counterData[i].CounterType;
                if (    (currentSampleType  == NativeMethods.PERF_AVERAGE_BULK) ||
                        (currentSampleType  == NativeMethods.PERF_100NSEC_MULTI_TIMER) ||
                        (currentSampleType  == NativeMethods.PERF_100NSEC_MULTI_TIMER_INV) ||
                        (currentSampleType  == NativeMethods.PERF_COUNTER_MULTI_TIMER) ||
                        (currentSampleType  == NativeMethods.PERF_COUNTER_MULTI_TIMER_INV) ||
                        (currentSampleType  == NativeMethods.PERF_RAW_FRACTION) ||
                        (currentSampleType  == NativeMethods.PERF_SAMPLE_FRACTION) ||
                        (currentSampleType  == NativeMethods.PERF_AVERAGE_TIMER)) {
            
                    if (counterData.Count <= (i + 1))
                        throw new InvalidOperationException(SR.GetString(SR.CounterLayout));
                    else {
                        currentSampleType = (int)counterData[i + 1].CounterType;
            
            
                        if (!PerformanceCounterLib.IsBaseCounter(currentSampleType))
                            throw new InvalidOperationException(SR.GetString(SR.CounterLayout));
                    }
                }
                else if (PerformanceCounterLib.IsBaseCounter(currentSampleType)) {            
                    if (i == 0)
                        throw new InvalidOperationException(SR.GetString(SR.CounterLayout));
                    else {
                        currentSampleType = (int)counterData[i - 1].CounterType;
            
                        if (
                        (currentSampleType  != NativeMethods.PERF_AVERAGE_BULK) &&
                        (currentSampleType  != NativeMethods.PERF_100NSEC_MULTI_TIMER) &&
                        (currentSampleType  != NativeMethods.PERF_100NSEC_MULTI_TIMER_INV) &&
                        (currentSampleType  != NativeMethods.PERF_COUNTER_MULTI_TIMER) &&
                        (currentSampleType  != NativeMethods.PERF_COUNTER_MULTI_TIMER_INV) &&
                        (currentSampleType  != NativeMethods.PERF_RAW_FRACTION) &&
                        (currentSampleType  != NativeMethods.PERF_SAMPLE_FRACTION) &&
                        (currentSampleType  != NativeMethods.PERF_AVERAGE_TIMER))
                            throw new InvalidOperationException(SR.GetString(SR.CounterLayout));
                    }
            
                }
            
                if (h.ContainsKey(counterData[i].CounterName)) {
                    throw new ArgumentException(SR.GetString(SR.DuplicateCounterName, counterData[i].CounterName));
                }
                else {
                    h.Add(counterData[i].CounterName, String.Empty);
            
                    // Ensure that all counter help strings aren't null or empty
                    if (counterData[i].CounterHelp == null || counterData[i].CounterHelp.Length == 0) {
                        counterData[i].CounterHelp = counterData[i].CounterName;
                    }
                }
            }
        }            

        /// <devdoc>
        ///     Removes the counter (category) from the system
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static void Delete(string categoryName) {
            CheckValidCategory(categoryName);
            string machineName = ".";

            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Administer,  machineName, categoryName);
            permission.Demand();

            SharedUtils.CheckNtEnvironment();

            categoryName = categoryName.ToLower(CultureInfo.InvariantCulture);

            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
                SharedUtils.EnterMutex(perfMutexName, ref mutex);
                if (!PerformanceCounterLib.IsCustomCategory(machineName, categoryName))
                    throw new InvalidOperationException(SR.GetString(SR.CantDeleteCategory));

                SharedPerformanceCounter.RemoveAllInstances(categoryName);
                
                PerformanceCounterLib.UnregisterCategory(categoryName);
                PerformanceCounterLib.CloseAllLibraries();
            }
            finally {
                if (mutex != null) {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }

        }

        /// <devdoc>
        ///     Returns true if the category is registered on the current machine.
        /// </devdoc>
        public static bool Exists(string categoryName) {
            return Exists(categoryName, ".");
        }

        /// <devdoc>
        ///     Returns true if the category is registered in the machine.
        /// </devdoc>
        public static bool Exists(string categoryName, string machineName) {
            if (categoryName == null)
                throw new ArgumentNullException("categoryName");

            if (categoryName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "categoryName", categoryName));

            if (!SyntaxCheck.CheckMachineName(machineName))
                    throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));

            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read,  machineName, categoryName);
            permission.Demand();

            if (PerformanceCounterLib.IsCustomCategory(machineName , categoryName))
                return true;

            return PerformanceCounterLib.CategoryExists(machineName , categoryName);
        }

        /// <devdoc>
        ///     Returns the instance names for a given category
        /// </devdoc>
        /// <internalonly/>
        internal static string[] GetCounterInstances(string categoryName, string machineName) {
            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read, machineName, categoryName);
            permission.Demand();

            CategorySample categorySample = PerformanceCounterLib.GetCategorySample(machineName, categoryName);
            if (categorySample.InstanceNameTable.Count == 0)
                return new string[0];

            string[] instanceNames = new string[categorySample.InstanceNameTable.Count];
            categorySample.InstanceNameTable.Keys.CopyTo(instanceNames, 0);
            if (instanceNames.Length == 1 && instanceNames[0].CompareTo(PerformanceCounterLib.SingleInstanceName) == 0)
                return new string[0];

            return instanceNames;
        }

        /// <devdoc>
        ///     Returns an array of counters in this category.  The counter must have only one instance.
        /// </devdoc>
        public PerformanceCounter[] GetCounters() {
            if (GetInstanceNames().Length != 0)
                throw new ArgumentException(SR.GetString(SR.InstanceNameRequired));
            return GetCounters("");
        }

        /// <devdoc>
        ///     Returns an array of counters in this category for the given instance.
        /// </devdoc>
        public PerformanceCounter[] GetCounters(string instanceName) {
            if (instanceName == null)
                throw new ArgumentNullException("instanceName");

            if (this.categoryName == null)
                throw new InvalidOperationException(SR.GetString(SR.CategoryNameNotSet));

            if (instanceName.Length != 0 && !InstanceExists(instanceName))
                throw new InvalidOperationException(SR.GetString(SR.MissingInstance, instanceName, categoryName));

            string[] counterNames = PerformanceCounterLib.GetCounters(machineName, categoryName);
            PerformanceCounter[] counters = new PerformanceCounter[counterNames.Length];
            for (int index = 0; index < counters.Length; index++)
                counters[index] = new PerformanceCounter(categoryName, counterNames[index], instanceName, machineName, true);

            return counters;
        }


        /// <devdoc>
        ///     Returns an array of performance counter categories for the current machine.
        /// </devdoc>
        public static PerformanceCounterCategory[] GetCategories() {
            return GetCategories(".");
        }

        /// <devdoc>
        ///     Returns an array of performance counter categories for a particular machine.
        /// </devdoc>
        public static PerformanceCounterCategory[] GetCategories(string machineName) {
            if (!SyntaxCheck.CheckMachineName(machineName))
                    throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));

            PerformanceCounterPermission permission = new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Read, machineName, "*");
            permission.Demand();

            string[] categoryNames = PerformanceCounterLib.GetCategories(machineName);
            PerformanceCounterCategory[] categories = new PerformanceCounterCategory[categoryNames.Length];
            for (int index = 0; index < categories.Length; index++)
                categories[index] = new PerformanceCounterCategory(categoryNames[index], machineName);

            return categories;
        }

        /// <devdoc>
        ///     Returns an array of instances for this category
        /// </devdoc>
        public string[] GetInstanceNames() {
            if (this.categoryName == null)
                    throw new InvalidOperationException(SR.GetString(SR.CategoryNameNotSet));

            return GetCounterInstances(categoryName, machineName);
        }

        /// <devdoc>
        ///     Returns true if the instance already exists for this category.
        /// </devdoc>
        public bool InstanceExists(string instanceName) {
            if (instanceName == null)
                throw new ArgumentNullException("instanceName");

            if (this.categoryName == null)
                    throw new InvalidOperationException(SR.GetString(SR.CategoryNameNotSet));

            CategorySample categorySample = PerformanceCounterLib.GetCategorySample(machineName, categoryName);
            return categorySample.InstanceNameTable.ContainsKey(instanceName);
        }

        /// <devdoc>
        ///     Returns true if the instance already exists for the category specified.
        /// </devdoc>
        public static bool InstanceExists(string instanceName, string categoryName) {
            return InstanceExists(instanceName, categoryName, ".");
        }

        /// <devdoc>
        ///     Returns true if the instance already exists for this category and machine specified.
        /// </devdoc>
        public static bool InstanceExists(string instanceName, string categoryName, string machineName) {
            if (instanceName == null)
                throw new ArgumentNullException("instanceName");

            if (categoryName == null)
                throw new ArgumentNullException("categoryName");

            if (categoryName.Length == 0)
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "categoryName", categoryName));

            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "machineName", machineName));

            PerformanceCounterCategory category = new PerformanceCounterCategory(categoryName, machineName);
            return category.InstanceExists(instanceName);
        }

        /// <devdoc>
        ///     Reads all the counter and instance data of this performance category.  Note that reading the entire category
        ///     at once can be as efficient as reading a single counter because of the way the system provides the data.
        /// </devdoc>
        public InstanceDataCollectionCollection ReadCategory() {
            if (this.categoryName == null)
                    throw new InvalidOperationException(SR.GetString(SR.CategoryNameNotSet));

            CategorySample categorySample = PerformanceCounterLib.GetCategorySample(this.machineName, this.categoryName);
            return categorySample.ReadCategory();
        }
    }

    [Flags]
    internal enum PerformanceCounterCategoryOptions {
        EnableReuse = 0x1,
        UseUniqueSharedMemory = 0x2,
    }
}


