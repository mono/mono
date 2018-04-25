//------------------------------------------------------------------------------
// <copyright file="CounterSample.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;

    using System;

    /// <devdoc>
    ///     A struct holding the raw data for a performance counter.
    /// </devdoc>    
    public struct CounterSample {
        private long rawValue;
        private long baseValue;
        private long timeStamp;
        private long counterFrequency;
        private PerformanceCounterType counterType;
        private long timeStamp100nSec;
        private long systemFrequency;
        private long counterTimeStamp;
    
        // Dummy holder for an empty sample
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static CounterSample Empty = new CounterSample(0, 0, 0, 0, 0, 0, PerformanceCounterType.NumberOfItems32);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterSample(long rawValue, long baseValue, long counterFrequency, long systemFrequency, long timeStamp, long timeStamp100nSec, PerformanceCounterType counterType) {
            this.rawValue = rawValue;
            this.baseValue = baseValue;
            this.timeStamp = timeStamp;
            this.counterFrequency = counterFrequency;
            this.counterType = counterType;
            this.timeStamp100nSec = timeStamp100nSec;
            this.systemFrequency = systemFrequency;
            this.counterTimeStamp = 0;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterSample(long rawValue, long baseValue, long counterFrequency, long systemFrequency, long timeStamp, long timeStamp100nSec, PerformanceCounterType counterType, long counterTimeStamp) {
            this.rawValue = rawValue;
            this.baseValue = baseValue;
            this.timeStamp = timeStamp;
            this.counterFrequency = counterFrequency;
            this.counterType = counterType;
            this.timeStamp100nSec = timeStamp100nSec;
            this.systemFrequency = systemFrequency;
            this.counterTimeStamp = counterTimeStamp;
        }         
         
        /// <devdoc>
        ///      Raw value of the counter.
        /// </devdoc>
        public long RawValue {
            get {
                return this.rawValue;
            }
        }

        internal ulong UnsignedRawValue {
             get {
                return (ulong)this.rawValue;
            }
        }
        
        /// <devdoc>
        ///      Optional base raw value for the counter (only used if multiple counter based).
        /// </devdoc>
        public long BaseValue {
            get {
                return this.baseValue;
            }
        }
        
        /// <devdoc>
        ///      Raw system frequency
        /// </devdoc>
        public long SystemFrequency {
            get {
               return this.systemFrequency;
            }
        }

        /// <devdoc>
        ///      Raw counter frequency
        /// </devdoc>
        public long CounterFrequency {
            get {
                return this.counterFrequency;
            }
        }

        /// <devdoc>
        ///      Raw counter frequency
        /// </devdoc>
        public long CounterTimeStamp {
            get {
                return this.counterTimeStamp;
            }
        }
        
        /// <devdoc>
        ///      Raw timestamp
        /// </devdoc>
        public long TimeStamp {
            get {
                return this.timeStamp;
            }
        }

        /// <devdoc>
        ///      Raw high fidelity timestamp
        /// </devdoc>
        public long TimeStamp100nSec {
            get {
                return this.timeStamp100nSec;
            }
        }
        
        /// <devdoc>
        ///      Counter type
        /// </devdoc>
        public PerformanceCounterType CounterType {
            get {
                return this.counterType;
            }
        }

        /// <devdoc>
        ///    Static functions to calculate the performance value off the sample
        /// </devdoc>
        public static float Calculate(CounterSample counterSample) {
            return CounterSampleCalculator.ComputeCounterValue(counterSample);
        }

        /// <devdoc>
        ///    Static functions to calculate the performance value off the samples
        /// </devdoc>
        public static float Calculate(CounterSample counterSample, CounterSample nextCounterSample) { 
            return CounterSampleCalculator.ComputeCounterValue(counterSample, nextCounterSample);
        }

        public override bool Equals(Object o) {
            return ( o is CounterSample) && Equals((CounterSample)o);               
        }
        
        public bool Equals(CounterSample sample) {
            return (rawValue == sample.rawValue) && 
                       (baseValue == sample.baseValue) && 
                       (timeStamp == sample.timeStamp) && 
                       (counterFrequency == sample.counterFrequency) &&
                       (counterType == sample.counterType) &&
                       (timeStamp100nSec == sample.timeStamp100nSec) && 
                       (systemFrequency == sample.systemFrequency) &&
                       (counterTimeStamp == sample.counterTimeStamp);                       
        }

        public override int GetHashCode() {
            return rawValue.GetHashCode();                
        }

        public static bool operator ==(CounterSample a, CounterSample b) {
            return a.Equals(b);
        }        

        public static bool operator !=(CounterSample a, CounterSample b) {
            return !(a.Equals(b));
        }
        
    }
}
