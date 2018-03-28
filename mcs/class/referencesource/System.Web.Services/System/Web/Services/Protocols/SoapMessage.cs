//------------------------------------------------------------------------------
// <copyright file="SoapMessage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class SoapMessage {
        SoapMessageStage stage;
        SoapHeaderCollection headers = new SoapHeaderCollection();
        Stream stream;
        SoapExtensionStream extensionStream;
        string contentType;
        string contentEncoding;
        object[] parameterValues;
        SoapException exception;

        internal SoapMessage() { }

        internal void SetParameterValues(object[] parameterValues) {
            this.parameterValues = parameterValues;
        }

        internal object[] GetParameterValues() {
            return parameterValues;
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.OneWay"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract bool OneWay {
            get;
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.GetInParameterValue"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object GetInParameterValue(int index) {
            EnsureInStage();
            EnsureNoException();
            if (index < 0 || index >= parameterValues.Length) throw new IndexOutOfRangeException(Res.GetString(Res.indexMustBeBetweenAnd0Inclusive, parameterValues.Length));
            return parameterValues[index];
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.GetOutParameterValue"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object GetOutParameterValue(int index) {
            EnsureOutStage();
            EnsureNoException();
            if (!MethodInfo.IsVoid) {
                if (index == int.MaxValue)
                    throw new IndexOutOfRangeException(Res.GetString(Res.indexMustBeBetweenAnd0Inclusive, parameterValues.Length));
                index++;
            }
            if (index < 0 || index >= parameterValues.Length) throw new IndexOutOfRangeException(Res.GetString(Res.indexMustBeBetweenAnd0Inclusive, parameterValues.Length));
            return parameterValues[index];
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.GetReturnValue"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object GetReturnValue() {
            EnsureOutStage();
            EnsureNoException();
            if (MethodInfo.IsVoid) throw new InvalidOperationException(Res.GetString(Res.WebNoReturnValue));
            return parameterValues[0];
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.EnsureOutStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void EnsureOutStage();
        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.EnsureInStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected abstract void EnsureInStage();

        void EnsureNoException() {
            if (exception != null) throw new InvalidOperationException(Res.GetString(Res.WebCannotAccessValue), exception);
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.Exception"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapException Exception {
            get { return exception; }
            set { exception = value; }
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.MethodInfo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract LogicalMethodInfo MethodInfo {
            get;
        }

        /*
        internal abstract SoapReflectedExtension[] Extensions {
            get;
        }

        internal abstract object[] ExtensionInitializers {
            get;
        }
        */

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.EnsureStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void EnsureStage(SoapMessageStage stage) {
            if ((this.stage & stage) == 0) throw new InvalidOperationException(Res.GetString(Res.WebCannotAccessValueStage, this.stage.ToString()));
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.Headers"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHeaderCollection Headers {
            get { return headers; }
        }

        internal void SetStream(Stream stream) {
            if (extensionStream != null) {
                extensionStream.SetInnerStream(stream);
                extensionStream.SetStreamReady();
                // The extension stream should now be referenced by either this.stream
                // or an extension that has chained it to another stream.
                extensionStream = null;
            }
            else
                this.stream = stream;
        }

        internal void SetExtensionStream(SoapExtensionStream extensionStream) {
            this.extensionStream = extensionStream;
            this.stream = extensionStream;
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.Stream"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Stream Stream {
            get { return stream; }
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.ContentType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string ContentType {
            get { EnsureStage(SoapMessageStage.BeforeSerialize | SoapMessageStage.BeforeDeserialize); return contentType; }
            set { EnsureStage(SoapMessageStage.BeforeSerialize | SoapMessageStage.BeforeDeserialize); contentType = value; }
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.ContentEncoding"]/*' />
        public string ContentEncoding {
            get { EnsureStage(SoapMessageStage.BeforeSerialize | SoapMessageStage.BeforeDeserialize); return contentEncoding; }
            set { EnsureStage(SoapMessageStage.BeforeSerialize | SoapMessageStage.BeforeDeserialize); contentEncoding = value; }
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.Stage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapMessageStage Stage {
            get { return stage; }
        }

        internal void SetStage(SoapMessageStage stage) {
            this.stage = stage;
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.Url"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract string Url {
            get;
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.Action"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract string Action {
            get;
        }

        /// <include file='doc\SoapMessage.uex' path='docs/doc[@for="SoapMessage.SoapVersion"]/*' />
        [ComVisible(false)]
        [DefaultValue(SoapProtocolVersion.Default)]
        public virtual SoapProtocolVersion SoapVersion {
            get { return SoapProtocolVersion.Default; }
        }

        internal static SoapExtension[] InitializeExtensions(SoapReflectedExtension[] reflectedExtensions, object[] extensionInitializers) {
            if (reflectedExtensions == null)
                return null;
            SoapExtension[] extensions = new SoapExtension[reflectedExtensions.Length];
            for (int i = 0; i < extensions.Length; i++) {
                extensions[i] = reflectedExtensions[i].CreateInstance(extensionInitializers[i]);
            }
            return extensions;
        }

        internal void InitExtensionStreamChain(SoapExtension[] extensions) {
            if (extensions == null)
                return;
            for (int i = 0; i < extensions.Length; i++) {
                stream = extensions[i].ChainStream(stream);
            }
        }

        internal void RunExtensions(SoapExtension[] extensions, bool throwOnException) {
            if (extensions == null)
                return;

            TraceMethod caller = Tracing.On ? new TraceMethod(this, "RunExtensions", extensions, throwOnException) : null;

            // Higher priority extensions (earlier in the list) run earlier for deserialization stages,
            // and later for serialization stages
            if ((stage & (SoapMessageStage.BeforeDeserialize | SoapMessageStage.AfterDeserialize)) != 0) {
                for (int i = 0; i < extensions.Length; i++) {
                    if (Tracing.On) Tracing.Enter("SoapExtension", caller, new TraceMethod(extensions[i], "ProcessMessage", stage));
                    extensions[i].ProcessMessage(this);
                    if (Tracing.On) Tracing.Exit("SoapExtension", caller);
                    if (Exception != null) {
                        if (throwOnException)
                            throw Exception;
                        if (Tracing.On) Tracing.ExceptionIgnore(TraceEventType.Warning, caller, Exception);
                    }
                }
            }
            else {
                for (int i = extensions.Length - 1; i >= 0; i--) {
                    if (Tracing.On) Tracing.Enter("SoapExtension", caller, new TraceMethod(extensions[i], "ProcessMessage", stage));
                    extensions[i].ProcessMessage(this);
                    if (Tracing.On) Tracing.Exit("SoapExtension", caller);
                    if (Exception != null) {
                        if (throwOnException)
                            throw Exception;
                        if (Tracing.On) Tracing.ExceptionIgnore(TraceEventType.Warning, caller, Exception);
                    }
                }
            }
        }
    }
}
