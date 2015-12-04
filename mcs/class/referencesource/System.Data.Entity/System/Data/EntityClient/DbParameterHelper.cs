//---------------------------------------------------------------------
// <copyright file="DbParameterHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------


namespace System.Data.EntityClient {

    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;

    public sealed partial class EntityParameter : DbParameter { 
        private object _value;

        private object _parent;

        private ParameterDirection _direction;
        private int? _size;


        private string _sourceColumn;
        private DataRowVersion _sourceVersion;
        private bool _sourceColumnNullMapping;

        private bool? _isNullable;

        private object _coercedValue;

        private EntityParameter(EntityParameter source) : this() { 
            EntityUtil.CheckArgumentNull(source, "source");

            source.CloneHelper(this);

            ICloneable cloneable = (_value as ICloneable);
            if (null != cloneable) { 
                _value = cloneable.Clone();
            }
        }

        private object CoercedValue { 
            get {
                return _coercedValue;
            }
            set {
                _coercedValue = value;
            }
        }

        [
        RefreshProperties(RefreshProperties.All),
        EntityResCategoryAttribute(EntityRes.DataCategory_Data),
        EntityResDescriptionAttribute(EntityRes.DbParameter_Direction),
        ]
        override public ParameterDirection Direction { 
            get {
                ParameterDirection direction = _direction;
                return ((0 != direction) ? direction : ParameterDirection.Input);
            }
            set {
                if (_direction != value) {
                    switch (value) { 
                    case ParameterDirection.Input:
                    case ParameterDirection.Output:
                    case ParameterDirection.InputOutput:
                    case ParameterDirection.ReturnValue:
                        PropertyChanging();
                        _direction = value;
                        break;
                    default:
                        throw EntityUtil.InvalidParameterDirection(value);
                    }
                }
            }
        }

        override public bool IsNullable { 
            get {
                bool result = this._isNullable.HasValue ? this._isNullable.Value : true;
                return result;
            }
            set {
                _isNullable = value;
            }
        }

        internal int Offset {
            get {
                return 0;
            }
        }

        [
        EntityResCategoryAttribute(EntityRes.DataCategory_Data),
        EntityResDescriptionAttribute(EntityRes.DbParameter_Size),
        ]
        override public int Size { 
            get {
                int size = _size.HasValue ? _size.Value : 0;
                if (0 == size) {
                    size = ValueSize(Value);
                }
                return size;
            }
            set {
                if (!_size.HasValue || _size.Value != value) {
                    if (value < -1) {
                        throw EntityUtil.InvalidSizeValue(value);
                    }
                    PropertyChanging();
                    if (0 == value) {
                        _size = null;
                    }
                    else {
                        _size = value;
                    }
                }
            }
        }

        private void ResetSize() {
            if (_size.HasValue) {
                PropertyChanging();
                _size = null;
            }
        }

        private bool ShouldSerializeSize() { 
            return (_size.HasValue && _size.Value != 0);
        }

        [
        EntityResCategoryAttribute(EntityRes.DataCategory_Update),
        EntityResDescriptionAttribute(EntityRes.DbParameter_SourceColumn),
        ]
        override public string SourceColumn { 
            get {
                string sourceColumn = _sourceColumn;
                return ((null != sourceColumn) ? sourceColumn : string.Empty);
            }
            set {
                _sourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping {
            get {
                return _sourceColumnNullMapping;
            }
            set {
                _sourceColumnNullMapping = value;
            }
        }

        [
        EntityResCategoryAttribute(EntityRes.DataCategory_Update),
        EntityResDescriptionAttribute(EntityRes.DbParameter_SourceVersion),
        ]
        override public DataRowVersion SourceVersion { 
            get {
                DataRowVersion sourceVersion = _sourceVersion;
                return ((0 != sourceVersion) ? sourceVersion : DataRowVersion.Current);
            }
            set {
                switch(value) { 
                case DataRowVersion.Original:
                case DataRowVersion.Current:
                case DataRowVersion.Proposed:
                case DataRowVersion.Default:
                    _sourceVersion = value;
                    break;
                default:
                    throw EntityUtil.InvalidDataRowVersion(value);
                }
            }
        }

        private void CloneHelperCore(EntityParameter destination) {
            destination._value                     = _value;
            
            destination._direction                 = _direction;
            destination._size                      = _size;

            destination._sourceColumn              = _sourceColumn;
            destination._sourceVersion             = _sourceVersion;
            destination._sourceColumnNullMapping   = _sourceColumnNullMapping;
            destination._isNullable                = _isNullable;
        }
        
        internal void CopyTo(DbParameter destination) {
            EntityUtil.CheckArgumentNull(destination, "destination");
            CloneHelper((EntityParameter)destination);
        }

        internal object CompareExchangeParent(object value, object comparand) {
            
            
            
            
            object parent = _parent;
            if (comparand == parent) {
                _parent = value;
            }
            return parent;
        }

        internal void ResetParent() {
            _parent = null;
        }

        override public string ToString() { 
            return ParameterName;
        }

        private byte ValuePrecisionCore(object value) { 
            if (value is Decimal) {
                return ((System.Data.SqlTypes.SqlDecimal)(Decimal) value).Precision; 
            }
            return 0;
        }

        private  byte ValueScaleCore(object value) { 
            if (value is Decimal) {
                return (byte)((Decimal.GetBits((Decimal)value)[3] & 0x00ff0000) >> 0x10);
            }
            return 0;
        }

        private  int ValueSizeCore(object value) { 
            if (!EntityUtil.IsNull(value)) {
                string svalue = (value as string);
                if (null != svalue) {
                    return svalue.Length;
                }
                byte[] bvalue = (value as byte[]);
                if (null != bvalue) {
                    return bvalue.Length;
                }
                char[] cvalue = (value as char[]);
                if (null != cvalue) {
                    return cvalue.Length;
                }
                if ((value is byte) || (value is char)) {
                    return 1;
                }
            }
            return 0;
        }
    }
}
