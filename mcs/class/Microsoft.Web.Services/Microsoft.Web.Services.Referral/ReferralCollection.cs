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

                public virtual void CopyTo (Array array, int index)
                {
                        list.CopyTo (array, index);
                }

		public virtual int Count {
			get { return list.Count; }
		}

                public virtual IEnumerator GetEnumerator ()
                {
                        return list.GetEnumerator ();
                }

                [MonoTODO]
                public virtual XmlElement GetXml (XmlDocument document)
                {
                        if (document == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("Argument is null."));

                        throw new NotImplementedException ();
                }

		public virtual bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

                [MonoTODO]
                public virtual void LoadXml (XmlElement element)
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

		public virtual object SyncRoot {
			get { return list.SyncRoot; }
		}
        }
}
