//---------------------------------------------------------------------
// <copyright file="EntityViewContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Text;


namespace System.Data.Mapping
{
    /// <summary>
    /// Base class for the type created at design time to store the generated views.
    /// </summary>
    public abstract class EntityViewContainer
    {
        #region Constructors
        protected EntityViewContainer()
        {
        }
        #endregion

        #region fields
        private string m_storedHashOverMappingClosure; // Hash value over the whole Metadata and Mapping closure
        private string m_storedhashOverAllExtentViews; // Hash value over all the extent views
        private string m_storededmEntityContainerName; // C side entity container name
        private string m_storedStoreEntityContainerName; // S side entity container name
        private int _viewCount;
        #endregion

        #region properties
        /// <summary>
        /// Returns the cached dictionary of (ExtentName,EsqlView)
        /// </summary>
        internal IEnumerable<KeyValuePair<string, string>> ExtentViews
        {
            get
            {
                for (int i = 0; i < ViewCount; i++)
                {
                    yield return GetViewAt(i);
                }
            }
        }

        protected abstract System.Collections.Generic.KeyValuePair<string, string> GetViewAt(int index);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public string EdmEntityContainerName
        {
            get
            {
                return this.m_storededmEntityContainerName;
            }
            set
            {
                this.m_storededmEntityContainerName = value;
            }
        }
        public string StoreEntityContainerName
        {
            get
            {
                return this.m_storedStoreEntityContainerName;
            }
            set
            {
                this.m_storedStoreEntityContainerName = value;
            }
        }
        public string HashOverMappingClosure
        {
            get
            {
                return this.m_storedHashOverMappingClosure;
            }
            set
            {
                this.m_storedHashOverMappingClosure = value;
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OverAll")]
        public string HashOverAllExtentViews
        {
            get
            {
                return this.m_storedhashOverAllExtentViews;
            }
            set
            {
                this.m_storedhashOverAllExtentViews = value;
            }
        }

        public int ViewCount
        {
            get { return _viewCount; }
            protected set { _viewCount = value; }
        }
        #endregion
    }
}
