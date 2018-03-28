using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Permissions;
using System.IO;
using System.Xml;
using System.Security;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Configuration
{
    // This class is used to read the <uri> section from a config file directly, without using System.Configuration
    internal class UriSectionReader
    {
        private const string rootElementName = "configuration";

        private string configFilePath;
        private XmlReader reader;

        // result data after parsing the configuration section
        private UriSectionData sectionData;

        private UriSectionReader(string configFilePath, UriSectionData parentData)
        {
            Debug.Assert(configFilePath != null, "'configFilePath' must not be null");

            this.configFilePath = configFilePath;
            this.sectionData = new UriSectionData();

            if (parentData != null)
            {
                sectionData.IriParsing = parentData.IriParsing;
                sectionData.IdnScope = parentData.IdnScope;

                foreach (KeyValuePair<string, SchemeSettingInternal> schemeSetting in parentData.SchemeSettings)
                {
                    sectionData.SchemeSettings.Add(schemeSetting.Key, schemeSetting.Value);
                }
            }
        }

        public static UriSectionData Read(string configFilePath)
        {
            return Read(configFilePath, null);
        }

        public static UriSectionData Read(string configFilePath, UriSectionData parentData)
        {
            UriSectionReader reader = new UriSectionReader(configFilePath, parentData);
            return reader.GetSectionData();
        }

        [SuppressMessage("Microsoft.Security","CA2106:SecureAsserts", Justification="Must Assert read permission for only this file")]
        [SuppressMessage("Microsoft.Security","CA2103:ReviewImperativeSecurity", Justification="Must Assert read permission for only this file")]
        private UriSectionData GetSectionData()
        {
            // Assert read permission for only this file.
            new FileIOPermission(FileIOPermissionAccess.Read, configFilePath).Assert();
            try
            {
                if (File.Exists(configFilePath))
                {
                    using (FileStream configFile = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
                    {
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreComments = true;
                        settings.IgnoreWhitespace = true;
                        settings.IgnoreProcessingInstructions = true;
                        using (reader = XmlReader.Create(configFile, settings))
                        {
                            if (ReadConfiguration())
                            {
                                return sectionData;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // In this case we really want to catch all exceptions: Uri never threw, therefore we can't
                // start throwing exceptions now when reading the configuration.
            }
            finally
            {
                FileIOPermission.RevertAssert();
            }

            return null;
        }

        private bool ReadConfiguration()
        {
            if (!ReadToUriSection())
            {
                return false;
            }

            while (reader.Read())
            {
                if (IsEndElement(CommonConfigurationStrings.UriSectionName))
                {
                    return true;
                }

                if (reader.NodeType != XmlNodeType.Element)
                {
                    return false;
                }

                string currentElementName = reader.Name;

                if (AreEqual(currentElementName, CommonConfigurationStrings.IriParsing))
                {
                    if (ReadIriParsing())
                    {
                        continue;
                    }
                }
                else if (AreEqual(currentElementName, CommonConfigurationStrings.Idn))
                {
                    if (ReadIdnScope())
                    {
                        continue;
                    }
                }
                else if (AreEqual(currentElementName, CommonConfigurationStrings.SchemeSettings))
                {
                    if (ReadSchemeSettings())
                    {
                        continue;
                    }
                }

                // we found an unknown element in <uri> section.
                return false;
            }

            // we reached EOF, but the <uri> node didn't have a final </uri> node
            return false;
        }

        private bool ReadIriParsing()
        {
            string attributeValue = reader.GetAttribute(CommonConfigurationStrings.Enabled);

            bool configIriParsingValue;
            if (bool.TryParse(attributeValue, out configIriParsingValue))
            {
                sectionData.IriParsing = configIriParsingValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ReadIdnScope()
        {
            string attributeValue = reader.GetAttribute(CommonConfigurationStrings.Enabled);

            try
            {
                sectionData.IdnScope = (UriIdnScope)Enum.Parse(typeof(UriIdnScope), attributeValue, true);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private bool ReadSchemeSettings()
        {
            while (reader.Read())
            {
                if (IsEndElement(CommonConfigurationStrings.SchemeSettings))
                {
                    return true;
                }

                if (reader.NodeType != XmlNodeType.Element)
                {
                    return false;
                }

                string currentElementName = reader.Name;

                if (AreEqual(currentElementName, SchemeSettingElementCollection.AddItemName))
                {
                    if (ReadAddSchemeSetting())
                    {
                        continue;
                    }
                }
                else if (AreEqual(currentElementName, SchemeSettingElementCollection.RemoveItemName))
                {
                    if (ReadRemoveSchemeSetting())
                    {
                        continue;
                    }
                }
                else if (AreEqual(currentElementName, SchemeSettingElementCollection.ClearItemsName))
                {
                    ClearSchemeSetting();
                    continue;
                }

                // unknown element found, e.g. if </schemeSettings> is missing, reading </uri> will end up here.
                return false;
            }

            // reached EOF without hitting </schemeSettings>
            return false;
        }

        private static bool AreEqual(string value1, string value2)
        {
            return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private bool ReadAddSchemeSetting()
        {
            string schemeValue = reader.GetAttribute(CommonConfigurationStrings.SchemeName);
            string genericUriParserOptionsValue = reader.GetAttribute(
                CommonConfigurationStrings.GenericUriParserOptions);

            if (string.IsNullOrEmpty(schemeValue) || string.IsNullOrEmpty(genericUriParserOptionsValue))
            {
                return false;
            }

            try
            {
                GenericUriParserOptions genericUriParserOptions = (GenericUriParserOptions)Enum.Parse(
                    typeof(GenericUriParserOptions), genericUriParserOptionsValue);

                SchemeSettingInternal schemeSetting = new SchemeSettingInternal(schemeValue,
                    genericUriParserOptions);

                sectionData.SchemeSettings[schemeSetting.Name] = schemeSetting;
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private bool ReadRemoveSchemeSetting()
        {
            string scheme = reader.GetAttribute(CommonConfigurationStrings.SchemeName);

            if (string.IsNullOrEmpty(scheme))
            {
                return false;
            }

            // ignore result value. It's ok if the scheme is not in the collection.
            sectionData.SchemeSettings.Remove(scheme);
            return true;
        }

        private void ClearSchemeSetting()
        {
            // no attributes to read, just clear collection
            sectionData.SchemeSettings.Clear();
        }

        private bool IsEndElement(string elementName)
        {
            return (reader.NodeType == XmlNodeType.EndElement) &&
                string.Compare(reader.Name, elementName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private bool ReadToUriSection()
        {
            if (!reader.ReadToFollowing(rootElementName))
            {
                return false;
            }

            if (reader.Depth != 0)
            {
                // 'configuration' must be the root element. If not, this is not a valid config file.
                return false;
            }

            // To be entirely correct, we should not look for <uri> section, but we should look for the name
            // specified in the <configSections>/<section> node for type 'System.Configuration.Uri'.
            // An admin/user can decide to rename the uri section to whatever he wants. However, since
            // all NCL (and some other) configuration implementations look for the hard-coded string,
            // we don't change the current behavior.
            do
            {
                // 'uri' must be at depth 1, otherwise it is not our <uri> section, but a custom one.
                if (!reader.ReadToFollowing(CommonConfigurationStrings.UriSectionName))
                {
                    return false;
                }

            } while (reader.Depth != 1);

            return true;
        }
    }
}
