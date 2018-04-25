//------------------------------------------------------------------------------
// <copyright file="XmlExtensionFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

namespace System.Xml.Xsl.Runtime {
    using Res           = System.Xml.Utils.Res;

    /// <summary>
    /// Table of bound extension functions.  Once an extension function is bound and entered into the table, future bindings
    /// will be very fast.  This table is not thread-safe.
    /// </summary>
    internal class XmlExtensionFunctionTable {
        private Dictionary<XmlExtensionFunction, XmlExtensionFunction> table;
        private XmlExtensionFunction funcCached;

        public XmlExtensionFunctionTable() {
            this.table = new Dictionary<XmlExtensionFunction, XmlExtensionFunction>();
        }

        public XmlExtensionFunction Bind(string name, string namespaceUri, int numArgs, Type objectType, BindingFlags flags) {
            XmlExtensionFunction func;

            if (this.funcCached == null)
                this.funcCached = new XmlExtensionFunction();

            // If the extension function already exists in the table, then binding has already been performed
            this.funcCached.Init(name, namespaceUri, numArgs, objectType, flags);
            if (!this.table.TryGetValue(this.funcCached, out func)) {
                // Function doesn't exist, so bind it and enter it into the table
                func = this.funcCached;
                this.funcCached = null;

                func.Bind();
                this.table.Add(func, func);
            }

            return func;
        }
    }

    /// <summary>
    /// This internal class contains methods that allow binding to extension functions and invoking them.
    /// </summary>
    internal class XmlExtensionFunction {
        private string namespaceUri;                // Extension object identifier
        private string name;                        // Name of this method
        private int numArgs;                        // Argument count
        private Type objectType;                    // Type of the object which will be searched for matching methods
        private BindingFlags flags;                 // Modifiers that were used to search for a matching signature
        private int hashCode;                       // Pre-computed hashcode

        private MethodInfo meth;                    // MethodInfo for extension function
        private Type[] argClrTypes;                 // Type array for extension function arguments
        private Type retClrType;                    // Type for extension function return value
        private XmlQueryType[] argXmlTypes;         // XmlQueryType array for extension function arguments
        private XmlQueryType retXmlType;            // XmlQueryType for extension function return value

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlExtensionFunction() {
        }

