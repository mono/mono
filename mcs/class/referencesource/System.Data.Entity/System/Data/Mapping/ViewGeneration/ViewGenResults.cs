//---------------------------------------------------------------------
// <copyright file="ViewgenResults.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Data.EntityModel;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Mapping.ViewGeneration
{

    // This class is responsible for keeping track of the results from view
    // generation - errors and correct views
    internal class ViewGenResults : InternalBase
    {

        #region Constructor
        internal ViewGenResults()
        {
            m_views = new KeyToListMap<EntitySetBase, GeneratedView>(EqualityComparer<EntitySetBase>.Default);
            m_errorLog = new ErrorLog();
        }
        #endregion

        #region Fields
        private KeyToListMap<EntitySetBase, GeneratedView> m_views;
        private ErrorLog m_errorLog;
        #endregion

        #region Properties
        // effects: Returns the generated views
        internal KeyToListMap<EntitySetBase, GeneratedView> Views
        {
            get
            {
                return m_views;
            }
        }

        // effects: Returns the errors that were generated. If no errors,
        // returns an empty list
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced (indirectly) by System.Data.Entity.Design.dll
        internal IEnumerable<EdmSchemaError> Errors
        {
            get
            {
                return m_errorLog.Errors;
            }
        }

        // effects: Returns true iff any error was generated
        internal bool HasErrors
        {
            get
            {
                return m_errorLog.Count > 0;
            }
        }
        #endregion

        #region Methods
        // effects: Add the set of errors in errorLog to this
        internal void AddErrors(ErrorLog errorLog)
        {
            m_errorLog.Merge(errorLog);
        }

        // effects: Returns all the errors as a string (not to be used for
        // end user strings, i.e., in exceptions etc)
        internal string ErrorsToString()
        {
            return m_errorLog.ToString();
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            // Number of views
            builder.Append(m_errorLog.Count);
            builder.Append(" ");
            // Print the errors only
            m_errorLog.ToCompactString(builder);
        }
        #endregion
    }
}
