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
#if WSE1
		XmlElement element;
#endif
		private ViaCollection _forward;
		private ViaCollection _reverse;
		private RoutingFault _fault;

#if WSE1
		internal Path () {}
#else
		public Path () {
			Actor = "http://schemas.xmlsoap.org/soap/actor/next";
			MustUnderstand = true;
		}
#endif

#if WSE1
                internal Path (XmlElement element)
#else
                public Path (XmlElement element) : base ()
#endif
                {
#if WSE1
                        this.element = element;
#else
			LoadXml (element);
#endif
                }

#if WSE2
                public void CheckValid ()
                {
			if(Actor.Length == 0 || Actor != "http://schemas.xmlsoap.org/soap/actor/next")
			{
				throw new RoutingFormatException ("Bad Actor value");
			}
                }
#endif

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
			throw new NotImplementedException ();
                }

#if WSE1
		[MonoTODO]
                public object Clone ()
#else
                public virtual object Clone ()
#endif
                {
#if WSE1
			throw new NotImplementedException ();
#else
			Path newPath = new Path ();

			if(_forward != null) {
				newPath._forward = _forward.Clone () as ViaCollection;
			}

			if(_reverse != null) {
				newPath._reverse = _reverse.Clone () as ViaCollection;
			}

			newPath._fault = _fault;

			return newPath;
#endif
                }

#if !WSE1	
		[Obsolete]
		public string Action {
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
#else
		[MonoTODO]
		public string Action { 
			get { return null; }
			set { ; }
		}
#endif

		public RoutingFault Fault {
			get { return _fault; }
			set { _fault = value; }
		}

#if !WSE1	
		[Obsolete]
		public Uri From {
			get { throw new InvalidOperationException (); }
			set { throw new InvalidOperationException (); }
		}
#else
		[MonoTODO]
		public Uri From { 
			get { return null; }
			set { ; }
		}
#endif

		public ViaCollection Fwd { 
			get { 
				if(_forward == null) {
					_forward = new ViaCollection ();
				}
				return _forward;
			}
		}

#if !WSE1	
		[Obsolete("Use SoapContext.MessageId")]
		public Uri Id {
			get { throw new InvalidOperationException (); }
		}
#else
		[MonoTODO]
		public Uri Id { 
			get { return null; }
		}
#endif

#if !WSE1	
		[Obsolete("Use SoapContext.RelatesTo")]
		public Uri RelatesTo {
			get { throw new InvalidOperationException (); }
			set { throw new InvalidOperationException (); }
		}
#else
		[MonoTODO]
		public Uri RelatesTo { 
			get { return null; }
			set { ; }
		}
#endif

		public ViaCollection Rev { 
			get { 
				if(_reverse == null) {
					_reverse = new ViaCollection ();
				}
				return _reverse;
			}
			set { _reverse = value; }
		}

#if !WSE1	
		[Obsolete("Use SoapContext.To")]
		public Uri To {
			get { throw new InvalidOperationException (); }
		}
#else
		[MonoTODO]
		public Uri To { 
			get { return null; }
		}
#endif
        }
}
