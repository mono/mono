//---------------------------------------------------------------------
// <copyright file="ICustomPluralizationMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data.Entity.Design.Common;

namespace System.Data.Entity.Design.PluralizationServices
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
    public interface ICustomPluralizationMapping
    {
        void AddWord(string singular, string plural);
    }
}
