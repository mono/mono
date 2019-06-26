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

namespace DbLinq.Schema
{
    /// <summary>
    /// Allows to manipulate words
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface INameFormatter
    {
        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        SchemaName GetSchemaName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        /// <summary>
        /// Gets the name of the procedure.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        ProcedureName GetProcedureName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        ParameterName GetParameterName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        TableName GetTableName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <param name="dbName">Name of the db.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        ColumnName GetColumnName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        /// <summary>
        /// Gets the name of the association.
        /// </summary>
        /// <param name="dbManyName">Name of the db many.</param>
        /// <param name="dbOneName">Name of the db one.</param>
        /// <param name="dbConstraintName">Name of the db constraint.</param>
        /// <param name="foreignKeyName">Name of the foreign key.</param>
        /// <param name="extraction">The extraction.</param>
        /// <param name="nameFormat">The name format.</param>
        /// <returns></returns>
        AssociationName GetAssociationName(string dbManyName, string dbOneName,
                                           string dbConstraintName, string foreignKeyName,
                                            WordsExtraction extraction, NameFormat nameFormat);
        // 
        /// <summary>
        /// Reformats a name by adjusting its case.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="newCase">The new case.</param>
        /// <returns></returns>
        string Format(string words, Case newCase);
    }
}
