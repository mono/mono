//---------------------------------------------------------------------
// <copyright file="SqlFunctionCallHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.SqlClient.SqlGen
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Enacapsulates the logic required to translate function calls represented as instances of DbFunctionExpression into SQL.
    /// There are several special cases that modify how the translation should proceed. These include:
    /// - 'Special' canonical functions, for which the function name or arguments differ between the EDM canonical function and the SQL function
    /// - 'Special' server functions, which are similar to the 'special' canonical functions but sourced by the SQL Server provider manifest
    /// - Niladic functions, which require the parentheses that would usually follow the function name to be omitted
    /// - Spatial canonical functions, which must translate to a static method call, instance method call, or instance property access against
    ///   one of the built-in spatial CLR UDTs (geography/geometry).
    /// </summary>
    internal static class SqlFunctionCallHandler
    {
        #region Static fields, include dictionaries used to dispatch function handling

        static private readonly Dictionary<string, FunctionHandler> _storeFunctionHandlers = InitializeStoreFunctionHandlers();
        static private readonly Dictionary<string, FunctionHandler> _canonicalFunctionHandlers = InitializeCanonicalFunctionHandlers();
        static private readonly Dictionary<string, string> _functionNameToOperatorDictionary = InitializeFunctionNameToOperatorDictionary();
        static private readonly Dictionary<string, string> _dateAddFunctionNameToDatepartDictionary = InitializeDateAddFunctionNameToDatepartDictionary();
        static private readonly Dictionary<string, string> _dateDiffFunctionNameToDatepartDictionary = InitializeDateDiffFunctionNameToDatepartDictionary();
        static private readonly Dictionary<string, FunctionHandler> _geographyFunctionNameToStaticMethodHandlerDictionary = InitializeGeographyStaticMethodFunctionsDictionary();
        static private readonly Dictionary<string, string> _geographyFunctionNameToInstancePropertyNameDictionary = InitializeGeographyInstancePropertyFunctionsDictionary();
        static private readonly Dictionary<string, string> _geographyRenamedInstanceMethodFunctionDictionary = InitializeRenamedGeographyInstanceMethodFunctions();
        static private readonly Dictionary<string, FunctionHandler> _geometryFunctionNameToStaticMethodHandlerDictionary = InitializeGeometryStaticMethodFunctionsDictionary();
        static private readonly Dictionary<string, string> _geometryFunctionNameToInstancePropertyNameDictionary = InitializeGeometryInstancePropertyFunctionsDictionary();
        static private readonly Dictionary<string, string> _geometryRenamedInstanceMethodFunctionDictionary = InitializeRenamedGeometryInstanceMethodFunctions();
        static private readonly Set<string> _datepartKeywords = new Set<string>(new string[] {  "year", "yy", "yyyy",
                                                                                                 "quarter", "qq", "q",
                                                                                                 "month", "mm", "m", 
                                                                                                 "dayofyear", "dy", "y", 
                                                                                                 "day", "dd", "d",
                                                                                                 "week", "wk", "ww",
                                                                                                 "weekday", "dw", "w",
                                                                                                 "hour", "hh",
                                                                                                 "minute", "mi", "n", 
                                                                                                 "second", "ss", "s",
                                                                                                 "millisecond", "ms",
                                                                                                 "microsecond", "mcs",
                                                                                                 "nanosecond", "ns",
                                                                                                 "tzoffset", "tz",
                                                                                                 "iso_week", "isoww", "isowk"},  
                                                                                        StringComparer.OrdinalIgnoreCase).MakeReadOnly();
        static private readonly Set<string> _functionRequiresReturnTypeCastToInt64 = new Set<string>(new string[] { "SqlServer.CHARINDEX" },
                                                                                              StringComparer.Ordinal).MakeReadOnly();
        static private readonly Set<string> _functionRequiresReturnTypeCastToInt32 = new Set<string>(new string[] { "SqlServer.LEN"      ,
                                                                                                                 "SqlServer.PATINDEX"    ,
                                                                                                                 "SqlServer.DATALENGTH"  ,
                                                                                                                 "SqlServer.CHARINDEX"   ,
                                                                                                                 "Edm.IndexOf"           ,
                                                                                                                 "Edm.Length"            },
                                                                                                      StringComparer.Ordinal).MakeReadOnly();
        static private readonly Set<string> _functionRequiresReturnTypeCastToInt16 = new Set<string>(new string[] { "Edm.Abs"            },
                                                                                                      StringComparer.Ordinal).MakeReadOnly();
        static private readonly Set<string> _functionRequiresReturnTypeCastToSingle = new Set<string>(new string[] { "Edm.Abs"           ,
                                                                                                                     "Edm.Round"         ,
                                                                                                                     "Edm.Floor"         ,
                                                                                                                     "Edm.Ceiling"       },
                                                                                                      StringComparer.Ordinal).MakeReadOnly();
        static private readonly Set<string> _maxTypeNames = new Set<string>(new string[] { "varchar(max)"    ,
                                                                                           "nvarchar(max)"   ,
                                                                                           "text"            ,
                                                                                           "ntext"           ,
                                                                                           "varbinary(max)"  ,
                                                                                           "image"           ,
                                                                                           "xml"             },
                                                                                StringComparer.Ordinal).MakeReadOnly();

        #endregion

        #region Static dictionary initialization

        private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

        /// <summary>
        /// All special store functions and their handlers
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, FunctionHandler> InitializeStoreFunctionHandlers()
        {
            Dictionary<string, FunctionHandler> functionHandlers = new Dictionary<string, FunctionHandler>(15, StringComparer.Ordinal);
            functionHandlers.Add("CONCAT", HandleConcatFunction);
            functionHandlers.Add("DATEADD", HandleDatepartDateFunction);
            functionHandlers.Add("DATEDIFF", HandleDatepartDateFunction);
            functionHandlers.Add("DATENAME", HandleDatepartDateFunction);
            functionHandlers.Add("DATEPART", HandleDatepartDateFunction);

            // Spatial functions are mapped to static or instance members of geography/geometry
            // Geography Static functions
            functionHandlers.Add("POINTGEOGRAPHY", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::Point"));

            // Geometry Static functions
            functionHandlers.Add("POINTGEOMETRY", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::Point"));

            // Spatial Instance functions (shared)
            functionHandlers.Add("ASTEXTZM", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "AsTextZM", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("BUFFERWITHTOLERANCE", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "BufferWithTolerance", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("ENVELOPEANGLE", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "EnvelopeAngle", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("ENVELOPECENTER", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "EnvelopeCenter", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("INSTANCEOF", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "InstanceOf", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("FILTER", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "Filter", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("MAKEVALID", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "MakeValid", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("REDUCE", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "Reduce", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("NUMRINGS", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "NumRings", functionExpression, isPropertyAccess: false));
            functionHandlers.Add("RINGN", (sqlgen, functionExpression) => WriteInstanceFunctionCall(sqlgen, "RingN", functionExpression, isPropertyAccess: false));

            return functionHandlers;
        }

        /// <summary>
        /// All special non-aggregate canonical functions and their handlers
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, FunctionHandler> InitializeCanonicalFunctionHandlers()
        {
            Dictionary<string, FunctionHandler> functionHandlers = new Dictionary<string, FunctionHandler>(16, StringComparer.Ordinal);
            functionHandlers.Add("IndexOf", HandleCanonicalFunctionIndexOf);
            functionHandlers.Add("Length", HandleCanonicalFunctionLength);
            functionHandlers.Add("NewGuid", HandleCanonicalFunctionNewGuid);
            functionHandlers.Add("Round", HandleCanonicalFunctionRound);
            functionHandlers.Add("Truncate", HandleCanonicalFunctionTruncate);
            functionHandlers.Add("Abs", HandleCanonicalFunctionAbs);
            functionHandlers.Add("ToLower", HandleCanonicalFunctionToLower);
            functionHandlers.Add("ToUpper", HandleCanonicalFunctionToUpper);
            functionHandlers.Add("Trim", HandleCanonicalFunctionTrim);
            functionHandlers.Add("Contains", HandleCanonicalFunctionContains);
            functionHandlers.Add("StartsWith", HandleCanonicalFunctionStartsWith);
            functionHandlers.Add("EndsWith", HandleCanonicalFunctionEndsWith);

            //DateTime Functions
            functionHandlers.Add("Year", HandleCanonicalFunctionDatepart);            
            functionHandlers.Add("Month", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Day", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Hour", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Minute", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Second", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Millisecond", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("DayOfYear", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("CurrentDateTime", HandleCanonicalFunctionCurrentDateTime);
            functionHandlers.Add("CurrentUtcDateTime", HandleCanonicalFunctionCurrentUtcDateTime);
            functionHandlers.Add("CurrentDateTimeOffset", HandleCanonicalFunctionCurrentDateTimeOffset);
            functionHandlers.Add("GetTotalOffsetMinutes", HandleCanonicalFunctionGetTotalOffsetMinutes);
            functionHandlers.Add("TruncateTime", HandleCanonicalFunctionTruncateTime);
            functionHandlers.Add("CreateDateTime", HandleCanonicalFunctionCreateDateTime);
            functionHandlers.Add("CreateDateTimeOffset", HandleCanonicalFunctionCreateDateTimeOffset);
            functionHandlers.Add("CreateTime", HandleCanonicalFunctionCreateTime);
            functionHandlers.Add("AddYears", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMonths", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddDays", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddHours", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMinutes", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddSeconds", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMilliseconds", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMicroseconds", HandleCanonicalFunctionDateAddKatmaiOrNewer);
            functionHandlers.Add("AddNanoseconds", HandleCanonicalFunctionDateAddKatmaiOrNewer);
            functionHandlers.Add("DiffYears", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMonths", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffDays", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffHours", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMinutes", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffSeconds", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMilliseconds", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMicroseconds", HandleCanonicalFunctionDateDiffKatmaiOrNewer);
            functionHandlers.Add("DiffNanoseconds", HandleCanonicalFunctionDateDiffKatmaiOrNewer);

            //Functions that translate to operators
            functionHandlers.Add("Concat", HandleConcatFunction);
            functionHandlers.Add("BitwiseAnd", HandleCanonicalFunctionBitwise);
            functionHandlers.Add("BitwiseNot", HandleCanonicalFunctionBitwise);
            functionHandlers.Add("BitwiseOr", HandleCanonicalFunctionBitwise);
            functionHandlers.Add("BitwiseXor", HandleCanonicalFunctionBitwise);

            return functionHandlers;
        }

        /// <summary>
        /// Initalizes the mapping from functions to TSql operators
        /// for all functions that translate to TSql operators
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeFunctionNameToOperatorDictionary()
        {
            Dictionary<string, string> functionNameToOperatorDictionary = new Dictionary<string, string>(5, StringComparer.Ordinal);
            functionNameToOperatorDictionary.Add("Concat", "+");    //canonical
            functionNameToOperatorDictionary.Add("CONCAT", "+");    //store
            functionNameToOperatorDictionary.Add("BitwiseAnd", "&");
            functionNameToOperatorDictionary.Add("BitwiseNot", "~");
            functionNameToOperatorDictionary.Add("BitwiseOr", "|");
            functionNameToOperatorDictionary.Add("BitwiseXor", "^");
            return functionNameToOperatorDictionary;
        }

        /// <summary>
        /// Initalizes the mapping from names of canonical function for date/time addition
        /// to corresponding dateparts
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeDateAddFunctionNameToDatepartDictionary()
        {
            Dictionary<string, string> dateAddFunctionNameToDatepartDictionary = new Dictionary<string, string>(5, StringComparer.Ordinal);
            dateAddFunctionNameToDatepartDictionary.Add("AddYears", "year");   
            dateAddFunctionNameToDatepartDictionary.Add("AddMonths", "month"); 
            dateAddFunctionNameToDatepartDictionary.Add("AddDays", "day");
            dateAddFunctionNameToDatepartDictionary.Add("AddHours", "hour");
            dateAddFunctionNameToDatepartDictionary.Add("AddMinutes", "minute");
            dateAddFunctionNameToDatepartDictionary.Add("AddSeconds", "second");
            dateAddFunctionNameToDatepartDictionary.Add("AddMilliseconds", "millisecond");
            dateAddFunctionNameToDatepartDictionary.Add("AddMicroseconds", "microsecond");
            dateAddFunctionNameToDatepartDictionary.Add("AddNanoseconds", "nanosecond");
            return dateAddFunctionNameToDatepartDictionary;
        }

        /// <summary>
        /// Initalizes the mapping from names of canonical function for date/time difference
        /// to corresponding dateparts
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeDateDiffFunctionNameToDatepartDictionary()
        {
            Dictionary<string, string> dateDiffFunctionNameToDatepartDictionary = new Dictionary<string, string>(5, StringComparer.Ordinal);
            dateDiffFunctionNameToDatepartDictionary.Add("DiffYears", "year");  
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMonths", "month");   
            dateDiffFunctionNameToDatepartDictionary.Add("DiffDays", "day");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffHours", "hour");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMinutes", "minute");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffSeconds", "second");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMilliseconds", "millisecond");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMicroseconds", "microsecond");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffNanoseconds", "nanosecond");
            return dateDiffFunctionNameToDatepartDictionary;
        }

        /// <summary>
        /// Initalizes the mapping from names of canonical function that represent static geography methods to their corresponding
        /// static method name, qualified with the 'geography::' prefix.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, FunctionHandler> InitializeGeographyStaticMethodFunctionsDictionary()
        {
            Dictionary<string, FunctionHandler> staticGeographyFunctions = new Dictionary<string, FunctionHandler>();
            
            // Well Known Text constructors
            staticGeographyFunctions.Add("GeographyFromText", HandleSpatialFromTextFunction);
            staticGeographyFunctions.Add("GeographyPointFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPointFromText"));
            staticGeographyFunctions.Add("GeographyLineFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STLineFromText"));
            staticGeographyFunctions.Add("GeographyPolygonFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPolyFromText"));
            staticGeographyFunctions.Add("GeographyMultiPointFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPointFromText"));
            staticGeographyFunctions.Add("GeographyMultiLineFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMLineFromText"));
            staticGeographyFunctions.Add("GeographyMultiPolygonFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPolyFromText"));
            staticGeographyFunctions.Add("GeographyCollectionFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STGeomCollFromText"));
            
            // Well Known Binary constructors
            staticGeographyFunctions.Add("GeographyFromBinary", HandleSpatialFromBinaryFunction);
            staticGeographyFunctions.Add("GeographyPointFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPointFromWKB"));
            staticGeographyFunctions.Add("GeographyLineFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STLineFromWKB"));
            staticGeographyFunctions.Add("GeographyPolygonFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STPolyFromWKB"));
            staticGeographyFunctions.Add("GeographyMultiPointFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPointFromWKB"));
            staticGeographyFunctions.Add("GeographyMultiLineFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMLineFromWKB"));
            staticGeographyFunctions.Add("GeographyMultiPolygonFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STMPolyFromWKB"));
            staticGeographyFunctions.Add("GeographyCollectionFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geography::STGeomCollFromWKB"));

            // GML constructor (non-OGC)
            staticGeographyFunctions.Add("GeographyFromGml", HandleSpatialFromGmlFunction);

            return staticGeographyFunctions;
        }
                
        /// <summary>
        /// Initalizes the mapping from names of canonical function that represent geography instance properties to their corresponding
        /// store property name.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeGeographyInstancePropertyFunctionsDictionary()
        {
            Dictionary<string, string> instancePropGeographyFunctions = new Dictionary<string, string>();

            instancePropGeographyFunctions.Add("CoordinateSystemId", "STSrid");
            instancePropGeographyFunctions.Add("Latitude", "Lat");
            instancePropGeographyFunctions.Add("Longitude", "Long");
            instancePropGeographyFunctions.Add("Measure", "M");
            instancePropGeographyFunctions.Add("Elevation", "Z");

            return instancePropGeographyFunctions;
        }

        /// <summary>
        /// Initalizes the mapping of canonical function name to instance method name for geography instance functions that differ in name from the sql server equivalent.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeRenamedGeographyInstanceMethodFunctions()
        {
            Dictionary<string, string> renamedInstanceMethodFunctions = new Dictionary<string, string>();

            renamedInstanceMethodFunctions.Add("AsText", "STAsText");
            renamedInstanceMethodFunctions.Add("AsBinary", "STAsBinary");
            renamedInstanceMethodFunctions.Add("SpatialTypeName", "STGeometryType");
            renamedInstanceMethodFunctions.Add("SpatialDimension", "STDimension");
            renamedInstanceMethodFunctions.Add("IsEmptySpatial", "STIsEmpty");
            renamedInstanceMethodFunctions.Add("SpatialEquals", "STEquals");
            renamedInstanceMethodFunctions.Add("SpatialDisjoint", "STDisjoint");
            renamedInstanceMethodFunctions.Add("SpatialIntersects", "STIntersects");
            renamedInstanceMethodFunctions.Add("SpatialBuffer", "STBuffer");
            renamedInstanceMethodFunctions.Add("Distance", "STDistance");
            renamedInstanceMethodFunctions.Add("SpatialUnion", "STUnion");
            renamedInstanceMethodFunctions.Add("SpatialIntersection", "STIntersection");
            renamedInstanceMethodFunctions.Add("SpatialDifference", "STDifference");
            renamedInstanceMethodFunctions.Add("SpatialSymmetricDifference", "STSymDifference");
            renamedInstanceMethodFunctions.Add("SpatialElementCount", "STNumGeometries");
            renamedInstanceMethodFunctions.Add("SpatialElementAt", "STGeometryN");
            renamedInstanceMethodFunctions.Add("SpatialLength", "STLength");
            renamedInstanceMethodFunctions.Add("StartPoint", "STStartPoint");
            renamedInstanceMethodFunctions.Add("EndPoint", "STEndPoint");
            renamedInstanceMethodFunctions.Add("IsClosedSpatial", "STIsClosed");
            renamedInstanceMethodFunctions.Add("PointCount", "STNumPoints");
            renamedInstanceMethodFunctions.Add("PointAt", "STPointN");
            renamedInstanceMethodFunctions.Add("Area", "STArea");

            return renamedInstanceMethodFunctions;
        }

        /// <summary>
        /// Initalizes the mapping from names of canonical function that represent static geometry methods to their corresponding
        /// static method name, qualified with the 'geometry::' prefix.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, FunctionHandler> InitializeGeometryStaticMethodFunctionsDictionary()
        {
            Dictionary<string, FunctionHandler> staticGeometryFunctions = new Dictionary<string, FunctionHandler>();
            
            // Well Known Text constructors
            staticGeometryFunctions.Add("GeometryFromText", HandleSpatialFromTextFunction);
            staticGeometryFunctions.Add("GeometryPointFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPointFromText"));
            staticGeometryFunctions.Add("GeometryLineFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STLineFromText"));
            staticGeometryFunctions.Add("GeometryPolygonFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPolyFromText"));
            staticGeometryFunctions.Add("GeometryMultiPointFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPointFromText"));
            staticGeometryFunctions.Add("GeometryMultiLineFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMLineFromText"));
            staticGeometryFunctions.Add("GeometryMultiPolygonFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPolyFromText"));
            staticGeometryFunctions.Add("GeometryCollectionFromText", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STGeomCollFromText"));
            
            // Well Known Binary constructors
            staticGeometryFunctions.Add("GeometryFromBinary", HandleSpatialFromBinaryFunction);
            staticGeometryFunctions.Add("GeometryPointFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPointFromWKB"));
            staticGeometryFunctions.Add("GeometryLineFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STLineFromWKB"));
            staticGeometryFunctions.Add("GeometryPolygonFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STPolyFromWKB"));
            staticGeometryFunctions.Add("GeometryMultiPointFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPointFromWKB"));
            staticGeometryFunctions.Add("GeometryMultiLineFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMLineFromWKB"));
            staticGeometryFunctions.Add("GeometryMultiPolygonFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STMPolyFromWKB"));
            staticGeometryFunctions.Add("GeometryCollectionFromBinary", (sqlgen, functionExpression) => HandleFunctionDefaultGivenName(sqlgen, functionExpression, "geometry::STGeomCollFromWKB"));

            // GML constructor (non-OGC)
            staticGeometryFunctions.Add("GeometryFromGml", HandleSpatialFromGmlFunction);

            return staticGeometryFunctions;
        }

        /// <summary>
        /// Initalizes the mapping from names of canonical function that represent geometry instance properties to their corresponding
        /// store property name.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeGeometryInstancePropertyFunctionsDictionary()
        {
            Dictionary<string, string> instancePropGeometryFunctions = new Dictionary<string, string>();
            
            instancePropGeometryFunctions.Add("CoordinateSystemId", "STSrid");
            instancePropGeometryFunctions.Add("Measure", "M");
            instancePropGeometryFunctions.Add("XCoordinate", "STX");
            instancePropGeometryFunctions.Add("YCoordinate", "STY");
            instancePropGeometryFunctions.Add("Elevation", "Z");
            
            return instancePropGeometryFunctions;
        }

        /// <summary>
        /// Initalizes the mapping of canonical function name to instance method name for geometry instance functions that differ in name from the sql server equivalent.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> InitializeRenamedGeometryInstanceMethodFunctions()
        {
            Dictionary<string, string> renamedInstanceMethodFunctions = new Dictionary<string, string>();

            renamedInstanceMethodFunctions.Add("AsText", "STAsText");
            renamedInstanceMethodFunctions.Add("AsBinary", "STAsBinary");
            renamedInstanceMethodFunctions.Add("SpatialTypeName", "STGeometryType");
            renamedInstanceMethodFunctions.Add("SpatialDimension", "STDimension");
            renamedInstanceMethodFunctions.Add("IsEmptySpatial", "STIsEmpty");
            renamedInstanceMethodFunctions.Add("IsSimpleGeometry", "STIsSimple");
            renamedInstanceMethodFunctions.Add("IsValidGeometry", "STIsValid");
            renamedInstanceMethodFunctions.Add("SpatialBoundary", "STBoundary");
            renamedInstanceMethodFunctions.Add("SpatialEnvelope", "STEnvelope");
            renamedInstanceMethodFunctions.Add("SpatialEquals", "STEquals");
            renamedInstanceMethodFunctions.Add("SpatialDisjoint", "STDisjoint");
            renamedInstanceMethodFunctions.Add("SpatialIntersects", "STIntersects");
            renamedInstanceMethodFunctions.Add("SpatialTouches", "STTouches");
            renamedInstanceMethodFunctions.Add("SpatialCrosses", "STCrosses");
            renamedInstanceMethodFunctions.Add("SpatialWithin", "STWithin");
            renamedInstanceMethodFunctions.Add("SpatialContains", "STContains");
            renamedInstanceMethodFunctions.Add("SpatialOverlaps", "STOverlaps");
            renamedInstanceMethodFunctions.Add("SpatialRelate", "STRelate");
            renamedInstanceMethodFunctions.Add("SpatialBuffer", "STBuffer");
            renamedInstanceMethodFunctions.Add("SpatialConvexHull", "STConvexHull");
            renamedInstanceMethodFunctions.Add("Distance", "STDistance");
            renamedInstanceMethodFunctions.Add("SpatialUnion", "STUnion");
            renamedInstanceMethodFunctions.Add("SpatialIntersection", "STIntersection");
            renamedInstanceMethodFunctions.Add("SpatialDifference", "STDifference");
            renamedInstanceMethodFunctions.Add("SpatialSymmetricDifference", "STSymDifference");
            renamedInstanceMethodFunctions.Add("SpatialElementCount", "STNumGeometries");
            renamedInstanceMethodFunctions.Add("SpatialElementAt", "STGeometryN");
            renamedInstanceMethodFunctions.Add("SpatialLength", "STLength");
            renamedInstanceMethodFunctions.Add("StartPoint", "STStartPoint");
            renamedInstanceMethodFunctions.Add("EndPoint", "STEndPoint");
            renamedInstanceMethodFunctions.Add("IsClosedSpatial", "STIsClosed");
            renamedInstanceMethodFunctions.Add("IsRing", "STIsRing");
            renamedInstanceMethodFunctions.Add("PointCount", "STNumPoints");
            renamedInstanceMethodFunctions.Add("PointAt", "STPointN");
            renamedInstanceMethodFunctions.Add("Area", "STArea");
            renamedInstanceMethodFunctions.Add("Centroid", "STCentroid");
            renamedInstanceMethodFunctions.Add("PointOnSurface", "STPointOnSurface");
            renamedInstanceMethodFunctions.Add("ExteriorRing", "STExteriorRing");
            renamedInstanceMethodFunctions.Add("InteriorRingCount", "STNumInteriorRing");
            renamedInstanceMethodFunctions.Add("InteriorRingAt", "STInteriorRingN");

            return renamedInstanceMethodFunctions;
        }

        private static ISqlFragment HandleSpatialFromTextFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
        {
            string functionNameWithSrid = (TypeSemantics.IsPrimitiveType(functionExpression.ResultType, PrimitiveTypeKind.Geometry) ? "geometry::STGeomFromText" : "geography::STGeomFromText");
            string functionNameWithoutSrid = (TypeSemantics.IsPrimitiveType(functionExpression.ResultType, PrimitiveTypeKind.Geometry) ? "geometry::Parse" : "geography::Parse");
            
            if (functionExpression.Arguments.Count == 2)
            {
                return HandleFunctionDefaultGivenName(sqlgen, functionExpression, functionNameWithSrid);
            }
            else
            {
                Debug.Assert(functionExpression.Arguments.Count == 1, "FromText function should have text or text + srid arguments only");
                return HandleFunctionDefaultGivenName(sqlgen, functionExpression, functionNameWithoutSrid);
            }
        }


        private static ISqlFragment HandleSpatialFromGmlFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
        {
            return HandleSpatialStaticMethodFunctionAppendSrid(sqlgen, functionExpression, (TypeSemantics.IsPrimitiveType(functionExpression.ResultType, PrimitiveTypeKind.Geometry) ? "geometry::GeomFromGml" : "geography::GeomFromGml"));
        }

        private static ISqlFragment HandleSpatialFromBinaryFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
        {
            return HandleSpatialStaticMethodFunctionAppendSrid(sqlgen, functionExpression, (TypeSemantics.IsPrimitiveType(functionExpression.ResultType, PrimitiveTypeKind.Geometry) ? "geometry::STGeomFromWKB" : "geography::STGeomFromWKB"));
        }

        private static readonly DbExpression defaultGeographySridExpression = DbExpressionBuilder.Constant(DbGeography.DefaultCoordinateSystemId);
        private static readonly DbExpression defaultGeometrySridExpression = DbExpressionBuilder.Constant(DbGeometry.DefaultCoordinateSystemId);

        private static ISqlFragment HandleSpatialStaticMethodFunctionAppendSrid(SqlGenerator sqlgen, DbFunctionExpression functionExpression, string functionName)
        {
            if (functionExpression.Arguments.Count == 2)
            {
                return HandleFunctionDefaultGivenName(sqlgen, functionExpression, functionName);
            }
            else
            {
                DbExpression sridExpression = (TypeSemantics.IsPrimitiveType(functionExpression.ResultType, PrimitiveTypeKind.Geometry) ? defaultGeometrySridExpression : defaultGeographySridExpression);
                SqlBuilder result = new SqlBuilder();
                result.Append(functionName);
                WriteFunctionArguments(sqlgen, functionExpression.Arguments.Concat(new[] { sridExpression }), result);
                return result;
            }
        }

        #endregion

        internal static ISqlFragment GenerateFunctionCallSql(SqlGenerator sqlgen, DbFunctionExpression functionExpression)
        {
            //
            // check if function requires special case processing, if so, delegates to it
            //
            if (IsSpecialCanonicalFunction(functionExpression))
            {
                return HandleSpecialCanonicalFunction(sqlgen, functionExpression);
            }

            if (IsSpecialStoreFunction(functionExpression))
            {
                return HandleSpecialStoreFunction(sqlgen, functionExpression);
            }

            PrimitiveTypeKind spatialTypeKind;
            if(IsSpatialCanonicalFunction(functionExpression, out spatialTypeKind))
            {
                return HandleSpatialCanonicalFunction(sqlgen, functionExpression, spatialTypeKind);
            }

            return HandleFunctionDefault(sqlgen, functionExpression);
        }
                        
        /// <summary>
        /// Determines whether the given function is a store function that
        /// requires special handling
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool IsSpecialStoreFunction(DbFunctionExpression e)
        {
            return IsStoreFunction(e.Function)
                && _storeFunctionHandlers.ContainsKey(e.Function.Name);
        }

        /// <summary>
        /// Determines whether the given function is a canonical function that
        /// requires special handling
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool IsSpecialCanonicalFunction(DbFunctionExpression e)
        {
            return TypeHelpers.IsCanonicalFunction(e.Function)
            && _canonicalFunctionHandlers.ContainsKey(e.Function.Name);
        }

        /// <summary>
        /// Determines whether the given function is a canonical function the translates
        /// to a spatial (geography/geometry) property access or method call.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool IsSpatialCanonicalFunction(DbFunctionExpression e, out PrimitiveTypeKind spatialTypeKind)
        {
            if (TypeHelpers.IsCanonicalFunction(e.Function))
            {
                if (Helper.IsSpatialType(e.ResultType, out spatialTypeKind))
                {
                    return true;
                }

                foreach (FunctionParameter functionParameter in e.Function.Parameters)
                {
                    if (Helper.IsSpatialType(functionParameter.TypeUsage, out spatialTypeKind))
                    {
                        return true;
                    }
                }
            }

            spatialTypeKind = default(PrimitiveTypeKind);
            return false;
        }
                
        /// <summary>
        /// Default handling for functions. 
        /// Translates them to FunctionName(arg1, arg2, ..., argn)
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleFunctionDefault(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionDefaultGivenName(sqlgen, e, null);
        }

        /// <summary>
        /// Default handling for functions with a given name.
        /// Translates them to FunctionName(arg1, arg2, ..., argn)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private static ISqlFragment HandleFunctionDefaultGivenName(SqlGenerator sqlgen, DbFunctionExpression e, string functionName)
        {
            // NOTE: The order of checks is important in case of CHARINDEX.
            if (CastReturnTypeToInt64(e))
            {
                return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "bigint");
            }
            else if (CastReturnTypeToInt32(sqlgen, e))
            {
                return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "int");
            }
            else if (CastReturnTypeToInt16(e))
            {
                return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "smallint");
            }
            else if (CastReturnTypeToSingle(e))
            {
                return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, "real");
            }
            else
            {
                return HandleFunctionDefaultCastReturnValue(sqlgen, e, functionName, null);
            }
        }

        /// <summary>
        /// Default handling for functions with a given name and given return value cast.
        /// Translates them to CAST(FunctionName(arg1, arg2, ..., argn) AS returnType)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="functionName"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        private static ISqlFragment HandleFunctionDefaultCastReturnValue(SqlGenerator sqlgen, DbFunctionExpression e, string functionName, string returnType)
        {
            return WrapWithCast(returnType, result =>
            {
                if (functionName == null)
                {
                    WriteFunctionName(result, e.Function);
                }
                else
                {
                    result.Append(functionName);
                }

                HandleFunctionArgumentsDefault(sqlgen, e, result);
            });
        }

        private static ISqlFragment WrapWithCast(string returnType, Action<SqlBuilder> toWrap)
        {
            SqlBuilder result = new SqlBuilder();
            if (returnType != null)
            {
                result.Append(" CAST(");
            }

            toWrap(result);

            if (returnType != null)
            {
                result.Append(" AS ");
                result.Append(returnType);
                result.Append(")");
            }
            return result;
        }

        /// <summary>
        /// Default handling on function arguments.
        /// Appends the list of arguments to the given result
        /// If the function is niladic it does not append anything, 
        /// otherwise it appends (arg1, arg2, .., argn)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="result"></param>
        private static void HandleFunctionArgumentsDefault(SqlGenerator sqlgen, DbFunctionExpression e, SqlBuilder result)
        {
            bool isNiladicFunction = e.Function.NiladicFunctionAttribute;
            Debug.Assert(!(isNiladicFunction && (0 < e.Arguments.Count)), "function attributed as NiladicFunction='true' in the provider manifest cannot have arguments");
            if (isNiladicFunction && e.Arguments.Count > 0)
            {
                EntityUtil.Metadata(System.Data.Entity.Strings.SqlGen_NiladicFunctionsCannotHaveParameters);
            }

            if (!isNiladicFunction)
            {
                WriteFunctionArguments(sqlgen, e.Arguments, result);
            }
        }

        private static void WriteFunctionArguments(SqlGenerator sqlgen, IEnumerable<DbExpression> functionArguments, SqlBuilder result)
        {
            result.Append("(");
            string separator = "";
            foreach (DbExpression arg in functionArguments)
            {
                result.Append(separator);
                result.Append(arg.Accept(sqlgen));
                separator = ", ";
            }
            result.Append(")");
        }

        /// <summary>
        /// Handler for functions that need to be translated to different store function based on version
        /// </summary>
        /// <param name="e"></param>
        /// <param name="preKatmaiName"></param>
        /// <param name="katmaiName"></param>
        /// <returns></returns>
        private static ISqlFragment HandleFunctionGivenNameBasedOnVersion(SqlGenerator sqlgen, DbFunctionExpression e, string preKatmaiName, string katmaiName)
        {
            if (sqlgen.IsPreKatmai)
            {
                return HandleFunctionDefaultGivenName(sqlgen, e, preKatmaiName);
            }
            return HandleFunctionDefaultGivenName(sqlgen, e, katmaiName);
        }
        
        /// <summary>
        /// Handler for special build in functions
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleSpecialStoreFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleSpecialFunction(_storeFunctionHandlers, sqlgen, e);
        }

        /// <summary>
        /// Handler for special canonical functions
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleSpecialCanonicalFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleSpecialFunction(_canonicalFunctionHandlers, sqlgen, e);
        }

        /// <summary>
        /// Dispatches the special function processing to the appropriate handler
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleSpecialFunction(Dictionary<string, FunctionHandler> handlers, SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(handlers.ContainsKey(e.Function.Name), "Special handling should be called only for functions in the list of special functions");
            return handlers[e.Function.Name](sqlgen, e);
        }

        private static ISqlFragment HandleSpatialCanonicalFunction(SqlGenerator sqlgen, DbFunctionExpression functionExpression, PrimitiveTypeKind spatialTypeKind)
        {
            Debug.Assert(spatialTypeKind == PrimitiveTypeKind.Geography || spatialTypeKind == PrimitiveTypeKind.Geometry, "Spatial function does not refer to a valid spatial primitive type kind?");
            if (spatialTypeKind == PrimitiveTypeKind.Geography)
            {
                return HandleSpatialCanonicalFunction(sqlgen, functionExpression, _geographyFunctionNameToStaticMethodHandlerDictionary, _geographyFunctionNameToInstancePropertyNameDictionary, _geographyRenamedInstanceMethodFunctionDictionary);
            }
            else
            {
                return HandleSpatialCanonicalFunction(sqlgen, functionExpression, _geometryFunctionNameToStaticMethodHandlerDictionary, _geometryFunctionNameToInstancePropertyNameDictionary, _geometryRenamedInstanceMethodFunctionDictionary);
            }
        }

        private static ISqlFragment HandleSpatialCanonicalFunction(SqlGenerator sqlgen,
                                                                   DbFunctionExpression functionExpression,
                                                                   Dictionary<string, FunctionHandler> staticMethodsMap, 
                                                                   Dictionary<string, string> instancePropertiesMap,
                                                                   Dictionary<string, string> renamedInstanceMethodsMap)
        {
            FunctionHandler staticFunctionHandler;
            string instancePropertyName;
            if (staticMethodsMap.TryGetValue(functionExpression.Function.Name, out staticFunctionHandler))
            {
                return staticFunctionHandler(sqlgen, functionExpression);
            }
            else if (instancePropertiesMap.TryGetValue(functionExpression.Function.Name, out instancePropertyName))
            {
                Debug.Assert(functionExpression.Function.Parameters.Count > 0 && Helper.IsSpatialType(functionExpression.Function.Parameters[0].TypeUsage), "Instance property function does not have instance parameter?");
                return WriteInstanceFunctionCall(sqlgen, instancePropertyName, functionExpression, isPropertyAccess: true, castReturnTypeTo: null);
            }
            else
            {
                // Default translation pattern is instance method; the instance method name may differ from that of the spatial canonical function
                Debug.Assert(functionExpression.Function.Parameters.Count > 0 && Helper.IsSpatialType(functionExpression.Function.Parameters[0].TypeUsage), "Instance method function does not have instance parameter?");
                string effectiveFunctionName;
                if (!renamedInstanceMethodsMap.TryGetValue(functionExpression.Function.Name, out effectiveFunctionName))
                {
                    effectiveFunctionName = functionExpression.Function.Name;
                }

                // For AsGml() calls, the XML result must be cast to string to match the declared function result type.
                string castResultType = null;
                if (effectiveFunctionName == "AsGml")
                {
                    castResultType = sqlgen.DefaultStringTypeName;
                }
                return WriteInstanceFunctionCall(sqlgen, effectiveFunctionName, functionExpression, isPropertyAccess: false, castReturnTypeTo: castResultType);
            }
        }

        private static ISqlFragment WriteInstanceFunctionCall(SqlGenerator sqlgen, string functionName, DbFunctionExpression functionExpression, bool isPropertyAccess)
        {
            return WriteInstanceFunctionCall(sqlgen, functionName, functionExpression, isPropertyAccess, null);
        }

        private static ISqlFragment WriteInstanceFunctionCall(SqlGenerator sqlgen, string functionName, DbFunctionExpression functionExpression, bool isPropertyAccess, string castReturnTypeTo)
        {
            Debug.Assert(!isPropertyAccess || functionExpression.Arguments.Count == 1, "Property accessor instance functions should have only the single instance argument");

            return WrapWithCast(castReturnTypeTo, result =>
            {
                DbExpression instanceExpression = functionExpression.Arguments[0];

                // Write the instance - if this is another function call, it need not be enclosed in parentheses.
                if (instanceExpression.ExpressionKind != DbExpressionKind.Function)
                {
                    sqlgen.ParenthesizeExpressionIfNeeded(instanceExpression, result);
                }
                else
                {
                    result.Append(instanceExpression.Accept(sqlgen));
                }
                result.Append(".");
                result.Append(functionName);

                if (!isPropertyAccess)
                {
                    WriteFunctionArguments(sqlgen, functionExpression.Arguments.Skip(1), result);
                }

            });
        }

        /// <summary>
        /// Handles functions that are translated into TSQL operators.
        /// The given function should have one or two arguments. 
        /// Functions with one arguemnt are translated into 
        ///     op arg
        /// Functions with two arguments are translated into
        ///     arg0 op arg1
        /// Also, the arguments can be optionaly enclosed in parethesis
        /// </summary>
        /// <param name="e"></param>
        /// <param name="parenthesiseArguments">Whether the arguments should be enclosed in parethesis</param>
        /// <returns></returns>
        private static ISqlFragment HandleSpecialFunctionToOperator(SqlGenerator sqlgen, DbFunctionExpression e, bool parenthesiseArguments)
        {
            SqlBuilder result = new SqlBuilder();
            Debug.Assert(e.Arguments.Count > 0 && e.Arguments.Count <= 2, "There should be 1 or 2 arguments for operator");

            if (e.Arguments.Count > 1)
            {
                if (parenthesiseArguments)
                {
                    result.Append("(");
                }
                result.Append(e.Arguments[0].Accept(sqlgen));
                if (parenthesiseArguments)
                {
                    result.Append(")");
                }
            }
            result.Append(" ");
            Debug.Assert(_functionNameToOperatorDictionary.ContainsKey(e.Function.Name), "The function can not be mapped to an operator");
            result.Append(_functionNameToOperatorDictionary[e.Function.Name]);
            result.Append(" ");

            if (parenthesiseArguments)
            {
                result.Append("(");
            }
            result.Append(e.Arguments[e.Arguments.Count - 1].Accept(sqlgen));
            if (parenthesiseArguments)
            {
                result.Append(")");
            }
            return result;
        }

        /// <summary>
        /// <see cref="HandleSpecialFunctionToOperator"></see>
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleConcatFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleSpecialFunctionToOperator(sqlgen, e, false);
        }

        /// <summary>
        /// <see cref="HandleSpecialFunctionToOperator"></see>
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionBitwise(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleSpecialFunctionToOperator(sqlgen, e, true);
        }

        /// <summary>
        /// Handles special case in which datapart 'type' parameter is present. all the functions
        /// handles here have *only* the 1st parameter as datepart. datepart value is passed along
        /// the QP as string and has to be expanded as TSQL keyword.
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleDatepartDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(e.Arguments.Count > 0, "e.Arguments.Count > 0");

            DbConstantExpression constExpr = e.Arguments[0] as DbConstantExpression;
            if (null == constExpr)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.SqlGen_InvalidDatePartArgumentExpression(e.Function.NamespaceName, e.Function.Name));
            }

            string datepart = constExpr.Value as string;
            if (null == datepart)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.SqlGen_InvalidDatePartArgumentExpression(e.Function.NamespaceName, e.Function.Name));
            }

            SqlBuilder result = new SqlBuilder();

            //
            // check if datepart value is valid
            //
            if (!_datepartKeywords.Contains(datepart))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.SqlGen_InvalidDatePartArgumentValue(datepart, e.Function.NamespaceName, e.Function.Name));
            }

            //
            // finaly, expand the function name
            //
            WriteFunctionName(result, e.Function);
            result.Append("(");

            // expand the datepart literal as tsql kword
            result.Append(datepart);
            string separator = ", ";

            // expand remaining arguments
            for (int i = 1; i < e.Arguments.Count; i++)
            {
                result.Append(separator);
                result.Append(e.Arguments[i].Accept(sqlgen));
            }

            result.Append(")");

            return result;
        }

        /// <summary>
        /// Handler for canonical functions for extracting date parts. 
        /// For example:
        ///     Year(date) -> DATEPART( year, date)
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleCanonicalFunctionDatepart(sqlgen, e.Function.Name.ToLowerInvariant(), e);
        }
    
        /// <summary>
        /// Handler for canonical funcitons for GetTotalOffsetMinutes.
        /// GetTotalOffsetMinutes(e) --> Datepart(tzoffset, e)
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns> 
        private static ISqlFragment HandleCanonicalFunctionGetTotalOffsetMinutes(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleCanonicalFunctionDatepart(sqlgen, "tzoffset", e);
        }

        /// <summary>
        /// Handler for turning a canonical function into DATEPART
        /// Results in DATEPART(datepart, e)
        /// </summary>
        /// <param name="datepart"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, string datepart, DbFunctionExpression e)
        {
            SqlBuilder result = new SqlBuilder();
            result.Append("DATEPART (");
            result.Append(datepart);
            result.Append(", ");

            Debug.Assert(e.Arguments.Count == 1, "Canonical datepart functions should have exactly one argument");
            result.Append(e.Arguments[0].Accept(sqlgen));

            result.Append(")");

            return result;
        }

        /// <summary>
        /// Handler for the canonical function CurrentDateTime
        /// For Sql8 and Sql9:  CurrentDateTime() -> GetDate()
        /// For Sql10:          CurrentDateTime() -> SysDateTime()
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionCurrentDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionGivenNameBasedOnVersion(sqlgen, e, "GetDate", "SysDateTime");
        }

        /// <summary>
        /// Handler for the canonical function CurrentUtcDateTime
        /// For Sql8 and Sql9:  CurrentUtcDateTime() -> GetUtcDate()
        /// For Sql10:          CurrentUtcDateTime() -> SysUtcDateTime()
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionCurrentUtcDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionGivenNameBasedOnVersion(sqlgen, e, "GetUtcDate", "SysUtcDateTime");
        }

        /// <summary>
        /// Handler for the canonical function CurrentDateTimeOffset
        /// For Sql8 and Sql9:  throw
        /// For Sql10: CurrentDateTimeOffset() -> SysDateTimeOffset()
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionCurrentDateTimeOffset(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            sqlgen.AssertKatmaiOrNewer(e);
            return HandleFunctionDefaultGivenName(sqlgen, e, "SysDateTimeOffset");
        }

        /// <summary>
        /// See <see cref="HandleCanonicalFunctionDateTimeTypeCreation"/> for exact translation
        /// Pre Katmai creates datetime.
        /// On Katmai creates datetime2.
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionCreateDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            string typeName = (sqlgen.IsPreKatmai) ? "datetime" : "datetime2";
            return HandleCanonicalFunctionDateTimeTypeCreation(sqlgen, typeName, e.Arguments, true, false);
        }

        /// <summary>
        /// See <see cref="HandleCanonicalFunctionDateTimeTypeCreation"/> for exact translation
        /// Pre Katmai not supported.
        /// On Katmai creates datetimeoffset.
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionCreateDateTimeOffset(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            sqlgen.AssertKatmaiOrNewer(e);
            return HandleCanonicalFunctionDateTimeTypeCreation(sqlgen, "datetimeoffset", e.Arguments, true, true);
        }

        /// <summary>
        /// See <see cref="HandleCanonicalFunctionDateTimeTypeCreation"/> for exact translation
        /// Pre Katmai not supported.
        /// On Katmai creates time.
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionCreateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            sqlgen.AssertKatmaiOrNewer(e);
            return HandleCanonicalFunctionDateTimeTypeCreation(sqlgen, "time", e.Arguments, false, false);
        }

        /// <summary>
        /// Helper for all date and time types creating functions. 
        /// 
        /// The given expression is in general trainslated into:
        /// 
        /// CONVERT(@typename, [datePart] + [timePart] + [timeZonePart], 121), where the datePart and the timeZonePart are optional
        /// 
        /// Only on Katmai, if a date part is present it is wrapped with a call for adding years as shown below.
        /// The individual parts are translated as:
        /// 
        /// Date part:  
        ///     PRE KATMAI: convert(varchar(255), @year) + '-' + convert(varchar(255), @month) + '-' + convert(varchar(255), @day)
        ///         KATMAI: DateAdd(year, @year-1, covert(@typename, '0001' + '-' + convert(varchar(255), @month) + '-' + convert(varchar(255), @day)  + [possibly time ], 121)     
        /// 
        /// Time part: 
        /// PRE KATMAI:  convert(varchar(255), @hour)+ ':' + convert(varchar(255), @minute)+ ':' + str(@second, 6, 3)
        ///     KATMAI:  convert(varchar(255), @hour)+ ':' + convert(varchar(255), @minute)+ ':' + str(@second, 10, 7)
        /// 
        /// Time zone part:
        ///     (case when @tzoffset >= 0 then '+' else '-' end) + convert(varchar(255), ABS(@tzoffset)/60) + ':' + convert(varchar(255), ABS(@tzoffset)%60) 
        /// 
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="args"></param>
        /// <param name="hasDatePart"></param>
        /// <param name="hasTimeZonePart"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionDateTimeTypeCreation(SqlGenerator sqlgen, string typeName, IList<DbExpression> args, bool hasDatePart, bool hasTimeZonePart)
        {
            Debug.Assert(args.Count == (hasDatePart ? 3 : 0) + 3 + (hasTimeZonePart ? 1 : 0), "Invalid number of parameters for a date time creating function");

            SqlBuilder result = new SqlBuilder();
            int currentArgumentIndex = 0;

            if (!sqlgen.IsPreKatmai && hasDatePart)
            {
                result.Append("DATEADD(year, ");
                sqlgen.ParenthesizeExpressionIfNeeded(args[currentArgumentIndex++], result);
                result.Append(" - 1, ");
            }
            
            result.Append("convert (");
            result.Append(typeName);
            result.Append(",");

            //Building the string representation
            if (hasDatePart)
            {
                //  YEAR:   PREKATMAI:               CONVERT(VARCHAR, @YEAR)
                //          KATMAI   :              '0001'
                if (!sqlgen.IsPreKatmai)
                {
                    result.Append("'0001'");
                }
                else
                {
                    AppendConvertToVarchar(sqlgen, result, args[currentArgumentIndex++]);
                }

                //  MONTH
                result.Append(" + '-' + ");
                AppendConvertToVarchar(sqlgen, result, args[currentArgumentIndex++]);
                
                //  DAY 
                result.Append(" + '-' + ");
                AppendConvertToVarchar(sqlgen, result, args[currentArgumentIndex++]);
                result.Append(" + ' ' + ");
            }
            
            //  HOUR
            AppendConvertToVarchar(sqlgen, result, args[currentArgumentIndex++]);

            // MINUTE
            result.Append(" + ':' + ");
            AppendConvertToVarchar(sqlgen, result, args[currentArgumentIndex++]);

            // SECOND
            result.Append(" + ':' + str(");
            result.Append(args[currentArgumentIndex++].Accept(sqlgen));

            if (sqlgen.IsPreKatmai)
            {
                result.Append(", 6, 3)");
            }
            else
            {
                result.Append(", 10, 7)");
            }

            //  TZOFFSET
            if (hasTimeZonePart)
            {
                result.Append(" + (CASE WHEN ");
                sqlgen.ParenthesizeExpressionIfNeeded(args[currentArgumentIndex], result);
                result.Append(" >= 0 THEN '+' ELSE '-' END) + convert(varchar(255), ABS(");
                sqlgen.ParenthesizeExpressionIfNeeded(args[currentArgumentIndex], result);
                result.Append("/60)) + ':' + convert(varchar(255), ABS(");
                sqlgen.ParenthesizeExpressionIfNeeded(args[currentArgumentIndex], result);
                result.Append("%60))");
            }

            result.Append(", 121)");

            if (!sqlgen.IsPreKatmai && hasDatePart)
            {
                result.Append(")");
            }
            return result;
        }

        /// <summary>
        /// Helper method that wrapps the given expession with a conver to varchar(255)
        /// </summary>
        /// <param name="result"></param>
        /// <param name="e"></param>
        private static void AppendConvertToVarchar(SqlGenerator sqlgen, SqlBuilder result, DbExpression e)
        {
            result.Append("convert(varchar(255), ");
            result.Append(e.Accept(sqlgen));
            result.Append(")");
        }
        
        /// <summary>
        /// TruncateTime(DateTime X) 
        ///   PreKatmai:    TRUNCATETIME(X) => CONVERT(DATETIME, CONVERT(VARCHAR(255), expression, 102),  102)
        ///      Katmai:    TRUNCATETIME(X) => CONVERT(DATETIME2, CONVERT(VARCHAR(255), expression, 102),  102)
        ///      
        /// TruncateTime(DateTimeOffset X) 
        ///                 TRUNCATETIME(X) => CONVERT(datetimeoffset, CONVERT(VARCHAR(255), expression,  102) 
        ///                                     + ' 00:00:00 ' +  Right(convert(varchar(255), @arg, 121), 6),  102)
        ///     
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionTruncateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            //The type that we need to return is based on the argument type.
            string typeName = null;
            bool isDateTimeOffset = false;
            
            PrimitiveTypeKind typeKind;
            bool isPrimitiveType = TypeHelpers.TryGetPrimitiveTypeKind(e.Arguments[0].ResultType, out typeKind);
            Debug.Assert(isPrimitiveType, "Expecting primitive type as input parameter to TruncateTime");

            if (typeKind == PrimitiveTypeKind.DateTime)
            {
                typeName = sqlgen.IsPreKatmai ? "datetime" : "datetime2";
            }
            else if (typeKind == PrimitiveTypeKind.DateTimeOffset)
            {
                typeName = "datetimeoffset";
                isDateTimeOffset = true;
            }
            else
            {
                Debug.Assert(true, "Unexpected type to TruncateTime" + typeKind.ToString());
            }

            SqlBuilder result = new SqlBuilder();
            result.Append("convert (");
            result.Append(typeName);
            result.Append(", convert(varchar(255), ");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", 102) ");

            if (isDateTimeOffset)
            {
                result.Append("+ ' 00:00:00 ' +  Right(convert(varchar(255), ");
                result.Append(e.Arguments[0].Accept(sqlgen));
                result.Append(", 121), 6)  ");
            }
     
            result.Append(",  102)");
            return result;
        }

        /// <summary>
        /// Handler for date addition functions supported only starting from Katmai
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionDateAddKatmaiOrNewer(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            sqlgen.AssertKatmaiOrNewer(e);
            return HandleCanonicalFunctionDateAdd(sqlgen, e);
        }

        /// <summary>
        /// Handler for all date/time addition canonical functions.
        /// Translation, e.g.
        /// AddYears(datetime, number) =>  DATEADD(year, number, datetime)
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionDateAdd(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append("DATEADD (");
            result.Append(_dateAddFunctionNameToDatepartDictionary[e.Function.Name]);
            result.Append(", ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(", ");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(")");

            return result;
        }

        /// <summary>
        /// Hanndler for date differencing functions supported only starting from Katmai
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionDateDiffKatmaiOrNewer(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            sqlgen.AssertKatmaiOrNewer(e);
            return HandleCanonicalFunctionDateDiff(sqlgen, e);
        }

        /// <summary>
        /// Handler for all date/time addition canonical functions.
        /// Translation, e.g.
        /// DiffYears(datetime, number) =>  DATEDIFF(year, number, datetime)
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionDateDiff(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append("DATEDIFF (");
            result.Append(_dateDiffFunctionNameToDatepartDictionary[e.Function.Name]);
            result.Append(", ");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(")");

            return result;
        }

        /// <summary>
        ///  Function rename IndexOf -> CHARINDEX
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionIndexOf(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionDefaultGivenName(sqlgen, e, "CHARINDEX");
        }

        /// <summary>
        ///  Function rename NewGuid -> NEWID
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionNewGuid(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionDefaultGivenName(sqlgen, e, "NEWID");
        }

        /// <summary>
        ///  Function rename Length -> LEN
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionLength(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            // We are aware of SQL Server's trimming of trailing spaces. We disclaim that behavior in general.
            // It's up to the user to decide whether to trim them explicitly or to append a non-blank space char explicitly.
            // Once SQL Server implements a function that computes Length correctly, we'll use it here instead of LEN,
            // and we'll drop the disclaimer. 
            return HandleFunctionDefaultGivenName(sqlgen, e, "LEN");
        }

        /// <summary>
        /// Round(numericExpression) -> Round(numericExpression, 0);
        /// Round(numericExpression, digits) -> Round(numericExpression, digits);
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionRound(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleCanonicalFunctionRoundOrTruncate(sqlgen, e, true);
        }

        /// <summary>
        /// Truncate(numericExpression) -> Round(numericExpression, 0, 1); (does not exist as canonical function yet)
        /// Truncate(numericExpression, digits) -> Round(numericExpression, digits, 1);
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionTruncate(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleCanonicalFunctionRoundOrTruncate(sqlgen, e, false);
        }

        /// <summary>
        /// Common handler for the canonical functions ROUND and TRUNCATE
        /// </summary>
        /// <param name="e"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionRoundOrTruncate(SqlGenerator sqlgen, DbFunctionExpression e, bool round)
        {
            SqlBuilder result = new SqlBuilder();

            // Do not add the cast for the Round() overload having two arguments. 
            // Round(Single,Int32) maps to Round(Double,Int32)due to implicit casting. 
            // We don't need to cast in that case, since the server returned type is same 
            // as the expected  type. Cast is only required for the overload - Round(Single)
            bool requiresCastToSingle = false;
            if (e.Arguments.Count == 1)
            {
                requiresCastToSingle = CastReturnTypeToSingle(e);
                if (requiresCastToSingle)
                {
                    result.Append(" CAST(");
                }
            }
            result.Append("ROUND(");

            Debug.Assert(e.Arguments.Count <= 2, "Round or truncate should have at most 2 arguments");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", ");
            
            if (e.Arguments.Count > 1)
            {
                result.Append(e.Arguments[1].Accept(sqlgen));
            }
            else
            {
                result.Append("0");
            }

            if (!round)
            {
                result.Append(", 1");
            }

            result.Append(")");
            
            if (requiresCastToSingle)
            {
                result.Append(" AS real)");
            }
            return result;
        }

        /// <summary>
        /// Handle the canonical function Abs(). 
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionAbs(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            // Convert the call to Abs(Byte) to a no-op, since Byte is an unsigned type. 
            if (TypeSemantics.IsPrimitiveType(e.Arguments[0].ResultType, PrimitiveTypeKind.Byte))
            {
                SqlBuilder result = new SqlBuilder();
                result.Append(e.Arguments[0].Accept(sqlgen));
                return result;
            }
            else
            {
                return HandleFunctionDefault(sqlgen, e);
            }
        }

        /// <summary>
        /// TRIM(string) -> LTRIM(RTRIM(string))
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionTrim(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            SqlBuilder result = new SqlBuilder();

            result.Append("LTRIM(RTRIM(");

            Debug.Assert(e.Arguments.Count == 1, "Trim should have one argument");
            result.Append(e.Arguments[0].Accept(sqlgen));

            result.Append("))");

            return result;
        }

        /// <summary>
        ///  Function rename ToLower -> LOWER
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionToLower(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionDefaultGivenName(sqlgen, e, "LOWER");
        }

        /// <summary>
        ///  Function rename ToUpper -> UPPER
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionToUpper(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleFunctionDefaultGivenName(sqlgen, e, "UPPER");
        }

        /// <summary>
        /// Function to translate the StartsWith, EndsWith and Contains canonical functions to LIKE expression in T-SQL
        /// and also add the trailing ESCAPE '~' when escaping of the search string for the LIKE expression has occurred
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="targetExpression"></param>
        /// <param name="constSearchParamExpression"></param>
        /// <param name="result"></param>
        /// <param name="insertPercentStart"></param>
        /// <param name="insertPercentEnd"></param>
        private static void TranslateConstantParameterForLike(SqlGenerator sqlgen, DbExpression targetExpression, DbConstantExpression constSearchParamExpression, SqlBuilder result, bool insertPercentStart, bool insertPercentEnd)
        {
            result.Append(targetExpression.Accept(sqlgen));
            result.Append(" LIKE ");

            // If it's a DbConstantExpression then escape the search parameter if necessary.
            bool escapingOccurred;

            StringBuilder searchParamBuilder = new StringBuilder();
            if (insertPercentStart == true)
                searchParamBuilder.Append("%");
            searchParamBuilder.Append(SqlProviderManifest.EscapeLikeText(constSearchParamExpression.Value as string, false,  out escapingOccurred));
            if (insertPercentEnd == true)
                searchParamBuilder.Append("%");

            DbConstantExpression escapedSearchParamExpression = new DbConstantExpression(constSearchParamExpression.ResultType, searchParamBuilder.ToString());
            result.Append(escapedSearchParamExpression.Accept(sqlgen));

            // If escaping did occur (special characters were found), then append the escape character used.
            if (escapingOccurred)
                result.Append(" ESCAPE '" + SqlProviderManifest.LikeEscapeChar + "'");
        }

        /// <summary>
        /// Handler for Contains. Wraps the normal translation with a case statement
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionContains(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return WrapPredicate( HandleCanonicalFunctionContains, sqlgen, e);
        }

        /// <summary>
        /// CONTAINS(arg0, arg1) => arg0 LIKE '%arg1%'
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static SqlBuilder HandleCanonicalFunctionContains(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
        {
            Debug.Assert(args.Count == 2, "Contains should have two arguments");
            // Check if args[1] is a DbConstantExpression
            DbConstantExpression constSearchParamExpression = args[1] as DbConstantExpression;
            if ((constSearchParamExpression != null) && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                TranslateConstantParameterForLike(sqlgen, args[0], constSearchParamExpression, result, true, true);
            }
            else
            {
                // We use CHARINDEX when the search param is a DbNullExpression because all of SQL Server 2008, 2005 and 2000
                // consistently return NULL as the result.
                //  However, if instead we use the optimized LIKE translation when the search param is a DbNullExpression,
                //  only SQL Server 2005 yields a True instead of a DbNull as compared to SQL Server 2008 and 2000. This is
                //  tracked in SQLBUDT #32315 in LIKE in SQL Server 2005.
                result.Append("CHARINDEX( ");
                result.Append(args[1].Accept(sqlgen));
                result.Append(", ");
                result.Append(args[0].Accept(sqlgen));
                result.Append(") > 0");
            }
            return result;
        }

        /// <summary>
        /// Handler for StartsWith. Wraps the normal translation with a case statement
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionStartsWith(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return WrapPredicate(HandleCanonicalFunctionStartsWith, sqlgen, e);
        }
        
        /// <summary>
        /// STARTSWITH(arg0, arg1) => arg0 LIKE 'arg1%'
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static SqlBuilder HandleCanonicalFunctionStartsWith(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
        {
            Debug.Assert(args.Count == 2, "StartsWith should have two arguments");
            // Check if args[1] is a DbConstantExpression
            DbConstantExpression constSearchParamExpression = args[1] as DbConstantExpression;
            if ((constSearchParamExpression != null) && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                TranslateConstantParameterForLike(sqlgen, args[0], constSearchParamExpression, result, false, true);
            }
            else
            {
                // We use CHARINDEX when the search param is a DbNullExpression because all of SQL Server 2008, 2005 and 2000
                // consistently return NULL as the result.
                //      However, if instead we use the optimized LIKE translation when the search param is a DbNullExpression,
                //      only SQL Server 2005 yields a True instead of a DbNull as compared to SQL Server 2008 and 2000. This is
                //      bug 32315 in LIKE in SQL Server 2005.
                result.Append("CHARINDEX( ");
                result.Append(args[1].Accept(sqlgen));
                result.Append(", ");
                result.Append(args[0].Accept(sqlgen));
                result.Append(") = 1");
            }

            return result;
        }

        /// <summary>
        /// Handler for EndsWith. Wraps the normal translation with a case statement
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment HandleCanonicalFunctionEndsWith(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return WrapPredicate(HandleCanonicalFunctionEndsWith, sqlgen, e);
        }

        /// <summary>
        /// ENDSWITH(arg0, arg1) => arg0 LIKE '%arg1'
        /// </summary>
        /// <param name="sqlgen"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static SqlBuilder HandleCanonicalFunctionEndsWith(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
        {
            Debug.Assert(args.Count == 2, "EndsWith should have two arguments");

            // Check if args[1] is a DbConstantExpression and if args [0] is a DbPropertyExpression
            DbConstantExpression constSearchParamExpression = args[1] as DbConstantExpression;
            DbPropertyExpression targetParamExpression = args[0] as DbPropertyExpression;
            if ((constSearchParamExpression != null) && (targetParamExpression != null) && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                // The LIKE optimization for EndsWith can only be used when the target is a column in table and
                // the search string is a constant. This is because SQL Server ignores a trailing space in a query like:
                // EndsWith('abcd ', 'cd'), which translates to:
                //      SELECT
                //      CASE WHEN ('abcd ' LIKE '%cd') THEN cast(1 as bit) WHEN ( NOT ('abcd ' LIKE '%cd')) THEN cast(0 as bit) END AS [C1]
                //      FROM ( SELECT 1 AS X ) AS [SingleRowTable1]
                // and "incorrectly" returns 1 (true), but the CLR would expect a 0 (false) back.

                TranslateConstantParameterForLike(sqlgen, args[0], constSearchParamExpression, result, true, false);
            }
            else
            {
                result.Append("CHARINDEX( REVERSE(");
                result.Append(args[1].Accept(sqlgen));
                result.Append("), REVERSE(");
                result.Append(args[0].Accept(sqlgen));
                result.Append(")) = 1");
            }
            return result;
        }

        /// <summary>
        /// Turns a predicate into a statement returning a bit
        /// PREDICATE => CASE WHEN (PREDICATE) THEN CAST(1 AS BIT) WHEN (NOT (PREDICATE)) CAST (O AS BIT) END
        /// The predicate is produced by the given predicateTranslator.
        /// </summary>
        /// <param name="predicateTranslator"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static ISqlFragment WrapPredicate(Func<SqlGenerator, IList<DbExpression>, SqlBuilder, SqlBuilder> predicateTranslator, SqlGenerator sqlgen, DbFunctionExpression e)
        {
            SqlBuilder result = new SqlBuilder();
            result.Append("CASE WHEN (");
            predicateTranslator(sqlgen, e.Arguments, result);
            result.Append(") THEN cast(1 as bit) WHEN ( NOT (");
            predicateTranslator(sqlgen, e.Arguments, result);
            result.Append(")) THEN cast(0 as bit) END");
            return result;
        }

        /// <summary>
        /// Writes the function name to the given SqlBuilder.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="result"></param>
        internal static void WriteFunctionName(SqlBuilder result, EdmFunction function)
        {
            string storeFunctionName;

            if (null != function.StoreFunctionNameAttribute)
            {
                storeFunctionName = function.StoreFunctionNameAttribute;
            }
            else
            {
                storeFunctionName = function.Name;
            }

            // If the function is a builtin (i.e. the BuiltIn attribute has been
            // specified, both store and canonical functions have this attribute), 
            // then the function name should not be quoted; 
            // additionally, no namespace should be used.
            if (TypeHelpers.IsCanonicalFunction(function))
            {
                result.Append(storeFunctionName.ToUpperInvariant());
            }
            else if (IsStoreFunction(function))
            {
                result.Append(storeFunctionName);
            }
            else
            {
                // Should we actually support this?
                if (String.IsNullOrEmpty(function.Schema))
                {
                    result.Append(SqlGenerator.QuoteIdentifier(function.NamespaceName));
                }
                else
                {
                    result.Append(SqlGenerator.QuoteIdentifier(function.Schema));
                }
                result.Append(".");
                result.Append(SqlGenerator.QuoteIdentifier(storeFunctionName));
            }
        }
        
                                                                              
        /// <summary>
        /// Is this a Store function (ie) does it have the builtinAttribute specified and it is not a canonical function?
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        internal static bool IsStoreFunction(EdmFunction function)
        {
            return function.BuiltInAttribute && !TypeHelpers.IsCanonicalFunction(function);
        }
                
        /// <summary>
        /// determines if the function requires the return type be enforeced by use of a cast expression
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool CastReturnTypeToInt64(DbFunctionExpression e)
        {
            return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToInt64, PrimitiveTypeKind.Int64);
        }

        /// <summary>
        /// determines if the function requires the return type be enforeced by use of a cast expression
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool CastReturnTypeToInt32(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            if (!_functionRequiresReturnTypeCastToInt32.Contains(e.Function.FullName))
            {
                return false;
            }

            for (int i = 0; i < e.Arguments.Count; i++)
            {
                TypeUsage storeType = sqlgen.StoreItemCollection.StoreProviderManifest.GetStoreType(e.Arguments[i].ResultType);
                if (_maxTypeNames.Contains(storeType.EdmType.Name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// determines if the function requires the return type be enforeced by use of a cast expression
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool CastReturnTypeToInt16(DbFunctionExpression e)
        {
            return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToInt16, PrimitiveTypeKind.Int16);
        }

        /// <summary>
        /// determines if the function requires the return type be enforeced by use of a cast expression
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool CastReturnTypeToSingle(DbFunctionExpression e)
        {
            //Do not add the cast for the Round() overload having 2 arguments. 
            //Round(Single,Int32) maps to Round(Double,Int32)due to implicit casting. 
            //We don't need to cast in that case, since we expect a Double as return type there anyways.
            return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToSingle, PrimitiveTypeKind.Single);
        }

        /// <summary>
        /// Determines if the function requires the return type be enforced by use of a cast expression
        /// </summary>
        /// <param name="e"></param>
        /// <param name="functionsRequiringReturnTypeCast"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool CastReturnTypeToGivenType(DbFunctionExpression e, Set<string> functionsRequiringReturnTypeCast, PrimitiveTypeKind type)
        {
            if (!functionsRequiringReturnTypeCast.Contains(e.Function.FullName))
            {
                return false;
            }

            for (int i = 0; i < e.Arguments.Count; i++)
            {
                if (TypeSemantics.IsPrimitiveType(e.Arguments[i].ResultType, type))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

