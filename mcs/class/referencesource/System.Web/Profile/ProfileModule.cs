//------------------------------------------------------------------------------
// <copyright file="ProfileModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * ProfileModule class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Profile {
    using System.Web;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Caching;
    using System.Collections;
    using System.Web.Util;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Web.Security;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Collections.Specialized;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.IO;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Configuration;
#if !FEATURE_PAL
    using System.Web.DataAccess;
#endif // !FEATURE_PAL

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ProfileModule : IHttpModule
    {
        private static object                   s_Lock              = new object();
        private ProfileEventHandler             _eventHandler       = null;

        private ProfileMigrateEventHandler _MigrateEventHandler;
        private ProfileAutoSaveEventHandler _AutoSaveEventHandler;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.ProfileModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public ProfileModule()
        {
        }

        /// <devdoc>
        ///    This is a Global.asax event which must be
        ///    named FormsAuthorize_OnAuthorize event. It's used by advanced users to
        ///    customize cookie authentication.
        /// </devdoc>
        public event ProfileEventHandler Personalize
        {
            add { _eventHandler += value; }
            remove { _eventHandler -= value; }
        }

        public event ProfileMigrateEventHandler MigrateAnonymous
        {
            add { _MigrateEventHandler += value; }
            remove { _MigrateEventHandler -= value; }
        }

        public event ProfileAutoSaveEventHandler ProfileAutoSaving {
            add { _AutoSaveEventHandler += value; }
            remove { _AutoSaveEventHandler -= value; }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Dispose()
        {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Init(HttpApplication app)
        {
            if (ProfileManager.Enabled) {
                app.AcquireRequestState += new EventHandler(this.OnEnter);
                if (ProfileManager.AutomaticSaveEnabled) {
                    app.EndRequest += new EventHandler(this.OnLeave);
                }
            }            
        }

        private void OnPersonalize(ProfileEventArgs e)
        {
            if (_eventHandler != null)
                _eventHandler(this, e);

            if (e.Profile != null)
            {
                e.Context._Profile = e.Profile;
                return;
            }

            e.Context._ProfileDelayLoad = true;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnEnter(Object source, EventArgs eventArgs)
        {
            HttpContext context = ((HttpApplication)source).Context;
            OnPersonalize(new ProfileEventArgs(context));
            if (context.Request.IsAuthenticated && !string.IsNullOrEmpty(context.Request.AnonymousID) && _MigrateEventHandler != null)
            {
                ProfileMigrateEventArgs e = new ProfileMigrateEventArgs(context, context.Request.AnonymousID);
                _MigrateEventHandler(this, e);
            }
        }

        private void OnLeave(Object source, EventArgs eventArgs)
        {
            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;

            if (context._Profile == null || (object)context._Profile == (object)ProfileBase.SingletonInstance)
                return;

            if (_AutoSaveEventHandler != null) {
                ProfileAutoSaveEventArgs args = new ProfileAutoSaveEventArgs(context);
                _AutoSaveEventHandler(this, args);
                if (!args.ContinueWithProfileAutoSave)
                    return;
            }

            context.Profile.Save();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        internal static void ParseDataFromDB(string[] names, string values, byte[] buf, SettingsPropertyValueCollection properties)
        {
            if (names == null || values == null || buf == null || properties == null) 
                return;
            try {
                for (int iter = 0; iter < names.Length / 4; iter++) {
                    string name = names[iter * 4];
                    SettingsPropertyValue pp = properties[name];

                    if (pp == null) // property not found
                        continue;

                    int startPos = Int32.Parse(names[iter * 4 + 2], CultureInfo.InvariantCulture);
                    int length = Int32.Parse(names[iter * 4 + 3], CultureInfo.InvariantCulture);

                    if (length == -1 && !pp.Property.PropertyType.IsValueType) // Null Value
                    {
                        pp.PropertyValue = null;
                        pp.IsDirty = false;
                        pp.Deserialized = true;
                    }
                    if (names[iter * 4 + 1] == "S" && startPos >= 0 && length > 0 && values.Length >= startPos + length) {
                        pp.SerializedValue = values.Substring(startPos, length);
                    }

                    if (names[iter * 4 + 1] == "B" && startPos >= 0 && length > 0 && buf.Length >= startPos + length) {
                        byte[] buf2 = new byte[length];

                        Buffer.BlockCopy(buf, startPos, buf2, 0, length);
                        pp.SerializedValue = buf2;
                    }
                }
            } catch { // Eat exceptions
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        internal static void PrepareDataForSaving(ref string allNames, ref string allValues, ref byte[] buf, bool binarySupported, SettingsPropertyValueCollection properties, bool userIsAuthenticated)
        {
            StringBuilder names = new StringBuilder();
            StringBuilder values = new StringBuilder();

            MemoryStream ms = (binarySupported ? new System.IO.MemoryStream() : null);
            try {
                try {
                    bool anyItemsToSave = false;

                    foreach (SettingsPropertyValue pp in properties) {
                        if (pp.IsDirty) {
                            if (!userIsAuthenticated) {
                                bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
                                if (!allowAnonymous)
                                    continue;
                            }
                            anyItemsToSave = true;
                            break;
                        }
                    }

                    if (!anyItemsToSave)
                        return;

                    foreach (SettingsPropertyValue pp in properties) {
                        if (!userIsAuthenticated) {
                            bool allowAnonymous = (bool)pp.Property.Attributes["AllowAnonymous"];
                            if (!allowAnonymous)
                                continue;
                        }

                        if (!pp.IsDirty && pp.UsingDefaultValue) // Not fetched from DB and not written to
                            continue;

                        int len = 0, startPos = 0;
                        string propValue = null;

                        if (pp.Deserialized && pp.PropertyValue == null) // is value null?
                            {
                            len = -1;
                        } else {
                            object sVal = pp.SerializedValue;

                            if (sVal == null) {
                                len = -1;
                            } else {
                                if (!(sVal is string) && !binarySupported) {
                                    sVal = Convert.ToBase64String((byte[])sVal);
                                }

                                if (sVal is string) {
                                    propValue = (string)sVal;
                                    len = propValue.Length;
                                    startPos = values.Length;
                                } else {
                                    byte[] b2 = (byte[])sVal;
                                    startPos = (int)ms.Position;
                                    ms.Write(b2, 0, b2.Length);
                                    ms.Position = startPos + b2.Length;
                                    len = b2.Length;
                                }
                            }
                        }

                        names.Append(pp.Name + ":" + ((propValue != null) ? "S" : "B") +
                                     ":" + startPos.ToString(CultureInfo.InvariantCulture) + ":" + len.ToString(CultureInfo.InvariantCulture) + ":");
                        if (propValue != null)
                            values.Append(propValue);
                    }

                    if (binarySupported) {
                        buf = ms.ToArray();
                    }
                } finally {
                    if (ms != null)
                        ms.Close();
                }
            } catch {
                throw;
            }
            allNames = names.ToString();
            allValues = values.ToString();
        }
    }

    public delegate void ProfileMigrateEventHandler(Object sender,  ProfileMigrateEventArgs e);

    public sealed class ProfileMigrateEventArgs : EventArgs {
        private HttpContext       _Context;
        private string            _AnonymousId;

        public  HttpContext       Context { get { return _Context;}}

        public  string            AnonymousID { get { return _AnonymousId;}}

        public ProfileMigrateEventArgs(HttpContext context, string anonymousId) {
            _Context = context;
            _AnonymousId = anonymousId;
        }
    }

    public delegate void ProfileAutoSaveEventHandler(Object sender, ProfileAutoSaveEventArgs e);

    public sealed class ProfileAutoSaveEventArgs : EventArgs
    {
        private     HttpContext         _Context;
        private     bool                _ContinueSave = true;

        public      HttpContext     Context                     { get { return _Context; } }
        public      bool            ContinueWithProfileAutoSave { get { return _ContinueSave; }  set { _ContinueSave = value; }}

        public ProfileAutoSaveEventArgs(HttpContext context) {
            _Context = context;
        }
    }
}
