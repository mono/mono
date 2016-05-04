//---------------------------------------------------------------------
// <copyright file="EntityDesignPluralizationHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.Metadata.Edm;
using System.IO;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Entity.Design.Common;
using System.Diagnostics;

namespace System.Data.Entity.Design
{
    internal class EntityDesignPluralizationHandler
    {
        /// <summary>
        /// user might set the service to null, so we have to check the null when using this property
        /// </summary>
        internal PluralizationService Service
        {
            get;
            set;
        }

        /// <summary>
        /// Handler for pluralization service in Entity Design
        /// </summary>
        /// <param name="doPluralization">overall switch for the service, the service only start working when the value is true</param>
        /// <param name="userDictionaryPath"></param>
        /// <param name="errors"></param>
        internal EntityDesignPluralizationHandler(PluralizationService service)
        {
            this.Service = service;
        }

        internal string GetEntityTypeName(string storeTableName)
        {
            return this.Service != null ? this.Service.Singularize(storeTableName) : storeTableName;
        }

        internal string GetEntitySetName(string storeTableName)
        {
            return this.Service != null ? this.Service.Pluralize(storeTableName) : storeTableName;
        }

        internal string GetNavigationPropertyName(AssociationEndMember toEnd, string storeTableName)
        {
            if (this.Service != null)
            {
                return toEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many ?
                    this.Service.Pluralize(storeTableName) : this.Service.Singularize(storeTableName);
            }
            else
            {
                return storeTableName;
            }
        }
    }
}
