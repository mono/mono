//------------------------------------------------------------------------------
// <copyright file="FactoryRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Text;
    using System.Threading;
    using System.Reflection;
    using System.Xml;

    [System.Diagnostics.DebuggerDisplay("FactoryRecord {ConfigKey}")]
    internal class FactoryRecord : IConfigErrorInfo {
        private const int Flag_AllowLocation                = 0x0001;   // Does the factory allow location directives?
        private const int Flag_RestartOnExternalChanges     = 0x0002;   // Restart on external changes?
        private const int Flag_RequirePermission            = 0x0004;   // Does access to the section require unrestricted ConfigurationPermission?
        private const int Flag_IsGroup                      = 0x0008;   // factory represents a group
        private const int Flag_IsFromTrustedConfigRecord    = 0x0010;   // Factory is defined in trusted config record
        private const int Flag_IsFactoryTrustedWithoutAptca = 0x0020;   // Factory is from trusted assembly without aptca
        private const int Flag_IsUndeclared                 = 0x0040;   // Factory is not declared - either implicit or unrecognized

        private string                          _configKey;             // full config key = group name + section name
        private string                          _group;                 // group name
        private string                          _name;                  // section name
        private SimpleBitVector32               _flags;                 // factory flags
        private string                          _factoryTypeName;       // the factory's type name
        private ConfigurationAllowDefinition    _allowDefinition;       // the allowed definition
        private ConfigurationAllowExeDefinition _allowExeDefinition;    // the allowed Exe definition
        private OverrideModeSetting             _overrideModeDefault;   // the default override mode for the section
        private string                          _filename;              // filename of the factory type name
        private int                             _lineNumber;            // line number of the factory type name
        private object                          _factory;               // the created factory
        private List<ConfigurationException>    _errors;                // errors

        // constructor used for Clone()
        FactoryRecord(
                string              configKey,
                string              group,
                string              name,
                object              factory,
                string              factoryTypeName,
                SimpleBitVector32   flags,
                ConfigurationAllowDefinition    allowDefinition,
                ConfigurationAllowExeDefinition allowExeDefinition,
                OverrideModeSetting             overrideModeDefault,
                string              filename,
                int                 lineNumber,
                ICollection<ConfigurationException> errors) {

            _configKey              = configKey;
            _group                  = group;
            _name                   = name;
            _factory                = factory;
            _factoryTypeName        = factoryTypeName;
            _flags                  = flags;
            _allowDefinition        = allowDefinition;
            _allowExeDefinition     = allowExeDefinition;
            _overrideModeDefault    = overrideModeDefault;
            _filename               = filename;
            _lineNumber             = lineNumber;

            AddErrors(errors);
        }

        // constructor used for group
        internal FactoryRecord(string configKey, string group, string name, string factoryTypeName, string filename, int lineNumber) {
            _configKey = configKey;
            _group = group;
            _name = name;
            _factoryTypeName = factoryTypeName;
            IsGroup = true;
            _filename = filename;
            _lineNumber = lineNumber;
        }

        // constructor used for a section
        internal FactoryRecord(
                string configKey,
                string group,
                string name,
                string factoryTypeName,
                bool allowLocation,
                ConfigurationAllowDefinition allowDefinition,
                ConfigurationAllowExeDefinition allowExeDefinition,
                OverrideModeSetting             overrideModeDefault,
                bool restartOnExternalChanges,
                bool requirePermission,
                bool isFromTrustedConfigRecord,
                bool isUndeclared,
                string filename,
                int lineNumber) {

            _configKey                  = configKey;
            _group                      = group;
            _name                       = name;
            _factoryTypeName            = factoryTypeName;
            _allowDefinition            = allowDefinition;
            _allowExeDefinition         = allowExeDefinition;
            _overrideModeDefault        = overrideModeDefault;
            AllowLocation               = allowLocation;
            RestartOnExternalChanges    = restartOnExternalChanges;
            RequirePermission           = requirePermission;
            IsFromTrustedConfigRecord   = isFromTrustedConfigRecord;
            IsUndeclared                = isUndeclared;
            _filename                   = filename;
            _lineNumber                 = lineNumber;
        }

        // by cloning we contain a single copy of the strings referred to in the factory and section records
        internal FactoryRecord CloneSection(string filename, int lineNumber) {
            return new FactoryRecord( _configKey,
                                      _group,
                                      _name,
                                      _factory,
                                      _factoryTypeName,
                                      _flags,
                                      _allowDefinition,
                                      _allowExeDefinition,
                                      _overrideModeDefault,
                                      filename,
                                      lineNumber,
                                      Errors);
        }

        // by cloning we contain a single copy of the strings referred to in the factory and section records
        internal FactoryRecord CloneSectionGroup(string factoryTypeName, string filename, int lineNumber) {
            if (_factoryTypeName != null) {
                factoryTypeName = _factoryTypeName;
            }

            return new FactoryRecord( _configKey,
                                      _group,
                                      _name,
                                      _factory,
                                      factoryTypeName,
                                      _flags,
                                      _allowDefinition,
                                      _allowExeDefinition,
                                      _overrideModeDefault,
                                      filename,
                                      lineNumber,
                                      Errors);
        }

        internal string ConfigKey {
            get {return _configKey;}
        }

        internal string Group {
            get {return _group;}
        }

        internal string Name {
            get {return _name;}
        }

        internal object Factory {
            get {return _factory;}
            set {_factory = value;}
        }

        internal string FactoryTypeName {
            get {return _factoryTypeName;}
            set {_factoryTypeName = value;}
        }

        internal ConfigurationAllowDefinition AllowDefinition {
            get {return _allowDefinition;}
            set {_allowDefinition = value;}
        }

        internal ConfigurationAllowExeDefinition AllowExeDefinition {
            get {return _allowExeDefinition;}
            set {_allowExeDefinition = value;}
        }

        internal OverrideModeSetting OverrideModeDefault {
            get {return _overrideModeDefault;}
        }

        internal bool AllowLocation {
            get {return _flags[Flag_AllowLocation];}
            set {_flags[Flag_AllowLocation] = value;}
        }

        internal bool RestartOnExternalChanges {
            get {return _flags[Flag_RestartOnExternalChanges];}
            set {_flags[Flag_RestartOnExternalChanges] = value;}
        }

        internal bool RequirePermission {
            get {return _flags[Flag_RequirePermission];}
            set {_flags[Flag_RequirePermission] = value;}
        }

        internal bool IsGroup {
            get {return _flags[Flag_IsGroup];}
            set {_flags[Flag_IsGroup] = value;}
        }

        internal bool IsFromTrustedConfigRecord {
            get {return _flags[Flag_IsFromTrustedConfigRecord];}
            set {_flags[Flag_IsFromTrustedConfigRecord] = value;}
        }

        internal bool IsUndeclared {
            get {return _flags[Flag_IsUndeclared];}
            set {_flags[Flag_IsUndeclared] = value;}
        }

        internal bool IsFactoryTrustedWithoutAptca {
            get {
                Debug.Assert(_factory != null, "_factory != null");
                return _flags[Flag_IsFactoryTrustedWithoutAptca];
            }

            set {_flags[Flag_IsFactoryTrustedWithoutAptca] = value;}
        }

        // This is used in HttpConfigurationRecord.EnsureSectionFactory() to give file and line source
        // when a section handler type is invalid or cannot be loaded.
        public string Filename {
            get {return _filename;}
            set {_filename = value;}
        }

        public int LineNumber {
            get {return _lineNumber;}
            set {_lineNumber = value;}
        }

        internal bool HasFile {
            get {return _lineNumber >= 0;}
        }

        internal bool IsEquivalentType(IInternalConfigHost host, string typeName) {
            try {
                if (_factoryTypeName == typeName)
                    return true;

                Type t1, t2;

                if (host != null) {
                    t1 = TypeUtil.GetTypeWithReflectionPermission(host, typeName, false);
                    t2 = TypeUtil.GetTypeWithReflectionPermission(host, _factoryTypeName, false);
                }
                else {
                    t1 = TypeUtil.GetTypeWithReflectionPermission(typeName, false);
                    t2 = TypeUtil.GetTypeWithReflectionPermission(_factoryTypeName, false);
                }

                return (t1 != null) && (t1 == t2);
            }
            catch {
            }

            return false;
        }

        internal bool IsEquivalentSectionGroupFactory(IInternalConfigHost host, string typeName) {
            if (typeName == null || _factoryTypeName == null)
                return true;

            return IsEquivalentType(host, typeName);
        }

        internal bool IsEquivalentSectionFactory(
            IInternalConfigHost             host,
            string                          typeName,
            bool                            allowLocation,
            ConfigurationAllowDefinition    allowDefinition,
            ConfigurationAllowExeDefinition allowExeDefinition,
            bool                            restartOnExternalChanges,
            bool                            requirePermission) {

            if (    allowLocation               != this.AllowLocation               ||
                    allowDefinition             != this.AllowDefinition             ||
                    allowExeDefinition          != this.AllowExeDefinition          ||
                    restartOnExternalChanges    != this.RestartOnExternalChanges    ||
                    requirePermission           != this.RequirePermission) {

                return false;
            }

            return IsEquivalentType(host, typeName);
        }

        //
        // Errors associated with the parse of a factory.
        //
        internal List<ConfigurationException> Errors {
            get {
                return _errors;
            }
        }

        internal bool HasErrors {
            get {
                return ErrorsHelper.GetHasErrors(_errors);
            }
        }

        internal void AddErrors(ICollection<ConfigurationException> coll) {
            ErrorsHelper.AddErrors(ref _errors, coll);
        }

        internal void ThrowOnErrors() {
            ErrorsHelper.ThrowOnErrors(_errors);
        }

        internal bool IsIgnorable() {
            if (_factory != null)
                return (_factory is IgnoreSectionHandler);
            else if (_factoryTypeName != null)
                return _factoryTypeName.Contains("System.Configuration.IgnoreSection");
            else
                return false;
        }
    }
}
