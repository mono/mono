// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Runtime.InteropServices 
{
    using System;
    using System.Threading;

    public sealed class HandleCollector {
        private const int deltaPercent = 10; // this is used for increasing the threshold.        
        private string name;
        private int initialThreshold;
        private int maximumThreshold;
        private int threshold;
        private int handleCount;
        
        private int[] gc_counts = new int [3];
        private int    gc_gen = 0;
            
        public HandleCollector( string name, int initialThreshold ) : 
            this( name, initialThreshold,  int.MaxValue) {
        }

        public HandleCollector( string name, int initialThreshold, int maximumThreshold ) {
            if( initialThreshold < 0) {
                throw new ArgumentOutOfRangeException("initialThreshold", 
                    SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            }                                                                         

            if( maximumThreshold < 0) {
                throw new ArgumentOutOfRangeException("maximumThreshold", 
                    SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNumRequired));
            }                                                                         

            if( initialThreshold > maximumThreshold) {
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidThreshold));
            }

            if ( name != null) {
                this.name = name;
            }
            else {
                this.name = String.Empty;
            }

            this.initialThreshold = initialThreshold;
            this.maximumThreshold = maximumThreshold ;
            this.threshold = initialThreshold;
            this.handleCount = 0;
        }

        public int Count { get {return handleCount;} }

        public int InitialThreshold { get  { return initialThreshold;} }

        public int MaximumThreshold { get { return  maximumThreshold;} }

        public string Name { get {return name;} }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]  // Keep call to GC.Collect()
#if FEATURE_LEGACYNETCF
        [System.Security.SecuritySafeCritical] 
#endif// FEATURE_LEGACYNETCF
        public void Add () {
            int gen_collect = -1;
            Interlocked.Increment( ref handleCount);
            if( handleCount < 0) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_HCCountOverflow));                 
            }

            if (handleCount > threshold) {
                lock (this) {
                    threshold = handleCount + (handleCount/deltaPercent);
                    gen_collect = gc_gen;
                    if (gc_gen < 2) {
                        gc_gen++;
                    }
                }                
            }

            if ((gen_collect >= 0) && 
                    ((gen_collect == 0) || 
                    (gc_counts[gen_collect] == GC.CollectionCount (gen_collect)))) {
                    GC.Collect (gen_collect);
                    Thread.Sleep (10*gen_collect);
            }

            //don't bother with gen0. 
            for (int i = 1; i < 3; i++) {
                gc_counts [i] = GC.CollectionCount (i);
            }
        }

        public void Remove () {
            Interlocked.Decrement( ref handleCount);
            if (handleCount < 0) {
                throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_HCCountOverflow)); 
            }

            int newThreshold =  handleCount + handleCount/deltaPercent;
            if (newThreshold < (threshold - threshold/deltaPercent)) {
                lock( this) {
                   if (newThreshold > initialThreshold) {
                        threshold = newThreshold;
                    }
                    else {
                        threshold = initialThreshold;
                    }
                    gc_gen = 0;
                }
            }
            
            for (int i = 1; i < 3; i++) {
                gc_counts [i] = GC.CollectionCount (i);
            }            
        }
    }
}
