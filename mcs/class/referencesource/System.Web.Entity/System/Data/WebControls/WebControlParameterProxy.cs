//---------------------------------------------------------------------
// <copyright file="WebControlParameterProxy.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner objsdev
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Data;
using System.Globalization;

namespace System.Web.UI.WebControls
{
    internal class WebControlParameterProxy
    {
        ParameterCollection _collection;
        EntityDataSource _entityDataSource;
        Parameter _parameter; // Can be null, that's why this class doesn't subclass Parameter

        internal WebControlParameterProxy(string propertyName, ParameterCollection parameterCollection, EntityDataSource entityDataSource)
        {
            Debug.Assert(null != entityDataSource);
            Debug.Assert(!String.IsNullOrEmpty(propertyName));

            _parameter = EntityDataSourceUtil.GetParameter(propertyName, parameterCollection);
            _collection = parameterCollection;
            _entityDataSource = entityDataSource;
            VerifyUniqueType(_parameter);
        }
        internal WebControlParameterProxy(Parameter parameter, ParameterCollection parameterCollection, EntityDataSource entityDataSource)
        {
            Debug.Assert(null != entityDataSource);
            _parameter = parameter;
            _collection = parameterCollection;
            _entityDataSource = entityDataSource;
            VerifyUniqueType(_parameter);
        }
        internal string Name
        {
            get 
            {
                if (null != _parameter)
                {
                    return _parameter.Name;
                }
                return null;
            }
        }
        internal bool HasValue
        {
            get 
            { 
                return null != _parameter && 
                    null != Value; 
            }
        }
        internal bool ConvertEmptyStringToNull
        {
            get
            {
                if (null != _parameter)
                {
                    return _parameter.ConvertEmptyStringToNull;
                }
                return false;
            }
        }
        internal TypeCode TypeCode
        {
            get
            {
                if (null != _parameter)
                {
                    return _parameter.Type;
                }
                return TypeCode.Empty;
            }
        }
        internal DbType DbType
        {
            get
            {
                if (null != _parameter)
                {
                    return _parameter.DbType;
                }
                return DbType.Object;
            }
        }
        internal Type ClrType
        {
            get
            {
                Debug.Assert(this.TypeCode != TypeCode.Empty || this.DbType != DbType.Object, "Need to have TypeCode or DbType to get a ClrType");
                if (this.TypeCode != TypeCode.Empty)
                {
                    return EntityDataSourceUtil.ConvertTypeCodeToType(this.TypeCode);
                }
                return EntityDataSourceUtil.ConvertDbTypeToType(this.DbType);
            }
        }

        internal object Value
        {
            get 
            {
                if (_parameter != null)
                {
                    object paramValue = EntityDataSourceUtil.GetParameterValue(_parameter.Name, _collection, _entityDataSource);

                    if (paramValue != null)
                    {
                        if (this.DbType == DbType.DateTimeOffset)
                        {
                            object value = (paramValue is DateTimeOffset)
                                ? paramValue
                                : DateTimeOffset.Parse(this.Value.ToString(), CultureInfo.CurrentCulture);
                            return value;
                        }
                        else if (this.DbType == DbType.Time)
                        {
                            object value = (paramValue is TimeSpan)
                                ? paramValue
                                : TimeSpan.Parse(paramValue.ToString(), CultureInfo.CurrentCulture);
                            return value;
                        }
                        else if (this.DbType == DbType.Guid)
                        {
                            object value = (paramValue is Guid)
                                ? paramValue
                                : new Guid(paramValue.ToString());
                            return value;
                        }
                    }

                    return paramValue;
                }

                return null;
            }
        }

        private static void VerifyUniqueType(Parameter parameter)
        {
            if (parameter != null && parameter.Type == TypeCode.Empty && parameter.DbType == DbType.Object)
            {
                throw new InvalidOperationException(Strings.WebControlParameterProxy_TypeDbTypeMutuallyExclusive);
            }

            if (parameter != null && parameter.DbType != DbType.Object && parameter.Type != TypeCode.Empty)
            {
                throw new InvalidOperationException(Strings.WebControlParameterProxy_TypeDbTypeMutuallyExclusive);
            }
        }

    }

}
