//---------------------------------------------------------------------
// <copyright file="ExpressionList.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.Common.CommandTrees.Internal
{
    internal sealed class DbExpressionList : System.Collections.ObjectModel.ReadOnlyCollection<DbExpression>
    {
        internal DbExpressionList(IList<DbExpression> elements) 
            : base(elements) 
        {
        }
    }
}
