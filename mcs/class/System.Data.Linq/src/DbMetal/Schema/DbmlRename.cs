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

using System.Linq;
using System.Xml.Serialization;
using DbLinq.Vendor;

namespace DbMetal.Schema
{
    /// <summary>
    /// This class main purpose is to allow renamings.
    /// It is based on DBML format (but simpler).
    /// </summary>
    //[XmlRoot("Database")]
    [XmlRoot("Database", Namespace = "http://schemas.microsoft.com/linqtosql/dbml/2007", IsNullable = false)]
    public class DbmlRename : INameAliases
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces XmlNamespaceDeclarations { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Class")]
        public string Class { get; set; }

        [XmlElement("Table")]
        public Table[] Tables { get; set; }

        public class Table
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("Member")]
            public string Member { get; set; }

            [XmlElement("Type")]
            public Type Type { get; set; }
        }

        public class Type
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlElement("Column")]
            public Column[] Columns { get; set; }
        }

        public class Column
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("Member")]
            public string Member { get; set; }

            [XmlAttribute("Storage")]
            public string Storage { get; set; }

            [XmlAttribute("Type")]
            public string Type { get; set; }
        }

        protected Table GetTable(string table, string schema)
        {
            string qualifiedName;
            if (!string.IsNullOrEmpty(schema))
                qualifiedName = string.Format("{0}.{1}", schema, table);
            else
                qualifiedName = table;
            return (from t in Tables where t.Name == qualifiedName select t).SingleOrDefault();
        }

        protected Column GetColumn(string column, string table, string schema)
        {
            var t = GetTable(table, schema);
            if (t == null || t.Type == null || t.Type.Columns == null)
                return null;
            return (from c in t.Type.Columns where c.Name == column select c).SingleOrDefault();
        }

        public string GetTableTypeAlias(string table, string schema)
        {
            var t = GetTable(table, schema);
            if (t == null || t.Type == null)
                return null;
            return t.Type.Name;
        }

        public string GetTableMemberAlias(string table, string schema)
        {
            var t = GetTable(table, schema);
            if (t == null)
                return null;
            return t.Member;
        }

        public string GetColumnMemberAlias(string column, string table, string schema)
        {
            var c = GetColumn(column, table, schema);
            if (c == null)
                return null;
            return c.Member;
        }

        public string GetColumnForcedType(string column, string table, string schema)
        {
            var c = GetColumn(column, table, schema);
            if (c == null)
                return null;
            return c.Type;
        }
    }
}
