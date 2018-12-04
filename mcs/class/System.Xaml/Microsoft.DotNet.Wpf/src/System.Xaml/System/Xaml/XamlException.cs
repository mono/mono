// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Xml;
using MS.Internal.Xaml.Context;
using System.Xaml.MS.Impl;
using MS.Internal.Xaml.Parser;

namespace System.Xaml
{
    [Serializable]  // FxCop advised this be Serializable.
    public class XamlException : Exception
    {
        public XamlException(string message, Exception innerException, int lineNumber, int linePosition)
            : base(message, innerException)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public XamlException(string message, Exception innerException)
            : base(message, innerException)
        {
            XamlException xex = innerException as XamlException;
            if (xex != null)
            {
                LineNumber = xex.LineNumber;
                LinePosition = xex.LinePosition;
            }
        }

        internal void SetLineInfo(int lineNumber, int linePosition)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public override string Message
        {
            get
            {
                if (LineNumber != 0)
                {
                    if (LinePosition != 0)
                    {
                        return SR.Get(SRID.LineNumberAndPosition, base.Message, LineNumber, LinePosition);
                    }
                    return SR.Get(SRID.LineNumberOnly, base.Message, LineNumber);
                }
                return base.Message;
            }
        }

        public int LineNumber { get; protected set; }
        public int LinePosition { get; protected set; }

        // FxCop required this.
        public XamlException() { }

        public XamlException(string message)
            :base(message) { }

        // FxCop required this.
        protected XamlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            LineNumber = info.GetInt32("Line");
            LinePosition = info.GetInt32("Offset");
        }

        /// <SecurityNote>
        /// Critical: calls Critical method Exception.GetObjectData
        /// </SecurityNote>
#if TARGETTING35SP1
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SecurityTreatAsSafe, SecurityCritical]
#else
        [SecurityCritical]
#endif
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Line", LineNumber);
            info.AddValue("Offset", LinePosition);
            base.GetObjectData(info, context);
        }
    }

    [Serializable]  // FxCop advised this be Serializable.
    public class XamlParseException : XamlException
    {
        internal XamlParseException(MeScanner meScanner, string message)
            : base(message, null, meScanner.LineNumber, meScanner.LinePosition) { }

        internal XamlParseException(XamlScanner xamlScanner, string message)
            : base(message, null, xamlScanner.LineNumber, xamlScanner.LinePosition) { }

        internal XamlParseException(int lineNumber, int linePosition, string message)
            : base(message, null, lineNumber, linePosition) { }
        
        // FxCop required these.
        public XamlParseException() { }

        public XamlParseException(string message)
            :base(message) { }

        public XamlParseException(string message, Exception innerException)
            : base(message, innerException) { }

        protected XamlParseException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        // FxCop and [Serializable] required this.
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //}
    }

    [Serializable]  // FxCop advised this be Serializable.
    public class XamlObjectWriterException : XamlException
    {
        // FxCop required this, default constructor.
        public XamlObjectWriterException() { }

        public XamlObjectWriterException(string message)
            : base(message) { }

        public XamlObjectWriterException(string message, Exception innerException)
            : base(message, innerException) { }

        // FxCop required this.
        protected XamlObjectWriterException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]  // FxCop advised this be Serializable.
    public class XamlDuplicateMemberException : XamlException
    {
        public XamlMember DuplicateMember { get; set; }
        public XamlType ParentType { get; set; }
        
        public XamlDuplicateMemberException() { }

        public XamlDuplicateMemberException(XamlMember member, XamlType type)
            : base(SR.Get(SRID.DuplicateMemberSet, (member != null) ? member.Name : null, (type != null) ? type.Name : null))
        {
            DuplicateMember = member;
            ParentType = type;
        }

        public XamlDuplicateMemberException(string message)
            : base(message) { }

        public XamlDuplicateMemberException(string message, Exception innerException)
            : base(message, innerException) { }

        protected XamlDuplicateMemberException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            DuplicateMember = (XamlMember)info.GetValue("DuplicateMember", typeof(XamlMember));
            ParentType = (XamlType)info.GetValue("ParentType", typeof(XamlType));
        }

        /// <SecurityNote>
        /// Critical: calls Critical method Exception.GetObjectData
        /// </SecurityNote>
#if TARGETTING35SP1
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SecurityTreatAsSafe, SecurityCritical]
#else
        [SecurityCritical]
#endif
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("DuplicateMember", DuplicateMember);
            info.AddValue("ParentType", ParentType);
            base.GetObjectData(info, context);
        }
    }

    [Serializable]  // FxCop advised this be Serializable.
    public class XamlInternalException : XamlException
    {
        const string MessagePrefix = "Internal XAML system error: ";

        // FxCop required this, default constructor.
        public XamlInternalException()
            : base(MessagePrefix) { }

        public XamlInternalException(string message)
            : base(MessagePrefix + message, null) { }

        public XamlInternalException(string message, Exception innerException)
            : base(MessagePrefix + message, innerException) { }

        // FxCop required this.
        protected XamlInternalException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]  // FxCop advised this be Serializable.
    public class XamlSchemaException : XamlException
    {
        // FxCop required this, default constructor.
        public XamlSchemaException() { }

        public XamlSchemaException(string message)
            : base(message, null) { }

        public XamlSchemaException(string message, Exception innerException)
            : base(message, innerException) { }

        protected XamlSchemaException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class XamlObjectReaderException : XamlException
    {
        public XamlObjectReaderException()
        {
        }

        public XamlObjectReaderException(string message)
            : base(message)
        {
        }

        public XamlObjectReaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected XamlObjectReaderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class XamlXmlWriterException : XamlException
    {
        public XamlXmlWriterException()
        {
        }

        public XamlXmlWriterException(string message)
            : base(message)
        {
        }

        public XamlXmlWriterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected XamlXmlWriterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}