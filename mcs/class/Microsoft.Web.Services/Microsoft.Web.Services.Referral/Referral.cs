//
// Microsoft.Web.Services.Referral.Referral.cs
//
// Authors:
//      Duncan Mak (duncan@ximian.com)
//      Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Collections;
using System.Globalization;
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.Web.Services.Routing;
#if !WSE1
using Microsoft.Web.Services.Xml;
#endif

namespace Microsoft.Web.Services.Referral {

        public class Referral : ICloneable, IXmlElement
        {
                Uri uri;

                public Referral ()
                {
                }

                public Referral (Uri uri)
                {
                        this.uri = uri;
                }

                [MonoTODO]
                public void CheckValid ()
                {
                }

#if WSE1
                public object Clone ()
#else
                public virtual object Clone ()
#endif
                {
                        return new Referral (uri);
                }

                [MonoTODO]
#if WSE1
                public XmlElement GetXml (XmlDocument document)
#else
                public virtual XmlElement GetXml (XmlDocument document)
#endif
                {
                        if (document == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null."));

                        throw new NotImplementedException ();
                }

                [MonoTODO]
#if WSE1
		public void LoadXml (XmlElement element)
#else
		public virtual void LoadXml (XmlElement element)
#endif
		{
                        if (element == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null."));

                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public Desc Desc {
                        get {
                                throw new NotImplementedException ();
                        }
                }

		[MonoTODO]
		public ViaCollection Go {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public For For{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public If If {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Uri RefId {
			get {
				throw new NotImplementedException ();
			}
			set {

				if (value == null)
					throw new ArgumentNullException ();
				
				throw new NotImplementedException ();
			}
		}
        }
}
