//---------------------------------------------------------------------
// <copyright file="Utils.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Xml;

    /// <summary>
    /// Summary description for Utils.
    /// </summary>
    // make class internal when friend assemblies are available
    internal static class Utils
    {
        #region Static Fields


        // this is what we should be doing for CDM schemas
        // the RegEx for valid identifiers are taken from the C# Language Specification (2.4.2 Identifiers)
        // (except that we exclude _ as a valid starting character).
        // This results in a somewhat smaller set of identifier from what System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier
        // allows. Not all identifiers allowed by IsValidLanguageIndependentIdentifier are valid in C#.IsValidLanguageIndependentIdentifier allows:
        //    Mn, Mc, and Pc as a leading character (which the spec and C# (at least for some Mn and Mc characters) do not allow)
        //    characters that Char.GetUnicodeCategory says are in Nl and Cf but which the RegEx does not accept (and which C# does allow).
        //
        // we could create the StartCharacterExp and OtherCharacterExp dynamically to force inclusion of the missing Nl and Cf characters...
        private const string StartCharacterExp = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\p{Nl}]";
        private const string OtherCharacterExp = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]";
        private const string NameExp = StartCharacterExp+OtherCharacterExp+"{0,}";
        //private static Regex ValidDottedName=new Regex(@"^"+NameExp+@"(\."+NameExp+@"){0,}$",RegexOptions.Singleline);
        private static Regex UndottedNameValidator = new Regex(@"^"+NameExp+@"$",RegexOptions.Singleline | RegexOptions.Compiled );
        #endregion

        #region Static Methods

        internal static void ExtractNamespaceAndName(SchemaDataModelOption dataModel, string qualifiedTypeName, out string namespaceName, out string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(qualifiedTypeName), "qualifiedTypeName parameter is null");
            GetBeforeAndAfterLastPeriod(qualifiedTypeName, out namespaceName, out name); 
        }


        internal static string ExtractTypeName(SchemaDataModelOption dataModel, string qualifiedTypeName)
        {
            Debug.Assert(!string.IsNullOrEmpty(qualifiedTypeName), "qualifiedTypeName parameter is null or empty");
            return GetEverythingAfterLastPeriod(qualifiedTypeName);
        }

        private static void GetBeforeAndAfterLastPeriod(string qualifiedTypeName, out string before, out string after)
        {
            int lastDot = qualifiedTypeName.LastIndexOf('.');
            if (lastDot < 0)
            {
                before = null;
                after = qualifiedTypeName;
            }
            else
            {
                before = qualifiedTypeName.Substring(0, lastDot);
                after = qualifiedTypeName.Substring(lastDot + 1);
            }
        }
        internal static string GetEverythingBeforeLastPeriod(string qualifiedTypeName)
        {
            int lastDot = qualifiedTypeName.LastIndexOf('.');
            if (lastDot < 0)
                return null;
            return qualifiedTypeName.Substring(0, lastDot);
        }

        private static string GetEverythingAfterLastPeriod(string qualifiedTypeName)
        {
            int lastDot = qualifiedTypeName.LastIndexOf('.');
            if (lastDot < 0)
                return qualifiedTypeName;

            return qualifiedTypeName.Substring(lastDot + 1);
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="reader"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetString(Schema schema, XmlReader reader, out string value)
        {
            Debug.Assert(schema != null, "schema parameter is null");
            Debug.Assert(reader != null, "reader parameter is null");

            if (reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid)
            {
                // an error has already been issued by the xsd validation
                value = null;
                return false;
            }

            value = reader.Value;

            if ( string.IsNullOrEmpty(value) )
            {
                schema.AddError( ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.InvalidName(value, reader.Name));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetDottedName(Schema schema, XmlReader reader,out string name)
        {
            if (!GetString(schema, reader, out name))
            {
                return false;
            }
            
            return ValidateDottedName(schema, reader, name);
        }

        internal static bool ValidateDottedName(Schema schema, XmlReader reader, string name)
        {
            Debug.Assert(schema != null, "schema parameter is null");
            Debug.Assert(reader != null, "reader parameter is null");
            Debug.Assert(!string.IsNullOrEmpty(name), "name parameter is null or empty");
            Debug.Assert(reader.SchemaInfo.Validity != System.Xml.Schema.XmlSchemaValidity.Invalid, "This method should not be called when the schema is invalid");

            if (schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                // each part of the dotted name needs to be a valid name
                foreach (string namePart in name.Split('.'))
                {
                    if (!ValidUndottedName(namePart))
                    {
                        schema.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                            System.Data.Entity.Strings.InvalidName(name, reader.Name));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GetUndottedName(Schema schema,XmlReader reader,out string name)
        {
            Debug.Assert(schema != null, "schema parameter is null");
            Debug.Assert(reader != null, "reader parameter is null");

            if (reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid)
            {
                // the xsd already put in an error
                name = null;
                return false;
            }

            name = reader.Value;
            if (string.IsNullOrEmpty(name))
            {
                schema.AddError( ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.EmptyName(reader.Name));
                return false;
            }

            if (schema.DataModel == SchemaDataModelOption.EntityDataModel &&
                !ValidUndottedName(name) )
            {
                schema.AddError( ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.InvalidName(name,reader.Name));
                return false;
            }

            Debug.Assert(!(schema.DataModel == SchemaDataModelOption.EntityDataModel && name.IndexOf('.') >= 0),
                string.Format(CultureInfo.CurrentCulture, "{1} ({0}) is not valid. {1} cannot be qualified.", name, reader.Name));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static bool ValidUndottedName(string name)
        {
            // CodeGenerator.IsValidLanguageIndependentIdentifier does demand a FullTrust Link
            // but this is safe since the function only walks over the string no risk is introduced
            return !string.IsNullOrEmpty(name) && UndottedNameValidator.IsMatch(name) 
                && IsValidLanguageIndependentIdentifier(name);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        [System.Security.SecuritySafeCritical]
        private static bool IsValidLanguageIndependentIdentifier(string name)
        {
            return System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="reader"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetBool(Schema schema, XmlReader reader, out bool value)
        {
            Debug.Assert(schema != null, "schema parameter is null");
            Debug.Assert(reader != null, "reader parameter is null");

            if ( reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid )
            {
                value = true; // we have to set the value to something before returning.
                return false;
            }

            // do this in a try catch, just in case the attribute wasn't validated against an xsd:boolean
            try
            {
                value = reader.ReadContentAsBoolean();
                return true;
            }
            catch (System.Xml.XmlException)
            {
                // we already handled the valid and invalid cases, so it must be NotKnown now.
                Debug.Assert(reader.SchemaInfo.Validity == Xml.Schema.XmlSchemaValidity.NotKnown, "The schema validity must be NotKnown at this point");
                schema.AddError(ErrorCode.BoolValueExpected, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.ValueNotUnderstood(reader.Value, reader.Name));
            }
            
            value = true; // we have to set the value to something before returning.
            return false;
        }

        public static bool GetInt(Schema schema,XmlReader reader,out int value)
        {
            Debug.Assert(schema != null, "schema parameter is null");
            Debug.Assert(reader != null, "reader parameter is null");

            if (reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid)
            {
                // an error has already been issued by the xsd validation
                value = 0; ;
                return false;
            }

            string text = reader.Value;
            value = int.MinValue;

            if ( int.TryParse(text,NumberStyles.Integer,System.Globalization.CultureInfo.InvariantCulture,out value) )
                return true;

            schema.AddError( ErrorCode.IntegerExpected, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.ValueNotUnderstood(reader.Value,reader.Name));
            return false;
        }

        public static bool GetByte(Schema schema,XmlReader reader,out byte value)
        {
            Debug.Assert(schema != null, "schema parameter is null");
            Debug.Assert(reader != null, "reader parameter is null");

            if (reader.SchemaInfo.Validity == System.Xml.Schema.XmlSchemaValidity.Invalid)
            {
                // an error has already been issued by the xsd validation
                value = 0; ;
                return false;
            }

            string text = reader.Value;
            value = byte.MinValue;

            if (byte.TryParse(text, NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            schema.AddError( ErrorCode.ByteValueExpected, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.ValueNotUnderstood(reader.Value, reader.Name));

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhsName"></param>
        /// <param name="rhsName"></param>
        /// <returns></returns>
        public static int CompareNames(string lhsName, string rhsName)
        {
            return string.Compare(lhsName,rhsName,StringComparison.Ordinal);
        }


        #endregion
    }
}
