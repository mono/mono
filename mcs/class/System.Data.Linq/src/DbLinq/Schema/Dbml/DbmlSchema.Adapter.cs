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
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;
using DbLinq.Schema.Dbml.Adapter;

/*
 * Here are additional methods and properties to DBML class.
 * Those properties are useful to dynamically change arrays
 */

namespace DbLinq.Schema.Dbml
{
    partial class Database
    {
        [Browsable(false)]
        [XmlIgnore]
        public ISimpleList<Table> Tables;
        [Browsable(false)]
        [XmlIgnore]
        public ISimpleList<Function> Functions;

        public Database()
        {
            SpecifiedPropertyUpdater.Register(this);
            Tables = new ArrayAdapter<Table>(this, "Table");
            Functions = new ArrayAdapter<Function>(this, "Function");
        }
    }

    partial class Table
    {
        public Table()
        {
            Type = new Type();
            SpecifiedPropertyUpdater.Register(this);
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}), {2}", Member, Name, Type);
        }
    }

    partial class Type
    {
        [Browsable(false)]
        [XmlIgnore]
        public readonly ISimpleList<Column> Columns;

        [Browsable(false)]
        [XmlIgnore]
        public readonly ISimpleList<Association> Associations;

        public Type()
        {
            SpecifiedPropertyUpdater.Register(this);
            Columns = new ArrayAdapter<Column>(this, "Items");
            Associations = new ArrayAdapter<Association>(this, "Items");
        }

        public override string ToString()
        {
            string summary = Columns.Count + " Columns";
            if (Associations.Count > 0)
                summary += ", " + Associations.Count + " Associations";
            return summary;
        }
    }

    partial class Function
    {
        [Browsable(false)]
        [XmlIgnore]
        public bool BodyContainsSelectStatement;

        [Browsable(false)]
        [XmlIgnore]
        public ISimpleList<Parameter> Parameters;
        [Browsable(false)]
        [XmlIgnore]
        public Return Return
        {
            get
            {
                if (Items == null)
                    return null;
                foreach (object item in Items)
                {
                    var r = item as Return;
                    if (r != null)
                        return r;
                }
                return null;
            }
            set
            {
                if (Items == null)
                {
                    Items = new[] { value };
                    return;
                }
                for (int index = 0; index < Items.Length; index++)
                {
                    if (Items[index] is Return)
                    {
                        Items[index] = value;
                        return;
                    }
                }
                List<object> items = new List<object>(Items);
                items.Add(value);
                Items = items.ToArray();
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        public object ElementType;

        public Function()
        {
            SpecifiedPropertyUpdater.Register(this);
            Parameters = new ArrayAdapter<Parameter>(this, "Parameter");
        }
    }

    partial class Association
    {
        /// <summary>
        /// ThisKey, provided as an array of strings (each string being a key)
        /// </summary>
        [XmlIgnore]
        public ISimpleList<string> TheseKeys;
        /// <summary>
        /// OtherKey, provided as an array of strings (each string being a key)
        /// </summary>
        [XmlIgnore]
        public ISimpleList<string> OtherKeys;

        public Association()
        {
            SpecifiedPropertyUpdater.Register(this);
            TheseKeys = new CsvArrayAdapter(this, "ThisKey");
            OtherKeys = new CsvArrayAdapter(this, "OtherKey");
        }

        public override string ToString()
        {
            return Name;
        }
    }

    partial class Column
    {
        private INamedType extendedType;
        [Browsable(false)]
        [XmlIgnore]
        public INamedType ExtendedType
        {
            get
            {
                if (extendedType == null)
                {
                    if (EnumType.IsEnum(Type))
                        extendedType = new EnumType(this, TypeMemberInfo);
                }
                return extendedType;
            }
        }

        public EnumType SetExtendedTypeAsEnumType()
        {
            return new EnumType(this, TypeMemberInfo);
        }

        private MemberInfo TypeMemberInfo
        {
            get
            {
                return GetType().GetMember("Type")[0];
            }
        }

        public Column()
        {
            SpecifiedPropertyUpdater.Register(this);
        }

        public override string ToString()
        {
            return String.Format("{0} ({1}): {2} ({3})", Member, Name, Type, DbType);
        }
    }

    partial class Connection
    {
        public Connection()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    partial class Parameter
    {
        public Parameter()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
        [Browsable(false)]
        [XmlIgnore]
        public bool DirectionIn
        {
            get { return Direction == ParameterDirection.In || Direction == ParameterDirection.InOut; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public bool DirectionOut
        {
            get { return Direction == ParameterDirection.Out || Direction == ParameterDirection.InOut; }
        }
    }

    partial class Return
    {
        public Return()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    partial class TableFunction
    {
        public TableFunction()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    partial class TableFunctionParameter
    {
        public TableFunctionParameter()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    partial class TableFunctionReturn
    {
        public TableFunctionReturn()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }
}
