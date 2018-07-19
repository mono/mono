//---------------------------------------------------------------------
// <copyright file="PluralizationService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       venkatja
// @backupOwner willa
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data.Entity.Design.Common;
using System.Data.Entity.Design;

namespace System.Data.Entity.Design.PluralizationServices
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
    public abstract class PluralizationService
    {
        public CultureInfo Culture { get; protected set; }

        public abstract bool IsPlural(string word);
        public abstract bool IsSingular(string word);
        public abstract string Pluralize(string word);
        public abstract string Singularize(string word);

        /// <summary>
        /// Factory method for PluralizationService. Only support english pluralization.
        /// Please set the PluralizationService on the System.Data.Entity.Design.EntityModelSchemaGenerator
        /// to extend the service to other locales.
        /// </summary>
        /// <param name="culture">CultureInfo</param>
        /// <returns>PluralizationService</returns>
        public static PluralizationService CreateService(CultureInfo culture)
        {
            EDesignUtil.CheckArgumentNull<CultureInfo>(culture, "culture");

            if (culture.TwoLetterISOLanguageName == "en")
            {
                return new EnglishPluralizationService();
            }
            else
            {
                throw new NotImplementedException(Strings.UnsupportedLocaleForPluralizationServices(culture.DisplayName));
            }
        }
    }
}
