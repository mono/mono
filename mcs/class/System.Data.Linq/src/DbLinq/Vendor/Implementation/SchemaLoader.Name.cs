#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;

namespace DbLinq.Vendor.Implementation
{
    partial class SchemaLoader
    {
        /// <summary>
        /// Checks all names in DBML schema, 
        /// and enquotes the ones where a casing problem could occur
        /// </summary>
        /// <param name="schema"></param>
        public virtual void CheckNamesSafety(Database schema)
        {
            schema.Name = Vendor.SqlProvider.GetSafeName(schema.Name);
            foreach (var table in schema.Table)
            {
                table.Name = Vendor.SqlProvider.GetSafeName(table.Name);
                foreach (var column in table.Type.Columns)
                {
                    column.Name = Vendor.SqlProvider.GetSafeName(column.Name);
                }
                foreach (var association in table.Type.Associations)
                {
                    association.Name = Vendor.SqlProvider.GetSafeName(association.Name);
                }
            }
            foreach (var storedProcedure in schema.Functions)
            {
                storedProcedure.Name = Vendor.SqlProvider.GetSafeName(storedProcedure.Name);
            }
        }

        /// <summary>
        /// Gets the primary keys.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        protected static List<string> GetPrimaryKeys(Table table)
        {
            return (from c in table.Type.Columns where c.IsPrimaryKey select c.Name).ToList();
        }

        /// <summary>
        /// Checks for problematic names on columns
        /// We currently have 1 case, where column is equal to table name
        /// </summary>
        /// <param name="schema"></param>
        protected virtual void CheckColumnsName(Database schema)
        {
            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Type.Columns)
                {
                    // THE case
                    if (column.Member == table.Type.Name)
                    {
                        // now, we try to append 1, then 2, etc.
                        var appendValue = 0;
                        for (; ; )
                        {
                            var newColumnMember = column.Member + ++appendValue;
                            if (!table.Type.Columns.Any(c => c.Member == newColumnMember))
                            {
                                column.Member = newColumnMember;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks for name conflicts, given lambdas to correct on conflicts
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="tableNamedAssociationRenamer"></param>
        /// <param name="columnNamedAssociationRenamer"></param>
        protected virtual void CheckConstraintsName(Database schema,
                                      Func<Association, string> tableNamedAssociationRenamer,
                                      Func<Association, string> columnNamedAssociationRenamer)
        {
            foreach (var table in schema.Tables)
            {
                foreach (var association in table.Type.Associations)
                {
                    var localAssociation = association;
                    if (association.Member == table.Type.Name)
                        association.Member = tableNamedAssociationRenamer(association);
                    else if ((from column in table.Type.Columns where column.Member == localAssociation.Member select column).FirstOrDefault() != null)
                    {
                        association.Member = columnNamedAssociationRenamer(association);
                    }
                }
            }
        }

        /// <summary>
        /// Checks for name conflicts
        /// </summary>
        /// <param name="schema"></param>
        protected virtual void CheckConstraintsName(Database schema)
        {
            CheckConstraintsName(schema,
                       association => association.ThisKey.Replace(',', '_') + association.Member,
                       association => association.Member + association.Type);
        }

        /// <summary>
        /// Generates storage fields, given a formatting method
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="storageGenerator"></param>
        protected virtual void GenerateStorageAndMemberFields(Database schema, Func<string, string> storageGenerator)
        {
            foreach (var table in schema.Tables)
            {
                foreach (var column in table.Type.Columns)
                {
                    column.Storage = storageGenerator(column.Member);
                }

                Dictionary<string, int> storageFields = new Dictionary<string,int>();
                Dictionary<string, int> memberFields = new Dictionary<string,int>();
                foreach (var association in table.Type.Associations)
                {
                    association.Storage = storageGenerator(association.Member);

                    //Associations may contain the same foreign key more than once - add a number suffix to duplicates
                    int storageSuffix = 0;
                    if ( storageFields.TryGetValue(association.Storage, out storageSuffix) )
                        association.Storage += storageSuffix;
                    storageFields[association.Storage] = storageSuffix + 1;

                    int memberSuffix = 0;
                    if ( memberFields.TryGetValue(association.Member, out memberSuffix) )
                        association.Member += memberSuffix;
                    memberFields[association.Member] = memberSuffix + 1;
                }
            }
        }

        /// <summary>
        /// Generates all storage fields
        /// </summary>
        /// <param name="schema"></param>
        protected virtual void GenerateStorageAndMemberFields(Database schema)
        {
            GenerateStorageAndMemberFields(schema, delegate(string name)
                                              {
                                                  //jgm 2008June: pre-pended underscore to have same storage format as MS
                                                  // TODO: add an option for this behavior
                                                  var storage = "_" + NameFormatter.Format(name, Case.camelCase);
                                                  if (storage == name)
                                                      storage = "_" + storage;
                                                  return storage;
                                              });
        }
    }
}
