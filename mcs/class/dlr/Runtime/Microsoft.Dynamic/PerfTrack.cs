/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_CORE_DLR
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Scripting.Utils;
using System.Dynamic;
using System.IO;

namespace Microsoft.Scripting {
    /// <summary>
    /// This class is useful for quickly collecting performance counts for expensive
    /// operations.  Usually this means operations involving either reflection or
    /// code gen.  Long-term we need to see if this can be plugged better into the
    /// standard performance counter architecture.
    /// </summary>
    public static class PerfTrack {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")] // TODO: fix
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")] // TODO: fix
        public enum Categories {
            /// <summary>
            /// temporary categories for quick investigation, use a custom key if you
            /// need to track multiple items, and if you want to keep it then create 
            /// a new Categories entry and rename all your temporary entries.
            /// </summary>
            Temporary,
            ReflectedTypes,
            Exceptions,     // exceptions thrown
            Properties,     // properties got or set
            Fields,         // fields got or set
            Methods,        // methods called through MethodBase.Invoke()...
            Compiler,       // Methods compiled via the ReflectOptimizer
            DelegateCreate, // we've created a new method for delegates
            DictInvoke,     // Dictionary accesses
            OperatorInvoke, // Invoking an operator against a type
            OverAllocate,   // a spot where we have an un-ideal algorithm that needs to allocate more than necessary
            Rules,          // related to rules / actions.
            RuleEvaluation, // a rule was evaluated
            Binding,        // a rule was bound
            BindingSlow,
            BindingFast,    
            BindingTarget,  // a rule was bound against a target of a specific type
            Count
        }

        [MultiRuntimeAware]
        private static int totalEvents;
        private static readonly Dictionary<Categories, Dictionary<string, int>> _events = MakeEventsDictionary();
        private static readonly Dictionary<Categories, int> summaryStats = new Dictionary<Categories, int>();

        private static Dictionary<Categories, Dictionary<string, int>> MakeEventsDictionary() {
            Dictionary<Categories, Dictionary<string, int>> result = new Dictionary<Categories, Dictionary<string, int>>();
            
            // We do not use Enum.GetValues here since it is not available in SILVERLIGHT 
            for (int i = 0; i <= (int)Categories.Count; i++) {
                result[(Categories)i] = new Dictionary<string, int>();
            }

            return result;
        }

#if FEATURE_BASIC_CONSOLE
        public static void DumpHistogram<TKey>(IDictionary<TKey, int> histogram) {
            DumpHistogram(histogram, Console.Out);
        }

        public static void DumpStats() {
            DumpStats(Console.Out);
        }
#endif

        public static void DumpHistogram<TKey>(IDictionary<TKey, int> histogram, TextWriter output) {
            var keys = ArrayUtils.MakeArray(histogram.Keys);
            var values = ArrayUtils.MakeArray(histogram.Values);
            
#if !WIN8 // TODO: 
            Array.Sort(values, keys);
#endif   
            for (int i = 0; i < keys.Length; i++) {
                output.WriteLine("{0} {1}", keys[i], values[i]);
            }
        }

        public static void AddHistograms<TKey>(IDictionary<TKey, int> result, IDictionary<TKey, int> addend) {
            foreach (var entry in addend) {
                int value;
                result[entry.Key] = entry.Value + (result.TryGetValue(entry.Key, out value) ? value : 0);
            }
        }

        public static void IncrementEntry<TKey>(IDictionary<TKey, int> histogram, TKey key) {
            int value;
            histogram.TryGetValue(key, out value);
            histogram[key] = value + 1;
        }

        public static void DumpStats(TextWriter output) {
            if (totalEvents == 0) return;

            // numbers from AMD Opteron 244 1.8 Ghz, 2.00GB of ram,
            // running on IronPython 1.0 Beta 4 against Whidbey RTM.
            const double CALL_TIME = 0.0000051442355;
            const double THROW_TIME = 0.000025365656;
            const double FIELD_TIME = 0.0000018080093;

            output.WriteLine();
            output.WriteLine("---- Performance Details ----");
            output.WriteLine();

            foreach (KeyValuePair<Categories, Dictionary<string, int>> kvpCategories in _events) {
                if (kvpCategories.Value.Count > 0) {
                    output.WriteLine("Category : " + kvpCategories.Key);
                    DumpHistogram(kvpCategories.Value, output);
                    output.WriteLine();
                }
            }

            output.WriteLine();
            output.WriteLine("---- Performance Summary ----");
            output.WriteLine();
            double knownTimes = 0;
            foreach (KeyValuePair<Categories, int> kvp in summaryStats) {
                switch (kvp.Key) {
                    case Categories.Exceptions:
                        output.WriteLine("Total Exception ({0}) = {1}  (throwtime = ~{2} secs)", kvp.Key, kvp.Value, kvp.Value * THROW_TIME);
                        knownTimes += kvp.Value * THROW_TIME;
                        break;
                    case Categories.Fields:
                        output.WriteLine("Total field = {0} (time = ~{1} secs)", kvp.Value, kvp.Value * FIELD_TIME);
                        knownTimes += kvp.Value * FIELD_TIME;
                        break;
                    case Categories.Methods:
                        output.WriteLine("Total calls = {0} (calltime = ~{1} secs)", kvp.Value, kvp.Value * CALL_TIME);
                        knownTimes += kvp.Value * CALL_TIME;
                        break;
                    //case Categories.Properties:
                    default:
                        output.WriteLine("Total {1} = {0}", kvp.Value, kvp.Key);
                        break;
                }
            }

            output.WriteLine();
            output.WriteLine("Total Known Times: {0}", knownTimes);
        }

        [Conditional("DEBUG")]
        public static void NoteEvent(Categories category, object key) {
            if (!DebugOptions.TrackPerformance) return;

            Dictionary<string, int> categoryEvents = _events[category];
            totalEvents++;
            lock (categoryEvents) {
                string name = key.ToString();
                Exception ex = key as Exception;
                if (ex != null) name = ex.GetType().ToString();
                int v;
                if (!categoryEvents.TryGetValue(name, out v)) categoryEvents[name] = 1;
                else categoryEvents[name] = v + 1;

                if (!summaryStats.TryGetValue(category, out v)) summaryStats[category] = 1;
                else summaryStats[category] = v + 1;
            }
        }
    }
}
