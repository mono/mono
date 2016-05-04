//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Security;
    using System.Reflection;

    static class JsonGlobals
    {
        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        public static readonly int DataContractXsdBaseNamespaceLength = Globals.DataContractXsdBaseNamespace.Length;

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        public static readonly XmlDictionaryString dDictionaryString = new XmlDictionary().Add("d");

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        public static readonly char[] floatingPointCharacters = new char[] { '.', 'e' };

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        public static readonly XmlDictionaryString itemDictionaryString = new XmlDictionary().Add("item");

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        public static readonly XmlDictionaryString rootDictionaryString = new XmlDictionary().Add("root");

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        public static readonly long unixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public const string applicationJsonMediaType = "application/json";
        public const string arrayString = "array";
        public const string booleanString = "boolean";
        public const string CacheControlString = "Cache-Control";
        public const byte CollectionByte = (byte)CollectionChar;
        public const char CollectionChar = '[';
        public const string DateTimeEndGuardReader = ")/";
        public const string DateTimeEndGuardWriter = ")\\/";
        public const string DateTimeStartGuardReader = "/Date(";
        public const string DateTimeStartGuardWriter = "\\/Date(";
        public const string dString = "d";
        public const byte EndCollectionByte = (byte)EndCollectionChar;
        public const char EndCollectionChar = ']';
        public const byte EndObjectByte = (byte)EndObjectChar;
        public const char EndObjectChar = '}';
        public const string ExpiresString = "Expires";
        public const string IfModifiedSinceString = "If-Modified-Since";
        public const string itemString = "item";
        public const string jsonerrorString = "jsonerror";
        public const string KeyString = "Key";
        public const string LastModifiedString = "Last-Modified";
        public const int maxScopeSize = 25;
        public const byte MemberSeparatorByte = (byte)MemberSeparatorChar;
        public const char MemberSeparatorChar = ',';
        public const byte NameValueSeparatorByte = (byte)NameValueSeparatorChar;
        public const char NameValueSeparatorChar = ':';
        public const string NameValueSeparatorString = ":";
        public const string nullString = "null";
        public const string numberString = "number";
        public const byte ObjectByte = (byte)ObjectChar;
        public const char ObjectChar = '{';
        public const string objectString = "object";
        public const string publicString = "public";
        public const byte QuoteByte = (byte)QuoteChar;
        public const char QuoteChar = '"';
        public const string rootString = "root";
        public const string serverTypeString = "__type";
        public const string stringString = "string";
        public const string textJsonMediaType = "text/json";
        public const string trueString = "true";
        public const string typeString = "type";
        public const string ValueString = "Value";
        public const char WhitespaceChar = ' ';
        public const string xmlnsPrefix = "xmlns";
        public const string xmlPrefix = "xml";
    }
}
