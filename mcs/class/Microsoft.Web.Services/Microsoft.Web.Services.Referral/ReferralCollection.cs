//
// Microsoft.Web.Services.Referral.ReferralCollection.cs
//
// Name: Duncan Mak (duncan@ximian.com)
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Collections;
using System.Globalization;
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Referral {

        public class ReferralCollection : SoapHeader, ICollection, IEnumerable, IXmlElement
        {
                ArrayList list;
                
                public ReferralCollection ()
                {
                        list = new ArrayList ();
                }

                public void Add (Referral referral)
                {
                        list.Add (referral);
                }

                public void AddRange (Referral [] referrals)
                {
                        list.AddRange (referrals);
                }

                public void Clear ()
                {
                        list.Clear ();
                }

                public bool Contains (Referral referral)
                {
                        return list.Contains (referral);
                }

                public bool Contains (Uri uri)
                {
                        return list.Contains (new Referral (uri));
                }

#if WSE1
		public void CopyTo (Array array, int index)
#else
		public virtual void CopyTo (Array array, int index)
#endif
                {
                        list.CopyTo (array, index);
                }

#if WSE1
		public int Count {
#else
		public virtual int Count {
#endif
			get { return list.Count; }
		}

#if WSE1
                public IEnumerator GetEnumerator ()
#else
		public virtual IEnumerator GetEnumerator ()
#endif
		{
                        return list.GetEnumerator ();
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
#if WSE1
		public bool IsSynchronized {
#else
		public virtual bool IsSynchronized {
#endif
			get { return list.IsSynchronized; }
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

                public void Remove (Referral referral)
                {
                        list.Remove (referral);
                }

                public void Remove (Uri uri)
                {
                        list.Remove (new Referral (uri));
                }

#if WSE1
		public object SyncRoot {
#else
		public virtual object SyncRoot {
#endif
			get { return list.SyncRoot; }
		}
        }
}
