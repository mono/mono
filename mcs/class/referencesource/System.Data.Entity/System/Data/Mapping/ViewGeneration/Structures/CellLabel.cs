//---------------------------------------------------------------------
// <copyright file="CellLabel.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System.Data.Common.Utils;

namespace System.Data.Mapping.ViewGeneration.Structures
{

    // A class that abstracts the notion of identifying table mapping
    // fragments or cells, e.g., line numbers, etc
    internal class CellLabel
    {

        #region Constructors

        /// <summary>
        /// Copy Constructor
        /// </summary>
        internal CellLabel(CellLabel source)
        {
            this.m_startLineNumber = source.m_startLineNumber;
            this.m_startLinePosition = source.m_startLinePosition;
            this.m_sourceLocation = source.m_sourceLocation;
        }

        internal CellLabel(StorageMappingFragment fragmentInfo) :
            this(fragmentInfo.StartLineNumber, fragmentInfo.StartLinePosition, fragmentInfo.SourceLocation) { }

        internal CellLabel(int startLineNumber, int startLinePosition, string sourceLocation)
        {
            m_startLineNumber = startLineNumber;
            m_startLinePosition = startLinePosition;
            m_sourceLocation = sourceLocation;
        }

        #endregion

        #region Fields
        private int m_startLineNumber;
        private int m_startLinePosition;
        private string m_sourceLocation;
        #endregion

        #region Properties

        internal int StartLineNumber
        {
            get { return m_startLineNumber; }
        }

        internal int StartLinePosition
        {
            get { return m_startLinePosition; }
        }

        internal string SourceLocation
        {
            get { return m_sourceLocation; }
        }

        #endregion
    }
}
