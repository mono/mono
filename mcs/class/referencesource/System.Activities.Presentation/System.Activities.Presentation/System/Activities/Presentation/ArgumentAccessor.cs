//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;

    /// <summary>
    /// A class containing a get/set delegate pair to access an argument in an activity instance.
    /// </summary>
    public class ArgumentAccessor
    {
        /// <summary>
        /// Gets or sets the method to retrieve an argument from an activity instance.
        /// </summary>
        public Func<Activity, Argument> Getter { get; set; }

        /// <summary>
        /// Gets or sets the method to set an argument into an activity instance.
        /// </summary>
        public Action<Activity, Argument> Setter { get; set; }
    }
}
