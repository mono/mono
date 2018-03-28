//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691
namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class JsonFaultDetail
    {
        ExceptionDetail exceptionDetail;
        string exceptionType;
        string message;
        string stackTrace;


        [DataMember(Name = "ExceptionDetail")]
        public ExceptionDetail ExceptionDetail
        {
           get
           {
              return exceptionDetail;
           }
           set
           {
              exceptionDetail = value;
           }
        }

        [DataMember(Name = "ExceptionType")]
        public string ExceptionType
        {
           get
           {
              return exceptionType;
           }
           set
           {
              exceptionType = value;
           }
        }

        [DataMember(Name = "Message")]
        public string Message
        {
           get
           {
               return message;
           }
           set
           {
               message = value;
           }
        }

        [DataMember(Name = "StackTrace")]
        public string StackTrace
        {
           get
           {
               return stackTrace;
           }
           set
           {
               stackTrace = value;
           }
        }
    }


}
