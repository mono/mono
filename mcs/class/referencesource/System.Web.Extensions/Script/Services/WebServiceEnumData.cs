//------------------------------------------------------------------------------
// <copyright file="WebServiceEnumData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Globalization;

namespace System.Web.Script.Services {
    using System;

    internal class WebServiceEnumData : WebServiceTypeData {
        bool isULong;
        string[] names;
        long[] values;

        internal WebServiceEnumData(string typeName, string typeNamespace, string[] names, long[] values, bool isULong)
            : base(typeName, typeNamespace) {
            InitWebServiceEnumData(names, values, isULong);
        }

        internal WebServiceEnumData(string typeName, string typeNamespace, Type t, string[] names, long[] values, bool isULong)
            : base(typeName, typeNamespace, t) {
            InitWebServiceEnumData(names, values, isULong);
        }

        internal WebServiceEnumData(string typeName, string typeNamespace, string[] names, Array values, bool isULong)
            : base(typeName, typeNamespace) {
            InitWebServiceEnumData(names, values, isULong);
        }

        internal WebServiceEnumData(string typeName, string typeNamespace, Type t, string[] names, Array values, bool isULong)
            : base(typeName, typeNamespace) {
            InitWebServiceEnumData(names, values, isULong);
        }

        internal bool IsULong {
            get {
                return isULong;
            }
        }

        internal string[] Names {
            get {
                return names;
            }
        }

        internal long[] Values {
            get {
                return values;
            }
        }

        private void InitWebServiceEnumData(string[] names, long[] values, bool isULong) {
            System.Diagnostics.Debug.Assert(names != null);
            System.Diagnostics.Debug.Assert(values != null);
            System.Diagnostics.Debug.Assert(names.Length == values.Length);
            this.names = names;
            this.values = values;
            this.isULong = isULong;
        }

        private void InitWebServiceEnumData(string[] names, Array values, bool isULong) {
            System.Diagnostics.Debug.Assert(names != null);
            System.Diagnostics.Debug.Assert(values != null);
            System.Diagnostics.Debug.Assert(names.Length == values.Length);
            this.names = names;
            this.values = new long[values.Length];
            for (int i = 0; i < values.Length; i++) {
                object enumValue = values.GetValue(i);
                if (isULong) {
                    this.values[i] = (long)((IConvertible)enumValue).ToUInt64(CultureInfo.InvariantCulture);
                }
                else {
                    this.values[i] = ((IConvertible)enumValue).ToInt64(CultureInfo.InvariantCulture);
                }
            }
            this.isULong = isULong;
        }

    }
}
