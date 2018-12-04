// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Xaml
{
    // XamlNode Double Buffering object that multi-threaded access
    //  ONE reader and ONE writer.
    // This is the bridge used to Parse on one thread and Build on a second.

    public class XamlBackgroundReader : XamlReader, IXamlLineInfo
    {
        EventWaitHandle _providerFullEvent;
        EventWaitHandle _dataReceivedEvent;

        XamlNode[] _incoming;
        int _inIdx;
        XamlNode[] _outgoing;
        int _outIdx;
        int _outValid;

        XamlNode _currentNode;

        XamlReader _wrappedReader;
        XamlReader _internalReader;
        XamlWriter _writer;

        bool _wrappedReaderHasLineInfo;
        int _lineNumber=0;
        int _linePosition=0;
        
        Thread _thread;
        Exception _caughtException;

        public XamlBackgroundReader(XamlReader wrappedReader)
        {
            if (wrappedReader == null)
            {
                throw new ArgumentNullException("wrappedReader");
            }
            Initialize(wrappedReader, 64);
        }

        private void Initialize(XamlReader wrappedReader, int bufferSize)
        {
            _providerFullEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _dataReceivedEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

            _incoming = new XamlNode[bufferSize];
            _outgoing = new XamlNode[bufferSize];

            _wrappedReader = wrappedReader;
            _wrappedReaderHasLineInfo = ((IXamlLineInfo)_wrappedReader).HasLineInfo;

            var xamlNodeAddDelegate = new XamlNodeAddDelegate(Add);
            XamlLineInfoAddDelegate lineInfoAddDelegate = null;
            if (_wrappedReaderHasLineInfo)
            {
                lineInfoAddDelegate = new XamlLineInfoAddDelegate(AddLineInfo);
            }
            _writer = new WriterDelegate(xamlNodeAddDelegate, lineInfoAddDelegate, _wrappedReader.SchemaContext);

            XamlNodeNextDelegate xamlNodeNextDelegate;
            if(_wrappedReaderHasLineInfo)
            {
                xamlNodeNextDelegate = new XamlNodeNextDelegate(Next_ProcessLineInfo);
            }
            else
            {
                xamlNodeNextDelegate = new XamlNodeNextDelegate(Next);
            }
            _internalReader = new ReaderDelegate(_wrappedReader.SchemaContext, xamlNodeNextDelegate, _wrappedReaderHasLineInfo);

            //Standin so it won't start null
            _currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
        }

        public void StartThread()
        {
            StartThread("XAML reader thread");
        }

        public void StartThread(string threadName)
        {
            if (_thread != null)
            {
                throw new InvalidOperationException(SR.Get(SRID.ThreadAlreadyStarted));
            }
            ParameterizedThreadStart start = new ParameterizedThreadStart(XamlReaderThreadStart);
            _thread = new Thread(start);
            _thread.Name = threadName;
            _thread.Start();
        }

        // The "ThreadStart" function
        private void XamlReaderThreadStart(object none)
        {
            try
            {
                InterruptableTransform(_wrappedReader, _writer, true);
            }
            catch (Exception ex)
            {
                _writer.Close();
                _caughtException = ex;
            }
        }

        internal bool IncomingFull
        {
            get { return _inIdx >= _incoming.Length; }
        }

        internal bool OutgoingEmpty
        {
            get { return _outIdx >= _outValid; }
        }

        private void SwapBuffers()
        {
            XamlNode[] tmp = _incoming;
            _incoming = _outgoing;
            _outgoing = tmp;

            _outIdx = 0;
            _outValid = _inIdx;  // in the EOF case the buffer can be short.
            _inIdx = 0;
        }

        private void AddToBuffer(XamlNode node)
        {
            _incoming[_inIdx] = node;
            _inIdx += 1;
            if (IncomingFull)
            {
                _providerFullEvent.Set();      // Reader is Full
                _dataReceivedEvent.WaitOne();  // Wait for data to be picked up.
            }
        }

        private void Add(XamlNodeType nodeType, object data)
        {
            if (IsDisposed)
            {
                return;
            }
            if (nodeType != XamlNodeType.None)
            {
                AddToBuffer(new XamlNode(nodeType, data));
                return;
            }
            System.Diagnostics.Debug.Assert(XamlNode.IsEof_Helper(nodeType, data));
            AddToBuffer(new XamlNode(XamlNode.InternalNodeType.EndOfStream));
            _providerFullEvent.Set();
        }

        private void AddLineInfo(int lineNumber, int linePosition)
        {
            if (IsDisposed)
            {
                return;
            }
            LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
            XamlNode node = new XamlNode(lineInfo);
            AddToBuffer(node);
        }

        private XamlNode Next()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("XamlBackgroundReader");
            }
            if (OutgoingEmpty)
            {
                // This is for users that read PAST the EOF record.
                // Don't let them hang on WaitOne() return EOF Again!
                if (_currentNode.IsEof)
                {
                    return _currentNode;
                }
                _providerFullEvent.WaitOne();   // Wait for provider to fill up.
                SwapBuffers();
                _dataReceivedEvent.Set();       // Let the Reader run.
            }
            _currentNode = _outgoing[_outIdx++];

            if (_currentNode.IsEof)
            {
                if (_thread != null)
                {
                    // If the input ended due to an (caught) exception on the background thread,
                    // then at the end of reading the input re-throw the exception on the
                    // foreground thread.
                    _thread.Join();
                    if (_caughtException != null)
                    {
                        Exception ex = _caughtException;
                        _caughtException = null;
                        throw ex;
                    }
                }
            }

            return _currentNode;
        }

        private XamlNode Next_ProcessLineInfo()
        {
            bool done = false;
            while (!done)
            {
                Next();
                if (_currentNode.IsLineInfo)
                {
                    _lineNumber = _currentNode.LineInfo.LineNumber;
                    _linePosition = _currentNode.LineInfo.LinePosition;
                }
                else
                {
                    done = true;
                }

            }
            return _currentNode;
        }

        private void InterruptableTransform(XamlReader reader, XamlWriter writer, bool closeWriter)
        {
            IXamlLineInfo xamlLineInfo = reader as IXamlLineInfo;
            IXamlLineInfoConsumer xamlLineInfoConsumer = writer as IXamlLineInfoConsumer;
            bool shouldPassLineNumberInfo = false;
            if ((xamlLineInfo != null && xamlLineInfo.HasLineInfo)
                && (xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo))
            {
                shouldPassLineNumberInfo = true;
            }
            while (reader.Read())
            {
                if (IsDisposed)
                {
                    break;
                }
                if (shouldPassLineNumberInfo)
                {
                    if (xamlLineInfo.LineNumber != 0)
                    {
                        xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
                    }
                }
                writer.WriteNode(reader);
            }

            if (closeWriter)
            {
                writer.Close();
            }
        }

        #region XamlReader

        public override bool Read()
        {
            return _internalReader.Read();
        }

        public override XamlNodeType NodeType
        {
            get { return _internalReader.NodeType; }
        }

        public override bool IsEof
        {
            get { return _internalReader.IsEof; }
        }

        public override NamespaceDeclaration Namespace
        {
            get { return _internalReader.Namespace; }
        }

        public override XamlType Type
        {
            get { return _internalReader.Type; }
        }

        public override object Value
        {
            get { return _internalReader.Value; }
        }

        public override XamlMember Member
        {
            get { return _internalReader.Member; }
        }

        public override XamlSchemaContext SchemaContext
        {
            get { return _internalReader.SchemaContext; }
        }

        #endregion

        #region IXamlLineInfo Members

        public bool HasLineInfo
        {
            get { return _wrappedReaderHasLineInfo; }
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public int LinePosition
        {
            get { return _linePosition; }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _dataReceivedEvent.Set(); // Release any blocked writers.
            ((IDisposable)_dataReceivedEvent).Dispose();
            ((IDisposable)_internalReader).Dispose();
            ((IDisposable)_providerFullEvent).Dispose();
            ((IDisposable)_writer).Dispose();
        }
    }
}
