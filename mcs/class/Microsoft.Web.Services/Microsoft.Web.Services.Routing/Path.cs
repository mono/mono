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
#if WSE1
		internal Path () {}
#else
		public Path () {}
#endif

#if WSE1
                internal Path (XmlElement element)
#else
                public Path (XmlElement element)
#endif
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
#if WSE1
                public object Clone ()
#else
                public virtual object Clone ()
#endif
                {
                        throw new NotImplementedException ();
                }

#if !WSE1	
		[Obsolete]
#endif
		[MonoTODO]
		public string Action { 
			get { return null; }
			set { ; }
		}

/*		[MonoTODO]
		public RoutingFault Fault {
			get { return null; }
			set { ; }
		}*/

#if !WSE1	
		[Obsolete]
#endif
		[MonoTODO]
		public Uri From { 
			get { return null; }
			set { ; }
		}

		[MonoTODO]
		public ViaCollection Fwd { 
			get { return null; }
		}

#if !WSE1	
		[Obsolete]
#endif
		[MonoTODO]
		public Uri Id { 
			get { return null; }
		}

#if !WSE1	
		[Obsolete]
#endif
		[MonoTODO]
		public Uri RelatesTo { 
			get { return null; }
			set { ; }
		}

		[MonoTODO]
		public ViaCollection Rev { 
			get { return null; }
			set { ; }
		}

#if !WSE1	
		[Obsolete]
#endif
		[MonoTODO]
		public Uri To { 
			get { return null; }
		}
        }
}
