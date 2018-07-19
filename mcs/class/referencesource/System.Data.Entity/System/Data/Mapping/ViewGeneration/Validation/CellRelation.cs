//---------------------------------------------------------------------
// <copyright file="CellRelation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System.Data.Common.Utils;
using System.Collections.Generic;

namespace System.Data.Mapping.ViewGeneration.Validation
{
    // Abstract class representing a relation signature for a cell query
    internal abstract class CellRelation : InternalBase
    {

        #region Constructor
        // effects: Given a cell number (for debugging purposes), creates a
        // cell relation 
        protected CellRelation(int cellNumber)
        {
            m_cellNumber = cellNumber;
        }
        #endregion

        #region Fields
        internal int m_cellNumber; // The number of the cell for which this
        // relation was made (for debugging) 
        #endregion

        #region Properties
        internal int CellNumber
        {
            get { return m_cellNumber; }
        }
        #endregion

        #region Methods

        protected abstract int GetHash();

        #endregion
    }
}
