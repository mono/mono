//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Text;
    using System.Xml;
    using SR2 = System.ServiceModel.Discovery.SR;

    static class SerializationUtility
    {
        static char[] whiteSpaceChars = new char[] { ' ', '\t', '\n', '\r' };

        public static XmlQualifiedName ParseQName(string prefixedQName, XmlReader reader)
        {
            Fx.Assert(prefixedQName != null, "The prefixedQName must be non null.");
            Fx.Assert(reader != null, "The reader must be non null.");
            int index = prefixedQName.IndexOf(':');

            string ns;
            string localname;
            if (index != -1)
            {
                string prefix = prefixedQName.Substring(0, index);
                ns = reader.LookupNamespace(prefix);
                if (ns == null)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlQNamePrefixNotDefined(prefix, prefixedQName)));
                }
                localname = prefixedQName.Substring(index + 1);
                if (localname == string.Empty)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlQNameLocalnameNotDefined(prefixedQName)));
                }
            }
            else
            {
                ns = string.Empty;
                localname = prefixedQName;
            }

            localname = XmlConvert.DecodeName(localname);
            return new XmlQualifiedName(localname, ns);
        }

        public static void ParseQNameList(string listOfQNamesAsString, Collection<XmlQualifiedName> qNameCollection, XmlReader reader)
        {
            Fx.Assert(listOfQNamesAsString != null, "The listOfQNamesAsString must be non null.");
            Fx.Assert(qNameCollection != null, "The qNameCollection must be non null.");
            Fx.Assert(reader != null, "The reader must be non null.");

            string[] prefixedQNames = listOfQNamesAsString.Split(whiteSpaceChars, StringSplitOptions.RemoveEmptyEntries);
            if (prefixedQNames.Length > 0)
            {
                for (int i = 0; i < prefixedQNames.Length; i++)
                {
                    qNameCollection.Add(ParseQName(prefixedQNames[i], reader));
                }
            }
        }

        public static void ParseUriList(string listOfUrisAsString, Collection<Uri> uriCollection, UriKind uriKind)
        {
            Fx.Assert(listOfUrisAsString != null, "The listOfUrisAsString must be non null.");
            Fx.Assert(uriCollection != null, "The uriCollection must be non null.");

            string[] uriStrings = listOfUrisAsString.Split(whiteSpaceChars, StringSplitOptions.RemoveEmptyEntries);
            if (uriStrings.Length > 0)
            {
                for (int i = 0; i < uriStrings.Length; i++)
                {
                    try
                    {
                        uriCollection.Add(new Uri(uriStrings[i], uriKind));
                    }
                    catch (FormatException fe)
                    {
                        if (uriKind == UriKind.Absolute)
                        {
                            throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlAbsoluteUriFormatError(uriStrings[i]), fe));
                        }
                        else
                        {
                            throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlUriFormatError(uriStrings[i]), fe));
                        }
                    }
                }
            }
        }

        public static long ReadUInt(string uintString, string notFoundExceptionString, string exceptionString)
        {
            long result;

            if (uintString == null)
            {
                throw FxTrace.Exception.AsError(new XmlException(notFoundExceptionString));
            }

            try
            {
                result = XmlConvert.ToUInt32(uintString);
            }
            catch (FormatException fe)
            {
                throw FxTrace.Exception.AsError(new XmlException(exceptionString, fe));
            }
            catch (OverflowException oe)
            {
                throw FxTrace.Exception.AsError(new XmlException(exceptionString, oe));
            }

            return result;
        }

        static void PrepareQNameString(StringBuilder listOfQNamesString, ref bool emptyNsDeclared, ref int prefixCount, XmlWriter writer, XmlQualifiedName qname)
        {
            string prefix;
            string encodedLocalName = XmlConvert.EncodeLocalName(qname.Name.Trim());
            if (qname.Namespace.Length == 0)
            {
                if (!emptyNsDeclared)
                {
                    writer.WriteAttributeString("xmlns", string.Empty, null, string.Empty);
                    emptyNsDeclared = true;
                }

                prefix = null;
            }
            else
            {
                prefix = writer.LookupPrefix(qname.Namespace);
                if (prefix == null)
                {
                    prefix = "dp" + prefixCount++;
                    writer.WriteAttributeString("xmlns", prefix, null, qname.Namespace);
                }
            }

            if (!string.IsNullOrEmpty(prefix))
            {
                listOfQNamesString.AppendFormat(CultureInfo.InvariantCulture, "{0}:{1}", prefix, encodedLocalName);
            }
            else
            {
                listOfQNamesString.AppendFormat(CultureInfo.InvariantCulture, "{0}", encodedLocalName);
            }
        }

        public static void WriteQName(XmlWriter writer, XmlQualifiedName qname)
        {
            Fx.Assert(writer != null, "The writer must be non null.");
            Fx.Assert(qname != null, "The qnames must be non null.");

            StringBuilder qNameString = new StringBuilder();
            int prefixCount = 0;
            bool emptyNsDeclared = false;
            PrepareQNameString(qNameString, ref emptyNsDeclared, ref prefixCount, writer, qname);
            writer.WriteString(qNameString.ToString());
        }

        public static void WriteListOfQNames(XmlWriter writer, Collection<XmlQualifiedName> qnames)
        {
            Fx.Assert(writer != null, "The writer must be non null.");
            Fx.Assert(qnames != null, "The qnames must be non null.");

            int prefixCount = 0;
            bool emptyNsDeclared = false;
            StringBuilder listOfQNamesString = new StringBuilder();
            foreach (XmlQualifiedName qname in qnames)
            {
                if (listOfQNamesString.Length != 0)
                {
                    listOfQNamesString.Append(' ');
                }
                PrepareQNameString(listOfQNamesString, ref emptyNsDeclared, ref prefixCount, writer, qname);
            }
            writer.WriteString(listOfQNamesString.ToString());
        }

        public static void WriteListOfUris(XmlWriter writer, Collection<Uri> uris)
        {
            Fx.Assert(writer != null, "The writer must be non null.");
            Fx.Assert(uris != null, "The uris must be non null.");

            if (uris.Count > 0)
            {
                for (int i = 0; i < uris.Count - 1; i++)
                {
                    writer.WriteString(uris[i].GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
                    writer.WriteWhitespace(" ");
                }

                writer.WriteString(uris[uris.Count - 1].GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
            }
        }

        public static int ReadMaxResults(XmlReader reader)
        {
            int maxResults = int.MaxValue;
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.ReadStartElement();
                maxResults = reader.ReadContentAsInt();
                if (maxResults <= 0)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlMaxResultsLessThanZero(maxResults)));
                }
                reader.ReadEndElement();
            }
            return maxResults;
        }

        public static TimeSpan ReadDuration(XmlReader reader)
        {
            TimeSpan timeout = TimeSpan.MaxValue;
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.ReadStartElement();
                string timeoutString = reader.ReadString();
                timeout = SerializationUtility.ReadTimespan(timeoutString, SR2.DiscoveryXmlDurationDeserializationError(timeoutString));
                if (timeout <= TimeSpan.Zero)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlDurationLessThanZero(timeout)));;
                }
                reader.ReadEndElement();
            }
            return timeout;
        }

        public static TimeSpan ReadTimespan(string timespanString, string exceptionString)
        {
            TimeSpan result;

            try
            {
                result = XmlConvert.ToTimeSpan(timespanString);
            }
            catch (FormatException fe)
            {
                throw FxTrace.Exception.AsError(new XmlException(exceptionString, fe));
            }
            catch (OverflowException oe)
            {
                throw FxTrace.Exception.AsError(new XmlException(exceptionString, oe));
            }

            return result;
        }

        public static EndpointAddress ReadEndpointAddress(DiscoveryVersion discoveryVersion, XmlReader reader)
        {
            Fx.Assert(discoveryVersion != null, "The discoveryVersion must be non null");
            Fx.Assert(reader != null, "The reader must be non null");

            if (discoveryVersion == DiscoveryVersion.WSDiscoveryApril2005 || discoveryVersion == DiscoveryVersion.WSDiscoveryCD1)
            {
                EndpointAddressAugust2004 endpoint = discoveryVersion.Implementation.EprSerializer.ReadObject(reader) as EndpointAddressAugust2004;
                if (endpoint == null)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlEndpointNull));
                }
                return endpoint.ToEndpointAddress();
            }
            else if (discoveryVersion == DiscoveryVersion.WSDiscovery11)
            {
                EndpointAddress10 endpoint = discoveryVersion.Implementation.EprSerializer.ReadObject(reader) as EndpointAddress10;
                if (endpoint == null)
                {
                    throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlEndpointNull));
                }
                return endpoint.ToEndpointAddress();
            }
            else
            {
                Fx.Assert("The discoveryVersion parameter cannot be null.");
                return null;
            }
        }

        public static void ReadContractTypeNames(Collection<XmlQualifiedName> contractTypeNames, XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {                                
                reader.ReadStartElement();

                string listOfQNamesAsStr = reader.ReadString();
                if (!string.IsNullOrEmpty(listOfQNamesAsStr))
                {             
                    SerializationUtility.ParseQNameList(listOfQNamesAsStr, contractTypeNames, reader);
                }

                reader.ReadEndElement();
            }
        }        

        public static Uri ReadScopes(Collection<Uri> scopes, XmlReader reader)
        {
            Uri scopeMatchBy = null;
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    if ((reader.NamespaceURI.Length == 0) &&
                        (reader.Name.Equals(ProtocolStrings.SchemaNames.MatchByAttribute)))
                    {
                        string scopeMatchByStr = reader.Value;
                        try
                        {
                            scopeMatchBy = new Uri(scopeMatchByStr, UriKind.RelativeOrAbsolute);
                        }
                        catch (FormatException fe)
                        {
                            throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlUriFormatError(scopeMatchByStr), fe));
                        }
                        break;
                    }
                }

                reader.MoveToElement();
            }

            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.ReadStartElement();

                string listOfUrisAsString = reader.ReadString();
                if (!string.IsNullOrEmpty(listOfUrisAsString))
                {
                    SerializationUtility.ParseUriList(listOfUrisAsString, scopes, UriKind.Absolute);
                }

                reader.ReadEndElement();
            }

            return scopeMatchBy;
        }

        public static void ReadListenUris(Collection<Uri> listenUris, XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.Read();
            }
            else
            {
                reader.ReadStartElement();

                string listOfUrisAsString = reader.ReadString();
                if (!string.IsNullOrEmpty(listOfUrisAsString))
                {
                    SerializationUtility.ParseUriList(listOfUrisAsString, listenUris, UriKind.RelativeOrAbsolute);
                }

                reader.ReadEndElement();
            }
        }

        public static int ReadMetadataVersion(XmlReader reader)
        {
            reader.ReadStartElement();

            int metadataVersion = reader.ReadContentAsInt();

            if (metadataVersion < 0)
            {
                throw FxTrace.Exception.AsError(new XmlException(SR2.DiscoveryXmlMetadataVersionLessThanZero(metadataVersion)));
            }

            reader.ReadEndElement();

            return metadataVersion;
        }

        public static void WriteEndPointAddress(DiscoveryVersion discoveryVersion, EndpointAddress endpointAddress, XmlWriter writer)
        {
            Fx.Assert(discoveryVersion != null, "The discoveryVersion must be non null");
            Fx.Assert(writer != null, "The writer must be non null");

            if (discoveryVersion == DiscoveryVersion.WSDiscoveryApril2005 || discoveryVersion == DiscoveryVersion.WSDiscoveryCD1)
            {
                EndpointAddressAugust2004 endpoint = EndpointAddressAugust2004.FromEndpointAddress(endpointAddress);
                discoveryVersion.Implementation.EprSerializer.WriteObject(writer, endpoint);
            }
            else if (discoveryVersion == DiscoveryVersion.WSDiscovery11)
            {
                EndpointAddress10 endpoint = EndpointAddress10.FromEndpointAddress(endpointAddress);
                discoveryVersion.Implementation.EprSerializer.WriteObject(writer, endpoint);
            }
            else
            {
                Fx.Assert("The discoveryVersion parameter cannot be null.");
            }   
        }

        public static void WriteContractTypeNames(DiscoveryVersion discoveryVersion, Collection<XmlQualifiedName> contractTypeNames, XmlWriter writer)
        {
            if ((contractTypeNames != null) && (contractTypeNames.Count > 0))
            {
                // using the prefix here allows us to redefine the empty namespace 
                // for serializing the QNames is required.
                writer.WriteStartElement(
                    ProtocolStrings.SchemaNames.DefaultPrefix, 
                    ProtocolStrings.SchemaNames.TypesElement, 
                    discoveryVersion.Namespace);
                SerializationUtility.WriteListOfQNames(writer, contractTypeNames);
                writer.WriteEndElement();
            }
        }

        public static void WriteScopes(DiscoveryVersion discoveryVersion, Collection<Uri> scopes, Uri scopeMatchBy, XmlWriter writer)
        {
            bool writeScopes = true;
            if (scopes == null || scopes.Count == 0)
            {
                // If there are no scopes defined, we write the Scopes section only if matchBy contract is None.
                writeScopes = (scopeMatchBy == FindCriteria.ScopeMatchByNone);
            }

            if (writeScopes)
            {
                writer.WriteStartElement(ProtocolStrings.SchemaNames.ScopesElement, discoveryVersion.Namespace);
                if (scopeMatchBy != null)
                {
                    Uri versionDependentScopeMatchBy = discoveryVersion.Implementation.ToVersionDependentScopeMatchBy(scopeMatchBy);
                    writer.WriteAttributeString(
                        ProtocolStrings.SchemaNames.MatchByAttribute, 
                        string.Empty,
                        versionDependentScopeMatchBy.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
                }

                if (scopes != null)
                {
                    SerializationUtility.WriteListOfUris(writer, scopes);
                }

                writer.WriteEndElement();
            }
        }

        public static void WriteListenUris(DiscoveryVersion discoveryVersion, Collection<Uri> listenUris, XmlWriter writer)
        {
            if ((listenUris != null) && (listenUris.Count > 0))
            {
                writer.WriteStartElement(ProtocolStrings.SchemaNames.XAddrsElement, discoveryVersion.Namespace);
                SerializationUtility.WriteListOfUris(writer, listenUris);
                writer.WriteEndElement();
            }
        }

        public static void WriteMetadataVersion(DiscoveryVersion discoveryVersion, int metadataVersion, XmlWriter writer)
        {
            writer.WriteStartElement(ProtocolStrings.SchemaNames.MetadataVersionElement, discoveryVersion.Namespace);
            writer.WriteValue(metadataVersion);
            writer.WriteEndElement();
        }
    }
}
