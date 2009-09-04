using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace nwind
{
    partial class Northwind
    {
        public Table<NoStorageCategory> NoStorageCategories { get { return GetTable<NoStorageCategory>(); } }
    }

    [Table(Name = "dbo.Employees")]
    partial class EmployeeWithStringIdentifier
    {
        private int _EmployeeID;
        private string _LastName;

        [Column(Storage = "_EmployeeID", Name = "EmployeeID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
        public string Identifier
        {
            get { return this._EmployeeID.ToString(); }
        }

        [Column(Storage = "_LastName", DbType = "NVarChar(20) NOT NULL", CanBeNull = false)]
        public string LastName
        {
            get
            {
                return this._LastName;
            }
            set
            {
                this._LastName = value;
            }
        }
    }


    [Table(Name = "dbo.Categories")]
    public partial class NoStorageCategory
    {
        public bool propertyInvoked_CategoryName = false;
        public bool propertyInvoked_Description = false;

        // Tests the Storage without a setter for the property.
        private int _categoryID;
        [Column(Storage = "_categoryID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
        public int CategoryID
        {
            get { return _categoryID; }
        }

        // No "Storage" attribute, this should go through the property.
        private string _categoryName;
        [Column(DbType = "NVarChar(15) NOT NULL", CanBeNull = false)]
        public string CategoryName
        {
            get { return _categoryName; }
            set
            {
                if (value != _categoryName)
                {
                    _categoryName = value;
                }
                propertyInvoked_CategoryName = true;
            }
        }

        // "Storage" and property, should set the field directly.
        private string _description;
        [DebuggerNonUserCode]
        [Column(Storage = "_description", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
        public string Description
        {
            get { return _description; }
            set
            {
                if (value != _description)
                {
                    _description = value;
                }
                propertyInvoked_Description = true;
            }
        }
    }
}
