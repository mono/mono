//---------------------------------------------------------------------
// <copyright file="SchemaType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.Data;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Summary description for SchemaType.
    /// </summary>
    internal abstract class SchemaType : SchemaElement
    {
        #region Public Properties
        
        /// <summary>
        /// Gets the Namespace that this type is in.
        /// </summary>
        /// <value></value>
        public string Namespace
        {
            get
            {
                return Schema.Namespace;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string Identity
        {
            get
            {
                return Namespace + "." + Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string FQName
        {
            get
            {
                return Namespace + "." + Name;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        internal SchemaType(Schema parentElement)
        :   base(parentElement)
        {
        }


        #endregion


    }
}
