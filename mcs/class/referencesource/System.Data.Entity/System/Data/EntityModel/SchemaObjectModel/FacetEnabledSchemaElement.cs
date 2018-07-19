//---------------------------------------------------------------------
// <copyright file="FacetEnabledSchemaElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Xml;

namespace System.Data.EntityModel.SchemaObjectModel
{
    internal abstract class FacetEnabledSchemaElement : SchemaElement
    {
        protected SchemaType _type = null;
        protected string _unresolvedType = null;
        protected TypeUsageBuilder _typeUsageBuilder;

        #region Properties

        internal new Function ParentElement
        {
            get
            {
                return base.ParentElement as Function;
            }
        }

        internal SchemaType Type
        {
            get
            {
                return _type;
            }
        }

        internal virtual TypeUsage TypeUsage
        {
            get
            {
                return _typeUsageBuilder.TypeUsage;
            }
        }

        internal TypeUsageBuilder TypeUsageBuilder
        {
            get
            {
                return _typeUsageBuilder;
            }
        }

        internal bool HasUserDefinedFacets
        {
            get
            {
                return _typeUsageBuilder.HasUserDefinedFacets;
            }
        }

        internal string UnresolvedType
        {
            get
            {
                return _unresolvedType;
            }
            set
            {
                _unresolvedType = value;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal FacetEnabledSchemaElement(Function parentElement)
            : base(parentElement)
        {

        }

        internal FacetEnabledSchemaElement(SchemaElement parentElement)
            : base(parentElement)
        {

        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();
            
            Debug.Assert(this.Type == null, "This must be resolved exactly once");

            if (Schema.ResolveTypeName(this, UnresolvedType, out _type))
            {
                if (Schema.DataModel == SchemaDataModelOption.ProviderManifestModel && _typeUsageBuilder.HasUserDefinedFacets)
                {
                    bool isInProviderManifest = Schema.DataModel == SchemaDataModelOption.ProviderManifestModel;
                    _typeUsageBuilder.ValidateAndSetTypeUsage((ScalarType)_type, !isInProviderManifest);
                }
            }
        }

        internal void ValidateAndSetTypeUsage(ScalarType scalar)
        {
            _typeUsageBuilder.ValidateAndSetTypeUsage(scalar, false);
        }

        internal void ValidateAndSetTypeUsage(EdmType edmType)
        {
            _typeUsageBuilder.ValidateAndSetTypeUsage(edmType, false);
        }

        #endregion

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (_typeUsageBuilder.HandleAttribute(reader))
            {
                return true;
            }

            return false;
        }

    }
}
