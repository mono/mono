//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    using System;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.ServiceModel.Administration;
    using System.Collections.Generic;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebGetAttribute : Attribute, IOperationContractAttributeProvider, IOperationBehavior, IWmiInstanceProvider
    {
        WebMessageBodyStyle bodyStyle;
        bool isBodyStyleDefined;
        bool isRequestMessageFormatSet;
        bool isResponseMessageFormatSet;
        WebMessageFormat requestMessageFormat;
        WebMessageFormat responseMessageFormat;
        string uriTemplate; // Note: HttpTransferEndpointBehavior interprets uriTemplate as: null means 'no opinion', whereas string.Empty means relative path of ""

        public WebGetAttribute()
        {
        }

        public WebMessageBodyStyle BodyStyle
        {
            get { return this.bodyStyle; }
            set
            {
                if (!WebMessageBodyStyleHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.bodyStyle = value;
                this.isBodyStyleDefined = true;
            }
        }

        public bool IsBodyStyleSetExplicitly
        {
            get { return this.isBodyStyleDefined; }
        }

        public bool IsRequestFormatSetExplicitly
        {
            get { return this.isRequestMessageFormatSet; }
        }

        public bool IsResponseFormatSetExplicitly
        {
            get { return this.isResponseMessageFormatSet; }
        }
        public WebMessageFormat RequestFormat
        {
            get
            {

                return this.requestMessageFormat;
            }
            set
            {
                if (!WebMessageFormatHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.requestMessageFormat = value;
                this.isRequestMessageFormatSet = true;
            }
        }

        public WebMessageFormat ResponseFormat
        {
            get
            {

                return this.responseMessageFormat;
            }
            set
            {
                if (!WebMessageFormatHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.responseMessageFormat = value;
                this.isResponseMessageFormatSet = true;
            }
        }

        public string UriTemplate
        { get { return this.uriTemplate; } set { this.uriTemplate = value; } }

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        } //  do nothing 
        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        } //  do nothing 
        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        } //  do nothing 
        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
        } //  do nothing 

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            if (wmiInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("wmiInstance");
            }
            wmiInstance.SetProperty("BodyStyle", this.BodyStyle.ToString());
            wmiInstance.SetProperty("IsBodyStyleSetExplicitly", this.IsBodyStyleSetExplicitly.ToString());
            wmiInstance.SetProperty("RequestFormat", this.RequestFormat.ToString());
            wmiInstance.SetProperty("IsRequestFormatSetExplicitly", this.IsRequestFormatSetExplicitly.ToString());
            wmiInstance.SetProperty("ResponseFormat", this.ResponseFormat.ToString());
            wmiInstance.SetProperty("IsResponseFormatSetExplicitly", this.IsResponseFormatSetExplicitly.ToString());
            wmiInstance.SetProperty("UriTemplate", this.UriTemplate);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "WebGetAttribute";
        }

        internal WebMessageBodyStyle GetBodyStyleOrDefault(WebMessageBodyStyle defaultStyle)
        {
            if (this.IsBodyStyleSetExplicitly)
            {
                return this.BodyStyle;
            }
            else
            {
                return defaultStyle;
            }
        }

        OperationContractAttribute IOperationContractAttributeProvider.GetOperationContractAttribute()
        {
            return new OperationContractAttribute();
        }
    }
}

