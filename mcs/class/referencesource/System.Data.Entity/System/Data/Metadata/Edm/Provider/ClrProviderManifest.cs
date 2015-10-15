//---------------------------------------------------------------------
// <copyright file="ClrProviderManifest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Data.Spatial;
using System.Threading;
using System.Linq;

namespace System.Data.Metadata.Edm
{
    internal class ClrProviderManifest : DbProviderManifest
    {
        private const int s_PrimitiveTypeCount = 17;
        private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> _primitiveTypes;
        private static ClrProviderManifest _instance = new ClrProviderManifest();

        /// <summary>
        /// A private constructor to prevent other places from instantiating this class
        /// </summary>
        private ClrProviderManifest()
        {
        }

        /// <summary>
        /// Gets the EDM provider manifest singleton instance
        /// </summary>
        internal static ClrProviderManifest Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Returns the namespace used by this provider manifest
        /// </summary>
        public override string NamespaceName
        {
            get { return EdmConstants.ClrPrimitiveTypeNamespace; }
        }
                
        /// <summary>
        /// Returns the primitive type corresponding to the given CLR type
        /// </summary>
        /// <param name="clrType">The CLR type for which the PrimitiveType object is retrieved</param>
        /// <param name="primitiveType">The retrieved primitive type</param>
        /// <returns>True if a primitive type is returned</returns>
        internal bool TryGetPrimitiveType(Type clrType, out PrimitiveType primitiveType)
        {
            primitiveType = null;
            PrimitiveTypeKind resolvedTypeKind;
            if (TryGetPrimitiveTypeKind(clrType, out resolvedTypeKind))
            {
                InitializePrimitiveTypes();
                primitiveType = _primitiveTypes[(int)resolvedTypeKind];
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Returns the <see cref="PrimitiveTypeKind"/> corresponding to the given CLR type
        /// </summary>
        /// <param name="clrType">The CLR type for which the PrimitiveTypeKind value should be resolved</param>
        /// <param name="primitiveType">The PrimitiveTypeKind value to which the CLR type resolves, if any.</param>
        /// <returns>True if the CLR type represents a primitive (EDM) type; otherwise false.</returns>
        internal bool TryGetPrimitiveTypeKind(Type clrType, out PrimitiveTypeKind resolvedPrimitiveTypeKind)
        {
            PrimitiveTypeKind? primitiveTypeKind = null;
            if (!clrType.IsEnum) // Enums return the TypeCode of their underlying type
            {
                // As an optimization, short-circuit when the provided type has a known type code.
                switch (Type.GetTypeCode(clrType))
                {
                    // PrimitiveTypeKind.Binary = byte[] = TypeCode.Object
                    case TypeCode.Boolean:
                        primitiveTypeKind = PrimitiveTypeKind.Boolean;
                        break;
                    case TypeCode.Byte:
                        primitiveTypeKind = PrimitiveTypeKind.Byte;
                        break;
                    case TypeCode.DateTime:
                        primitiveTypeKind = PrimitiveTypeKind.DateTime;
                        break;
                    // PrimitiveTypeKind.DateTimeOffset = System.DateTimeOffset = TypeCode.Object
                    case TypeCode.Decimal:
                        primitiveTypeKind = PrimitiveTypeKind.Decimal;
                        break;
                    case TypeCode.Double:
                        primitiveTypeKind = PrimitiveTypeKind.Double;
                        break;
                    // PrimitiveTypeKind.Geography = System.Data.Spatial.DbGeometry (or subtype) = TypeCode.Object
                    // PrimitiveTypeKind.Geometry = System.Data.Spatial.DbGeometry (or subtype) = TypeCode.Object
                    // PrimitiveTypeKind.Guid = System.Guid = TypeCode.Object
                    case TypeCode.Int16:
                        primitiveTypeKind = PrimitiveTypeKind.Int16;
                        break;
                    case TypeCode.Int32:
                        primitiveTypeKind = PrimitiveTypeKind.Int32;
                        break;
                    case TypeCode.Int64:
                        primitiveTypeKind = PrimitiveTypeKind.Int64;
                        break;
                    case TypeCode.SByte:
                        primitiveTypeKind = PrimitiveTypeKind.SByte;
                        break;
                    case TypeCode.Single:
                        primitiveTypeKind = PrimitiveTypeKind.Single;
                        break;
                    case TypeCode.String:
                        primitiveTypeKind = PrimitiveTypeKind.String;
                        break;
                    // PrimitiveTypeKind.Time = System.TimeSpan = TypeCode.Object
                    case TypeCode.Object:
                        {
                            if (typeof(byte[]) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Binary;
                            }
                            else if (typeof(DateTimeOffset) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.DateTimeOffset;
                            }
                            // DbGeography/Geometry are abstract so subtypes must be allowed
                            else if (typeof(System.Data.Spatial.DbGeography).IsAssignableFrom(clrType))
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Geography;
                            }
                            else if (typeof(System.Data.Spatial.DbGeometry).IsAssignableFrom(clrType))
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Geometry;
                            }
                            else if (typeof(Guid) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Guid;
                            }
                            else if (typeof(TimeSpan) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Time;
                            }
                            break;
                        }
                }
            }

            if (primitiveTypeKind.HasValue)
            {
                resolvedPrimitiveTypeKind = primitiveTypeKind.Value;
                return true;
            }
            else
            {
                resolvedPrimitiveTypeKind = default(PrimitiveTypeKind);
                return false;
            }
        }

