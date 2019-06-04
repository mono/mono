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
    /// <summary>
    /// Adataper for Database
    /// </summary>
    partial class Database
    {
        /// <summary>
        /// Wraps all tables
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public ISimpleList<Table> Tables;
        /// <summary>
        /// Wraps all stored procedures
        /// </summary>
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

    /// <summary>
    /// Adapter for Table
    /// </summary>
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

    /// <summary>
    /// Adapter for Type
    /// </summary>
    partial class Type
    {
        /// <summary>
        /// Wrapper for columns
        /// </summary>
        [Browsable(false)]
        [XmlIgnore]
        public readonly ISimpleList<Column> Columns;

        /// <summary>
        /// Wrapper for associations
        /// </summary>
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

    /// <summary>
    /// Adapter for function
    /// </summary>
    partial class Function
    {
        // TODO: remove this attribute
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

    /// <summary>
    /// Adapter for Association
    /// </summary>
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

    /// <summary>
    /// Adapter for Column
    /// </summary>
    partial class Column
    {
        private INamedType extendedType;
        /// <summary>
        /// Extended type, for handling enum types, for example.
        /// </summary>
        /// <value>The type of the extended.</value>
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

    /// <summary>
    /// Adapter for Parameter
    /// </summary>
    partial class Parameter
    {
        public Parameter()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
        /// <summary>
        /// Gets a value indicating whether [direction in].
        /// </summary>
        /// <value><c>true</c> if [direction in]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        [XmlIgnore]
        public bool DirectionIn
        {
            get { return Direction == ParameterDirection.In || Direction == ParameterDirection.InOut; }
        }

        /// <summary>
        /// Gets a value indicating whether [direction out].
        /// </summary>
        /// <value><c>true</c> if [direction out]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        [XmlIgnore]
        public bool DirectionOut
        {
            get { return Direction == ParameterDirection.Out || Direction == ParameterDirection.InOut; }
        }
    }

    /// <summary>
    /// Adapter for Return
    /// </summary>
    partial class Return
    {
        public Return()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    /// <summary>
    /// Adapter for TableFunction
    /// </summary>
    partial class TableFunction
    {
        public TableFunction()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    /// <summary>
    /// Adapter for TableFunctionParameter
    /// </summary>
    partial class TableFunctionParameter
    {
        public TableFunctionParameter()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }

    /// <summary>
    /// Adapter for TableFunctionReturn
    /// </summary>
    partial class TableFunctionReturn
    {
        public TableFunctionReturn()
        {
            SpecifiedPropertyUpdater.Register(this);
        }
    }
}
