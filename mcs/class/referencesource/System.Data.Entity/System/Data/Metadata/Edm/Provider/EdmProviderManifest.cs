//---------------------------------------------------------------------
// <copyright file="EdmProviderManifest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Spatial;
using System.Diagnostics;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    internal class EdmProviderManifest : DbProviderManifest
    {
        /// <summary>
        /// The ConcurrencyMode Facet Name
        /// </summary>
        internal const string ConcurrencyModeFacetName = "ConcurrencyMode";
        /// <summary>
        /// The StoreGeneratedPattern Facet Name
        /// </summary>
        internal const string StoreGeneratedPatternFacetName = "StoreGeneratedPattern";
        private Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription>> _facetDescriptions;
        private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> _primitiveTypes;
        private System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> _functions;
        private static EdmProviderManifest _instance = new EdmProviderManifest();
        private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>[] _promotionTypes;
        static TypeUsage[] _canonicalModelTypes;

        internal const byte MaximumDecimalPrecision = Byte.MaxValue;
        internal const byte MaximumDateTimePrecision = Byte.MaxValue;

        /// <summary>
        /// A private constructor to prevent other places from instantiating this class
        /// </summary>
        private EdmProviderManifest()
        {
        }

        /// <summary>
        /// Gets the EDM provider manifest singleton instance
        /// </summary>
        internal static EdmProviderManifest Instance
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
            get { return EdmConstants.EdmNamespace; }
        }

        /// <summary>
        /// Store version hint
        /// </summary>
        internal string Token
        {
            // we shouldn't throw exception on properties
            get { return String.Empty; }
        }

        /// <summary>
        /// Returns the list of all the canonical functions
        /// </summary>
        /// <returns></returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            InitializeCanonicalFunctions();
            return _functions;
        }

        /// <summary>
        /// Returns all the FacetDescriptions for a particular type
        /// </summary>
        /// <param name="type">the type to return FacetDescriptions for.</param>
        /// <returns>The FacetDescriptions for the type given.</returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType type)
        {
            Debug.Assert(type is PrimitiveType, "EdmProviderManifest.GetFacetDescriptions(): Argument is not a PrimitiveType");

            InitializeFacetDescriptions();

            // Some types may not have facets, so just try to get them, if there aren't any, just return an empty list
            System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription> collection = null;
            if (_facetDescriptions.TryGetValue(type as PrimitiveType, out collection))
            {
                return collection;
            }
            return Helper.EmptyFacetDescriptionEnumerable;
        }

        /// <summary>
        /// Returns a primitive type from this manifest having the specified primitive type kind
        /// </summary>
        /// <param name="primitiveTypeKind">The value specifying the kind of primitive type to return</param>
        /// <returns>A primitive type having the given primitive type kind</returns>
        public PrimitiveType GetPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            InitializePrimitiveTypes();
            return _primitiveTypes[(int)primitiveTypeKind];
        }

        /// <summary>
        /// Boostrapping all the primitive types for the EDM Provider Manifest
        /// </summary>
        private void InitializePrimitiveTypes()
        {
            if (_primitiveTypes != null)
            {
                return;
            }

            PrimitiveType[] primitiveTypes = new PrimitiveType[EdmConstants.NumPrimitiveTypes];
            primitiveTypes[(int)PrimitiveTypeKind.Binary] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Boolean] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Byte] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.DateTime] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Decimal] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Double] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Single] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Guid] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Int16] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Int32] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Int64] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.SByte] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.String] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Time] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.DateTimeOffset] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Geometry] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryPoint] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryLineString] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryPolygon] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiPoint] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiLineString] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiPolygon] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeometryCollection] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.Geography] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyPoint] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyLineString] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyPolygon] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiPoint] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiLineString] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiPolygon] = new PrimitiveType();
            primitiveTypes[(int)PrimitiveTypeKind.GeographyCollection] = new PrimitiveType();

            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Binary], PrimitiveTypeKind.Binary, EdmConstants.Binary, typeof(Byte[]));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Boolean], PrimitiveTypeKind.Boolean, EdmConstants.Boolean, typeof(Boolean));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Byte], PrimitiveTypeKind.Byte, EdmConstants.Byte, typeof(Byte));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.DateTime], PrimitiveTypeKind.DateTime, EdmConstants.DateTime, typeof(DateTime));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Decimal], PrimitiveTypeKind.Decimal, EdmConstants.Decimal, typeof(Decimal));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Double], PrimitiveTypeKind.Double, EdmConstants.Double, typeof(Double));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Single], PrimitiveTypeKind.Single, EdmConstants.Single, typeof(Single));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Guid], PrimitiveTypeKind.Guid, EdmConstants.Guid, typeof(Guid));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Int16], PrimitiveTypeKind.Int16, EdmConstants.Int16, typeof(Int16));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Int32], PrimitiveTypeKind.Int32, EdmConstants.Int32, typeof(Int32));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Int64], PrimitiveTypeKind.Int64, EdmConstants.Int64, typeof(Int64));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.SByte], PrimitiveTypeKind.SByte, EdmConstants.SByte, typeof(SByte));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.String], PrimitiveTypeKind.String, EdmConstants.String, typeof(String));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Time], PrimitiveTypeKind.Time, EdmConstants.Time, typeof(TimeSpan));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.DateTimeOffset], PrimitiveTypeKind.DateTimeOffset, EdmConstants.DateTimeOffset, typeof(DateTimeOffset));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Geography], PrimitiveTypeKind.Geography, EdmConstants.Geography, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyPoint], PrimitiveTypeKind.GeographyPoint, EdmConstants.GeographyPoint, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyLineString], PrimitiveTypeKind.GeographyLineString, EdmConstants.GeographyLineString, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyPolygon], PrimitiveTypeKind.GeographyPolygon, EdmConstants.GeographyPolygon, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiPoint], PrimitiveTypeKind.GeographyMultiPoint, EdmConstants.GeographyMultiPoint, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiLineString], PrimitiveTypeKind.GeographyMultiLineString, EdmConstants.GeographyMultiLineString, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiPolygon], PrimitiveTypeKind.GeographyMultiPolygon, EdmConstants.GeographyMultiPolygon, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeographyCollection], PrimitiveTypeKind.GeographyCollection, EdmConstants.GeographyCollection, typeof(DbGeography));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.Geometry], PrimitiveTypeKind.Geometry, EdmConstants.Geometry, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryPoint], PrimitiveTypeKind.GeometryPoint, EdmConstants.GeometryPoint, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryLineString], PrimitiveTypeKind.GeometryLineString, EdmConstants.GeometryLineString, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryPolygon], PrimitiveTypeKind.GeometryPolygon, EdmConstants.GeometryPolygon, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiPoint], PrimitiveTypeKind.GeometryMultiPoint, EdmConstants.GeometryMultiPoint, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiLineString], PrimitiveTypeKind.GeometryMultiLineString, EdmConstants.GeometryMultiLineString, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiPolygon], PrimitiveTypeKind.GeometryMultiPolygon, EdmConstants.GeometryMultiPolygon, typeof(DbGeometry));
            InitializePrimitiveType(primitiveTypes[(int)PrimitiveTypeKind.GeometryCollection], PrimitiveTypeKind.GeometryCollection, EdmConstants.GeometryCollection, typeof(DbGeometry));
           
            // Set all primitive types to be readonly
            foreach (PrimitiveType primitiveType in primitiveTypes)
            {
                primitiveType.ProviderManifest = this;
                primitiveType.SetReadOnly();
            }

            System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> readOnlyTypes = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(primitiveTypes);

            // Set the result to _primitiveTypes at the end
            Interlocked.CompareExchange<System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>>(ref _primitiveTypes, readOnlyTypes, null);
        }

        /// <summary>
        /// Initialize all the primitive type with the given primitive type kind and name
        /// </summary>
        /// <param name="primitiveType">The primitive type to initialize</param>
        /// <param name="primitiveTypeKind">Type of the primitive type which is getting initialized</param>
        /// <param name="name">name of the built in type</param>
        /// <param name="clrType">the CLR Type of that maps to the EDM PrimitiveType</param>
        private void InitializePrimitiveType(PrimitiveType primitiveType,
                                             PrimitiveTypeKind primitiveTypeKind,
                                             string name,
                                             Type clrType)
        {
            // Only null types are not abstract and they are sealed, all others are abstract and unsealed
            EdmType.Initialize(primitiveType, name,
                               EdmConstants.EdmNamespace,
                               DataSpace.CSpace,
                               true /* isabstract */,
                               null /* baseType */);
            PrimitiveType.Initialize(primitiveType,
                                     primitiveTypeKind,
                                     true, // isDefault
                                     this);
            Debug.Assert(clrType == primitiveType.ClrEquivalentType, "ClrEquivalentType mismatch");
        }

        /// <summary>
        /// Boostrapping all the facet descriptions for the EDM Provider Manifest
        /// </summary>
        private void InitializeFacetDescriptions()
        {
            if (_facetDescriptions != null)
            {
                return;
            }

            // Ensure the primitive types are there
            InitializePrimitiveTypes();

            // Create the dictionary of facet descriptions
            Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription>> facetDescriptions = new Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription>>();

            // String facets
            FacetDescription[] list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.String);
            PrimitiveType applicableType = _primitiveTypes[(int)PrimitiveTypeKind.String];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            // Binary facets
            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.Binary);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.Binary];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            // DateTime facets
            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.DateTime);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.DateTime];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            // Time facets
            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.Time);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.Time];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            // DateTimeOffset facets
            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.DateTimeOffset);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.DateTimeOffset];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));


            // Decimal facets
            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.Decimal);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.Decimal];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            // Spatial facets
            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.Geography);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.Geography];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyPoint);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyPoint];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyLineString);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyLineString];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyPolygon);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyPolygon];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyMultiPoint);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiPoint];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyMultiLineString);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiLineString];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyMultiPolygon);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyMultiPolygon];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeographyCollection);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeographyCollection];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.Geometry);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.Geometry];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryPoint);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryPoint];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryLineString);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryLineString];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryPolygon);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryPolygon];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryMultiPoint);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiPoint];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryMultiLineString);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiLineString];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryMultiPolygon);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryMultiPolygon];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            list = EdmProviderManifest.GetInitialFacetDescriptions(PrimitiveTypeKind.GeometryCollection);
            applicableType = _primitiveTypes[(int)PrimitiveTypeKind.GeometryCollection];
            facetDescriptions.Add(applicableType, Array.AsReadOnly(list));

            // Set the result to _facetDescriptions at the end
            Interlocked.CompareExchange<Dictionary<PrimitiveType, System.Collections.ObjectModel.ReadOnlyCollection<FacetDescription>>>(ref _facetDescriptions,
                                                                                                         facetDescriptions,
                                                                                                         null);
        }

        internal static FacetDescription[] GetInitialFacetDescriptions(PrimitiveTypeKind primitiveTypeKind)
        {
            FacetDescription[] list;

            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    {
                        list = new FacetDescription[3];

                        list[0] = (new FacetDescription(DbProviderManifest.MaxLengthFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32),
                                                      0,
                                                      Int32.MaxValue, 
                                                      null));
                        list[1] = (new FacetDescription(DbProviderManifest.UnicodeFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean),
                                                      null,
                                                      null,
                                                      null));
                        list[2] = (new FacetDescription(DbProviderManifest.FixedLengthFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean),
                                                      null,
                                                      null,
                                                      null));

                        return list;
                    }

                case PrimitiveTypeKind.Binary:
                    {
                        list = new FacetDescription[2];

                        list[0] = (new FacetDescription(DbProviderManifest.MaxLengthFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32),
                                                      0,
                                                      Int32.MaxValue, 
                                                      null));
                        list[1] = (new FacetDescription(DbProviderManifest.FixedLengthFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean),
                                                      null,
                                                      null,
                                                      null));
                        return list;
                    }

                case PrimitiveTypeKind.DateTime:
                    {
                        list = new FacetDescription[1];

                        list[0] = (new FacetDescription(DbProviderManifest.PrecisionFacetName,
                              MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte),
                              0, EdmProviderManifest.MaximumDateTimePrecision, null));

                        return list;
                    }
                case PrimitiveTypeKind.Time:
                    {
                        list = new FacetDescription[1];

                        list[0] = (new FacetDescription(DbProviderManifest.PrecisionFacetName,
                              MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte),
                              0, EdmProviderManifest.MaximumDateTimePrecision, TypeUsage.DefaultDateTimePrecisionFacetValue));

                        return list;
                    }
                case PrimitiveTypeKind.DateTimeOffset:
                    {
                        list = new FacetDescription[1];
                        list[0] = (new FacetDescription(DbProviderManifest.PrecisionFacetName,
                              MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte),
                              0, EdmProviderManifest.MaximumDateTimePrecision, TypeUsage.DefaultDateTimePrecisionFacetValue));

                        return list;
                    }
                case PrimitiveTypeKind.Decimal:
                    {
                        list = new FacetDescription[2];

                        list[0] = (new FacetDescription(DbProviderManifest.PrecisionFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte),
                                                      1,
                                                      EdmProviderManifest.MaximumDecimalPrecision,
                                                      null));
                        list[1] = (new FacetDescription(DbProviderManifest.ScaleFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Byte),
                                                      0,
                                                      EdmProviderManifest.MaximumDecimalPrecision,
                                                      null));
                        return list;
                    }
                case PrimitiveTypeKind.Geometry:
                case PrimitiveTypeKind.GeometryPoint:
                case PrimitiveTypeKind.GeometryLineString:
                case PrimitiveTypeKind.GeometryPolygon:
                case PrimitiveTypeKind.GeometryMultiPoint:
                case PrimitiveTypeKind.GeometryMultiLineString:
                case PrimitiveTypeKind.GeometryMultiPolygon:
                case PrimitiveTypeKind.GeometryCollection:
                    {
                        list = new FacetDescription[2];

                        list[0] = (new FacetDescription(DbProviderManifest.SridFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32),
                                                      0,
                                                      Int32.MaxValue,
                                                      DbGeometry.DefaultCoordinateSystemId));
                        list[1] = (new FacetDescription(DbProviderManifest.IsStrictFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean),
                                                      null,
                                                      null,
                                                      true));
                        return list;
                    }
                case PrimitiveTypeKind.Geography:
                case PrimitiveTypeKind.GeographyPoint:
                case PrimitiveTypeKind.GeographyLineString:
                case PrimitiveTypeKind.GeographyPolygon:
                case PrimitiveTypeKind.GeographyMultiPoint:
                case PrimitiveTypeKind.GeographyMultiLineString:
                case PrimitiveTypeKind.GeographyMultiPolygon:
                case PrimitiveTypeKind.GeographyCollection:
                    {
                        list = new FacetDescription[2];

                        list[0] = (new FacetDescription(DbProviderManifest.SridFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Int32),
                                                      0,
                                                      Int32.MaxValue,
                                                      DbGeography.DefaultCoordinateSystemId));
                        list[1] = (new FacetDescription(DbProviderManifest.IsStrictFacetName,
                                                      MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.Boolean),
                                                      null,
                                                      null,
                                                      true));
                        return list;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Boostrapping all the canonical functions for the EDM Provider Manifest
        /// </summary>
        private void InitializeCanonicalFunctions()
        {
            if (_functions != null)
            {
                return;
            }
                        
            // Ensure primitive types are available
            InitializePrimitiveTypes();

            EdmProviderManifestFunctionBuilder functions = new EdmProviderManifestFunctionBuilder(_primitiveTypes);
            PrimitiveTypeKind[] parameterTypes;
            
            #region Aggregate Functions

            // Max, Min
            parameterTypes = new[] { PrimitiveTypeKind.Byte,
                                     PrimitiveTypeKind.DateTime,
                                     PrimitiveTypeKind.Decimal,
                                     PrimitiveTypeKind.Double,
                                     PrimitiveTypeKind.Int16,
                                     PrimitiveTypeKind.Int32,
                                     PrimitiveTypeKind.Int64,
                                     PrimitiveTypeKind.SByte,
                                     PrimitiveTypeKind.Single,
                                     PrimitiveTypeKind.String,
                                     PrimitiveTypeKind.Binary,
                                     PrimitiveTypeKind.Time,
                                     PrimitiveTypeKind.DateTimeOffset };

            functions.ForTypes(parameterTypes, type => functions.AddAggregate("Max", type));
            functions.ForTypes(parameterTypes, type => functions.AddAggregate("Min", type));

            // Avg, Sum
            parameterTypes = new[] { PrimitiveTypeKind.Decimal,
                                     PrimitiveTypeKind.Double,
                                     PrimitiveTypeKind.Int32,
                                     PrimitiveTypeKind.Int64 };

            functions.ForTypes(parameterTypes, type => functions.AddAggregate("Avg", type));
            functions.ForTypes(parameterTypes, type => functions.AddAggregate("Sum", type));

            // STDEV, STDEVP, VAR, VARP
            parameterTypes = new[] { PrimitiveTypeKind.Decimal,
                                     PrimitiveTypeKind.Double,
                                     PrimitiveTypeKind.Int32,
                                     PrimitiveTypeKind.Int64};

            functions.ForTypes(parameterTypes, type => functions.AddAggregate(PrimitiveTypeKind.Double, "StDev", type));
            functions.ForTypes(parameterTypes, type => functions.AddAggregate(PrimitiveTypeKind.Double, "StDevP", type));
            functions.ForTypes(parameterTypes, type => functions.AddAggregate(PrimitiveTypeKind.Double, "Var", type));
            functions.ForTypes(parameterTypes, type => functions.AddAggregate(PrimitiveTypeKind.Double, "VarP", type));

            // Count and Big Count must be supported for all edm types, except the strong spatial types.
            functions.ForAllBasePrimitiveTypes(type => functions.AddAggregate(PrimitiveTypeKind.Int32, "Count", type));
            functions.ForAllBasePrimitiveTypes(type => functions.AddAggregate(PrimitiveTypeKind.Int64, "BigCount", type));
            
            #endregion

            #region String Functions

            functions.AddFunction(PrimitiveTypeKind.String, "Trim", PrimitiveTypeKind.String, "stringArgument");
            functions.AddFunction(PrimitiveTypeKind.String, "RTrim", PrimitiveTypeKind.String, "stringArgument");
            functions.AddFunction(PrimitiveTypeKind.String, "LTrim", PrimitiveTypeKind.String, "stringArgument");
            functions.AddFunction(PrimitiveTypeKind.String, "Concat", PrimitiveTypeKind.String, "string1", PrimitiveTypeKind.String, "string2");
            functions.AddFunction(PrimitiveTypeKind.Int32, "Length", PrimitiveTypeKind.String, "stringArgument");

            // Substring, Left, Right overloads 
            parameterTypes = new[] { PrimitiveTypeKind.Byte,
                                     PrimitiveTypeKind.Int16,
                                     PrimitiveTypeKind.Int32,
                                     PrimitiveTypeKind.Int64,
                                     PrimitiveTypeKind.SByte };

            functions.ForTypes(parameterTypes, type => functions.AddFunction(PrimitiveTypeKind.String, "Substring", PrimitiveTypeKind.String, "stringArgument", type, "start", type, "length"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(PrimitiveTypeKind.String, "Left", PrimitiveTypeKind.String, "stringArgument", type, "length"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(PrimitiveTypeKind.String, "Right", PrimitiveTypeKind.String, "stringArgument", type, "length"));
            
            functions.AddFunction(PrimitiveTypeKind.String,  "Replace", PrimitiveTypeKind.String, "stringArgument", PrimitiveTypeKind.String, "toReplace", PrimitiveTypeKind.String, "replacement");
            functions.AddFunction(PrimitiveTypeKind.Int32,   "IndexOf", PrimitiveTypeKind.String, "searchString", PrimitiveTypeKind.String, "stringToFind");
            functions.AddFunction(PrimitiveTypeKind.String,  "ToUpper", PrimitiveTypeKind.String, "stringArgument");
            functions.AddFunction(PrimitiveTypeKind.String,  "ToLower", PrimitiveTypeKind.String, "stringArgument");
            functions.AddFunction(PrimitiveTypeKind.String,  "Reverse", PrimitiveTypeKind.String, "stringArgument");
            functions.AddFunction(PrimitiveTypeKind.Boolean, "Contains", PrimitiveTypeKind.String, "searchedString", PrimitiveTypeKind.String, "searchedForString");
            functions.AddFunction(PrimitiveTypeKind.Boolean, "StartsWith", PrimitiveTypeKind.String, "stringArgument", PrimitiveTypeKind.String, "prefix");
            functions.AddFunction(PrimitiveTypeKind.Boolean, "EndsWith", PrimitiveTypeKind.String, "stringArgument", PrimitiveTypeKind.String, "suffix");
            
            #endregion

            #region DateTime Functions
            
            PrimitiveTypeKind[] dateTimeParameterTypes = { PrimitiveTypeKind.DateTimeOffset,
                                                           PrimitiveTypeKind.DateTime };
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Year", type, "dateValue"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Month", type, "dateValue"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Day", type, "dateValue"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DayOfYear", type, "dateValue"));

            PrimitiveTypeKind[] timeParameterTypes = { PrimitiveTypeKind.DateTimeOffset,
                                                       PrimitiveTypeKind.DateTime,
                                                       PrimitiveTypeKind.Time };
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Hour", type, "timeValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Minute", type, "timeValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Second", type, "timeValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "Millisecond", type, "timeValue"));

            functions.AddFunction(PrimitiveTypeKind.DateTime, "CurrentDateTime");
            functions.AddFunction(PrimitiveTypeKind.DateTimeOffset, "CurrentDateTimeOffset");
            functions.AddFunction(PrimitiveTypeKind.Int32, "GetTotalOffsetMinutes", PrimitiveTypeKind.DateTimeOffset, "dateTimeOffsetArgument");
            functions.AddFunction(PrimitiveTypeKind.DateTime, "CurrentUtcDateTime");
            
            //TruncateTime
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(type, "TruncateTime", type, "dateValue"));
            
            //DateTime constructor
            functions.AddFunction(PrimitiveTypeKind.DateTime, "CreateDateTime", PrimitiveTypeKind.Int32, "year",
                                                                                PrimitiveTypeKind.Int32, "month",
                                                                                PrimitiveTypeKind.Int32, "day",
                                                                                PrimitiveTypeKind.Int32, "hour",
                                                                                PrimitiveTypeKind.Int32, "minute",
                                                                                PrimitiveTypeKind.Double, "second");


            //DateTimeOffset constructor
            functions.AddFunction(PrimitiveTypeKind.DateTimeOffset, "CreateDateTimeOffset", PrimitiveTypeKind.Int32, "year",
                                                                                            PrimitiveTypeKind.Int32, "month",
                                                                                            PrimitiveTypeKind.Int32, "day",
                                                                                            PrimitiveTypeKind.Int32, "hour",
                                                                                            PrimitiveTypeKind.Int32, "minute",
                                                                                            PrimitiveTypeKind.Double, "second", 
                                                                                            PrimitiveTypeKind.Int32, "timeZoneOffset");

            //Time constructor
            functions.AddFunction(PrimitiveTypeKind.Time, "CreateTime", PrimitiveTypeKind.Int32, "hour", PrimitiveTypeKind.Int32, "minute", PrimitiveTypeKind.Double, "second");

            //Date and time addition functions
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(type, "AddYears", type, "dateValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(type, "AddMonths", type, "dateValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(type, "AddDays", type, "dateValue", PrimitiveTypeKind.Int32, "addValue"));

            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(type, "AddHours", type, "timeValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(type, "AddMinutes", type, "timeValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(type, "AddSeconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(type, "AddMilliseconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(type, "AddMicroseconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(type, "AddNanoseconds", type, "timeValue", PrimitiveTypeKind.Int32, "addValue"));
            
            // Date and time diff functions
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffYears", type, "dateValue1", type, "dateValue2"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMonths", type, "dateValue1", type, "dateValue2"));
            functions.ForTypes(dateTimeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffDays", type, "dateValue1", type, "dateValue2"));
            
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffHours", type, "timeValue1", type, "timeValue2"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMinutes", type, "timeValue1", type, "timeValue2"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffSeconds", type, "timeValue1", type, "timeValue2"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMilliseconds", type, "timeValue1", type, "timeValue2"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffMicroseconds", type, "timeValue1", type, "timeValue2"));
            functions.ForTypes(timeParameterTypes, type => functions.AddFunction(PrimitiveTypeKind.Int32, "DiffNanoseconds", type, "timeValue1", type, "timeValue2"));
                        
            #endregion // DateTime Functions
            
            #region Math Functions

            // Overloads for ROUND, FLOOR, CEILING functions
            parameterTypes = new[] { PrimitiveTypeKind.Single,
                                     PrimitiveTypeKind.Double,
                                     PrimitiveTypeKind.Decimal };
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "Round", type, "value"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "Floor", type, "value"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "Ceiling", type, "value"));

            // Overloads for ROUND, TRUNCATE
            parameterTypes = new [] { PrimitiveTypeKind.Double,
                                      PrimitiveTypeKind.Decimal };
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "Round", type, "value", PrimitiveTypeKind.Int32, "digits"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "Truncate", type, "value", PrimitiveTypeKind.Int32, "digits"));
            
            // Overloads for ABS functions
            parameterTypes = new[] { PrimitiveTypeKind.Decimal,
                                     PrimitiveTypeKind.Double,
                                     PrimitiveTypeKind.Int16,
                                     PrimitiveTypeKind.Int32,
                                     PrimitiveTypeKind.Int64,
                                     PrimitiveTypeKind.Byte,
                                     PrimitiveTypeKind.Single };
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "Abs", type, "value"));
            
            // Overloads for POWER functions
            PrimitiveTypeKind[] powerFirstParameterTypes = { PrimitiveTypeKind.Decimal,
                                        PrimitiveTypeKind.Double,
                                        PrimitiveTypeKind.Int32,
                                        PrimitiveTypeKind.Int64};

            PrimitiveTypeKind[] powerSecondParameterTypes = { PrimitiveTypeKind.Decimal,
                                        PrimitiveTypeKind.Double,
                                        PrimitiveTypeKind.Int64 };

            foreach (PrimitiveTypeKind kind1 in powerFirstParameterTypes)
            {
                foreach (PrimitiveTypeKind kind2 in powerSecondParameterTypes)
                {
                    functions.AddFunction(kind1, "Power", kind1, "baseArgument", kind2, "exponent");
                }
            }

            #endregion // Math Functions

            #region Bitwise Functions

            // Overloads for BitwiseAND, BitwiseNOT, BitwiseOR, BitwiseXOR functions
            parameterTypes = new[] { PrimitiveTypeKind.Int16,
                                     PrimitiveTypeKind.Int32,
                                     PrimitiveTypeKind.Int64,
                                     PrimitiveTypeKind.Byte };

            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "BitwiseAnd", type, "value1", type, "value2"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "BitwiseOr", type, "value1", type, "value2"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "BitwiseXor", type, "value1", type, "value2"));
            functions.ForTypes(parameterTypes, type => functions.AddFunction(type, "BitwiseNot", type, "value"));
            
            #endregion

            #region Misc Functions

            functions.AddFunction(PrimitiveTypeKind.Guid, "NewGuid");
            
            #endregion // Misc Functions

            #region Spatial Functions

            EdmProviderManifestSpatialFunctions.AddFunctions(functions);

            #endregion

            System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> readOnlyFunctions = functions.ToFunctionCollection();

            Interlocked.CompareExchange<System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>>(ref _functions, readOnlyFunctions, null);
        }
                
        #region Edm Provider Specific Functionality
        /// <summary>
        /// Returns the list of super-types for the given primitiveType
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <returns></returns>
        internal System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetPromotionTypes(PrimitiveType primitiveType)
        {
            InitializePromotableTypes();

            return _promotionTypes[(int)primitiveType.PrimitiveTypeKind];
        }

        /// <summary>
        /// Initializes Promotion Type relation
        /// </summary>
        private void InitializePromotableTypes()
        {
            if (null != _promotionTypes)
            {
                return;
            }

            System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>[] promotionTypes = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>[EdmConstants.NumPrimitiveTypes];

            for (int i = 0; i < EdmConstants.NumPrimitiveTypes; i++)
            {
                promotionTypes[i] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] { _primitiveTypes[i] });
            }

            //
            // PrimitiveTypeKind.Byte
            //
            promotionTypes[(int)PrimitiveTypeKind.Byte] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] {
                _primitiveTypes[(int)PrimitiveTypeKind.Byte],
                _primitiveTypes[(int)PrimitiveTypeKind.Int16],
                _primitiveTypes[(int)PrimitiveTypeKind.Int32],
                _primitiveTypes[(int)PrimitiveTypeKind.Int64],
                _primitiveTypes[(int)PrimitiveTypeKind.Decimal],
                _primitiveTypes[(int)PrimitiveTypeKind.Single],
                _primitiveTypes[(int)PrimitiveTypeKind.Double]
            });

            //
            // PrimitiveTypeKind.Int16
            //
            promotionTypes[(int)PrimitiveTypeKind.Int16] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] {
                _primitiveTypes[(int)PrimitiveTypeKind.Int16],
                _primitiveTypes[(int)PrimitiveTypeKind.Int32],
                _primitiveTypes[(int)PrimitiveTypeKind.Int64],
                _primitiveTypes[(int)PrimitiveTypeKind.Decimal],
                _primitiveTypes[(int)PrimitiveTypeKind.Single],
                _primitiveTypes[(int)PrimitiveTypeKind.Double]
            });

            //
            // PrimitiveTypeKind.Int32
            //
            promotionTypes[(int)PrimitiveTypeKind.Int32] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] {
                _primitiveTypes[(int)PrimitiveTypeKind.Int32],
                _primitiveTypes[(int)PrimitiveTypeKind.Int64],
                _primitiveTypes[(int)PrimitiveTypeKind.Decimal],
                _primitiveTypes[(int)PrimitiveTypeKind.Single],
                _primitiveTypes[(int)PrimitiveTypeKind.Double]
            });

            //
            // PrimitiveTypeKind.Int64
            //
            promotionTypes[(int)PrimitiveTypeKind.Int64] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] {
                _primitiveTypes[(int)PrimitiveTypeKind.Int64],
                _primitiveTypes[(int)PrimitiveTypeKind.Decimal],
                _primitiveTypes[(int)PrimitiveTypeKind.Single],
                _primitiveTypes[(int)PrimitiveTypeKind.Double]
            });

            //
            // PrimitiveTypeKind.Single
            //
            promotionTypes[(int)PrimitiveTypeKind.Single] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] {
                _primitiveTypes[(int)PrimitiveTypeKind.Single],
                _primitiveTypes[(int)PrimitiveTypeKind.Double]
            });

            InitializeSpatialPromotionGroup(
                promotionTypes,
                new PrimitiveTypeKind[] {
                        PrimitiveTypeKind.GeographyPoint, PrimitiveTypeKind.GeographyLineString, PrimitiveTypeKind.GeographyPolygon, 
                        PrimitiveTypeKind.GeographyMultiPoint, PrimitiveTypeKind.GeographyMultiLineString, PrimitiveTypeKind.GeographyMultiPolygon, 
                        PrimitiveTypeKind.GeographyCollection},
                PrimitiveTypeKind.Geography);

            InitializeSpatialPromotionGroup(
                promotionTypes,
                new PrimitiveTypeKind[] {
                    PrimitiveTypeKind.GeometryPoint, PrimitiveTypeKind.GeometryLineString, PrimitiveTypeKind.GeometryPolygon, 
                    PrimitiveTypeKind.GeometryMultiPoint, PrimitiveTypeKind.GeometryMultiLineString, PrimitiveTypeKind.GeometryMultiPolygon, 
                    PrimitiveTypeKind.GeometryCollection},
                PrimitiveTypeKind.Geometry);

            Interlocked.CompareExchange<System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>[]>(ref _promotionTypes,
                                                                             promotionTypes,
                                                                             null);
        }

        private void InitializeSpatialPromotionGroup(System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>[] promotionTypes, PrimitiveTypeKind[] promotableKinds, PrimitiveTypeKind baseKind)
        {
            foreach (PrimitiveTypeKind promotableKind in promotableKinds)
            {
                promotionTypes[(int)promotableKind] = new System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType>(new PrimitiveType[] {
                    _primitiveTypes[(int)promotableKind],
                    _primitiveTypes[(int)baseKind]
                });
            }
        }

        internal TypeUsage GetCanonicalModelTypeUsage(PrimitiveTypeKind primitiveTypeKind)
        {
            if (null == _canonicalModelTypes)
            {
                InitializeCanonicalModelTypes();
            }
            return _canonicalModelTypes[(int)primitiveTypeKind];
        }

        /// <summary>
        /// Initializes Canonical Model Types
        /// </summary>
        private void InitializeCanonicalModelTypes()
        {
            InitializePrimitiveTypes();

            TypeUsage[] canonicalTypes = new TypeUsage[EdmConstants.NumPrimitiveTypes];
            for (int primitiveTypeIndex = 0; primitiveTypeIndex < EdmConstants.NumPrimitiveTypes; primitiveTypeIndex++)
            {
                PrimitiveType primitiveType = _primitiveTypes[primitiveTypeIndex];
                TypeUsage typeUsage = TypeUsage.CreateDefaultTypeUsage(primitiveType);
                Debug.Assert(null != typeUsage, "TypeUsage must not be null");
                canonicalTypes[primitiveTypeIndex] = typeUsage;
            }

            Interlocked.CompareExchange<TypeUsage[]>(ref _canonicalModelTypes, canonicalTypes, null);
        }
        #endregion

        #region DbProviderManifest Interface
        /// <summary>
        /// Returns all the primitive types supported by the provider manifest
        /// </summary>
        /// <returns>A collection of primitive types</returns>
        public override System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            InitializePrimitiveTypes();
            return _primitiveTypes;
        }

        public override TypeUsage GetEdmType(TypeUsage storeType)
        {
            throw new NotImplementedException();
        }

        public override TypeUsage GetStoreType(TypeUsage edmType)
        {
            throw new NotImplementedException();
        }

        internal TypeUsage ForgetScalarConstraints(TypeUsage type)
        {
            PrimitiveType primitiveType = type.EdmType as PrimitiveType;
            Debug.Assert(primitiveType != null, "type argument must be primitive in order to use this function");
            if (primitiveType != null)
            {
                return GetCanonicalModelTypeUsage(primitiveType.PrimitiveTypeKind);
            }
            else
            {
                return type;
            }
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
        #endregion

    }
}
