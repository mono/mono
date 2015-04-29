//------------------------------------------------------------------------------
// <copyright file="SectionXmlInfo.cs" company="Microsoft">
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

    internal sealed class SectionXmlInfo : IConfigErrorInfo {
        private string  _configKey;                 // configKey
        private string  _definitionConfigPath;      // the config path of the configuration record where this directive was defined
        private string  _targetConfigPath;          // the full config path this location directive applies to
        private string  _subPath;                   // the "path" attribute specified in the <location> directive  
        private string  _filename;                  // filename containg this section definition
        private int     _lineNumber;                // line number where definition occurs
        private object  _streamVersion;             // version of the filestream containing this section
        private string  _configSource;              // the "configSource" attribute
        private string  _configSourceStreamName;    // filename of the configSource
        private object  _configSourceStreamVersion; // version of the configSource filestream
        private bool    _skipInChildApps;           // skip inheritence by child apps?                             
        private string  _rawXml;                    // raw xml input of the section
        private string  _protectionProviderName;    // name of the protection provider

        private OverrideModeSetting _overrideMode;  // override mode for child config paths

        internal SectionXmlInfo(
                string configKey, string definitionConfigPath, string targetConfigPath, string subPath,
                string filename, int lineNumber, object streamVersion, 
                string rawXml, string configSource, string configSourceStreamName, object configSourceStreamVersion,
                string protectionProviderName, OverrideModeSetting overrideMode, bool skipInChildApps) {

            _configKey = configKey;
            _definitionConfigPath = definitionConfigPath;
            _targetConfigPath = targetConfigPath;
            _subPath = subPath;
            _filename = filename;
            _lineNumber = lineNumber;
            _streamVersion  = streamVersion;
            _rawXml = rawXml;
            _configSource = configSource;
            _configSourceStreamName = configSourceStreamName;
            _configSourceStreamVersion = configSourceStreamVersion;
            _protectionProviderName = protectionProviderName;
            _overrideMode = overrideMode;
            _skipInChildApps = skipInChildApps;
        }


        // IConfigErrorInfo interface
        public string Filename {
            get {return _filename;}
        }

        public int LineNumber {
            get {return _lineNumber;}
            set {_lineNumber = value;}
        }

        // other access methods
        internal object StreamVersion {
            get {return _streamVersion;}
            set {_streamVersion = value;}
        }

        internal string ConfigSource {
            get {return _configSource;}
            set {_configSource = value;}
        }

        internal string ConfigSourceStreamName {
            get {return _configSourceStreamName;}
            set {_configSourceStreamName = value;}
        }

        internal object ConfigSourceStreamVersion {
#if UNUSED_CODE
            get {return _configSourceStreamVersion; }
#endif

            set {_configSourceStreamVersion = value; }
        }

        internal string ConfigKey {
            get {return _configKey;}
        }

        internal string DefinitionConfigPath {
            get {return _definitionConfigPath;}
        }

        internal string TargetConfigPath {
            get {return _targetConfigPath;}
            set {_targetConfigPath = value;}
        }

        internal string SubPath {
            get {return _subPath;}
        }

        internal string RawXml {
            get {return _rawXml;}
            set {_rawXml = value;}
        }

        internal string ProtectionProviderName {
            get {return _protectionProviderName;}
            set {_protectionProviderName = value;}
        }

        internal OverrideModeSetting OverrideModeSetting {
            get { return _overrideMode; }
            set { _overrideMode = value; }
        }

        internal bool SkipInChildApps {
            get {return _skipInChildApps;}
            set {_skipInChildApps = value;}
        }
    }
}