        /// <summary>
        /// Returns all the functions in this provider manifest
        /// </summary>
        /// <returns>A collection of functions</returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            return Helper.EmptyEdmFunctionReadOnlyCollection;
        }

        /// <summary>
        /// Returns all the FacetDescriptions for a particular type
        /// </summary>
        /// <param name="type">the type to return FacetDescriptions for.</param>
        /// <returns>The FacetDescriptions for the type given.</returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType type)
        {
            if (Helper.IsPrimitiveType(type) && ((PrimitiveType)type).DataSpace == DataSpace.OSpace)
            {
                // we don't have our own facets, just defer to the edm primitive type facets
                PrimitiveType basePrimitive = (PrimitiveType)type.BaseType;
                return basePrimitive.ProviderManifest.GetFacetDescriptions(basePrimitive);
            }

            return Helper.EmptyFacetDescriptionEnumerable;
        }

        /// <summary>
        /// Initializes all the primitive types
        /// </summary>
        private void InitializePrimitiveTypes()
        {
            if (_primitiveTypes != null)
            {
                return;
            }

            PrimitiveType[] primitiveTypes = new PrimitiveType[s_PrimitiveTypeCount];
            primitiveTypes[(int)PrimitiveTypeKind.Binary] = CreatePrimitiveType(typeof(Byte[]), PrimitiveTypeKind.Binary);
            primitiveTypes[(int)PrimitiveTypeKind.Boolean] = CreatePrimitiveType(typeof(Boolean), PrimitiveTypeKind.Boolean);
            primitiveTypes[(int)PrimitiveTypeKind.Byte] = CreatePrimitiveType(typeof(Byte), PrimitiveTypeKind.Byte);
            primitiveTypes[(int)PrimitiveTypeKind.DateTime] = CreatePrimitiveType(typeof(DateTime), PrimitiveTypeKind.DateTime);
            primitiveTypes[(int)PrimitiveTypeKind.Time] = CreatePrimitiveType(typeof(TimeSpan), PrimitiveTypeKind.Time);
            primitiveTypes[(int)PrimitiveTypeKind.DateTimeOffset] = CreatePrimitiveType(typeof(DateTimeOffset), PrimitiveTypeKind.DateTimeOffset);
            primitiveTypes[(int)PrimitiveTypeKind.Decimal] = CreatePrimitiveType(typeof(Decimal), PrimitiveTypeKind.Decimal);
            primitiveTypes[(int)PrimitiveTypeKind.Double] = CreatePrimitiveType(typeof(Double), PrimitiveTypeKind.Double);
            primitiveTypes[(int)PrimitiveTypeKind.Geography] = CreatePrimitiveType(typeof(DbGeography), PrimitiveTypeKind.Geography);
            primitiveTypes[(int)PrimitiveTypeKind.Geometry] = CreatePrimitiveType(typeof(DbGeometry), PrimitiveTypeKind.Geometry);
            primitiveTypes[(int)PrimitiveTypeKind.Guid] = CreatePrimitiveType(typeof(Guid), PrimitiveTypeKind.Guid);
            primitiveTypes[(int)PrimitiveTypeKind.Int16] = CreatePrimitiveType(typeof(Int16), PrimitiveTypeKind.Int16);
            primitiveTypes[(int)PrimitiveTypeKind.Int32] = CreatePrimitiveType(typeof(Int32), PrimitiveTypeKind.Int32);
            primitiveTypes[(int)PrimitiveTypeKind.Int64] = CreatePrimitiveType(typeof(Int64), PrimitiveTypeKind.Int64);
            primitiveTypes[(int)PrimitiveTypeKind.SByte] = CreatePrimitiveType(typeof(SByte), PrimitiveTypeKind.SByte);
            primitiveTypes[(int)PrimitiveTypeKind.Single] = CreatePrimitiveType(typeof(Single), PrimitiveTypeKind.Single);
            primitiveTypes[(int)PrimitiveTypeKind.String] = CreatePrimitiveType(typeof(String), PrimitiveTypeKind.String);

            System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> readOnlyTypes = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(primitiveTypes);

            // Set the result to _primitiveTypes at the end
            Interlocked.CompareExchange<System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>>(ref _primitiveTypes, readOnlyTypes, null);
        }

        /// <summary>
        /// Initialize the primitive type with the given 
        /// </summary>
        /// <param name="clrType">The CLR type of this type</param>
        /// <param name="primitiveTypeKind">The primitive type kind of the primitive type</param>
        private PrimitiveType CreatePrimitiveType(Type clrType, PrimitiveTypeKind primitiveTypeKind)
        {
            // Figures out the base type
            PrimitiveType baseType = MetadataItem.EdmProviderManifest.GetPrimitiveType(primitiveTypeKind);
            PrimitiveType primitiveType = new PrimitiveType(clrType, baseType, this);
            primitiveType.SetReadOnly();
            return primitiveType;
        }


        public override System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            InitializePrimitiveTypes();
            return this._primitiveTypes;
        }

        public override TypeUsage GetEdmType(TypeUsage storeType)
        {
            throw new NotImplementedException();
        }

        public override TypeUsage GetStoreType(TypeUsage edmType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Providers should override this to return information specific to their provider.  
        /// 
        /// This method should never return null.
        /// </summary>
        /// <param name="informationType">The name of the information to be retrieved.</param>
        /// <returns>An XmlReader at the begining of the information requested.</returns>
        protected override System.Xml.XmlReader GetDbInformation(string informationType)
        {
            throw new NotImplementedException();
        }
    }
}
