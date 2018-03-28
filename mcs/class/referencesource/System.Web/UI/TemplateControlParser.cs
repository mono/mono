//------------------------------------------------------------------------------
// <copyright file="TemplateControlParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements the ASP.NET template parser
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {

using System.Text;
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Configuration;
using System.Web.Caching;
using System.Web.Util;
using System.Web.Compilation;
using HttpException = System.Web.HttpException;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Permissions;


/*
 * Parser for TemplateControl's (UserControls and Pages)
 */

/// <internalonly/>
/// <devdoc>
///    <para>[To be supplied.]</para>
/// </devdoc>
public abstract class TemplateControlParser : BaseTemplateParser {

    // Attributes in <%@ outputcache ... %> directive
    private IDictionary _outputCacheDirective;

    private OutputCacheParameters _outputCacheSettings;
    internal OutputCacheParameters OutputCacheParameters { get { return _outputCacheSettings; } }

    internal bool FAutoEventWireup { get { return !flags[noAutoEventWireup]; } }

    internal override bool RequiresCompilation {
        get { return flags[requiresCompilation] || CompilationMode == CompilationMode.Always; }
    }

    // Get default settings from config
    internal override void ProcessConfigSettings() {
        base.ProcessConfigSettings();

        if (PagesConfig != null) {
            flags[noAutoEventWireup] = !PagesConfig.AutoEventWireup;

            // If config has a non-default EnableViewState value, set it in the main directive
            if (PagesConfig.EnableViewState != Control.EnableViewStateDefault)
                _mainDirectiveConfigSettings["enableviewstate"] = Util.GetStringFromBool(PagesConfig.EnableViewState);

            CompilationMode = PagesConfig.CompilationMode;
        }

        // if there is a filter, ask it if it wants to use a different mode
        if (_pageParserFilter != null)
            CompilationMode = _pageParserFilter.GetCompilationMode(CompilationMode);
    }

    internal override void ProcessDirective(string directiveName, IDictionary directive) {

        if (StringUtil.EqualsIgnoreCase(directiveName, "outputcache")) {

            // Ignore the OutputCache directive in design mode (VSWhidbey 470314)
            if (FInDesigner)
                return;

            if (_outputCacheSettings == null) {
                _outputCacheSettings = new OutputCacheParameters();
            }

            // Make sure the outputcache directive was not already specified
            if (_outputCacheDirective != null) {
                throw new HttpException(
                    SR.GetString(SR.Only_one_directive_allowed, directiveName));
            }

            ProcessOutputCacheDirective(directiveName, directive);

            _outputCacheDirective = directive;
        }
        else if (StringUtil.EqualsIgnoreCase(directiveName, "reference")) {

            // Ignore the OutputCache directive in design mode (VSWhidbey 517783)
            if (FInDesigner) {
                return;
            }

            // Even though this only makes sense for compiled pages, Sharepoint needs us to
            // ignore instead of throw when the page in non-compiled.


            // For historical reasons, the virtual path can be specified by 3 different attributes:
            // virtualpath, page and control.  They all do the same, and virtualpath is the recommended
            // one (the other two are deprecated).

            // Make sure that no more than one is specified.

            VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualpath");

            bool enforcePage = false;
            bool enforceControl = false;

            VirtualPath tmp = Util.GetAndRemoveVirtualPathAttribute(directive, "page");
            if (tmp != null) {
                if (virtualPath != null) {
                    ProcessError(SR.GetString(SR.Invalid_reference_directive));
                    return;
                }
                virtualPath = tmp;
                enforcePage = true;
            }

            tmp = Util.GetAndRemoveVirtualPathAttribute(directive, "control");
            if (tmp != null) {
                if (virtualPath != null) {
                    ProcessError(SR.GetString(SR.Invalid_reference_directive));
                    return;
                }
                virtualPath = tmp;
                enforceControl = true;
            }

            // If we didn't get a virtual path, fail
            if (virtualPath == null) {
                ProcessError(SR.GetString(SR.Invalid_reference_directive));
                return;
            }

            Type t = GetReferencedType(virtualPath);

            if (t == null) {
                ProcessError(SR.GetString(SR.Invalid_reference_directive_attrib, virtualPath));
            }

            // If the 'page' attribute was used, make sure it's indeed a Page
            if (enforcePage && !typeof(Page).IsAssignableFrom(t)) {
                ProcessError(SR.GetString(SR.Invalid_reference_directive_attrib, virtualPath));
            }

            // If the 'control' attribute was used, make sure it's indeed a UserControl
            if (enforceControl && !typeof(UserControl).IsAssignableFrom(t)) {
                ProcessError(SR.GetString(SR.Invalid_reference_directive_attrib, virtualPath));
            }

            // If there are some attributes left, fail
            Util.CheckUnknownDirectiveAttributes(directiveName, directive);
        }
        else {
            base.ProcessDirective(directiveName, directive);
        }
    }

    internal override void ProcessMainDirective(IDictionary mainDirective) {

        // We want to make sure that we process the compilationmode attribute before any
        // of the other main directive attributes, since its presence can cause other
        // attributes to be illegal.  So we handle it instead of in ProcessMainDirectiveAttribute.
        object tmpObj = null;

        try {
            tmpObj = Util.GetAndRemoveEnumAttribute(mainDirective,
                typeof(CompilationMode), "compilationmode");
        }
        // catch the exception here so we can continue to process the rest of the main directive
        // when called from CBM
        catch (Exception ex) {
            ProcessError(ex.Message);
        }

        if (tmpObj != null) {
            CompilationMode = (CompilationMode) tmpObj;

            // if there is a filter, ask it if it wants to use a different mode
            if (_pageParserFilter != null)
                CompilationMode = _pageParserFilter.GetCompilationMode(CompilationMode);
        }

        base.ProcessMainDirective(mainDirective);
    }

    internal override bool ProcessMainDirectiveAttribute(string deviceName, string name,
        string value, IDictionary parseData) {

        switch (name) {

        // Ignore 'targetschema' attribute (ASURT 85670)
        case "targetschema":
            break;

        case "autoeventwireup":
            // This only makes sense for compiled pages
            OnFoundAttributeRequiringCompilation(name);

            flags[noAutoEventWireup] = !Util.GetBooleanAttribute(name, value);
            break;

        case "enabletheming":
            // Return false to let the generic attribute processing continue
            // which will cause the EnableTheming property to be set on the
            // TemplateControl instance.
            return false;

        case CodeFileBaseClassAttributeName:
            // Remember the base class for post processing
            parseData[name] = Util.GetNonEmptyAttribute(name, value);
            break;

        default:
            // We didn't handle the attribute.  Try the base class
            return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
        }

        // The attribute was handled

        // Make sure no device filter or resource expression was specified
        ValidateBuiltInAttribute(deviceName, name, value);

        return true;
    }

    internal override void ProcessUnknownMainDirectiveAttribute(string filter, string attribName, string value) {

        // Don't allow the id to be specified on the directive, even though it is
        // a public member of the control class (VSWhidbey 85384)
        if (attribName == "id") {
            base.ProcessUnknownMainDirectiveAttribute(filter, attribName, value);
            return;
        }

        // Process unknown attributes as regular control attribute, hence allowing
        // arbitrary properties of the base class to be set.
        // But turn off IAttributeAccessor support, otherwise any bad string on a
        // user control's directive won't be caught.

        try {
            RootBuilder.PreprocessAttribute(filter, attribName, value, true /*mainDirectiveMode*/);
        }
        catch (Exception e) {
            ProcessError(SR.GetString(SR.Attrib_parse_error, attribName, e.Message));
        }
    }

    /*
     * Add assembly dependencies for a collection of static objects
     */
    private void AddStaticObjectAssemblyDependencies(HttpStaticObjectsCollection staticObjects) {
        if (staticObjects == null || staticObjects.Objects == null) return;

        IDictionaryEnumerator en = staticObjects.Objects.GetEnumerator();
        while (en.MoveNext()) {
            HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry)en.Value;

            AddTypeDependency(entry.ObjectType);
        }
    }

    internal Type GetDirectiveType(IDictionary directive, string directiveName) {
        string typeName = Util.GetAndRemoveNonEmptyNoSpaceAttribute(directive, "typeName");
        VirtualPath virtualPath = Util.GetAndRemoveVirtualPathAttribute(directive, "virtualPath");

        Type resultType = null;

        // If neither or both are specified, fail
        if ((typeName == null) == (virtualPath == null)) {
            throw new HttpException(SR.GetString(SR.Invalid_typeNameOrVirtualPath_directive, directiveName));
        }

        if (typeName != null) {
            resultType = GetType(typeName);
            AddTypeDependency(resultType);
        }
        else {
            resultType = GetReferencedType(virtualPath);
        }

        // If there are some attributes left, fail
        Util.CheckUnknownDirectiveAttributes(directiveName, directive);

        return resultType;
    }

    internal override void HandlePostParse() {
        base.HandlePostParse();

        if (!FInDesigner) {
            // Omit AutoEventWireup if there can't possibly be any events defined (ASURT 97772)
            if (ScriptList.Count == 0 && BaseType == DefaultBaseType && CodeFileVirtualPath == null)
                flags[noAutoEventWireup] = true;

            _applicationObjects = HttpApplicationFactory.ApplicationState.StaticObjects;
            AddStaticObjectAssemblyDependencies(_applicationObjects);

            _sessionObjects = HttpApplicationFactory.ApplicationState.SessionStaticObjects;
            AddStaticObjectAssemblyDependencies(_sessionObjects);
        }
    }

    /*
     * Process the contents of the <%@ OutputCache ... %> directive
     */
    internal virtual void ProcessOutputCacheDirective(string directiveName, IDictionary directive) {
        int duration = 0;   // Unit is second
        string varyByParams;
        string varyByCustom;
        string outputCacheProfile = null;
        string varyByControls;

        bool fHasDuration = Util.GetAndRemovePositiveIntegerAttribute(directive, "duration", ref duration);
        if (fHasDuration) {
            OutputCacheParameters.Duration = duration;
        }

        // 
        if (this is PageParser) {
            outputCacheProfile = Util.GetAndRemoveNonEmptyAttribute(directive, "cacheProfile");
            if (outputCacheProfile != null) {
                OutputCacheParameters.CacheProfile = outputCacheProfile;
            }
        }

        if (!fHasDuration && (outputCacheProfile == null || outputCacheProfile.Length == 0) && FDurationRequiredOnOutputCache)
            throw new HttpException(SR.GetString(SR.Missing_attr, "duration"));

        varyByCustom = Util.GetAndRemoveNonEmptyAttribute(directive, "varybycustom");
        if (varyByCustom != null) {
            OutputCacheParameters.VaryByCustom = varyByCustom;
        }

        varyByControls = Util.GetAndRemoveNonEmptyAttribute(directive, "varybycontrol");
        if (varyByControls != null) {
            OutputCacheParameters.VaryByControl = varyByControls;
        }

        varyByParams = Util.GetAndRemoveNonEmptyAttribute(directive, "varybyparam");
        if (varyByParams != null) {
            OutputCacheParameters.VaryByParam = varyByParams;
        }

        // VaryByParams is required (ASURT 76763)
        if (varyByParams == null && 
            varyByControls == null && 
            (outputCacheProfile == null || outputCacheProfile.Length == 0) && 
            FVaryByParamsRequiredOnOutputCache)
            throw new HttpException(SR.GetString(SR.Missing_varybyparam_attr));

        // If it's "none", set it to null
        if (StringUtil.EqualsIgnoreCase(varyByParams, "none"))
            OutputCacheParameters.VaryByParam = null;

        if (StringUtil.EqualsIgnoreCase(varyByControls, "none"))
            OutputCacheParameters.VaryByControl = null;

        // If there are some attributes left, fail
        Util.CheckUnknownDirectiveAttributes(directiveName, directive, UnknownOutputCacheAttributeError);
    }

    internal virtual bool FDurationRequiredOnOutputCache {
        get { return true; }
    }

    internal virtual bool FVaryByParamsRequiredOnOutputCache {
        get { return true; }
    }

    internal abstract string UnknownOutputCacheAttributeError {
        get;
    }
}
}
