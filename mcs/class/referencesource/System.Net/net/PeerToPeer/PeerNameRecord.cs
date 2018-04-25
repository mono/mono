//------------------------------------------------------------------------------
// <copyright file="PeerNameRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Net.PeerToPeer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Net;
    [Serializable]
    public class PeerNameRecord : ISerializable
    {
        private const int MAX_COMMENT_SIZE = 39;
        private const int MAX_DATA_SIZE = 4096;

        private PeerName m_PeerName;
        private IPEndPointCollection m_EndPointCollection = new IPEndPointCollection();
        private string m_Comment;
        private byte[] m_Data;
        

        /// <summary>
        /// Constructor to enable serialization 
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected PeerNameRecord(SerializationInfo info, StreamingContext context)
        {
            m_Comment = info.GetString("_Comment");
            m_Data = info.GetValue("_Data", typeof(byte[])) as byte[];
            m_EndPointCollection = info.GetValue("_EndpointList", typeof(IPEndPointCollection)) as IPEndPointCollection;
            m_PeerName  = info.GetValue("_PeerName", typeof(PeerName)) as PeerName;

        }


        // <SecurityKernel Critical="True" Ring="0">
        // <SatisfiesLinkDemand Name="GetObjectData(SerializationInfo, StreamingContext):Void" />
        // </SecurityKernel>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.Net.dll is still using pre-v4 security model and needs this demand")]
        [System.Security.SecurityCritical]
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        /// <summary>
        /// This is made virtual so that derived types can be implemented correctly
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_PeerName", m_PeerName);
            info.AddValue("_EndpointList", m_EndPointCollection);
            info.AddValue("_Comment", m_Comment);
            info.AddValue("_Data", m_Data);
        }

        public PeerNameRecord()
        {
        }

        /*
        public PeerNameRecord(PeerName peerName)
        {
            m_PeerName = peerName;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerNameRecord created with PeerName {0}", m_PeerName);
        }

        public PeerNameRecord(PeerName peerName, int Port)
        {
            m_PeerName = peerName;
            m_Port = Port;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerNameRecord created with PeerName {0} Port {1}", m_PeerName, m_Port);
        }

        public PeerNameRecord(PeerName peerName, IPEndPoint[] endPointList, Cloud cloud, string comment, byte[] data)
        {
            m_PeerName = peerName;
            m_Port = Port;
            m_EndPointList = endPointList;
            m_Cloud = cloud;
            m_Comment = comment;
            m_Data = data;
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "PeerNameRecord created");
            TracePeerNameRecord();
        }
         */

        public PeerName PeerName
        {
            get
            {
                return m_PeerName;
            }
            set
            {
                m_PeerName = value;

            }
        }

        public IPEndPointCollection EndPointCollection
        {
            get
            {
                return m_EndPointCollection;
            }
        }

        public string Comment
        {
            get
            {
                return m_Comment;
            }
            set
            {
                //--------------------------------------------------------------------
                //We don't allow null or empty comments since they are not very useful
                //--------------------------------------------------------------------
                if (value == null)
                    throw new ArgumentNullException("Comment", SR.GetString(SR.Pnrp_CommentCantBeNull));

                if(value.Length <= 0)
                    throw new ArgumentException(SR.GetString(SR.Pnrp_CommentCantBeNull), "Comment");

                if (value.Length > MAX_COMMENT_SIZE)
                    throw new ArgumentException(SR.GetString(SR.Pnrp_CommentMaxLengthExceeded, MAX_COMMENT_SIZE));

                m_Comment = value;
            }
        }
        public byte[] Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Data", SR.GetString(SR.Pnrp_DataCantBeNull));

                if (value.Length <= 0)
                    throw new ArgumentException(SR.GetString(SR.Pnrp_DataCantBeNull), "Data");                       

                if(value.Length > MAX_DATA_SIZE)
                    throw new ArgumentException(SR.GetString(SR.Pnrp_DataLengthExceeded, MAX_DATA_SIZE));

                m_Data = value;
            }
        }

        internal void TracePeerNameRecord()
        {
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "Contents of the PeerNameRecord");
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tPeerName: {0}", PeerName);
            Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tComment: {0}", Comment);
            if (EndPointCollection != null && EndPointCollection.Count != 0)
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tThe EndPointList is ");
                foreach(IPEndPoint ipe in EndPointCollection)
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\t\tIPEndPoint is {0}", ipe);
                }
            }
            else
            {
                Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tThe EndPointList is empty or null");
            }
            if (Data != null)
            {
                if (Logging.P2PTraceSource.Switch.ShouldTrace(TraceEventType.Verbose))
                {
                    Logging.DumpData(Logging.P2PTraceSource, TraceEventType.Verbose, Logging.P2PTraceSource.MaxDataSize, Data, 0, Data.Length);
                }
                else
                {
                    Logging.P2PTraceSource.TraceEvent(TraceEventType.Information, 0, "\tCustom data length {0}", Data.Length);
                }
            }
        }
    }
}