        /// <summary>
        /// Constructor (directly binds to passed MethodInfo).
        /// </summary>
        public XmlExtensionFunction(string name, string namespaceUri, MethodInfo meth) {
            this.name = name;
            this.namespaceUri = namespaceUri;
            Bind(meth);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlExtensionFunction(string name, string namespaceUri, int numArgs, Type objectType, BindingFlags flags) {
            Init(name, namespaceUri, numArgs, objectType, flags);
        }

        /// <summary>
        /// Initialize, but do not bind.
        /// </summary>
        public void Init(string name, string namespaceUri, int numArgs, Type objectType, BindingFlags flags) {
            this.name = name;
            this.namespaceUri = namespaceUri;
            this.numArgs = numArgs;
            this.objectType = objectType;
            this.flags = flags;
            this.meth = null;
            this.argClrTypes = null;
            this.retClrType = null;
            this.argXmlTypes = null;
            this.retXmlType = null;

            // Compute hash code so that it is not recomputed each time GetHashCode() is called
            this.hashCode = namespaceUri.GetHashCode() ^ name.GetHashCode() ^ ((int) flags << 16) ^ (int) numArgs;
        }

        /// <summary>
        /// Once Bind has been successfully called, Method will be non-null.
        /// </summary>
        public MethodInfo Method {
            get { return this.meth; }
        }

        /// <summary>
        /// Once Bind has been successfully called, the Clr type of each argument can be accessed.
        /// Note that this may be different than Method.GetParameterInfo().ParameterType.
        /// </summary>
        public Type GetClrArgumentType(int index) {
            return this.argClrTypes[index];
        }

        /// <summary>
        /// Once Bind has been successfully called, the Clr type of the return value can be accessed.
        /// Note that this may be different than Method.GetParameterInfo().ReturnType.
        /// </summary>
        public Type ClrReturnType {
            get { return this.retClrType; }
        }

        /// <summary>
        /// Once Bind has been successfully called, the inferred Xml types of the arguments can be accessed.
        /// </summary>
        public XmlQueryType GetXmlArgumentType(int index) {
            return this.argXmlTypes[index];
        }

        /// <summary>
        /// Once Bind has been successfully called, the inferred Xml type of the return value can be accessed.
        /// </summary>
        public XmlQueryType XmlReturnType {
            get { return this.retXmlType; }
        }

        /// <summary>
        /// Return true if the CLR type specified in the Init() call has a matching method.
        /// </summary>
        public bool CanBind() {
            MethodInfo[] methods = this.objectType.GetMethods(this.flags);
            bool ignoreCase = (this.flags & BindingFlags.IgnoreCase) != 0;
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // Find method in object type
            foreach (MethodInfo methSearch in methods) {
                if (methSearch.Name.Equals(this.name, comparison) && (this.numArgs == -1 || methSearch.GetParameters().Length == this.numArgs)) {
                    // Binding to generic methods will never succeed
                    if (!methSearch.IsGenericMethodDefinition)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Bind to the CLR type specified in the Init() call.  If a matching method cannot be found, throw an exception.
        /// </summary>
        public void Bind() {
            MethodInfo[] methods = this.objectType.GetMethods(this.flags);
            MethodInfo methMatch = null;
            bool ignoreCase = (this.flags & BindingFlags.IgnoreCase) != 0;
            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // Find method in object type
            foreach (MethodInfo methSearch in methods) {
                if (methSearch.Name.Equals(this.name, comparison) && (this.numArgs == -1 || methSearch.GetParameters().Length == this.numArgs)) {
                    if (methMatch != null)
                        throw new XslTransformException(/*[XT_037]*/Res.XmlIl_AmbiguousExtensionMethod, this.namespaceUri, this.name, this.numArgs.ToString(CultureInfo.InvariantCulture));

                    methMatch = methSearch;
                }
            }

            if (methMatch == null) {
                methods = this.objectType.GetMethods(this.flags | BindingFlags.NonPublic);
                foreach (MethodInfo methSearch in methods) {
                    if (methSearch.Name.Equals(this.name, comparison) && methSearch.GetParameters().Length == this.numArgs)
                        throw new XslTransformException(/*[XT_038]*/Res.XmlIl_NonPublicExtensionMethod, this.namespaceUri, this.name);
                }
                throw new XslTransformException(/*[XT_039]*/Res.XmlIl_NoExtensionMethod, this.namespaceUri, this.name, this.numArgs.ToString(CultureInfo.InvariantCulture));
            }

            if (methMatch.IsGenericMethodDefinition)
                throw new XslTransformException(/*[XT_040]*/Res.XmlIl_GenericExtensionMethod, this.namespaceUri, this.name);

            Debug.Assert(methMatch.ContainsGenericParameters == false);

            Bind(methMatch);
        }

        /// <summary>
        /// Bind to the specified MethodInfo.
        /// </summary>
        private void Bind(MethodInfo meth) {
            ParameterInfo[] paramInfo = meth.GetParameters();
            int i;

            // Save the MethodInfo
            this.meth = meth;

            // Get the Clr type of each parameter
            this.argClrTypes = new Type[paramInfo.Length];
            for (i = 0; i < paramInfo.Length; i++)
                this.argClrTypes[i] = GetClrType(paramInfo[i].ParameterType);

            // Get the Clr type of the return value
            this.retClrType = GetClrType(this.meth.ReturnType);

            // Infer an Xml type for each Clr type
            this.argXmlTypes = new XmlQueryType[paramInfo.Length];
            for (i = 0; i < paramInfo.Length; i++) {
                this.argXmlTypes[i] = InferXmlType(this.argClrTypes[i]);

                // 






                if (this.namespaceUri.Length == 0) {
                    if ((object) this.argXmlTypes[i] == (object) XmlQueryTypeFactory.NodeNotRtf)
                        this.argXmlTypes[i] = XmlQueryTypeFactory.Node;
                    else if ((object) this.argXmlTypes[i] == (object) XmlQueryTypeFactory.NodeSDod)
                        this.argXmlTypes[i] = XmlQueryTypeFactory.NodeS;
                }
                else {
                    if ((object) this.argXmlTypes[i] == (object) XmlQueryTypeFactory.NodeSDod)
                        this.argXmlTypes[i] = XmlQueryTypeFactory.NodeNotRtfS;
                }
            }

            // Infer an Xml type for the return Clr type
            this.retXmlType = InferXmlType(this.retClrType);
        }

        /// <summary>
        /// Convert the incoming arguments to an array of CLR objects, and then invoke the external function on the "extObj" object instance.
        /// </summary>
        public object Invoke(object extObj, object[] args) {
            Debug.Assert(this.meth != null, "Must call Bind() before calling Invoke.");
            Debug.Assert(args.Length == this.argClrTypes.Length, "Mismatched number of actual and formal arguments.");

            try {
                return this.meth.Invoke(extObj, this.flags, null, args, CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException e) {
                throw new XslTransformException(e.InnerException, Res.XmlIl_ExtensionError, this.name);
            }
            catch (Exception e) {
                if (!XmlException.IsCatchableException(e)) {
                    throw;
                }
                throw new XslTransformException(e, Res.XmlIl_ExtensionError, this.name);
            }
        }

        /// <summary>
        /// Return true if this XmlExtensionFunction has the same values as another XmlExtensionFunction.
        /// </summary>
        public override bool Equals(object other) {
            XmlExtensionFunction that = other as XmlExtensionFunction;
            Debug.Assert(that != null);

            // Compare name, argument count, object type, and binding flags
            return (this.hashCode == that.hashCode && this.name == that.name && this.namespaceUri == that.namespaceUri &&
                    this.numArgs == that.numArgs && this.objectType == that.objectType && this.flags == that.flags);
        }

        /// <summary>
        /// Return this object's hash code, previously computed for performance.
        /// </summary>
        public override int GetHashCode() {
            return this.hashCode;
        }

        /// <summary>
        /// 1. Map enumerations to the underlying integral type.
        /// 2. Throw an exception if the type is ByRef
        /// </summary>
        private Type GetClrType(Type clrType) {
            if (clrType.IsEnum)
                return Enum.GetUnderlyingType(clrType);

            if (clrType.IsByRef)
                throw new XslTransformException(/*[XT_050]*/Res.XmlIl_ByRefType, this.namespaceUri, this.name);

            return clrType;
        }

        /// <summary>
        /// Infer an Xml type from a Clr type using Xslt infererence rules
        /// </summary>
        private XmlQueryType InferXmlType(Type clrType) {
            return XsltConvert.InferXsltType(clrType);
        }
    }
}
