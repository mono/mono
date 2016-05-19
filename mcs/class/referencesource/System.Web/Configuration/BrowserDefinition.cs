//------------------------------------------------------------------------------
// <copyright file="BrowserDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Globalization;

    //
    //
    //    <browsers>
    //    <browser id="XXX" parentID="YYY">
    //         <identification>
    //              <userAgent match="xxx" />
    //              <header name="HTTP_X_JPHONE_DISPLAY" match="xxx" />
    //              <capability name="majorVersion" match="^6$" />
    //         </identification>
    //         <capture>
    //              <header name="HTTP_X_UP_DEVCAP_NUMSOFTKEYS" match="?'softkeys'\d+)" />
    //         </capture>
    //         <capabilities>
    //              <mobileDeviceManufacturer>OpenWave</mobileDeviceManufacturer>
    //              <numberOfSoftKeys>$(softkeys)</numberOfSoftKeys>
    //         </capabilities>
    //         <controlAdapters>
    //              <adapter controlType="System.Web.UI.WebControls.Image"
    //                       adapterType="System.Web.UI.WebControls.Adapters.Html32ImageAdapter" />
    //         </controlAdapters>
    //    </browser>
    //    </browsers>

    internal class BrowserDefinition {

        internal static string MakeValidTypeNameFromString(string s) {
            if (s == null) {
                return s;
            }

            s = s.ToLower(CultureInfo.InvariantCulture);
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<s.Length; i++) {
                // To be CLS-compliant (CS3008), public method name cannot starts with _{digit}
                if (i == 0) {
                    if(Char.IsDigit(s[0])) {
                        sb.Append("N");
                    }
                    else if(Char.IsLetter(s[0])) {
                        sb.Append(s.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture));
                        continue;
                    }
                }

                if (Char.IsLetterOrDigit(s[i]) || s[i] == '_') {
                    sb.Append(s[i]);
                }
                else {
                    //
                    sb.Append('A');
                }
            }
            return sb.ToString();
        }

        // _idHeaderChecks are the header name and match string for <identification>
        private ArrayList _idHeaderChecks;
        //_idCapabilityChecks are the capability name and match string for <identification>
        private ArrayList _idCapabilityChecks;
        //_captureHeaderChecks are the header name and match string for <capture>
        private ArrayList _captureHeaderChecks;
        //_captureCapabilityChecks are the capability name and match string for <capture>
        private ArrayList _captureCapabilityChecks;
        private AdapterDictionary _adapters;
        private string _id;
        private string _parentID;
        private string _name;
        private string _parentName;
        private NameValueCollection _capabilities;
        private BrowserDefinitionCollection _browsers;
        private BrowserDefinitionCollection _gateways;
        private BrowserDefinitionCollection _refBrowsers;
        private BrowserDefinitionCollection _refGateways;
        private XmlNode _node;
        private bool _isRefID = false;
        private bool _isDeviceNode;
        private bool _isDefaultBrowser;
        private string _htmlTextWriterString;
        private int _depth = 0;

        internal BrowserDefinition(XmlNode node) : this(node, false) {
        }

        internal BrowserDefinition(XmlNode node, bool isDefaultBrowser) {
            if (node == null)
                throw new ArgumentNullException("node");

            _capabilities = new NameValueCollection();
            _idHeaderChecks = new ArrayList();
            _idCapabilityChecks = new ArrayList();
            _captureHeaderChecks = new ArrayList();
            _captureCapabilityChecks = new ArrayList();
            _adapters = new AdapterDictionary();
            _browsers = new BrowserDefinitionCollection();
            _gateways = new BrowserDefinitionCollection();
            _refBrowsers = new BrowserDefinitionCollection();
            _refGateways = new BrowserDefinitionCollection();
            _node = node;
            _isDefaultBrowser = isDefaultBrowser;

            string refID = null;

            HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "id", ref _id);
            HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "refID", ref refID);

            if((refID != null) && (_id != null)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Browser_mutually_exclusive_attributes, "id", "refID"), node);
            }

            if (_id != null) {
                if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(_id)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Browser_InvalidID, "id", _id), node);
                }
            }
            else {
                if (refID == null) {
                    if (this is GatewayDefinition) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_attributes_required, "gateway", "refID", "id"), node);
                    }

                    throw new ConfigurationErrorsException(SR.GetString(SR.Browser_attributes_required, "browser", "refID", "id"), node);
                }
                else {
                    if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(refID)) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_InvalidID, "refID", refID), node);
                    }
                }

                _parentID = refID;
                _isRefID = true;
                _id = refID;

                if (this is GatewayDefinition) {
                    _name = "refgatewayid$";
                }
                else {
                    _name = "refbrowserid$";
                }

                String parentID = null;
                HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "parentID", ref parentID);
                if ((parentID != null) && (parentID.Length != 0)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Browser_mutually_exclusive_attributes, "parentID", "refID"), node);
                }
            }

            _name = MakeValidTypeNameFromString(_id + _name);

            if(!_isRefID) {
                // Not a default browser definition
                if (!("Default".Equals(_id))) {
                    HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "parentID", ref _parentID);
                }
                // Make sure parentID is not specified on default browser
                else {
                    HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "parentID", ref _parentID);
                    if (_parentID != null)
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Browser_parentID_applied_to_default), node);
                }
            }

            _parentName = MakeValidTypeNameFromString(_parentID);

            if(_id.IndexOf(" ", StringComparison.Ordinal) != -1) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Space_attribute, "id " + _id), node);
            }

            foreach(XmlNode child in node.ChildNodes) {
                if(child.NodeType != XmlNodeType.Element) {
                    continue;
                }

                switch (child.Name) {
                    case "identification":
                        // refID nodes do not allow <identification>
                        if (_isRefID) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Browser_refid_prohibits_identification), node);
                        }

                        this.ProcessIdentificationNode(child, BrowserCapsElementType.Identification);
                        break;

                    case "capture":
                        this.ProcessCaptureNode(child, BrowserCapsElementType.Capture);
                        break;

                    case "capabilities":
                        this.ProcessCapabilitiesNode(child);
                        break;

                    case "controlAdapters":
                        this.ProcessControlAdaptersNode(child);
                        break;

                    case "sampleHeaders":
                        break;

                    default:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Browser_invalid_element, child.Name), node);
                }
            }
        }

        public bool IsDefaultBrowser {
            get {
                return _isDefaultBrowser;
            }
        }

        public BrowserDefinitionCollection Browsers {
            get {
                return _browsers;
            }
        }

        public BrowserDefinitionCollection RefBrowsers {
            get {
                return _refBrowsers;
            }
        }

        public BrowserDefinitionCollection RefGateways {
            get {
                return _refGateways;
            }
        }

        public BrowserDefinitionCollection Gateways {
            get {
                return _gateways;
            }
        }

        public string ID {
            get {
                return _id;
            }
        }

        public string Name {
            get {
                return _name;
            }
        }

        public string ParentName {
            get {
                return _parentName;
            }
        }

        // Indicate whether this node represents a real device, ie. all ancestor nodes are browser deinitions.
        internal bool IsDeviceNode {
            get {
                return _isDeviceNode;
            }
            set {
                _isDeviceNode = value;
            }
        }

        internal int Depth {
            get {
                return _depth;
            }
            set {
                _depth = value;
            }
        }

        public string ParentID {
            get {
                return _parentID;
            }
        }

        internal bool IsRefID {
            get {
                return _isRefID;
            }
        }

        public NameValueCollection Capabilities {
            get {
                return _capabilities;
            }
        }

        public ArrayList IdHeaderChecks {
            get {
                return _idHeaderChecks;
            }
        }

        public ArrayList CaptureHeaderChecks {
            get {
                return _captureHeaderChecks;
            }
        }

        public ArrayList IdCapabilityChecks {
            get {
                return _idCapabilityChecks;
            }
        }

        public ArrayList CaptureCapabilityChecks {
            get {
                return _captureCapabilityChecks;
            }
        }

        public AdapterDictionary Adapters {
            get {
                return _adapters;
            }
        }

        internal XmlNode XmlNode {
            get {
                return _node;
            }
        }

        public string HtmlTextWriterString {
            get {
                return _htmlTextWriterString;
            }
        }

        private void DisallowNonMatchAttribute(XmlNode node) {
            string check = null;
            HandlerBase.GetAndRemoveStringAttribute(node, "nonMatch", ref check);
            if(check != null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Browser_mutually_exclusive_attributes, "match", "nonMatch"), node);
            }
        }

        private void HandleMissingMatchAndNonMatchError(XmlNode node) {
            throw new ConfigurationErrorsException(
                SR.GetString(SR.Missing_required_attributes, "match", "nonMatch", node.Name),
                node);
        }

        /*
        /* 
        <identification>
        <userAgent match="xxx" />
        <header name="HTTP_X_JPHONE_DISPLAY" match="xxx" />
        <capability name="majorVersion" match="^6$" />
        </identification>
        <capture>
        <header name="HTTP_X_UP_DEVCAP_NUMSOFTKEYS" match="?'softkeys'\d+)" />
        </capture>
        */
        internal void ProcessIdentificationNode(XmlNode node, BrowserCapsElementType elementType) {
            string match = null;
            string header = null;
            bool nonMatch;
            bool emptyIdentification = true;

            foreach(XmlNode child in node.ChildNodes) {
                match = String.Empty;
                nonMatch = false;
                if(child.NodeType != XmlNodeType.Element) {
                    continue;
                }

                switch (child.Name) {
                    case "userAgent":
                        emptyIdentification = false;

                        //match the user agent
                        HandlerBase.GetAndRemoveNonEmptyStringAttribute(child, "match", ref match);
                        if (String.IsNullOrEmpty(match)) {
                            HandlerBase.GetAndRemoveNonEmptyStringAttribute(child, "nonMatch", ref match);

                            if (String.IsNullOrEmpty(match)) {
                                HandleMissingMatchAndNonMatchError(child);
                            }

                            nonMatch = true;
                        }
                        _idHeaderChecks.Add(new CheckPair("User-Agent", match, nonMatch));
                        if (nonMatch == false) {
                            DisallowNonMatchAttribute(child);
                        }

                        break;

                    case "header":
                        emptyIdentification = false;

                        //match some arbitrary header
                        HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "name", ref header);
                        HandlerBase.GetAndRemoveNonEmptyStringAttribute(child, "match", ref match);
                        if (String.IsNullOrEmpty(match)) {
                            HandlerBase.GetAndRemoveNonEmptyStringAttribute(child, "nonMatch", ref match);

                            if (String.IsNullOrEmpty(match)) {
                                HandleMissingMatchAndNonMatchError(child);
                            }

                            nonMatch = true;
                        }
                        _idHeaderChecks.Add(new CheckPair(header, match, nonMatch));
                        if (nonMatch == false) {
                            DisallowNonMatchAttribute(child);
                        }
                        break;

                    case "capability":
                        emptyIdentification = false;

                        //match against an already set capability
                        HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "name", ref header);
                        HandlerBase.GetAndRemoveNonEmptyStringAttribute(child, "match", ref match);
                        if (String.IsNullOrEmpty(match)) {
                            HandlerBase.GetAndRemoveNonEmptyStringAttribute(child, "nonMatch", ref match);

                            if (String.IsNullOrEmpty(match)) {
                                HandleMissingMatchAndNonMatchError(child);
                            }

                            nonMatch = true;
                        }
                        _idCapabilityChecks.Add(new CheckPair(header, match, nonMatch));
                        //verify that match and nonMatch are not both specified
                        if (nonMatch == false) {
                            DisallowNonMatchAttribute(child);
                        }
                        break;
                    default:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_invalid_element, child.ToString()), child);
                }
            }

            if (emptyIdentification) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Browser_empty_identification), node);
            }

            return;
        }

        internal void ProcessCaptureNode(XmlNode node, BrowserCapsElementType elementType) {
            string match = null;
            string header = null;
            foreach(XmlNode child in node.ChildNodes) {
                if(child.NodeType != XmlNodeType.Element) {
                    continue;
                }
                switch(child.Name) {
                case "userAgent":
                    //match the user agent
                    HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "match", ref match);
                    _captureHeaderChecks.Add(new CheckPair("User-Agent", match));
                    break;
                case "header":
                    //match some arbitrary header
                    HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "name", ref header);
                    HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "match", ref match);
                    _captureHeaderChecks.Add(new CheckPair(header, match));
                    break;
                case "capability":
                    //match against an already set capability
                    HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "name", ref header);
                    HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "match", ref match);
                    _captureCapabilityChecks.Add(new CheckPair(header, match));
                    break;
                default:
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_invalid_element, child.ToString()), child);
                }
            }
            return;
        }


        /*
        <capabilities>
             <capability name="mobileDeviceManufacturer" value="OpenWave"</capability>
             <capability name="numberOfSoftKeys" value="$(softkeys)"</capability>
        </capabilities>
        */

        internal void ProcessCapabilitiesNode(XmlNode node) {
            foreach(XmlNode child in node.ChildNodes) {
                if(child.NodeType != XmlNodeType.Element) {
                    continue;
                }
                if (child.Name != "capability") {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_element), child);
                }
                string capabilityName = null;
                string capabilityValue = null;
                HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "name", ref capabilityName);
                HandlerBase.GetAndRemoveRequiredStringAttribute(child, "value", ref capabilityValue);
                _capabilities[capabilityName] = capabilityValue;
            }
            return;
        }

        /*
        <controlAdapters>
             <adapter controlType="System.Web.UI.WebControls.Image"
                      adapterType="System.Web.UI.WebControls.Adapters.Html32ImageAdapter" />
        </controlAdapters>
        */
        internal void ProcessControlAdaptersNode(XmlNode node) {
            HandlerBase.GetAndRemoveStringAttribute(node, "markupTextWriterType", ref _htmlTextWriterString);
            foreach(XmlNode child in node.ChildNodes) {
                if(child.NodeType != XmlNodeType.Element) {
                    continue;
                }
                if(child.Name != "adapter") {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_element), child);
                }
                XmlAttributeCollection nodeAttributes = child.Attributes;
                string controlString = null;
                string adapterString = null;
                HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(child, "controlType", ref controlString);
                HandlerBase.GetAndRemoveRequiredStringAttribute(child, "adapterType", ref adapterString);

                Type type = CheckType(controlString, typeof(Control), child);

                // Normalize control type name
                controlString = type.AssemblyQualifiedName;

                if (!String.IsNullOrEmpty(adapterString)) {
                    CheckType(adapterString, typeof(System.Web.UI.Adapters.ControlAdapter), child);
                }

                _adapters[controlString] = adapterString;
            }
            return;
        }

        private static Type CheckType(string typeName, Type baseType, XmlNode child) {
            // Use BuildManager to verify control types.
            // Note for machine level browser files, this will only check assemblies in GAC.
            Type type = ConfigUtil.GetType(typeName, child, true /*ignoreCase*/);

            if (!baseType.IsAssignableFrom(type)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Type_doesnt_inherit_from_type, typeName, 
                        baseType.FullName), child);
            }

            if (!HttpRuntime.IsTypeAllowedInConfig(type)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Type_from_untrusted_assembly, typeName), child);
            }

            return type;
        }

        internal void MergeWithDefinition(BrowserDefinition definition) {
            Debug.Assert(definition.IsRefID);

            // Copy the capabilities
            foreach (String key in definition.Capabilities.Keys) {
                this._capabilities[key] = definition.Capabilities[key];
            }

            // Copy the adapter definition
            foreach (String key in definition.Adapters.Keys) {
                this._adapters[key] = definition.Adapters[key];
            }

            this._htmlTextWriterString = definition.HtmlTextWriterString;
        }
    }
}
