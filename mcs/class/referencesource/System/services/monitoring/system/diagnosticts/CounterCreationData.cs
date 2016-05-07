//------------------------------------------------------------------------------
// <copyright file="CounterCreationData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;

    using System;
    using System.ComponentModel;
    
    /// <devdoc>
    ///     A struct defining the counter type, name and help string for a custom counter.
    /// </devdoc>
    [
    TypeConverter("System.Diagnostics.Design.CounterCreationDataConverter, " + AssemblyRef.SystemDesign), 
    Serializable
    ]
    public class CounterCreationData {
        private PerformanceCounterType counterType = PerformanceCounterType.NumberOfItems32;
        private string counterName = String.Empty;
        private string counterHelp = String.Empty;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterCreationData() {            
        }
    
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterCreationData(string counterName, string counterHelp, PerformanceCounterType counterType) {
            CounterType = counterType;
            CounterName = counterName;
            CounterHelp = counterHelp;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(PerformanceCounterType.NumberOfItems32),
        MonitoringDescription(SR.CounterType)
        ]
        public PerformanceCounterType CounterType {
            get {
                return counterType;
            }
            set {
                if (!Enum.IsDefined(typeof(PerformanceCounterType), value)) 
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(PerformanceCounterType));
            
                counterType = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        MonitoringDescription(SR.CounterName),        
        TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign)        
        ]
        public string CounterName {
            get {
                return counterName;
            }
            set {
                PerformanceCounterCategory.CheckValidCounter(value);
                counterName = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        MonitoringDescription(SR.CounterHelp)
        ]
        public string CounterHelp {
            get {
                return counterHelp;
            }
            set {
                PerformanceCounterCategory.CheckValidHelp(value);
                counterHelp = value;
            }
        }
    }
}
