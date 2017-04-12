//---------------------------------------------------------------------
// <copyright file="Property.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml;
using System.Data;
using System.Data.Metadata.Edm;
using System.Reflection;
using System.IO;
using System.Globalization;

namespace System.Data.EntityModel.SchemaObjectModel
{

    internal abstract class Property : SchemaElement
    {
        /// <summary>
        /// Creates a Property object
        /// </summary>
        /// <param name="parentElement">The parent element</param>
        internal Property(StructuredType parentElement)
            : base(parentElement)
        {
        }

        /// <summary>
        /// Gets the Type of the property
        /// </summary>
        public abstract SchemaType Type { get;}

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
            }
            return false;
        }
    }
}
