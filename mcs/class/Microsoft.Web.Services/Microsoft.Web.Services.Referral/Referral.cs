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

                public virtual object Clone ()
                {
                        return new Referral (uri);
                }

                [MonoTODO]
                public virtual XmlElement GetXml (XmlDocument document)
                {
                        if (document == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null."));

                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public virtual void LoadXml (XmlElement element)
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
		public  For For{
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
