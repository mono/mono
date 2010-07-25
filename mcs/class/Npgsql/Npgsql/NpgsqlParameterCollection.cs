// created on 09/07/2003 at 20:20
// Npgsql.NpgsqlParameterCollection.cs
//
// Author:
// Brar Piening (brar@gmx.de)
//
// Rewritten from the scratch to derive from MarshalByRefObject instead of ArrayList.
// Recycled some parts of the original NpgsqlParameterCollection.cs
// by Francisco Jr. (fxjrlists@yahoo.com.br)
//
// Copyright (C) 2002 The Npgsql Development Team
// npgsql-general@gborg.postgresql.org
// http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Reflection;
using System.Data;
using System.Collections;
using System.ComponentModel;
using NpgsqlTypes;

#if WITHDESIGN
using Npgsql.Design;
#endif

namespace Npgsql
{
    /// <summary>
    /// Represents a collection of parameters relevant to a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>
    /// as well as their respective mappings to columns in a <see cref="System.Data.DataSet">DataSet</see>.
    /// This class cannot be inherited.
    /// </summary>
    
    #if WITHDESIGN
    [ListBindable(false)]
    [Editor(typeof(NpgsqlParametersEditor), typeof(System.Drawing.Design.UITypeEditor))]
    #endif
    
    public sealed class NpgsqlParameterCollection : MarshalByRefObject, IDataParameterCollection
    {
        private ArrayList InternalList = new ArrayList();

        // Logging related value
        private static readonly String CLASSNAME = "NpgsqlParameterCollection";

        // Our resource manager
        private System.Resources.ResourceManager resman;

        /// <summary>
        /// Initializes a new instance of the NpgsqlParameterCollection class.
        /// </summary>
        internal NpgsqlParameterCollection()
        {
            this.resman = new System.Resources.ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
        }

#region NpgsqlParameterCollection Member

        /// <summary>
        /// Gets the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to retrieve.</param>
        /// <value>The <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> with the specified name, or a null reference if the parameter is not found.</value>
        
        #if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        #endif
        
        public NpgsqlParameter this[string parameterName] {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, parameterName);
                return (NpgsqlParameter)this.InternalList[IndexOf(parameterName)];
            }
            set
            {
                NpgsqlEventLog.LogIndexerSet(LogLevel.Debug, CLASSNAME, parameterName, value);
                this.InternalList[IndexOf(parameterName)] = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to retrieve.</param>
        /// <value>The <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> at the specified index.</value>
        
        #if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        #endif
        
        public NpgsqlParameter this[int index] {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, index);
                return (NpgsqlParameter)this.InternalList[index];
            }
            set
            {
                NpgsqlEventLog.LogIndexerSet(LogLevel.Debug, CLASSNAME, index, value);
                this.InternalList[index] = value;
            }
        }

        /// <summary>
        /// Adds the specified <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to add to the collection.</param>
        /// <returns>The index of the new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        public NpgsqlParameter Add(NpgsqlParameter value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", value);

            // Do not allow parameters without name.
            
            this.InternalList.Add(value);
            
            // Check if there is a name. If not, add a name based in the index of parameter.
            if (value.ParameterName.Trim() == String.Empty ||
            (value.ParameterName.Length == 1 && value.ParameterName[0] == ':'))
                value.ParameterName = ":" + "Parameter" + (IndexOf(value) + 1);
        
            
            return value;
        }

        /// <summary>
        /// Adds a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see> given the specified parameter name and value.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.</param>
        /// <param name="value">The Value of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to add to the collection.</param>
        /// <returns>The index of the new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        /// <remarks>
        /// Use caution when using this overload of the
        /// <b>Add</b> method to specify integer parameter values.
        /// Because this overload takes a <i>value</i> of type Object,
        /// you must convert the integral value to an <b>Object</b>
        /// type when the value is zero, as the following C# example demonstrates.
        /// <code>parameters.Add(":pname", Convert.ToInt32(0));</code>
        /// If you do not perform this conversion, the compiler will assume you
        /// are attempting to call the NpgsqlParameterCollection.Add(string, DbType) overload.
        /// </remarks>
        public NpgsqlParameter Add(string parameterName, object value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, value);
            return this.Add(new NpgsqlParameter(parameterName, value));
        }

        /// <summary>
        /// Adds a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see> given the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterType">One of the DbType values.</param>
        /// <returns>The index of the new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        public NpgsqlParameter Add(string parameterName, NpgsqlDbType parameterType)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, parameterType);
            return this.Add(new NpgsqlParameter(parameterName, parameterType));
        }

        /// <summary>
        /// Adds a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see> with the parameter name, the data type, and the column length.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterType">One of the DbType values.</param>
        /// <param name="size">The length of the column.</param>
        /// <returns>The index of the new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        public NpgsqlParameter Add(string parameterName, NpgsqlDbType parameterType, int size)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, parameterType, size);
            return this.Add(new NpgsqlParameter(parameterName, parameterType, size));
        }

        /// <summary>
        /// Adds a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see> with the parameter name, the data type, the column length, and the source column name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="parameterType">One of the DbType values.</param>
        /// <param name="size">The length of the column.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <returns>The index of the new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        public NpgsqlParameter Add(string parameterName, NpgsqlDbType parameterType, int size, string sourceColumn)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", parameterName, parameterType, size, sourceColumn);
            return this.Add(new NpgsqlParameter(parameterName, parameterType, size, sourceColumn));
        }

