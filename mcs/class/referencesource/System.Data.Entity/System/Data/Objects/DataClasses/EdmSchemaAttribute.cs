//---------------------------------------------------------------------
// <copyright file="EdmSchemaAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// Attribute for static types
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class EdmSchemaAttribute : System.Attribute
    {
        /// <summary>
        /// Constructor for EdmSchemaAttribute
        /// </summary>
        public EdmSchemaAttribute()
        {
        }
        /// <summary>
        /// Setting this parameter to a unique value for each model file in a Visual Basic
        /// assembly will prevent the following error: 
        /// "'System.Data.Objects.DataClasses.EdmSchemaAttribute' cannot be specified more than once in this project, even with identical parameter values."
        /// </summary>
        public EdmSchemaAttribute(string assemblyGuid)
        {
            if (null == assemblyGuid)
            {
                throw new System.ArgumentNullException("assemblyGuid");
            }
        }
    }
}

