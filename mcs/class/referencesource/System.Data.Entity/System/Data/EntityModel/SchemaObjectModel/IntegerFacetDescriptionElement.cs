//---------------------------------------------------------------------
// <copyright file="IntegerFacetDescriptionElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Diagnostics;

namespace System.Data.EntityModel.SchemaObjectModel
{
    internal sealed class IntegerFacetDescriptionElement : FacetDescriptionElement
    {
        public IntegerFacetDescriptionElement(TypeElement type, string name)
        :base(type, name)
        {
        }

        public override EdmType FacetType
        {
            get { return MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32); }
        }

        /////////////////////////////////////////////////////////////////////
        // Attribute Handlers

        /// <summary>
        /// Handler for the Default attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Default attribute</param>
        protected override void HandleDefaultAttribute(XmlReader reader)
        {
            int value = -1;
            if (HandleIntAttribute(reader, ref value))
            {
                DefaultValue = value;
            }
        }
    }
}
