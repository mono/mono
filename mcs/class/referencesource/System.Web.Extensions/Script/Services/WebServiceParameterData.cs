//------------------------------------------------------------------------------
// <copyright file="WebServiceParameterData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;

namespace System.Web.Script.Services {
    internal class WebServiceParameterData {
        private ParameterInfo _param;
        private int _index;     // Index of the parameter in the method
        private string _paramName;
        private Type _paramType;

        internal WebServiceParameterData(ParameterInfo param, int index) {
            _param = param;
            _index = index;
        }
        
        // This constructor is only used by indigo
        internal WebServiceParameterData(string paramName, Type paramType, int index) {
            _paramName = paramName;
            _paramType = paramType;
            _index = index;

        }

        internal int Index {
            get { return _index; }
        }

        internal ParameterInfo ParameterInfo {
            get { return _param; }
        }

        internal string ParameterName {
            get {
                if (_param != null) {
                    return _param.Name;
                }
                else {
                    return _paramName;
                }
            }
        }

        internal Type ParameterType {
            get {
                if (_param != null) {
                    return _param.ParameterType;
                }
                else {
                    return _paramType;
                }
            }
        }
       
    }
}
