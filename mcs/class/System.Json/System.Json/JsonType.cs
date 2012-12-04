// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Json
{
    /// <summary>
    /// An enumeration that specifies primitive and structured JavaScript Object 
    /// Notation (JSON) common language runtime (CLR) types.
    /// </summary>
    public enum JsonType
    {
        /// <summary>
        /// Specifies the JSON string CLR type.
        /// </summary>
        String,

        /// <summary>
        /// Specifies the JSON number CLR type.
        /// </summary>
        Number,

        /// <summary>
        /// Specifies the JSON object CLR type that consists of an unordered collection
        /// of key/value pairs, where the key is of type String and the value is of
        /// type <see cref="System.Json.JsonValue"/>, which can, in turn, be either a
        /// primitive or a structured JSON type.
        /// </summary>
        Object,

        /// <summary>
        /// Specifies the JSON array CLR type that consists of an ordered collection of
        /// <see cref="System.Json.JsonValue"/>types, which can, in turn, be either
        /// primitive or structured JSON types.
        /// </summary>
        Array,

        /// <summary>
        /// Specifies the JSON Boolean CLR type.
        /// </summary>
        Boolean,

        /// <summary>
        /// Specifies the type returned by calls to <see cref="System.Json.JsonValue.ValueOrDefault(string)"/>
        /// or <see cref="System.Json.JsonValue.ValueOrDefault(int)"/>
        /// when the element searches doesn't exist in the JSON collection. This is a special
        /// value which does not represent any JSON element, and cannot be added to any
        /// JSON collections.
        /// </summary>
        Default
    }
}
