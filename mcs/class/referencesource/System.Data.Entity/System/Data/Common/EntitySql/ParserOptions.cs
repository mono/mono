//---------------------------------------------------------------------
// <copyright file="ParserOptions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System;
    using System.Globalization;
  
    /// <summary>
    /// Represents eSQL compilation options.
    /// </summary>
    internal sealed class ParserOptions
    {
        internal enum CompilationMode
        {
            /// <summary>
            /// Normal mode. Compiles eSQL command without restrictions.
            /// Name resolution is case-insensitive (eSQL default).
            /// </summary>
            NormalMode,

            /// <summary>
            /// View generation mode: optimizes compilation process to ignore uncessary eSQL constructs:
            ///     - GROUP BY, HAVING and ORDER BY clauses are ignored.
            ///     - WITH RELATIONSHIP clause is allowed in type constructors.
            ///     - Name resolution is case-sensitive.
            /// </summary>
            RestrictedViewGenerationMode,

            /// <summary>
            /// Same as CompilationMode.Normal plus WITH RELATIONSHIP clause is allowed in type constructors.
            /// </summary>
            UserViewGenerationMode
        }

        /// <summary>
        /// Sets/Gets eSQL parser compilation mode.
        /// </summary>
        internal CompilationMode ParserCompilationMode;

        internal StringComparer NameComparer
        {
            get
            {
                return this.NameComparisonCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            }
        }

        internal bool NameComparisonCaseInsensitive
        {
            get
            {
                return this.ParserCompilationMode == CompilationMode.RestrictedViewGenerationMode ? false : true;
            }
        }
    }
}
