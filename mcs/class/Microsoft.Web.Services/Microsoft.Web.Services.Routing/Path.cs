//
// Microsoft.Web.Services.Routing.Path.cs
//
// Name: Duncan Mak (duncan@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Globalization;
using System.Web.Services.Protocols;
using System.Xml;

namespace Microsoft.Web.Services.Routing {

        [MonoTODO]
        public class Path : SoapHeader, ICloneable
        {
                XmlElement element;
                
                public Path ()
                {
                }

                public Path (XmlElement element)
                {
                        this.element = element;
                }

                [MonoTODO]
                public void CheckValid ()
                {
                        
                }

                [MonoTODO]
                public XmlElement GetXml (XmlDocument document)
                {
                        if (document == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null."));
			throw new NotImplementedException ();
                }

                [MonoTODO]
                public void LoadXml (XmlElement element)
                {
                        if (element == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null."));
                }

                [MonoTODO]
                public virtual object Clone ()
                {
                        throw new NotImplementedException ();
                }
        }
}
