//------------------------------------------------------------------------------
// <copyright file="WebConfigManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls.Util
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.IO;
    using System.Windows.Forms;
    using System.Web.UI.MobileControls;
    using System.Web.UI.Design;
    using System.Web.UI.Design.MobileControls;
    using System.Xml;
    using SR = System.Web.UI.Design.MobileControls.SR;    

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class WebConfigManager
    {
        private readonly String _path = null;
        private ISite _site = null;
        private XmlDocument _document = null;

        private WebConfigManager()
        {
        }

        internal WebConfigManager(ISite site)
        {
            Debug.Assert(site != null);
            _site = site;

            IWebApplication webApplicationService = (IWebApplication)_site.GetService(typeof(IWebApplication));
            if (webApplicationService != null) {
                IProjectItem dataFileProjectItem = webApplicationService.GetProjectItemFromUrl("~/web.config");
                if (dataFileProjectItem != null) {
                    _path = dataFileProjectItem.PhysicalPath;
                }
            }

/* VSWhidbey 271075, 257678
            // the following inspired by:
            // \VSDesigner\Designer\Microsoft\VisualStudio\Designer\Serialization\BaseDesignerLoader.cs

            Type projectItemType = Type.GetType("EnvDTE.ProjectItem, " + AssemblyRef.EnvDTE);
            if (projectItemType != null)
            {
                Object currentProjItem = _site.GetService(projectItemType);
                PropertyInfo containingProjectProp = projectItemType.GetProperty("ContainingProject");
                Object dteProject = containingProjectProp.GetValue(currentProjItem, new Object[0]);
                Type projectType = Type.GetType("EnvDTE.Project, " + AssemblyRef.EnvDTE);
                PropertyInfo fullNameProperty = projectType.GetProperty("FullName");
                String projectPath = (String)fullNameProperty.GetValue(dteProject, new Object[0]);

                _path = Path.GetDirectoryName(projectPath) + "\\web.config";
            }
*/
        }

        internal XmlDocument Document
        {
            get
            {
                if (_document == null)
                {
                    _document = new XmlDocument();
                }

                return _document;
            }
        }

        private void LoadDocument()
        {
            try
            {
                Document.Load(_path);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                throw new Exception(SR.GetString(SR.WebConfig_FileLoadException));
            }
        }

        internal void EnsureWebConfigCheckedOut()
        {
            DesignerUtility.EnsureFileCheckedOut(_site, _path);
        }

        internal void EnsureWebConfigIsPresent()
        {
            if(!File.Exists(_path))
            {
                // We throw our own exception type so we can easily tell
                // between a corrupt web.config and a missing one.
                throw new FileNotFoundException(
                    SR.GetString(SR.WebConfig_FileNotFoundException)
                );
            }
        }

        internal ArrayList ReadDeviceFilters()
        {
            EnsureWebConfigIsPresent();

            // Reload the document everytime we read filters
            LoadDocument();

            XmlNode filters = FindFilterSection(Document, false);
            ArrayList DeviceFilterList = new ArrayList();

            if (filters != null)
            {
                Hashtable filterTable = new Hashtable();

                foreach(XmlNode childNode in filters.ChildNodes)
                {
                    if (childNode.Name != null && childNode.Name.Equals("filter"))
                    {
                        // Ignore the empty filter.
                        if (childNode.Attributes["name"] == null ||
                            childNode.Attributes["name"].Value == null ||
                            childNode.Attributes["name"].Value.Length == 0)
                        {
                            continue;
                        }

                        String filterName = childNode.Attributes["name"].Value;
                        if (filterTable[filterName] != null)
                        {
                            throw new Exception(SR.GetString(SR.DeviceFilterEditorDialog_DuplicateNames));
                        }

                        DeviceFilterNode node = new DeviceFilterNode(this, childNode);
                        DeviceFilterList.Add(node);
                        filterTable[filterName] = true;
                    }
                }
            }

            return DeviceFilterList;
        }

        private bool IsSystemWebSectionPresent()
        {
            EnsureWebConfigIsPresent();
            XmlNode filters = FindFilterSection(Document, false);

            return filters != null;
        }

        private XmlElement FindXmlNode(XmlNode node, string name) {
            Debug.Assert(node != null);

            if (node.HasChildNodes) {
                foreach (XmlNode child in node.ChildNodes) {
                    if (String.Equals(child.Name, name, StringComparison.Ordinal) &&
                        child is XmlElement) {
                        return (XmlElement)child;
                    }
                }
            }

            return null;
        }

        internal XmlElement FindFilterSection(XmlDocument document, bool createIfNotExists) {
            XmlNode configSection = FindXmlNode(document, "configuration");
            if (configSection == null) {
                if (createIfNotExists) {
                    throw new Exception(SR.GetString(SR.WebConfig_FileLoadException));
                }

                return null;
            }

            XmlElement webSection = FindXmlNode(configSection, "system.web");
            if (webSection == null) {
                if (createIfNotExists) {
                    webSection = Document.CreateElement("system.web");
                    configSection.AppendChild(webSection);
                }

                return null;
            }

            XmlElement filters = FindXmlNode(webSection, "deviceFilters");
            if (filters == null) {
                if (createIfNotExists) {
                    filters = Document.CreateElement("deviceFilters");
                    webSection.AppendChild(filters);
                }

                return null;
            }

            return filters;
        }

        internal void EnsureSystemWebSectionIsPresent() {
            if (!IsSystemWebSectionPresent()) {
                Debug.Assert(Document != null);

                FindFilterSection(Document, true);
            }
        }

        internal void Save()
        {
            Document.Save(_path);
        }
    }

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DeviceFilterNode : ICloneable
    {
        internal enum DeviceFilterMode
        {
            Compare,
            Delegate
        };

        private WebConfigManager _webConfig = null;
        private XmlNode _xmlNode = null;
        internal DeviceFilterMode Mode;

        private String _name;
        private String _type;
        private String _method;
        private String _compare;
        private String _argument;

        internal DeviceFilterNode(WebConfigManager webConfig) : base()
        {
            _webConfig = webConfig;
            _xmlNode = webConfig.Document.CreateElement("filter");

            Name = SR.GetString(SR.DeviceFilterNode_DefaultFilterName);
            Mode = DeviceFilterMode.Compare;
        }

        internal DeviceFilterNode(
            WebConfigManager webConfig,
            XmlNode xmlNode
        ) {
            _webConfig = webConfig;
            _xmlNode = xmlNode;

            Debug.Assert(_xmlNode.Attributes != null);

            if (_xmlNode.Attributes["type"] != null)
            {
                Mode = DeviceFilterMode.Delegate;
                Debug.Assert(
                    _xmlNode.Attributes["argument"] == null && _xmlNode.Attributes["compare"] == null,
                    "Managementobject contains both Compare and Delegate mode properties."
                );
            }
            else
            {
                Mode = DeviceFilterMode.Compare;
                // we already know type is null, but it nevers hurts...
                Debug.Assert(
                    _xmlNode.Attributes["type"] == null && _xmlNode.Attributes["method"] == null,
                    "Managementobject contains both Compare and Delegate mode properties."
                );
            }

            _name = _xmlNode.Attributes["name"] == null? 
                null : _xmlNode.Attributes["name"].Value;

            _compare = _xmlNode.Attributes["compare"] == null?
                null : _xmlNode.Attributes["compare"].Value;

            _argument = _xmlNode.Attributes["argument"] == null?
                null : _xmlNode.Attributes["argument"].Value;

            _type = _xmlNode.Attributes["type"] == null?
                null : _xmlNode.Attributes["type"].Value;

            _method = _xmlNode.Attributes["method"] == null?
                null : _xmlNode.Attributes["method"].Value;
        }

        internal void Delete()
        {
            Debug.Assert(_xmlNode != null);

            if (_xmlNode.ParentNode != null)
            {
                _xmlNode.ParentNode.RemoveChild(_xmlNode);
            }
        }

        internal void Save()
        {
            Debug.Assert(_xmlNode != null);

            if (_xmlNode.Attributes["name"] == null)
            {
                _xmlNode.Attributes.Append(_webConfig.Document.CreateAttribute("name"));
            }
            _xmlNode.Attributes["name"].Value = Name;

            if(Mode == DeviceFilterMode.Compare)
            {
                Type = null;
                Method = null;
                _xmlNode.Attributes.RemoveNamedItem("type");
                _xmlNode.Attributes.RemoveNamedItem("method");

                if (_xmlNode.Attributes["compare"] == null)
                {
                    _xmlNode.Attributes.Append(_webConfig.Document.CreateAttribute("compare"));
                }
                _xmlNode.Attributes["compare"].Value = Compare;

                if (_xmlNode.Attributes["argument"] == null)
                {
                    _xmlNode.Attributes.Append(_webConfig.Document.CreateAttribute("argument"));
                }
                _xmlNode.Attributes["argument"].Value = Argument;

            }
            else
            {
                Compare = null;
                Argument = null;
                _xmlNode.Attributes.RemoveNamedItem("compare");
                _xmlNode.Attributes.RemoveNamedItem("argument");

                if (_xmlNode.Attributes["type"] == null)
                {
                    _xmlNode.Attributes.Append(_webConfig.Document.CreateAttribute("type"));
                }
                _xmlNode.Attributes["type"].Value = Type;

                if (_xmlNode.Attributes["method"] == null)
                {
                    _xmlNode.Attributes.Append(_webConfig.Document.CreateAttribute("method"));
                }
                _xmlNode.Attributes["method"].Value = Method;
            }

            _webConfig.EnsureSystemWebSectionIsPresent();

            XmlNode filters = _webConfig.FindFilterSection(_webConfig.Document, false);
            filters.AppendChild(_xmlNode);
        }

        // Cause the DeviceFilterNode to display correctly when inserted
        // in a combo box.
        public override String ToString()
        {
            if(Name == null || Name.Length == 0)
            {
                return SR.GetString(SR.DeviceFilter_DefaultChoice);
            }
            return Name;
        }

        internal String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        internal String Type
        {
            get { return _type; }
            set { _type = value; }
        }

        internal String Method
        {
            get { return _method; }
            set { _method = value; }
        }

        internal String Compare
        {
            get { return _compare; }
            set { _compare = value; }
        }

        internal String Argument
        {
            get { return _argument; }
            set { _argument = value; }
        }
        
        // <summary>
        //    Returns a copy of this node's encapsulated data.  Does not
        //    copy TreeNode fields.
        // </summary>
        public Object Clone()
        {
            DeviceFilterNode newNode = new DeviceFilterNode(
                _webConfig
            );
            newNode.Name = this.Name;
            newNode.Mode = this.Mode;
            if(this.Mode == DeviceFilterMode.Compare)
            {
                newNode.Compare = this.Compare;
                newNode.Argument = this.Argument;
            }
            else
            {
                newNode.Type = this.Type;
                newNode.Method = this.Method;
            }
            return (Object) newNode;
        }
    }
}
