//------------------------------------------------------------------------------
// <copyright file="DataSysAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

/*
 */
namespace System.Data {
    using System;
    using System.ComponentModel;

    /// <devdoc>
    ///    <para>
    ///       DescriptionAttribute marks a property, event, or extender with a
    ///       description. Visual designers can display this description when referencing
    ///       the member.
    ///    </para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    [ Obsolete("DataSysDescriptionAttribute has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202", false) ]
    public class DataSysDescriptionAttribute : DescriptionAttribute {

        private bool replaced = false;

        /// <devdoc>
        ///     Constructs a new sys description.
        /// </devdoc>
        [ Obsolete("DataSysDescriptionAttribute has been deprecated.  http://go.microsoft.com/fwlink/?linkid=14202", false) ]
        public DataSysDescriptionAttribute(string description) : base(description) {
        }

        /// <devdoc>
        ///     Retrieves the description text.
        /// </devdoc>
        public override string Description {
            get {
                if (!replaced) {
                    replaced = true;
                    DescriptionValue = Res.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }
}
