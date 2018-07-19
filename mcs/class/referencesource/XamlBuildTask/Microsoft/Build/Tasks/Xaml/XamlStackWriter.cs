//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Xaml;
    using System.Runtime;
    using XamlBuildTask;

    internal enum XamlStackFrameType
    {
        None,
        Object,
        GetObject,
        Member
    }

    internal struct XamlStackFrame
    {
        private static object s_setSentinel = new object();

        private object _data;
        private object _isSet;

        public XamlStackFrameType FrameType { get; private set; }

        public XamlType Type
        {
            get { return FrameType == XamlStackFrameType.Object ? (XamlType)_data : null; }
        }

        public XamlMember Member
        {
            get { return FrameType == XamlStackFrameType.Member ? (XamlMember)_data : null; }
        }

        public bool IsSet()
        {
            return _isSet != null;
        }

        public bool IsSet(XamlMember member)
        {
            HashSet<XamlMember> setMembers = _isSet as HashSet<XamlMember>;
            return (setMembers == null) ? false : setMembers.Contains(member);
        }

        internal void Set()
        {
            if (FrameType != XamlStackFrameType.Member)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnexpectedXaml));
            }
            _isSet = s_setSentinel;
        }

        internal void Set(XamlMember member)
        {
            if (FrameType != XamlStackFrameType.Object && FrameType != XamlStackFrameType.GetObject)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnexpectedXaml));
            }
            HashSet<XamlMember> setMembers = _isSet as HashSet<XamlMember>;
            if (setMembers == null)
            {
                setMembers = new HashSet<XamlMember>();
                _isSet = setMembers;
            }
            if (!setMembers.Contains(member))
            {
                setMembers.Add(member);
            }
        }

        internal static XamlStackFrame ForObject(XamlType type)
        {
            XamlStackFrame result = new XamlStackFrame();
            result._data = type;
            result.FrameType = XamlStackFrameType.Object;
            return result;
        }

        internal static XamlStackFrame ForGetObject()
        {
            XamlStackFrame result = new XamlStackFrame();
            result.FrameType = XamlStackFrameType.GetObject;
            return result;
        }

        internal static XamlStackFrame ForMember(XamlMember member)
        {
            XamlStackFrame result = new XamlStackFrame();
            result._data = member;
            result.FrameType = XamlStackFrameType.Member;
            return result;
        }
    }


    internal class XamlStackWriter : XamlWriter
    {
        List<XamlStackFrame> _stack = new List<XamlStackFrame>();

        // the stack writer does not care about schema context
        public override XamlSchemaContext SchemaContext { get { return null; } }

        public int Depth { get { return _stack.Count; } }

        public XamlStackFrame TopFrame
        {
            get
            {
                Fx.Assert(_stack.Count != 0, "Stack cannot be empty");
                return _stack[_stack.Count - 1];
            }
        }

        public XamlStackFrame FrameAtDepth(int ndx)
        {
            return _stack[ndx - 1];
        }


        public override void WriteEndMember()
        {
            if (TopFrame.FrameType != XamlStackFrameType.Member)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnexpectedXaml));
            }
            PopStack();
        }

        public override void WriteEndObject()
        {
            if (TopFrame.FrameType != XamlStackFrameType.Object &&
                TopFrame.FrameType != XamlStackFrameType.GetObject)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnexpectedXaml));
            }
            PopStack();
        }

        public override void WriteGetObject()
        {
            SetTopFrame();
            _stack.Add(XamlStackFrame.ForGetObject());
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        public override void WriteStartMember(XamlMember property)
        {
            if (property == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnexpectedXamlValueNull("property")));
            }
            SetTopFrame(property);
            _stack.Add(XamlStackFrame.ForMember(property));
        }

        public override void WriteStartObject(XamlType type)
        {
            if (type == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UnexpectedXamlValueNull("type")));
            }
            SetTopFrame();
            _stack.Add(XamlStackFrame.ForObject(type));
        }

        public override void WriteValue(object value)
        {
            SetTopFrame();
        }

        private void PopStack()
        {
            _stack.RemoveAt(_stack.Count - 1);
        }

        private void SetTopFrame()
        {
            if (Depth > 0 && TopFrame.FrameType == XamlStackFrameType.Member)
            {
                TopFrame.Set();
            }
        }

        private void SetTopFrame(XamlMember member)
        {
            if (Depth > 0 &&
                (TopFrame.FrameType == XamlStackFrameType.GetObject ||
                 TopFrame.FrameType == XamlStackFrameType.Object))
            {
                TopFrame.Set(member);
            }
        }
    }
}
