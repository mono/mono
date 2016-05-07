//---------------------------------------------------------------------
// <copyright file="SymbolTable.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;

namespace System.Data.SqlClient.SqlGen
{
    /// <summary>
    /// The symbol table is quite primitive - it is a stack with a new entry for
    /// each scope.  Lookups search from the top of the stack to the bottom, until
    /// an entry is found.
    /// 
    /// The symbols are of the following kinds
    /// <list type="bullet">
    /// <item><see cref="Symbol"/> represents tables (extents/nested selects/unnests)</item>
    /// <item><see cref="JoinSymbol"/> represents Join nodes</item>
    /// <item><see cref="Symbol"/> columns.</item>
    /// </list>
    /// 
    /// Symbols represent names <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)"/> to be resolved, 
    /// or things to be renamed.
    /// </summary>
    internal sealed class SymbolTable
    {
        private List<Dictionary<string, Symbol>> symbols = new List<Dictionary<string, Symbol>>();

        internal void EnterScope()
        {
            symbols.Add(new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase));
        }

        internal void ExitScope()
        {
            symbols.RemoveAt(symbols.Count - 1);
        }

        internal void Add(string name, Symbol value)
        {
            symbols[symbols.Count - 1][name] = value;
        }

        internal Symbol Lookup(string name)
        {
            for (int i = symbols.Count - 1; i >= 0; --i)
            {
                if (symbols[i].ContainsKey(name))
                {
                    return symbols[i][name];
                }
            }

            return null;
        }
    }
}
