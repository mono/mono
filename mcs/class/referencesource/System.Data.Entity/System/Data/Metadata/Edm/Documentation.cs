//---------------------------------------------------------------------
// <copyright file="Documentation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Data.Common.Utils;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class representing the Documentation associated with an item
    /// </summary>
    public sealed class Documentation: MetadataItem
    {
        #region Fields
        private string _summary = "";
        private string _longDescription = "";
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor - primarily created for supporting usage of this Documentation class by SOM.
        /// </summary>
        internal Documentation()
        {
        }        
        #endregion
       
        #region Properties

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.Documentation; } }

        /// <summary>
        /// Gets the Summary for this Documentation instance.
        /// </summary>
        /// 
        public string Summary
        {
            get
            {
                return _summary;
            }
            internal set
            {
                if (value != null)
                    _summary = value;
                else
                    _summary = "";
            }
        }

        /// <summary>
        /// Gets the LongDescription for this Documentation instance.
        /// </summary>
        /// 
        public string LongDescription
        {
            get
            {
                return _longDescription;
            }
            internal set
            {
                if (value != null)
                    _longDescription = value;
                else
                    _longDescription = "";
            }
        }


        /// <summary>
        /// This property is required to be implemented for inheriting from MetadataItem. As there can be atmost one
        /// instance of a nested-Documentation, return the constant "Documentation" as it's identity.
        /// </summary>
        internal override string Identity
        {
            get
            {
                return "Documentation";
            }
        }

        /// <summary>
        /// Returns true if this Documentation instance contains only null/empty summary and longDescription
        /// </summary>
        /// 
        public bool IsEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(_summary) && string.IsNullOrEmpty(_longDescription) )
                {
                    return true;
                }

                return false;
            }
        }


        #endregion
        
        #region Methods

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return _summary;
        }

        #endregion
    }
}
