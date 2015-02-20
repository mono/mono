namespace System.Workflow.ComponentModel.Compiler
{
    using System.Xml;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Xml.Serialization;
    using System.Text.RegularExpressions;

    internal sealed class WorkflowCompilerConfigurationSectionGroup : ConfigurationSectionGroup
    {
        public WorkflowCompilerConfigurationSectionGroup()
        {
        }
    }

    internal sealed class AuthorizedTypesSectionHandler : IConfigurationSectionHandler
    {
        const string TargetFxVersionAttribute = "version";
        #region IConfigurationSectionHandler Members

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            Dictionary<string, IList<AuthorizedType>> authorizedTypes = new Dictionary<string, IList<AuthorizedType>>();

            XmlAttributeOverrides authorizedTypeOverrides = new XmlAttributeOverrides();
            XmlAttributes authorizedTypeAttributes = new XmlAttributes();
            authorizedTypeAttributes.XmlRoot = new XmlRootAttribute("authorizedType");
            authorizedTypeOverrides.Add(typeof(AuthorizedType), authorizedTypeAttributes);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AuthorizedType), authorizedTypeOverrides);
            foreach (XmlNode targetFx in section.ChildNodes)
            {
                XmlAttribute versionAttribute = targetFx.Attributes.GetNamedItem(TargetFxVersionAttribute) as XmlAttribute;
                if (versionAttribute != null)
                {
                    string version = versionAttribute.Value;
                    if (!string.IsNullOrEmpty(version))
                    {
                        IList<AuthorizedType> versionTypes;
                        if (!authorizedTypes.TryGetValue(version, out versionTypes))
                        {
                            versionTypes = new List<AuthorizedType>();
                            authorizedTypes.Add(version, versionTypes);
                        }
                        foreach (XmlNode authorizedTypeNode in targetFx.ChildNodes)
                        {
                            AuthorizedType authorizedType = xmlSerializer.Deserialize(new XmlNodeReader(authorizedTypeNode)) as AuthorizedType;
                            if (authorizedType != null)
                            {
                                versionTypes.Add(authorizedType);
                            }
                        }
                    }
                }
            }
            return authorizedTypes;
        }

        #endregion
    }
    
    [XmlType("authorizedType")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class AuthorizedType
    {
        private string assemblyName;
        private string namespaceName;
        private string typeName;
        private bool isAuthorized;
        private Regex regex;

        [XmlAttribute]
        public string Assembly
        {
            get
            {
                return this.assemblyName;
            }
            set
            {
                this.assemblyName = value;
            }
        }

        [XmlAttribute]
        public string Namespace
        {
            get
            {
                return this.namespaceName;
            }

            set
            {
                this.namespaceName = value;
            }
        }

        [XmlAttribute]
        public string TypeName
        {
            get
            {
                return this.typeName;
            }

            set
            {
                this.typeName = value;
            }
        }

        [XmlAttribute]
        public string Authorized
        {
            get
            {
                return this.isAuthorized.ToString();
            }

            set
            {
                this.isAuthorized = bool.Parse(value);
            }
        }

        [XmlIgnore]
        public Regex RegularExpression
        {
            get
            {
                if (this.regex == null)
                {
                    this.regex = new Regex(MakeRegex(string.Format(CultureInfo.InvariantCulture, "{0}.{1}, {2}", new object[] { this.namespaceName, this.typeName, this.assemblyName })), RegexOptions.Compiled);
                    return this.regex;
                }
                return this.regex;
            }
        }

        private static string MakeRegex(string inputString)
        {
            // RegEx uses the following as meta characters:
            // [\^$.|?*+()
            // Of these we translate * and ? to DOS wildcard equivalents in RegEx. 
            // We escape rest of the Regex meta characters to thwart any luring 
            // attacks caused by malformed inputString using meta characters.
            string outputString = inputString.Replace(@"\", @"\\");
            outputString = outputString.Replace("[", @"\[");
            outputString = outputString.Replace("^", @"\^");
            outputString = outputString.Replace("$", @"\$");
            outputString = outputString.Replace("|", @"\|");
            outputString = outputString.Replace("+", @"\+");
            outputString = outputString.Replace("(", @"\(");
            outputString = outputString.Replace(")", @"\)");
            outputString = outputString.Replace(".", @"\x2E");

            outputString = outputString.Replace("*", @"[\w\x60\x2E]{0,}");
            outputString = outputString.Replace("?", @"\w{1,1}");

            // Make whitespaces optional
            outputString = outputString.Replace(" ", @"\s{0,}");
            return outputString;
        }
    }
}

