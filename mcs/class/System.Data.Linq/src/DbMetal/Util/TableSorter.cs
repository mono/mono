#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
//        Andrey Shchekin
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbLinq.Schema;

namespace DbMetal.Util
{
    /// <summary>
    /// sort tables - parent tables first, child tables next.
    /// </summary>
    public class TableSorter : IComparer<DbLinq.Schema.Dbml.Table>
    {
        Dictionary<string, DbLinq.Schema.Dbml.Table> _typeNameToTableMap; // = tables.ToDictionary(t => t.Name);
        //Dictionary<DbLinq.Schema.Dbml.Table, int> _originalOrder = new Dictionary<DbLinq.Schema.Dbml.Table, int>();

        public TableSorter(IEnumerable<DbLinq.Schema.Dbml.Table> tables)
        {
            _typeNameToTableMap = tables.ToDictionary(t => t.Type.Name);

            //int indx = 0;
            foreach (DbLinq.Schema.Dbml.Table t in tables)
            {
                //_originalOrder[t] = indx++;
                foreach (DbLinq.Schema.Dbml.Table child in EnumChildTables(t))
                {
                    child._isChild = true;
                }
            }
        }

        #region IComparer<Table> Members

        public int Compare(DbLinq.Schema.Dbml.Table x, DbLinq.Schema.Dbml.Table y)
        {
            if (x == y)
                return 0; //crappy sort implementation in .NET framework?!

            foreach (DbLinq.Schema.Dbml.Table child_of_x in EnumChildTables(x))
            {
                if (y == child_of_x)
                    return -1;
            }
            foreach (DbLinq.Schema.Dbml.Table child_of_y in EnumChildTables(y))
            {
                if (x == child_of_y)
                    return +1;
            }

            //if we get here, x/y are not above or below each other.
            return x._isChild.CompareTo(y._isChild);
            //return 0;
        }
        #endregion

        #region recursive walk through child table hierarchy
        private IEnumerable<DbLinq.Schema.Dbml.Table> EnumChildTables(DbLinq.Schema.Dbml.Table parent)
        {
            Dictionary<DbLinq.Schema.Dbml.Table, bool> visitedMap = new Dictionary<DbLinq.Schema.Dbml.Table, bool>();
            return EnumChildTables_(parent, visitedMap);
        }

        /// <summary>
        /// recursively list all child tables.
        /// We use visitedMap to prevent duplicates.
        /// </summary>
        private IEnumerable<DbLinq.Schema.Dbml.Table> EnumChildTables_(DbLinq.Schema.Dbml.Table parent, Dictionary<DbLinq.Schema.Dbml.Table, bool> visitedMap)
        {
            //In Northwind DB, Employee.ReportsTo points back to itself, 
            //mark as visited to prevent recursion
            visitedMap[parent] = true; 

            var q1 = parent.Type.Associations.Where(a => !a.IsForeignKey
                                                         && a.OtherKey != null
                                                         && _typeNameToTableMap.ContainsKey(a.Type));
            var q = q1.ToList(); //for debugging

            //loop through direct child tables ...
            foreach (var assoc in q)
            {
                DbLinq.Schema.Dbml.Table child = _typeNameToTableMap[assoc.Type];
                if (visitedMap.ContainsKey(child))
                    continue;

                visitedMap[child] = true;
                yield return child;

                //... and recurse into children of children:
                foreach (DbLinq.Schema.Dbml.Table child2 in EnumChildTables_(child, visitedMap))
                {
                    yield return child2;
                }

            }
        }

        #endregion
    }
}