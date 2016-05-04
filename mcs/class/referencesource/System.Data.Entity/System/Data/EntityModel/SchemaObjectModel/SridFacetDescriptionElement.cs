//---------------------------------------------------------------------
// <copyright file="IntegerFacetDescriptionElement.cs" company="Microsoft">
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
    internal sealed class SridFacetDescriptionElement : FacetDescriptionElement
    {
        public SridFacetDescriptionElement(TypeElement type, string name)
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
            string value = reader.Value;
            if (value.Trim() == XmlConstants.Variable)
            {
                DefaultValue = EdmConstants.VariableValue;
                return;
            }

            int intValue = -1;
            if (HandleIntAttribute(reader, ref intValue))
            {
                DefaultValue = intValue;
            }
        }
    }
}
