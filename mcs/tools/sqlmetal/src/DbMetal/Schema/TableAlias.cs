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
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DbLinq.Vendor;
using DbMetal;

namespace DbMetal.Schema
{
#if !MONO_STRICT
    public
#endif
    class TableAlias
    {
        public class Renamings : INameAliases
        {
            [XmlAttribute("Name")]
            public string Name { get; set; }

            [XmlAttribute("Class")]
            public string Class { get; set; }

            [XmlElement("Renaming")]
            public readonly List<Renaming> Arr = new List<Renaming>();

            protected string GetAlias(string name)
            {
                return (from r in Arr where r.old == name select r.@new).SingleOrDefault();
            }

            public string GetTableTypeAlias(string table, string schema)
            {
                return GetAlias(table);
            }

            public string GetTableMemberAlias(string table, string schema)
            {
                return null;
            }

            public string GetColumnMemberAlias(string column, string table, string schema)
            {
                return GetAlias(column);
            }

            public string GetColumnForcedType(string column, string table, string schema)
            {
                return null;
            }

            public bool? GetColumnGenerated(string column, string table, string schema)
            {
                return null;
            }

            public DbLinq.Schema.Dbml.AutoSync? GetColumnAutoSync(string column, string table, string schema)
            {
                return null;
            }

            public string GetDatabaseNameAlias(string databaseName)
            {
                return Name;
            }

            public string GetClassNameAlias(string className)
            {
                return Class;
            }
        }

        public class Renaming
        {
            [XmlAttribute]
            public string old;
            [XmlAttribute]
            public string @new;
        }

        public static Renamings Load(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var renamingsXmlSerializer = new XmlSerializer(typeof(Renamings));
                var renamings = (Renamings)renamingsXmlSerializer.Deserialize(stream);
                return renamings;
            }
        }

        public static IDictionary<string, string> Load(string fileName, Parameters parameters)
        {
            if (!System.IO.File.Exists(fileName))
                throw new ArgumentException("Renames file missing:" + parameters.Aliases);

            Console.WriteLine("Loading renames file: " + fileName);

            Renamings renamings = Load(parameters.Aliases);

            Dictionary<string, string> aliases = new Dictionary<string, string>();
            foreach (Renaming renaming in renamings.Arr)
            {
                aliases[renaming.old] = renaming.@new;
            }
            return aliases;
        }

        public static IDictionary<string, string> Load(Parameters parameters)
        {
            if (parameters.Aliases == null)
                return new Dictionary<string, string>();
            return Load(parameters.Aliases, parameters);
        }

    }
}