//---------------------------------------------------------------------
// <copyright file="Singleton.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  	 Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
namespace System.Data.Common.Utils
{
    /// <summary>
    /// Allows for delayed creation of a singleton that can be used safely by multiple threads. 
    /// Instantiation of the singleton instance is not synchronized and may be invoked multiple times by different threads,
    /// however only one evaluation will 'win' and provide the value for the Singleton instance. 
    /// The <see cref="Value"/> property is guaranteed to always return the same instance.
    /// Limitations:
    /// 1. Reference types only (to simplify the 'is initialized?' check)
    /// 2. The instantiation function will be invoked whenever the Value property is accessed and the current value is null, 
    ///    not just the first time; avoid returning null from the instantiation function unless the intent is for instantiation to be retried.
    /// </summary>
    /// <typeparam name="TValue">Type of the singleton instance.</typeparam>
    internal sealed class Singleton<TValue>
        where TValue : class
    {
        private readonly Func<TValue> valueProvider;
        private TValue value;

        /// <summary>
        /// Constructs a new Singleton that uses the specified function to instantiate its value.
        /// </summary>
        /// <param name="function">Required. Function to evaluate to produce the singleton instance.</param>
        internal Singleton(Func<TValue> function)
        {
            EntityUtil.CheckArgumentNull(function, "function");
            this.valueProvider = function;
        }

        /// <summary>
        /// Retrieves the singleton value, either by evaluating the value function or returning the already cached instance.
        /// </summary>
        internal TValue Value
        {
            get
            {
                TValue result = this.value; // reading of reference types is atomic.
                if (result == null)
                {
                    TValue newValue = this.valueProvider();
                    Interlocked.CompareExchange(ref this.value, newValue, null);
                    result = this.value;
                }
                return result;
            }
        }
    }
}