#endregion

#region IDataParameterCollection Member

        object System.Data.IDataParameterCollection.this[string parameterName] {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, parameterName);
                return this.InternalList[IndexOf(parameterName)];
            }
            set
            {
                NpgsqlEventLog.LogIndexerSet(LogLevel.Debug, CLASSNAME, parameterName, value);
                CheckType(value);
                this.InternalList[IndexOf(parameterName)] = value;
            }
        }

        /// <summary>
        /// Removes the specified <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> from the collection using the parameter name.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to retrieve.</param>
        public void RemoveAt(string parameterName)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "RemoveAt", parameterName);
            this.InternalList.RemoveAt(IndexOf(parameterName));
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> with the specified parameter name exists in the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to find.</param>
        /// <returns><b>true</b> if the collection contains the parameter; otherwise, <b>false</b>.</returns>
        public bool Contains(string parameterName)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Contains", parameterName);
            return (IndexOf(parameterName) != -1);
        }

        /// <summary>
        /// Gets the location of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> in the collection with a specific parameter name.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to find.</param>
        /// <returns>The zero-based location of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> in the collection.</returns>
        public int IndexOf(string parameterName)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IndexOf", parameterName);

            // Iterate values to see what is the index of parameter.
            Int32 index = 0;
            if ((parameterName[0] == ':') || (parameterName[0] == '@'))
                parameterName = parameterName.Remove(0, 1);

            foreach (NpgsqlParameter parameter in this)
            {
                if (parameter.ParameterName.Remove(0, 1) == parameterName)
                    return index;
                index++;
            }
            return -1;
        }

#endregion

