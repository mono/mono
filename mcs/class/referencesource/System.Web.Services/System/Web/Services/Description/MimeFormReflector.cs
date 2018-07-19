//------------------------------------------------------------------------------
// <copyright file="MimeFormReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;

    internal class MimeFormReflector : MimeReflector {
        internal override bool ReflectParameters() {
            if (!HtmlFormParameterReader.IsSupported(ReflectionContext.Method))
                return false;
            ReflectionContext.ReflectStringParametersMessage();
            MimeContentBinding mimeContentBinding = new MimeContentBinding();
            mimeContentBinding.Type = HtmlFormParameterReader.MimeType;
            ReflectionContext.OperationBinding.Input.Extensions.Add(mimeContentBinding);
            return true;
        }

        internal override bool ReflectReturn() {
            return false;
        }
    }
}
