//---------------------------------------------------------------------
// <copyright file="ByteFacetDescriptionElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Diagnostics;

namespace System.Data.EntityModel.SchemaObjectModel
{
    internal sealed class ByteFacetDescriptionElement : FacetDescriptionElement
    {
        public ByteFacetDescriptionElement(TypeElement type, string name)
        :base(type, name)
        {
        }

        public override EdmType FacetType
        {
            get { return MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte); }
        }

        /////////////////////////////////////////////////////////////////////
        // Attribute Handlers

        /// <summary>
        /// Handler for the Default attribute
        /// </summary>
        /// <param name="reader">xml reader currently positioned at Default attribute</param>
        protected override void HandleDefaultAttribute(XmlReader reader)
        {
            byte value = 0;
            if (HandleByteAttribute(reader, ref value))
            {
                DefaultValue = (Byte)value;
            }
        }
    }
}
