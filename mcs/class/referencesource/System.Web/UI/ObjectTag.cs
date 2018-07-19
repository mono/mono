//------------------------------------------------------------------------------
// <copyright file="ObjectTag.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Implements the <object runat=server> tag
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {

using System;
using System.IO;
using System.Collections;
using System.Web;
using System.Web.Util;
using System.Globalization;
using System.Security.Permissions;

/*
 * ObjectTag is a marker class, that should never be instantiated.  Its
 * only purpose is to point to the ObjectTagBuilder class through its
 * metadata.
 */
[
    ControlBuilderAttribute(typeof(ObjectTagBuilder))
]
internal class ObjectTag {
    private ObjectTag() {
    }
}


/// <internalonly/>
/// <devdoc>
/// </devdoc>
public sealed class ObjectTagBuilder : ControlBuilder {

    private ObjectTagScope _scope;
    private Type _type;
    private bool _lateBound;
    private string _progid; // Only used for latebound objects
    private string _clsid;  // Only used for latebound objects
    private bool _fLateBinding; // Force latebinding when early binding could be done


    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public override void Init(TemplateParser parser, ControlBuilder parentBuilder,
        Type type, string tagName,
        string id, IDictionary attribs) {

        if (id == null) {
            throw new HttpException(
                SR.GetString(SR.Object_tag_must_have_id));
        }

        ID = id;

        // Get the scope attribute of the object tag
        string scope = (string) attribs["scope"];

        // Map it to an ObjectTagScope enum
        if (scope == null)
            _scope = ObjectTagScope.Default;
        else if (StringUtil.EqualsIgnoreCase(scope, "page"))
            _scope = ObjectTagScope.Page;
        else if (StringUtil.EqualsIgnoreCase(scope, "session"))
            _scope = ObjectTagScope.Session;
        else if (StringUtil.EqualsIgnoreCase(scope, "application"))
            _scope = ObjectTagScope.Application;
        else if (StringUtil.EqualsIgnoreCase(scope, "appinstance"))
            _scope = ObjectTagScope.AppInstance;
        else
            throw new HttpException(SR.GetString(SR.Invalid_scope, scope));

        Util.GetAndRemoveBooleanAttribute(attribs, "latebinding",
            ref _fLateBinding);

        string tmp = (string) attribs["class"];

        // Is there a 'class' attribute?
        if (tmp != null) {
            // Get a Type object from the type string
            _type = parser.GetType(tmp);
        }

        // If we don't have a type, check for a classid attribute
        if (_type == null) {
            tmp = (string) attribs["classid"];

            if (tmp != null) {
                // Create a Guid out of it
                Guid clsid = new Guid(tmp);

                // Turn it into a type
                _type = Type.GetTypeFromCLSID(clsid);

                if (_type == null)
                    throw new HttpException(SR.GetString(SR.Invalid_clsid, tmp));

                // 




                if (_fLateBinding || Util.IsLateBoundComClassicType(_type)) {
                    _lateBound = true;
                    _clsid = tmp;
                }
                else {

                    // Add a dependency to the type, so that the user can use it without
                    // having to import it
                    parser.AddTypeDependency(_type);
                }
            }
        }

        // If we don't have a type, check for a progid attribute
        if (_type == null) {
            tmp = (string) attribs["progid"];

            if (tmp != null) {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
                // Turn it into a type
                _type = Type.GetTypeFromProgID(tmp);
#else // !FEATURE_PAL
                throw new NotImplementedException("ROTORTODO");
#endif // !FEATURE_PAL


                if (_type == null)
                    throw new HttpException(SR.GetString(SR.Invalid_progid, tmp));

                Debug.Trace("Template", "<object> type: " + _type.FullName);

                // 




                if (_fLateBinding || Util.IsLateBoundComClassicType(_type)) {
                    _lateBound = true;
                    _progid = tmp;
                }
                else {

                    // Add a dependency to the type, so that the user can use it without
                    // having to import it
                    parser.AddTypeDependency(_type);
                }
            }
        }

        // If we still don't have a type, fail
        if (_type == null) {
            throw new HttpException(
                SR.GetString(SR.Object_tag_must_have_class_classid_or_progid));
        }
    }

    // Ignore all content

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public override void AppendSubBuilder(ControlBuilder subBuilder) {}

    /// <internalonly/>
    /// <devdoc>
    /// </devdoc>
    public override void AppendLiteralString(string s) {}

    internal ObjectTagScope Scope {
        get { return _scope; }
    }

    internal Type ObjectType {
        get { return _type; }
    }

    internal bool LateBound {
        get { return _lateBound;}
    }

    internal Type DeclaredType {
        get { return _lateBound ? typeof(object) : ObjectType; }
    }

    internal string Progid {
        get { return _progid; }
    }

    internal string Clsid {
        get { return _clsid; }
    }
}

/*
 * Enum for the scope of an object tag
 */
internal enum ObjectTagScope {
    Default = 0,
    Page = 1,
    Session = 2,
    Application = 3,
    AppInstance = 4
}

}
