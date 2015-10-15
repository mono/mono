//------------------------------------------------------------------------------
// <copyright file="RoleServiceManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Text;
    using System.Web.Script.Serialization;
    using System.Web;
    using System.Web.ApplicationServices;
    using System.Web.Resources;
    using System.Web.Security;

    [
    DefaultProperty("Path"),
    TypeConverter(typeof(EmptyStringExpandableObjectConverter))
    ]
    public class RoleServiceManager {

        private bool _loadRoles;
        private string _path;

        internal static void ConfigureRoleService(ref StringBuilder sb, HttpContext context, ScriptManager scriptManager, List<ScriptManagerProxy> proxies) {
            string roleServiceUrl = null;
            bool loadRoles = false;
            RoleServiceManager roleManager;

            if(scriptManager.HasRoleServiceManager) {
                roleManager = scriptManager.RoleService;

                // load roles?
                loadRoles = roleManager.LoadRoles;

                // get ScriptManager.Path
                roleServiceUrl = roleManager.Path.Trim();
                if(roleServiceUrl.Length > 0) {
                    roleServiceUrl = scriptManager.ResolveClientUrl(roleServiceUrl);
                }
            }

            // combine proxy ServiceUrls (find the first one that has specified one)
            if(proxies != null) {
                foreach(ScriptManagerProxy proxy in proxies) {
                    if(proxy.HasRoleServiceManager) {
                        roleManager = proxy.RoleService;

                        // combine load roles
                        if (roleManager.LoadRoles) {
                            loadRoles = true;
                        }

                        // combine urls
                        roleServiceUrl = ApplicationServiceManager.MergeServiceUrls(roleManager.Path, roleServiceUrl, proxy);
                    }
                }
            }

            RoleServiceManager.GenerateInitializationScript(ref sb, context, scriptManager, roleServiceUrl, loadRoles);
        }

        private static void GenerateInitializationScript(ref StringBuilder sb, HttpContext context, ScriptManager scriptManager, string serviceUrl, bool loadRoles) {
            bool enabled = ApplicationServiceHelper.RoleServiceEnabled;
            string defaultServicePath = null;

            if (enabled) {
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }

                // The default path points to the built-in service (if it is enabled)
                // Note that the client can't default to this path because it doesn't know what the app root is, we must tell it.
                // We must specify the default path to the proxy even if a custom path is provided, because on the client they could
                // reset the path back to the default if they want.   
                defaultServicePath = scriptManager.ResolveClientUrl("~/" + System.Web.Script.Services.WebServiceData._roleServiceFileName);
                sb.Append("Sys.Services._RoleService.DefaultWebServicePath = '");
                sb.Append(HttpUtility.JavaScriptStringEncode(defaultServicePath));
                sb.Append("';\n");
            }

            bool pathSpecified = !String.IsNullOrEmpty(serviceUrl);
            if (pathSpecified) {
                // DevDiv 


                if (defaultServicePath == null){
                    defaultServicePath = scriptManager.ResolveClientUrl("~/" + System.Web.Script.Services.WebServiceData._roleServiceFileName);
                }
                if (loadRoles && !String.Equals(serviceUrl, defaultServicePath, StringComparison.OrdinalIgnoreCase)) {
                    throw new InvalidOperationException(AtlasWeb.RoleServiceManager_LoadRolesWithNonDefaultPath);
                }
                if (sb == null) {
                    sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                }
                sb.Append("Sys.Services.RoleService.set_path('");
                sb.Append(HttpUtility.JavaScriptStringEncode(serviceUrl));
                sb.Append("');\n");
            }

            // Dev10 757178: Do not attempt during design mode. This code isn't important for intellisense anyway.
            if (loadRoles) {
                if (scriptManager.DesignMode) {
                    // Dev10 757178: Do not lookup user roles at design time.
                    // But at DesignTime this method is only important because if it produces any init script,
                    // it prompts AddFrameworkScripts to add the MicrosoftAjaxApplicationServices.js script reference. 
                    // So just append a comment to ensure at least some script is generated.
                    if (sb == null) {
                        sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                    }
                    sb.Append("// loadRoles\n");                    
                }
                else {
                    string[] roles = Roles.GetRolesForUser();
                    if(roles != null && roles.Length > 0) {
                        if (sb == null) {
                            sb = new StringBuilder(ApplicationServiceManager.StringBuilderCapacity);
                        }
                        sb.Append("Sys.Services.RoleService._roles = ");
                        sb.Append(new JavaScriptSerializer().Serialize(roles, JavaScriptSerializer.SerializationFormat.JavaScript));
                        sb.Append(";\n");
                    }
                }
            }
        }

        [
        DefaultValue(false),
        Category("Behavior"),
        NotifyParentProperty(true),
        ResourceDescription("RoleServiceManager_LoadRoles")
        ]
        public bool LoadRoles {
            get {
                return _loadRoles;
            }
            set {
                _loadRoles = value;
            }
        }

        [
        DefaultValue(""),
        Category("Behavior"),
        NotifyParentProperty(true),
        ResourceDescription("ApplicationServiceManager_Path"),
        UrlProperty()
        ]
        public string Path
        {
            get {
                return _path ?? String.Empty;
            }
            set {
                _path = value;
            }
        }
    }
}
