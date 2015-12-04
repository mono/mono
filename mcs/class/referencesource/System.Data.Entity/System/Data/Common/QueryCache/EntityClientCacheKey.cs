//------------------------------------------------------------------------------
// <copyright file="EntityClientCacheKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common.Internal;
    using System.Data.EntityClient;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
        
    /// <summary>
    /// Represents EntityCommand Cache key context
    /// </summary>
    internal sealed class EntityClientCacheKey : QueryCacheKey
    {
        /// <summary>
        /// Stored procedure or command text?
        /// </summary>
        readonly CommandType _commandType;

        /// <summary>
        /// Entity Sql statement
        /// </summary>
        readonly string _eSqlStatement;

        /// <summary>
        /// parameter collection token
        /// </summary>
        readonly string _parametersToken;

        /// <summary>
        /// number of parameters
        /// </summary>
        readonly int _parameterCount;

        /// <summary>
        /// Combined Hashcode based on field hashcodes
        /// </summary>
        readonly int _hashCode;

        /// <summary>
        /// Creates a new instance of EntityClientCacheKey given a entityCommand instance
        /// </summary>
        /// <param name="entityCommand"></param>
        internal EntityClientCacheKey(EntityCommand entityCommand)
            : base()
        {
            // Command Type
            _commandType = entityCommand.CommandType;

            // Statement
            _eSqlStatement = entityCommand.CommandText;
            
            // Parameters
            _parametersToken = GetParametersToken(entityCommand);
            _parameterCount = entityCommand.Parameters.Count;

            // Hashcode
            _hashCode = _commandType.GetHashCode() ^
                        _eSqlStatement.GetHashCode() ^
                        _parametersToken.GetHashCode();
        }

        /// <summary>
        /// determines equality of two cache keys based on cache context values
        /// </summary>
        /// <param name="otherObject"></param>
        /// <returns></returns>
        public override bool Equals( object otherObject )
        {
            Debug.Assert(null != otherObject, "otherObject must not be null");
            if (typeof(EntityClientCacheKey) != otherObject.GetType())
            {
                return false;
            }

            EntityClientCacheKey otherEntityClientCacheKey = (EntityClientCacheKey)otherObject;

            return (_commandType == otherEntityClientCacheKey._commandType &&
                    _parameterCount == otherEntityClientCacheKey._parameterCount) &&
                    Equals(otherEntityClientCacheKey._eSqlStatement, _eSqlStatement) &&
                    Equals(otherEntityClientCacheKey._parametersToken, _parametersToken);
        }

        /// <summary>
        /// Returns Context Hash Code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        private static string GetTypeUsageToken(TypeUsage type)
        {
            string result = null;

            // Dev10#537010: EntityCommand false positive cache hits caused by insufficient parameter type information in cache key
            // Ensure String types are correctly differentiated.
            if (object.ReferenceEquals(type, DbTypeMap.AnsiString))
            {
                result = "AnsiString";
            }
            else if (object.ReferenceEquals(type, DbTypeMap.AnsiStringFixedLength))
            {
                result = "AnsiStringFixedLength";
            }
            else if (object.ReferenceEquals(type, DbTypeMap.String))
            {
                result = "String";
            }
            else if (object.ReferenceEquals(type, DbTypeMap.StringFixedLength))
            {
                result = "StringFixedLength";
            }
            else if (object.ReferenceEquals(type, DbTypeMap.Xml))
            {
                // Xml is currently mapped to (unicode, variable-length) string, so the TypeUsage
                // given to the provider is actually a String TypeUsage.
                Debug.Assert(TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String), "Update GetTypeUsageToken to return 'Xml' for Xml parameters");
                result = "String";
            }
            else if (TypeSemantics.IsEnumerationType(type))
            {
                result = type.EdmType.FullName;
            }
            else
            {
                // String/Xml TypeUsages are the only DbType-derived TypeUsages that carry meaningful facets.
                // Otherwise, the primitive type name is a sufficient token (note that full name is not required
                // since model types always have the 'Edm' namespace).
                Debug.Assert(TypeSemantics.IsPrimitiveType(type), "EntityParameter TypeUsage not a primitive type?");
                Debug.Assert(!TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String), "String TypeUsage not derived from DbType.AnsiString, AnsiString, String, StringFixedLength or Xml?");
                result = type.EdmType.Name;
            }

            return result;
        }

        /// <summary>
        /// Returns a string representation of the parameter list
        /// </summary>
        /// <param name="entityCommand"></param>
        /// <returns></returns>
        private static string GetParametersToken(EntityCommand entityCommand)
        {
            if (null == entityCommand.Parameters || 0 == entityCommand.Parameters.Count)
            {
                //
                // means no parameters
                //
                return "@@0";
            }
            
            // Ensure that parameter DbTypes are valid and there are no duplicate names
            Dictionary<string, TypeUsage> paramTypeUsage = entityCommand.GetParameterTypeUsage();
            Debug.Assert(paramTypeUsage.Count == entityCommand.Parameters.Count, "entityParameter collection and query parameter collection must have the same number of entries");
            if (1 == paramTypeUsage.Count)
            {
                // if its one parameter only, there is no need to use stringbuilder
                return "@@1:" +
                    entityCommand.Parameters[0].ParameterName + ":" +
                    GetTypeUsageToken(paramTypeUsage[entityCommand.Parameters[0].ParameterName]);
            }
            else
            {
                StringBuilder sb = new StringBuilder(entityCommand.Parameters.Count * EstimatedParameterStringSize);
                Debug.Assert(paramTypeUsage.Count == entityCommand.Parameters.Count, "entityParameter collection and query parameter collection must have the same number of entries");
                sb.Append("@@");
                sb.Append(entityCommand.Parameters.Count);
                sb.Append(":");
                string separator = "";
                foreach (KeyValuePair<string, TypeUsage> param in paramTypeUsage)
                {
                    sb.Append(separator);
                    sb.Append(param.Key);
                    sb.Append(":");
                    sb.Append(GetTypeUsageToken(param.Value));
                    separator = ";";
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// returns the composed cache key
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Join("|", new string[] { Enum.GetName(typeof(CommandType), _commandType), _eSqlStatement, _parametersToken });
        }

    }
}
