//---------------------------------------------------------------------
// <copyright file="EntityDataSourceViewSchema.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Web.UI.WebControls
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    internal class EntityDataSourceViewSchema : DataTable
    {
        internal EntityDataSourceViewSchema(EntityDataSourceWrapperCollection wrappers)
        {
            DataColumn column;
            List<DataColumn> keys = new List<DataColumn>();
            PropertyDescriptorCollection properties = wrappers.GetItemProperties(null);
            MetadataWorkspace workspace = wrappers.Context.MetadataWorkspace;
            foreach (EntityDataSourceWrapperPropertyDescriptor property in properties)
            {
                Type propertyType = property.PropertyType;
                column = ConstructColumn(property);

                column.AllowDBNull = property.Column.IsNullable;

                EntityDataSourcePropertyColumn propertyColumn  = property.Column as EntityDataSourcePropertyColumn;
                if (null!= propertyColumn && propertyColumn.IsKey)
                {
                    keys.Add(column);
                }
                Columns.Add(column);
            }
            this.PrimaryKey = keys.ToArray();
        }

        internal EntityDataSourceViewSchema(ITypedList typedList)
        {
            PropertyDescriptorCollection properties = typedList.GetItemProperties(null);
            CreateColumnsFromPropDescs(properties, null);
        }

        internal EntityDataSourceViewSchema(IEnumerable results)
            : this(results, null)
        {
        }

        /// <summary>
        /// Creates a view schema with a set of typed results and an optional set of keyName properties on those results
        /// </summary>
        internal EntityDataSourceViewSchema(IEnumerable results, string[] keyNames)
        {
            Type type = GetListItemType(results.GetType());
            PropertyInfo[] infos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
            CreateColumnsFromPropDescs(properties, keyNames);
        }

        /// <summary>
        /// Creates a set of DataTable columns from a collection of property descriptors and an optional
        /// set of key names from metadata
        /// </summary>
        private void CreateColumnsFromPropDescs(PropertyDescriptorCollection properties, string[] keyNames)
        {
            List<DataColumn> keys = new List<DataColumn>();
            foreach (PropertyDescriptor property in properties)
            {
                System.ComponentModel.BrowsableAttribute attr =
                    (System.ComponentModel.BrowsableAttribute)property.Attributes[typeof(System.ComponentModel.BrowsableAttribute)];
                if (attr.Browsable)
                {
                    DataColumn column = ConstructColumn(property);
                    // If there are keyNames, check if this column is one of the keys
                    if (keyNames != null && keyNames.Contains(column.ColumnName))
                    {
                        keys.Add(column);
                    }
                    this.Columns.Add(column);
                }
            }
            if (keys.Count > 0)
            {
                this.PrimaryKey = keys.ToArray();
            }
        }

        private static DataColumn ConstructColumn(PropertyDescriptor property)
        {
            DataColumn column = new DataColumn();
            column.ColumnName = property.Name;
            Type propertyType = property.PropertyType;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type[] typeArguments = propertyType.GetGenericArguments();
                Debug.Assert(typeArguments.Length == 1, "should only have one type argument from Nullable<> condition.");

                column.DataType = typeArguments[0];
                column.AllowDBNull = true;
            }
            else 
            {
                column.DataType = propertyType;
                column.AllowDBNull = !propertyType.IsValueType;
            }
            column.ReadOnly = property.IsReadOnly;

            column.Unique = false;
            column.AutoIncrement = false;
            column.MaxLength = -1;
            return column;
        }

        // see System.Data.Objects.DataRecordObjectView.cs
        private static PropertyInfo GetTypedIndexer(Type type)
        {
            PropertyInfo indexer = null;

            if (typeof(IList).IsAssignableFrom(type) ||
                typeof(ITypedList).IsAssignableFrom(type) ||
                typeof(IListSource).IsAssignableFrom(type))
            {
                System.Reflection.PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                for (int idx = 0; idx < props.Length; idx++)
                {
                    if (props[idx].GetIndexParameters().Length > 0 && props[idx].PropertyType != typeof(object))
                    {
                        indexer = props[idx];
                        //Prefer the standard indexer, if there is one
                        if (indexer.Name == "Item")
                        {
                            break;
                        }
                    }
                }
            }

            return indexer;
        }

        // see System.Data.Objects.DataRecordObjectView.cs
        private static Type GetListItemType(Type type)
        {
            Type itemType;

            if (typeof(Array).IsAssignableFrom(type))
            {
                itemType = type.GetElementType();
            }
            else
            {
                PropertyInfo typedIndexer = GetTypedIndexer(type);

                if (typedIndexer != null)
                {
                    itemType = typedIndexer.PropertyType;
                }
                else
                {
                    itemType = type;
                }
            }

            return itemType;
        }

    }



}
