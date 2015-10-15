//------------------------------------------------------------------------------
// <copyright file="EntityDesignerDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------
using System.Collections;
using System.Data;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
    public class EntityDesignerDataSourceView : DesignerDataSourceView
    {
        private EntityDataSourceDesignerHelper _helper;

        public EntityDesignerDataSourceView(EntityDataSourceDesigner owner)
            : base(owner, EntityDataSourceDesignerHelper.DefaultViewName)
        {
            _helper = owner.Helper;
        }

        public override bool CanDelete
        {
            get
            {
                return CanModify && _helper.EnableDelete;
            }
        }

        public override bool CanInsert
        {
            get
            {
                return CanModify && _helper.EnableInsert;
            }
        }

        internal bool CanModify
        {
            get
            {
                return !String.IsNullOrEmpty(_helper.EntitySetName) &&
                    String.IsNullOrEmpty(_helper.Select) && 
                    String.IsNullOrEmpty(_helper.CommandText) &&
                    String.IsNullOrEmpty(_helper.GroupBy);
            }
        }

        public override bool CanPage
        {
            get
            {
                return _helper.CanPage;
            }
        }

        public override bool CanSort
        {
            get
            {
                return _helper.CanSort;
            }
        }

        public override bool CanUpdate
        {
            get
            {
                return CanModify && _helper.EnableUpdate;
            }
        }

        public override IDataSourceViewSchema Schema
        {
            get
            {
                DataTable schemaTable = _helper.LoadSchema();
                if (schemaTable == null)
                {
                    return null;
                }
                return new DataSetViewSchema(schemaTable);
            }
        }

        public override IEnumerable GetDesignTimeData(int minimumRows, out bool isSampleData)
        {
            DataTable schemaTable = _helper.LoadSchema();
            if (schemaTable != null)
            {
                isSampleData = true;
                return DesignTimeData.GetDesignTimeDataSource(DesignTimeData.CreateSampleDataTable(new DataView(schemaTable), true), minimumRows);
            }

            // Couldn't find design-time schema, use base implementation
            return base.GetDesignTimeData(minimumRows, out isSampleData);
        }
    }
}
