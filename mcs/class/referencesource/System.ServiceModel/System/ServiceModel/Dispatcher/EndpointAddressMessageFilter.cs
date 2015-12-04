//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    using QName = System.ServiceModel.Dispatcher.EndpointAddressProcessor.QName;
    using HeaderBit = System.ServiceModel.Dispatcher.EndpointAddressProcessor.HeaderBit;

    public class EndpointAddressMessageFilter : MessageFilter
    {
        EndpointAddress address;
        bool includeHostNameInComparison;
        EndpointAddressMessageFilterHelper helper;
        UriComparer comparer;

        public EndpointAddressMessageFilter(EndpointAddress address)
            : this(address, false)
        {
        }

        public EndpointAddressMessageFilter(EndpointAddress address, bool includeHostNameInComparison)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            this.address = address;
            this.includeHostNameInComparison = includeHostNameInComparison;
            this.helper = new EndpointAddressMessageFilterHelper(this.address);

            if (includeHostNameInComparison)
            {
                this.comparer = HostUriComparer.Value;
            }
            else
            {
                this.comparer = NoHostUriComparer.Value;
            }
        }

        public EndpointAddress Address
        {
            get
            {
                return this.address;
            }
        }

        public bool IncludeHostNameInComparison
        {
            get { return this.includeHostNameInComparison; }
        }

        protected internal override IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return new EndpointAddressMessageFilterTable<FilterData>();
        }

        public override bool Match(MessageBuffer messageBuffer)
        {
            if (messageBuffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageBuffer");
            }

            Message msg = messageBuffer.CreateMessage();
            try
            {
                return Match(msg);
            }
            finally
            {
                msg.Close();
            }
        }

        public override bool Match(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            // To
#pragma warning suppress 56506 // [....], Message.Headers can never be null
            Uri to = message.Headers.To;
            Uri actingAs = this.address.Uri;

            if (to == null || !this.comparer.Equals(actingAs, to))
            {
                return false;
            }

            return this.helper.Match(message);
        }

        internal Dictionary<string, HeaderBit[]> HeaderLookup
        {
            get { return this.helper.HeaderLookup; }
        }

        internal bool ComparePort
        {
            set
            {
                comparer.ComparePort = value;
            }
        }

        internal abstract class UriComparer : EqualityComparer<Uri>
        {
            protected UriComparer()
            {
                ComparePort = true;
            }

            protected abstract bool CompareHost { get; }

            internal bool ComparePort
            {
                get;
                set;
            }

            public override bool Equals(Uri u1, Uri u2)
            {
                return EndpointAddress.UriEquals(u1, u2, true /* ignoreCase */, CompareHost, ComparePort);
            }

            public override int GetHashCode(Uri uri)
            {
                return EndpointAddress.UriGetHashCode(uri, CompareHost, ComparePort);
            }
        }

        internal sealed class HostUriComparer : UriComparer
        {
            internal static readonly UriComparer Value = new HostUriComparer();

            HostUriComparer()
            {
            }

            protected override bool CompareHost
            {
                get
                {
                    return true;
                }
            }
        }

        internal sealed class NoHostUriComparer : UriComparer
        {
            internal static readonly UriComparer Value = new NoHostUriComparer();

            NoHostUriComparer()
            {
            }

            protected override bool CompareHost
            {
                get
                {
                    return false;
                }
            }
        }
    }

    internal class EndpointAddressMessageFilterHelper
    {
        EndpointAddress address;
        WeakReference processorPool;

        int size;
        byte[] mask;
        Dictionary<QName, int> qnameLookup;
        Dictionary<string, HeaderBit[]> headerLookup;

        internal EndpointAddressMessageFilterHelper(EndpointAddress address)
        {
            this.address = address;

            if (this.address.Headers.Count > 0)
            {
                CreateMask();
                this.processorPool = new WeakReference(null);
            }
            else
            {
                this.qnameLookup = null;
                this.headerLookup = null;
                this.size = 0;
                this.mask = null;
            }
        }

        void CreateMask()
        {
            int nextBit = 0;
            string key;
            HeaderBit[] bits;
            QName qname;
            this.qnameLookup = new Dictionary<QName, int>(EndpointAddressProcessor.QNameComparer);
            this.headerLookup = new Dictionary<string, HeaderBit[]>();
            StringBuilder builder = null;

            for (int i = 0; i < this.address.Headers.Count; ++i)
            {
                if (builder == null)
                    builder = new StringBuilder();
                else
                    builder.Remove(0, builder.Length);


                key = this.address.Headers[i].GetComparableForm(builder);
                if (this.headerLookup.TryGetValue(key, out bits))
                {
                    Array.Resize(ref bits, bits.Length + 1);
                    bits[bits.Length - 1] = new HeaderBit(nextBit++);
                    this.headerLookup[key] = bits;
                }
                else
                {

                    this.headerLookup.Add(key, new HeaderBit[] { new HeaderBit(nextBit++) });
                    AddressHeader parameter = this.address.Headers[i];

                    qname.name = parameter.Name;
                    qname.ns = parameter.Namespace;
                    this.qnameLookup[qname] = 1;
                }
            }

            if (nextBit == 0)
            {
                this.size = 0;
            }
            else
            {
                this.size = (nextBit - 1) / 8 + 1;
            }

            if (this.size > 0)
            {
                this.mask = new byte[size];
                for (int i = 0; i < this.size - 1; ++i)
                {
                    this.mask[i] = 0xff;
                }

                if ((nextBit % 8) == 0)
                {
                    this.mask[this.size - 1] = 0xff;
                }
                else
                {
                    this.mask[this.size - 1] = (byte)((1 << (nextBit % 8)) - 1);
                }
            }
        }

        internal Dictionary<string, HeaderBit[]> HeaderLookup
        {
            get
            {
                if (this.headerLookup == null)
                    this.headerLookup = new Dictionary<string, HeaderBit[]>();
                return this.headerLookup;
            }
        }

        EndpointAddressProcessor CreateProcessor(int length)
        {
            if (this.processorPool.Target != null)
            {
                lock (this.processorPool)
                {
                    object o = this.processorPool.Target;
                    if (o != null)
                    {
                        EndpointAddressProcessor p = (EndpointAddressProcessor)o;
                        this.processorPool.Target = p.Next;
                        p.Next = null;
                        p.Clear(length);
                        return p;
                    }
                }
            }

            return new EndpointAddressProcessor(length);
        }

        internal bool Match(Message message)
        {
            if (this.size == 0)
            {
                return true;
            }

            EndpointAddressProcessor context = CreateProcessor(this.size);
            context.ProcessHeaders(message, this.qnameLookup, this.headerLookup);
            bool result = context.TestExact(this.mask);
            ReleaseProcessor(context);
            return result;
        }

        void ReleaseProcessor(EndpointAddressProcessor context)
        {
            lock (this.processorPool)
            {
                context.Next = this.processorPool.Target as EndpointAddressProcessor;
                this.processorPool.Target = context;
            }
        }

    }
}
