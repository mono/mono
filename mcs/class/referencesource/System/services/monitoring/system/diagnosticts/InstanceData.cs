//------------------------------------------------------------------------------
// <copyright file="InstanceData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;

    using System;
    using System.Collections;

    /// <devdoc>
    ///     A holder of instance data.
    /// </devdoc>    
    public class InstanceData {
        private string instanceName;
        private CounterSample sample;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public InstanceData(string instanceName, CounterSample sample) {
            this.instanceName = instanceName;
            this.sample = sample;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string InstanceName {
            get {
                return instanceName;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterSample Sample {
            get {
                return sample;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public long RawValue {
            get {
                return sample.RawValue;
            }
        }
    }
}