#region IList Member

        bool IList.IsReadOnly {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsReadOnly");
                return this.InternalList.IsReadOnly;
            }
        }

        object System.Collections.IList.this[int index] {
            get
            {
                NpgsqlEventLog.LogIndexerGet(LogLevel.Debug, CLASSNAME, index);
                return (NpgsqlParameter)this.InternalList[index];
            }
            set
            {
                NpgsqlEventLog.LogIndexerSet(LogLevel.Debug, CLASSNAME, index, value);
                CheckType(value);
                this.InternalList[index] = value;
            }
        }

        /// <summary>
        /// Removes the specified <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> from the collection using a specific index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter.</param>
        public void RemoveAt(int index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "RemoveAt", index);
            this.InternalList.RemoveAt(index);
        }

        /// <summary>
        /// Inserts a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index where the parameter is to be inserted within the collection.</param>
        /// <param name="value">The <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to add to the collection.</param>
        public void Insert(int index, object value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Insert", index, value);
            CheckType(value);
            this.InternalList.Insert(index, value);
        }

        /// <summary>
        /// Removes the specified <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> from the collection.
        /// </summary>
        /// <param name="value">The <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to remove from the collection.</param>
        public void Remove(object value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Remove", value);
            CheckType(value);
            this.InternalList.Remove(value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> exists in the collection.
        /// </summary>
        /// <param name="value">The value of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to find.</param>
        /// <returns>true if the collection contains the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object; otherwise, false.</returns>
        public bool Contains(object value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Contains", value);
            CheckType(value);
            return this.InternalList.Contains(value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> with the specified parameter name exists in the collection.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to find.</param>
        /// <param name="parameter">A reference to the requested parameter is returned in this out param if it is found in the list.  This value is null if the parameter is not found.</param>
        /// <returns><b>true</b> if the collection contains the parameter and param will contain the parameter; otherwise, <b>false</b>.</returns>
        public bool TryGetValue(string parameterName, out NpgsqlParameter parameter)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "TryGetValue", parameterName);
            int index = IndexOf(parameterName);
            if (index != -1)
            {
                parameter = this[index];
                return true;
            }
            else
            {
                parameter = null;
                return false;
            }
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Clear");
            this.InternalList.Clear();
        }

        /// <summary>
        /// Gets the location of a <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> in the collection.
        /// </summary>
        /// <param name="value">The value of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to find.</param>
        /// <returns>The zero-based index of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object in the collection.</returns>
        public int IndexOf(object value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IndexOf", value);
            CheckType(value);
            return this.InternalList.IndexOf(value);
        }

        /// <summary>
        /// Adds the specified <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object to the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see>.
        /// </summary>
        /// <param name="value">The <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> to add to the collection.</param>
        /// <returns>The zero-based index of the new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> object.</returns>
        public int Add(object value)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Add", value);
            CheckType(value);
            this.Add((NpgsqlParameter)value);
            return IndexOf(value);
        }

        bool IList.IsFixedSize {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsFixedSize");
                return this.InternalList.IsFixedSize;
            }
        }

#endregion

#region ICollection Member

        bool ICollection.IsSynchronized {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsSynchronized");
                return this.InternalList.IsSynchronized;
            }
        }

        /// <summary>
        /// Gets the number of <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> objects in the collection.
        /// </summary>
        /// <value>The number of <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> objects in the collection.</value>
        
        #if WITHDESIGN
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        #endif
        
        public int Count {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Count");
                return this.InternalList.Count;
            }
        }

        /// <summary>
        /// Copies <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> objects from the <see cref="Npgsql.NpgsqlParameterCollection">NpgsqlParameterCollection</see> to the specified array.
        /// </summary>
        /// <param name="array">An <see cref="System.Array">Array</see> to which to copy the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> objects in the collection.</param>
        /// <param name="index">The starting index of the array.</param>
        public void CopyTo(Array array, int index)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CopyTo", array, index);
            this.InternalList.CopyTo(array, index);
        }

        object ICollection.SyncRoot {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "SyncRoot");
                return this.InternalList.SyncRoot;
            }
        }

#endregion

#region IEnumerable Member

        /// <summary>
        /// Returns an enumerator that can iterate through the collection.
        /// </summary>
        /// <returns>An <see cref="System.Collections.IEnumerator">IEnumerator</see> that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "GetEnumerator");
            return this.InternalList.GetEnumerator();
        }

#endregion

        /// <summary>
        /// In methods taking an object as argument this method is used to verify
        /// that the argument has the type <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// </summary>
        /// <param name="Object">The object to verify</param>
        private void CheckType(object Object)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CheckType", Object);
            if(Object.GetType() != typeof(NpgsqlParameter))
                throw new InvalidCastException(String.Format(this.resman.GetString("Exception_WrongType"), Object.GetType().ToString()));
        }

    }
}
