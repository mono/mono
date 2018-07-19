//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;

    [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
        + " data from being modified or leaked to other components in appdomain."
        + " Changes to static fields could affect serialization/deserialization; should be reviewed.")]
#if USE_REFEMIT
    public static class DictionaryGlobals
#else
    internal static class DictionaryGlobals
#endif
    {
        public readonly static XmlDictionaryString EmptyString;
        public readonly static XmlDictionaryString SchemaInstanceNamespace;
        public readonly static XmlDictionaryString SchemaNamespace;
        public readonly static XmlDictionaryString SerializationNamespace;
        public readonly static XmlDictionaryString XmlnsNamespace;
        public readonly static XmlDictionaryString XsiTypeLocalName;
        public readonly static XmlDictionaryString XsiNilLocalName;
        public readonly static XmlDictionaryString ClrTypeLocalName;
        public readonly static XmlDictionaryString ClrAssemblyLocalName;
        public readonly static XmlDictionaryString ArraySizeLocalName;
        public readonly static XmlDictionaryString IdLocalName;
        public readonly static XmlDictionaryString RefLocalName;
        public readonly static XmlDictionaryString ISerializableFactoryTypeLocalName;
        public readonly static XmlDictionaryString CharLocalName;
        public readonly static XmlDictionaryString BooleanLocalName;
        public readonly static XmlDictionaryString SignedByteLocalName;
        public readonly static XmlDictionaryString UnsignedByteLocalName;
        public readonly static XmlDictionaryString ShortLocalName;
        public readonly static XmlDictionaryString UnsignedShortLocalName;
        public readonly static XmlDictionaryString IntLocalName;
        public readonly static XmlDictionaryString UnsignedIntLocalName;
        public readonly static XmlDictionaryString LongLocalName;
        public readonly static XmlDictionaryString UnsignedLongLocalName;
        public readonly static XmlDictionaryString FloatLocalName;
        public readonly static XmlDictionaryString DoubleLocalName;
        public readonly static XmlDictionaryString DecimalLocalName;
        public readonly static XmlDictionaryString DateTimeLocalName;
        public readonly static XmlDictionaryString StringLocalName;
        public readonly static XmlDictionaryString ByteArrayLocalName;
        public readonly static XmlDictionaryString ObjectLocalName;
        public readonly static XmlDictionaryString TimeSpanLocalName;
        public readonly static XmlDictionaryString GuidLocalName;
        public readonly static XmlDictionaryString UriLocalName;
        public readonly static XmlDictionaryString QNameLocalName;
        public readonly static XmlDictionaryString Space;

        public readonly static XmlDictionaryString timeLocalName;
        public readonly static XmlDictionaryString dateLocalName;
        public readonly static XmlDictionaryString hexBinaryLocalName;
        public readonly static XmlDictionaryString gYearMonthLocalName;
        public readonly static XmlDictionaryString gYearLocalName;
        public readonly static XmlDictionaryString gMonthDayLocalName;
        public readonly static XmlDictionaryString gDayLocalName;
        public readonly static XmlDictionaryString gMonthLocalName;
        public readonly static XmlDictionaryString integerLocalName;
        public readonly static XmlDictionaryString positiveIntegerLocalName;
        public readonly static XmlDictionaryString negativeIntegerLocalName;
        public readonly static XmlDictionaryString nonPositiveIntegerLocalName;
        public readonly static XmlDictionaryString nonNegativeIntegerLocalName;
        public readonly static XmlDictionaryString normalizedStringLocalName;
        public readonly static XmlDictionaryString tokenLocalName;
        public readonly static XmlDictionaryString languageLocalName;
        public readonly static XmlDictionaryString NameLocalName;
        public readonly static XmlDictionaryString NCNameLocalName;
        public readonly static XmlDictionaryString XSDIDLocalName;
        public readonly static XmlDictionaryString IDREFLocalName;
        public readonly static XmlDictionaryString IDREFSLocalName;
        public readonly static XmlDictionaryString ENTITYLocalName;
        public readonly static XmlDictionaryString ENTITIESLocalName;
        public readonly static XmlDictionaryString NMTOKENLocalName;
        public readonly static XmlDictionaryString NMTOKENSLocalName;
        public readonly static XmlDictionaryString AsmxTypesNamespace;

        static DictionaryGlobals()
        {
            // Update array size when adding new strings or templates
            XmlDictionary dictionary = new XmlDictionary(61);

            try
            {
                // 0
                SchemaInstanceNamespace = dictionary.Add(Globals.SchemaInstanceNamespace);
                SerializationNamespace = dictionary.Add(Globals.SerializationNamespace);
                SchemaNamespace = dictionary.Add(Globals.SchemaNamespace);
                XsiTypeLocalName = dictionary.Add(Globals.XsiTypeLocalName);
                XsiNilLocalName = dictionary.Add(Globals.XsiNilLocalName);

                // 5
                IdLocalName = dictionary.Add(Globals.IdLocalName);
                RefLocalName = dictionary.Add(Globals.RefLocalName);
                ArraySizeLocalName = dictionary.Add(Globals.ArraySizeLocalName);
                EmptyString = dictionary.Add(String.Empty);
                ISerializableFactoryTypeLocalName = dictionary.Add(Globals.ISerializableFactoryTypeLocalName);

                // 10
                XmlnsNamespace = dictionary.Add(Globals.XmlnsNamespace);
                CharLocalName = dictionary.Add("char");
                BooleanLocalName = dictionary.Add("boolean");
                SignedByteLocalName = dictionary.Add("byte");
                UnsignedByteLocalName = dictionary.Add("unsignedByte");

                // 15
                ShortLocalName = dictionary.Add("short");
                UnsignedShortLocalName = dictionary.Add("unsignedShort");
                IntLocalName = dictionary.Add("int");
                UnsignedIntLocalName = dictionary.Add("unsignedInt");
                LongLocalName = dictionary.Add("long");

                // 20
                UnsignedLongLocalName = dictionary.Add("unsignedLong");
                FloatLocalName = dictionary.Add("float");
                DoubleLocalName = dictionary.Add("double");
                DecimalLocalName = dictionary.Add("decimal");
                DateTimeLocalName = dictionary.Add("dateTime");

                // 25
                StringLocalName = dictionary.Add("string");
                ByteArrayLocalName = dictionary.Add("base64Binary");
                ObjectLocalName = dictionary.Add("anyType");
                TimeSpanLocalName = dictionary.Add("duration");
                GuidLocalName = dictionary.Add("guid");

                // 30
                UriLocalName = dictionary.Add("anyURI");
                QNameLocalName = dictionary.Add("QName");
                ClrTypeLocalName = dictionary.Add(Globals.ClrTypeLocalName);
                ClrAssemblyLocalName = dictionary.Add(Globals.ClrAssemblyLocalName);
                Space = dictionary.Add(Globals.Space);

                // 35
                timeLocalName = dictionary.Add("time");
                dateLocalName = dictionary.Add("date");
                hexBinaryLocalName = dictionary.Add("hexBinary");
                gYearMonthLocalName = dictionary.Add("gYearMonth");
                gYearLocalName = dictionary.Add("gYear");

                // 40
                gMonthDayLocalName = dictionary.Add("gMonthDay");
                gDayLocalName = dictionary.Add("gDay");
                gMonthLocalName = dictionary.Add("gMonth");
                integerLocalName = dictionary.Add("integer");
                positiveIntegerLocalName = dictionary.Add("positiveInteger");

                // 45
                negativeIntegerLocalName = dictionary.Add("negativeInteger");
                nonPositiveIntegerLocalName = dictionary.Add("nonPositiveInteger");
                nonNegativeIntegerLocalName = dictionary.Add("nonNegativeInteger");
                normalizedStringLocalName = dictionary.Add("normalizedString");
                tokenLocalName = dictionary.Add("token");

                // 50
                languageLocalName = dictionary.Add("language");
                NameLocalName = dictionary.Add("Name");
                NCNameLocalName = dictionary.Add("NCName");
                XSDIDLocalName = dictionary.Add("ID");
                IDREFLocalName = dictionary.Add("IDREF");

                // 55
                IDREFSLocalName = dictionary.Add("IDREFS");
                ENTITYLocalName = dictionary.Add("ENTITY");
                ENTITIESLocalName = dictionary.Add("ENTITIES");
                NMTOKENLocalName = dictionary.Add("NMTOKEN");
                NMTOKENSLocalName = dictionary.Add("NMTOKENS");

                // 60
                AsmxTypesNamespace = dictionary.Add("http://microsoft.com/wsdl/types/");

                // Add new templates here
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(ex.Message, ex);
            }
        }

    }
}

