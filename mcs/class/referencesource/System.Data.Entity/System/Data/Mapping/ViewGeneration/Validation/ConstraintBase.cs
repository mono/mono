//---------------------------------------------------------------------
// <copyright file="ConstraintBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner srimand
// @backupOwner cmeek
//---------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Data.Common.Utils.Boolean;
using System.Data.Mapping.ViewGeneration.Structures;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Mapping.ViewGeneration.Validation
{

    using WrapperBoolExpr = BoolExpr<LeftCellWrapper>;
    using WrapperTreeExpr = TreeExpr<LeftCellWrapper>;
    using WrapperAndExpr = AndExpr<LeftCellWrapper>;
    using WrapperOrExpr = OrExpr<LeftCellWrapper>;
    using WrapperNotExpr = NotExpr<LeftCellWrapper>;
    using WrapperTermExpr = TermExpr<LeftCellWrapper>;
    using WrapperTrueExpr = TrueExpr<LeftCellWrapper>;
    using WrapperFalseExpr = FalseExpr<LeftCellWrapper>;

    // A superclass for constraint errors. It also contains useful constraint
    // checking methods
    internal abstract class ConstraintBase : InternalBase
    {

        #region Methods
        // effects: Returns an error log record with this constraint's information
        internal abstract ErrorLog.Record GetErrorRecord();
        #endregion
    }
}
