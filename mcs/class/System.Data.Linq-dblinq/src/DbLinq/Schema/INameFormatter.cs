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

using DbLinq.Schema;

namespace DbLinq.Schema
{
#if MONO_STRICT
    internal
#else
    public
#endif
    interface INameFormatter
    {
        SchemaName GetSchemaName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        ProcedureName GetProcedureName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        ParameterName GetParameterName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        TableName GetTableName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        ColumnName GetColumnName(string dbName, WordsExtraction extraction, NameFormat nameFormat);
        AssociationName GetAssociationName(string dbManyName, string dbOneName,
                                           string dbConstraintName, string foreignKeyName,
                                            WordsExtraction extraction, NameFormat nameFormat);
        // reformats a name by adjusting its case
        string Format(string words, Case newCase);
    }
}
