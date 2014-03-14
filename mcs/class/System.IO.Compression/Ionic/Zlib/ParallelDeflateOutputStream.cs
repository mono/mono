//#define Trace

// ParallelDeflateOutputStream.cs
// ------------------------------------------------------------------
//
// A DeflateStream that does compression only, it uses a
// divide-and-conquer approach with multiple threads to exploit multiple
// CPUs for the DEFLATE computation.
//
// last saved: <2011-July-31 14:49:40>
//
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 by Dino Chiesa
// All rights reserved!
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using Ionic.Zlib;
using System.IO;


namespace Ionic.Zlib
{
    internal class WorkItem
    {
        public byte[] buffer;
        public byte[] compressed;
        public int crc;
        public int index;
        public int ordinal;
        public int inputBytesAvailable;
        public int compressedBytesAvailable;
        public ZlibCodec compressor;

        public WorkItem(int size,
                        Ionic.Zlib.CompressionLevel compressLevel,
                        CompressionStrategy strategy,
                        int ix)
        {
            this.buffer= new byte[size];
            // alloc 5 bytes overhead for every block (margin of safety= 2)
            int n = size + ((size / 32768)+1) * 5 * 2;
            this.compressed = new byte[n];
            this.compressor = new ZlibCodec();
            this.compressor.InitializeDeflate(compressLevel, false);
            this.compressor.OutputBuffer = this.compressed;
            this.compressor.InputBuffer = this.buffer;
            this.index = ix;
        }
    }

    /// <summary>
    ///   A class for compressing streams using the
    ///   Deflate algorithm with multiple threads.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   This class performs DEFLATE compression through writing.  For
    ///   more information on the Deflate algorithm, see IETF RFC 1951,
    ///   "DEFLATE Compressed Data Format Specification version 1.3."
    /// </para>
    ///
    /// <para>
    ///   This class is similar to <see cref="Ionic.Zlib.DeflateStream"/>, except
    ///   that this class is for compression only, and this implementation uses an
    ///   approach that employs multiple worker threads to perform the DEFLATE.  On
    ///   a multi-cpu or multi-core computer, the performance of this class can be
    ///   significantly higher than the single-threaded DeflateStream, particularly
    ///   for larger streams.  How large?  Anything over 10mb is a good candidate
    ///   for parallel compression.
    /// </para>
    ///
    /// <para>
    ///   The tradeoff is that this class uses more memory and more CPU than the
    ///   vanilla DeflateStream, and also is less efficient as a compressor. For
    ///   large files the size of the compressed data stream can be less than 1%
    ///   larger than the size of a compressed data stream from the vanialla
    ///   DeflateStream.  For smaller files the difference can be larger.  The
    ///   difference will also be larger if you set the BufferSize to be lower than
    ///   the default value.  Your mileage may vary. Finally, for small files, the
    ///   ParallelDeflateOutputStream can be much slower than the vanilla
    ///   DeflateStream, because of the overhead associated to using the thread
    ///   pool.
    /// </para>
    ///
    /// </remarks>
    /// <seealso cref="Ionic.Zlib.DeflateStream" />
    internal class ParallelDeflateOutputStream : System.IO.Stream
    {

        private static readonly int IO_BUFFER_SIZE_DEFAULT = 64 * 1024;  // 128k
        private static readonly int BufferPairsPerCore = 4;

        private System.Collections.Generic.List<WorkItem> _pool;
        private bool                        _leaveOpen;
        private bool                        emitting;
        private System.IO.Stream            _outStream;
        private int                         _maxBufferPairs;
        private int                         _bufferSize = IO_BUFFER_SIZE_DEFAULT;
        private AutoResetEvent              _newlyCompressedBlob;
        //private ManualResetEvent            _writingDone;
        //private ManualResetEvent            _sessionReset;
        private object                      _outputLock = new object();
        private bool                        _isClosed;
        private bool                        _firstWriteDone;
        private int                         _currentlyFilling;
        private int                         _lastFilled;
        private int                         _lastWritten;
        private int                         _latestCompressed;
        private int                         _Crc32;
        private Ionic.Crc.CRC32             _runningCrc;
        private object                      _latestLock = new object();
        private System.Collections.Generic.Queue<int>     _toWrite;
        private System.Collections.Generic.Queue<int>     _toFill;
        private Int64                       _totalBytesProcessed;
        private Ionic.Zlib.CompressionLevel _compressLevel;
        private volatile Exception          _pendingException;
        private bool                        _handlingException;
        private object                      _eLock = new Object();  // protects _pendingException

        // This bitfield is used only when Trace is defined.
        //private TraceBits _DesiredTrace = TraceBits.Write | TraceBits.WriteBegin |
        //TraceBits.WriteDone | TraceBits.Lifecycle | TraceBits.Fill | TraceBits.Flush |
        //TraceBits.Session;

        //private TraceBits _DesiredTrace = TraceBits.WriteBegin | TraceBits.WriteDone | TraceBits.Synch | TraceBits.Lifecycle  | TraceBits.Session ;

        private TraceBits _DesiredTrace =
            TraceBits.Session |
            TraceBits.Compress |
            TraceBits.WriteTake |
            TraceBits.WriteEnter |
            TraceBits.EmitEnter |
            TraceBits.EmitDone |
            TraceBits.EmitLock |
            TraceBits.EmitSkip |
            TraceBits.EmitBegin;

        /// <summary>
        /// Create a ParallelDeflateOutputStream.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   This stream compresses data written into it via the DEFLATE
        ///   algorithm (see RFC 1951), and writes out the compressed byte stream.
        /// </para>
        ///
        /// <para>
        ///   The instance will use the default compression level, the default
        ///   buffer sizes and the default number of threads and buffers per
        ///   thread.
        /// </para>
        ///
        /// <para>
        ///   This class is similar to <see cref="Ionic.Zlib.DeflateStream"/>,
        ///   except that this implementation uses an approach that employs
        ///   multiple worker threads to perform the DEFLATE.  On a multi-cpu or
        ///   multi-core computer, the performance of this class can be
        ///   significantly higher than the single-threaded DeflateStream,
        ///   particularly for larger streams.  How large?  Anything over 10mb is
        ///   a good candidate for parallel compression.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        /// This example shows how to use a ParallelDeflateOutputStream to compress
        /// data.  It reads a file, compresses it, and writes the compressed data to
        /// a second, output file.
        ///
        /// <code>
        /// byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        /// int n= -1;
        /// String outputFile = fileToCompress + ".compressed";
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(outputFile))
        ///     {
        ///         using (Stream compressor = new ParallelDeflateOutputStream(raw))
        ///         {
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim buffer As Byte() = New Byte(4096) {}
        /// Dim n As Integer = -1
        /// Dim outputFile As String = (fileToCompress &amp; ".compressed")
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(outputFile)
        ///         Using compressor As Stream = New ParallelDeflateOutputStream(raw)
        ///             Do While (n &lt;&gt; 0)
        ///                 If (n &gt; 0) Then
        ///                     compressor.Write(buffer, 0, n)
        ///                 End If
        ///                 n = input.Read(buffer, 0, buffer.Length)
        ///             Loop
        ///         End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        public ParallelDeflateOutputStream(System.IO.Stream stream)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, false)
        {
        }

        /// <summary>
        ///   Create a ParallelDeflateOutputStream using the specified CompressionLevel.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        public ParallelDeflateOutputStream(System.IO.Stream stream, CompressionLevel level)
            : this(stream, level, CompressionStrategy.Default, false)
        {
        }

        /// <summary>
        /// Create a ParallelDeflateOutputStream and specify whether to leave the captive stream open
        /// when the ParallelDeflateOutputStream is closed.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="leaveOpen">
        ///    true if the application would like the stream to remain open after inflation/deflation.
        /// </param>
        public ParallelDeflateOutputStream(System.IO.Stream stream, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        /// <summary>
        /// Create a ParallelDeflateOutputStream and specify whether to leave the captive stream open
        /// when the ParallelDeflateOutputStream is closed.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        /// <param name="leaveOpen">
        ///    true if the application would like the stream to remain open after inflation/deflation.
        /// </param>
        public ParallelDeflateOutputStream(System.IO.Stream stream, CompressionLevel level, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        /// <summary>
        /// Create a ParallelDeflateOutputStream using the specified
        /// CompressionLevel and CompressionStrategy, and specifying whether to
        /// leave the captive stream open when the ParallelDeflateOutputStream is
        /// closed.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        /// <param name="strategy">
        ///   By tweaking this parameter, you may be able to optimize the compression for
        ///   data with particular characteristics.
        /// </param>
        /// <param name="leaveOpen">
        ///    true if the application would like the stream to remain open after inflation/deflation.
        /// </param>
        public ParallelDeflateOutputStream(System.IO.Stream stream,
                                           CompressionLevel level,
                                           CompressionStrategy strategy,
                                           bool leaveOpen)
        {
            TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "-------------------------------------------------------");
            TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "Create {0:X8}", this.GetHashCode());
            _outStream = stream;
            _compressLevel= level;
            Strategy = strategy;
            _leaveOpen = leaveOpen;
            this.MaxBufferPairs = 16; // default
        }


        /// <summary>
        ///   The ZLIB strategy to be used during compression.
        /// </summary>
        ///
        public CompressionStrategy Strategy
        {
            get;
            private set;
        }

        /// <summary>
        ///   The maximum number of buffer pairs to use.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This property sets an upper limit on the number of memory buffer
        ///   pairs to create.  The implementation of this stream allocates
        ///   multiple buffers to facilitate parallel compression.  As each buffer
        ///   fills up, this stream uses <see
        ///   cref="System.Threading.ThreadPool.QueueUserWorkItem(WaitCallback)">
        ///   ThreadPool.QueueUserWorkItem()</see>
        ///   to compress those buffers in a background threadpool thread. After a
        ///   buffer is compressed, it is re-ordered and written to the output
        ///   stream.
        /// </para>
        ///
        /// <para>
        ///   A higher number of buffer pairs enables a higher degree of
        ///   parallelism, which tends to increase the speed of compression on
        ///   multi-cpu computers.  On the other hand, a higher number of buffer
        ///   pairs also implies a larger memory consumption, more active worker
        ///   threads, and a higher cpu utilization for any compression. This
        ///   property enables the application to limit its memory consumption and
        ///   CPU utilization behavior depending on requirements.
        /// </para>
        ///
        /// <para>
        ///   For each compression "task" that occurs in parallel, there are 2
        ///   buffers allocated: one for input and one for output.  This property
        ///   sets a limit for the number of pairs.  The total amount of storage
        ///   space allocated for buffering will then be (N*S*2), where N is the
        ///   number of buffer pairs, S is the size of each buffer (<see
        ///   cref="BufferSize"/>).  By default, DotNetZip allocates 4 buffer
        ///   pairs per CPU core, so if your machine has 4 cores, and you retain
        ///   the default buffer size of 128k, then the
        ///   ParallelDeflateOutputStream will use 4 * 4 * 2 * 128kb of buffer
        ///   memory in total, or 4mb, in blocks of 128kb.  If you then set this
        ///   property to 8, then the number will be 8 * 2 * 128kb of buffer
        ///   memory, or 2mb.
        /// </para>
        ///
        /// <para>
        ///   CPU utilization will also go up with additional buffers, because a
        ///   larger number of buffer pairs allows a larger number of background
        ///   threads to compress in parallel. If you find that parallel
        ///   compression is consuming too much memory or CPU, you can adjust this
        ///   value downward.
        /// </para>
        ///
        /// <para>
        ///   The default value is 16. Different values may deliver better or
        ///   worse results, depending on your priorities and the dynamic
        ///   performance characteristics of your storage and compute resources.
        /// </para>
        ///
        /// <para>
        ///   This property is not the number of buffer pairs to use; it is an
        ///   upper limit. An illustration: Suppose you have an application that
        ///   uses the default value of this property (which is 16), and it runs
        ///   on a machine with 2 CPU cores. In that case, DotNetZip will allocate
        ///   4 buffer pairs per CPU core, for a total of 8 pairs.  The upper
        ///   limit specified by this property has no effect.
        /// </para>
        ///
        /// <para>
        ///   The application can set this value at any time, but it is effective
        ///   only before the first call to Write(), which is when the buffers are
        ///   allocated.
        /// </para>
        /// </remarks>
        public int MaxBufferPairs
        {
            get
            {
                return _maxBufferPairs;
            }
            set
            {
                if (value < 4)
                    throw new ArgumentException("MaxBufferPairs",
                                                "Value must be 4 or greater.");
                _maxBufferPairs = value;
            }
        }

        /// <summary>
        ///   The size of the buffers used by the compressor threads.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   The default buffer size is 128k. The application can set this value
        ///   at any time, but it is effective only before the first Write().
        /// </para>
        ///
        /// <para>
        ///   Larger buffer sizes implies larger memory consumption but allows
        ///   more efficient compression. Using smaller buffer sizes consumes less
        ///   memory but may result in less effective compression.  For example,
        ///   using the default buffer size of 128k, the compression delivered is
        ///   within 1% of the compression delivered by the single-threaded <see
        ///   cref="Ionic.Zlib.DeflateStream"/>.  On the other hand, using a
        ///   BufferSize of 8k can result in a compressed data stream that is 5%
        ///   larger than that delivered by the single-threaded
        ///   <c>DeflateStream</c>.  Excessively small buffer sizes can also cause
        ///   the speed of the ParallelDeflateOutputStream to drop, because of
        ///   larger thread scheduling overhead dealing with many many small
        ///   buffers.
        /// </para>
        ///
        /// <para>
        ///   The total amount of storage space allocated for buffering will be
        ///   (N*S*2), where N is the number of buffer pairs, and S is the size of
        ///   each buffer (this property). There are 2 buffers used by the
        ///   compressor, one for input and one for output.  By default, DotNetZip
        ///   allocates 4 buffer pairs per CPU core, so if your machine has 4
        ///   cores, then the number of buffer pairs used will be 16. If you
        ///   accept the default value of this property, 128k, then the
        ///   ParallelDeflateOutputStream will use 16 * 2 * 128kb of buffer memory
        ///   in total, or 4mb, in blocks of 128kb.  If you set this property to
        ///   64kb, then the number will be 16 * 2 * 64kb of buffer memory, or
        ///   2mb.
        /// </para>
        ///
        /// </remarks>
        public int BufferSize
        {
            get { return _bufferSize;}
            set
            {
                if (value < 1024)
                    throw new ArgumentOutOfRangeException("BufferSize",
                                                          "BufferSize must be greater than 1024 bytes");
                _bufferSize = value;
            }
        }

        /// <summary>
        /// The CRC32 for the data that was written out, prior to compression.
        /// </summary>
        /// <remarks>
        /// This value is meaningful only after a call to Close().
        /// </remarks>
        public int Crc32 { get { return _Crc32; } }


        /// <summary>
        /// The total number of uncompressed bytes processed by the ParallelDeflateOutputStream.
        /// </summary>
        /// <remarks>
        /// This value is meaningful only after a call to Close().
        /// </remarks>
        public Int64 BytesProcessed { get { return _totalBytesProcessed; } }


        private void _InitializePoolOfWorkItems()
        {
            _toWrite = new Queue<int>();
            _toFill = new Queue<int>();
            _pool = new System.Collections.Generic.List<WorkItem>();
            int nTasks = BufferPairsPerCore * Environment.ProcessorCount;
            nTasks = Math.Min(nTasks, _maxBufferPairs);
            for(int i=0; i < nTasks; i++)
            {
                _pool.Add(new WorkItem(_bufferSize, _compressLevel, Strategy, i));
                _toFill.Enqueue(i);
            }

            _newlyCompressedBlob = new AutoResetEvent(false);
            _runningCrc = new Ionic.Crc.CRC32();
            _currentlyFilling = -1;
            _lastFilled = -1;
            _lastWritten = -1;
            _latestCompressed = -1;
        }




        /// <summary>
        ///   Write data to the stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   To use the ParallelDeflateOutputStream to compress data, create a
        ///   ParallelDeflateOutputStream with CompressionMode.Compress, passing a
        ///   writable output stream.  Then call Write() on that
        ///   ParallelDeflateOutputStream, providing uncompressed data as input.  The
        ///   data sent to the output stream will be the compressed form of the data
        ///   written.
        /// </para>
        ///
        /// <para>
        ///   To decompress data, use the <see cref="Ionic.Zlib.DeflateStream"/> class.
        /// </para>
        ///
        /// </remarks>
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            bool mustWait = false;

            // This method does this:
            //   0. handles any pending exceptions
            //   1. write any buffers that are ready to be written,
            //   2. fills a work buffer; when full, flip state to 'Filled',
            //   3. if more data to be written,  goto step 1

            if (_isClosed)
                throw new InvalidOperationException();

            // dispense any exceptions that occurred on the BG threads
            if (_pendingException != null)
            {
                _handlingException = true;
                var pe = _pendingException;
                _pendingException = null;
                throw pe;
            }

            if (count == 0) return;

            if (!_firstWriteDone)
            {
                // Want to do this on first Write, first session, and not in the
                // constructor.  We want to allow MaxBufferPairs to
                // change after construction, but before first Write.
                _InitializePoolOfWorkItems();
                _firstWriteDone = true;
            }


            do
            {
                // may need to make buffers available
                EmitPendingBuffers(false, mustWait);

                mustWait = false;
                // use current buffer, or get a new buffer to fill
                int ix = -1;
                if (_currentlyFilling >= 0)
                {
                    ix = _currentlyFilling;
                    TraceOutput(TraceBits.WriteTake,
                                "Write    notake   wi({0}) lf({1})",
                                ix,
                                _lastFilled);
                }
                else
                {
                    TraceOutput(TraceBits.WriteTake, "Write    take?");
                    if (_toFill.Count == 0)
                    {
                        // no available buffers, so... need to emit
                        // compressed buffers.
                        mustWait = true;
                        continue;
                    }

                    ix = _toFill.Dequeue();
                    TraceOutput(TraceBits.WriteTake,
                                "Write    take     wi({0}) lf({1})",
                                ix,
                                _lastFilled);
                    ++_lastFilled; // TODO: consider rollover?
                }

                WorkItem workitem = _pool[ix];

                int limit = ((workitem.buffer.Length - workitem.inputBytesAvailable) > count)
                    ? count
                    : (workitem.buffer.Length - workitem.inputBytesAvailable);

                workitem.ordinal = _lastFilled;

                TraceOutput(TraceBits.Write,
                            "Write    lock     wi({0}) ord({1}) iba({2})",
                            workitem.index,
                            workitem.ordinal,
                            workitem.inputBytesAvailable
                            );

                // copy from the provided buffer to our workitem, starting at
                // the tail end of whatever data we might have in there currently.
                Buffer.BlockCopy(buffer,
                                 offset,
                                 workitem.buffer,
                                 workitem.inputBytesAvailable,
                                 limit);

                count -= limit;
                offset += limit;
                workitem.inputBytesAvailable += limit;
                if (workitem.inputBytesAvailable == workitem.buffer.Length)
                {
                    // No need for interlocked.increment: the Write()
                    // method is documented as not multi-thread safe, so
                    // we can assume Write() calls come in from only one
                    // thread.
                    TraceOutput(TraceBits.Write,
                                "Write    QUWI     wi({0}) ord({1}) iba({2}) nf({3})",
                                workitem.index,
                                workitem.ordinal,
                                workitem.inputBytesAvailable );

                    if (!ThreadPool.QueueUserWorkItem( _DeflateOne, workitem ))
                        throw new Exception("Cannot enqueue workitem");

                    _currentlyFilling = -1; // will get a new buffer next time
                }
                else
                    _currentlyFilling = ix;

                if (count > 0)
                    TraceOutput(TraceBits.WriteEnter, "Write    more");
            }
            while (count > 0);  // until no more to write

            TraceOutput(TraceBits.WriteEnter, "Write    exit");
            return;
        }



        private void _FlushFinish()
        {
            // After writing a series of compressed buffers, each one closed
            // with Flush.Sync, we now write the final one as Flush.Finish,
            // and then stop.
            byte[] buffer = new byte[128];
            var compressor = new ZlibCodec();
            int rc = compressor.InitializeDeflate(_compressLevel, false);
            compressor.InputBuffer = null;
            compressor.NextIn = 0;
            compressor.AvailableBytesIn = 0;
            compressor.OutputBuffer = buffer;
            compressor.NextOut = 0;
            compressor.AvailableBytesOut = buffer.Length;
            rc = compressor.Deflate(FlushType.Finish);

            if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                throw new Exception("deflating: " + compressor.Message);

            if (buffer.Length - compressor.AvailableBytesOut > 0)
            {
                TraceOutput(TraceBits.EmitBegin,
                            "Emit     begin    flush bytes({0})",
                            buffer.Length - compressor.AvailableBytesOut);

                _outStream.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);

                TraceOutput(TraceBits.EmitDone,
                            "Emit     done     flush");
            }

            compressor.EndDeflate();

            _Crc32 = _runningCrc.Crc32Result;
        }


        private void _Flush(bool lastInput)
        {
            if (_isClosed)
                throw new InvalidOperationException();

            if (emitting) return;

            // compress any partial buffer
            if (_currentlyFilling >= 0)
            {
                WorkItem workitem = _pool[_currentlyFilling];
                _DeflateOne(workitem);
                _currentlyFilling = -1; // get a new buffer next Write()
            }

            if (lastInput)
            {
                EmitPendingBuffers(true, false);
                _FlushFinish();
            }
            else
            {
                EmitPendingBuffers(false, false);
            }
        }



        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            if (_pendingException != null)
            {
                _handlingException = true;
                var pe = _pendingException;
                _pendingException = null;
                throw pe;
            }
            if (_handlingException)
                return;

            _Flush(false);
        }


        /// <summary>
        /// Close the stream.
        /// </summary>
        /// <remarks>
        /// You must call Close on the stream to guarantee that all of the data written in has
        /// been compressed, and the compressed data has been written out.
        /// </remarks>
        public override void Close()
        {
            TraceOutput(TraceBits.Session, "Close {0:X8}", this.GetHashCode());

            if (_pendingException != null)
            {
                _handlingException = true;
                var pe = _pendingException;
                _pendingException = null;
                throw pe;
            }

            if (_handlingException)
                return;

            if (_isClosed) return;

            _Flush(true);

            if (!_leaveOpen)
                _outStream.Close();

            _isClosed= true;
        }



        // workitem 10030 - implement a new Dispose method

        /// <summary>Dispose the object</summary>
        /// <remarks>
        ///   <para>
        ///     Because ParallelDeflateOutputStream is IDisposable, the
        ///     application must call this method when finished using the instance.
        ///   </para>
        ///   <para>
        ///     This method is generally called implicitly upon exit from
        ///     a <c>using</c> scope in C# (<c>Using</c> in VB).
        ///   </para>
        /// </remarks>
        new public void Dispose()
        {
            TraceOutput(TraceBits.Lifecycle, "Dispose  {0:X8}", this.GetHashCode());
            Close();
            _pool = null;
            Dispose(true);
        }



        /// <summary>The Dispose method</summary>
        /// <param name="disposing">
        ///   indicates whether the Dispose method was invoked by user code.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        /// <summary>
        ///   Resets the stream for use with another stream.
        /// </summary>
        /// <remarks>
        ///   Because the ParallelDeflateOutputStream is expensive to create, it
        ///   has been designed so that it can be recycled and re-used.  You have
        ///   to call Close() on the stream first, then you can call Reset() on
        ///   it, to use it again on another stream.
        /// </remarks>
        ///
        /// <param name="stream">
        ///   The new output stream for this era.
        /// </param>
        ///
        /// <example>
        /// <code>
        /// ParallelDeflateOutputStream deflater = null;
        /// foreach (var inputFile in listOfFiles)
        /// {
        ///     string outputFile = inputFile + ".compressed";
        ///     using (System.IO.Stream input = System.IO.File.OpenRead(inputFile))
        ///     {
        ///         using (var outStream = System.IO.File.Create(outputFile))
        ///         {
        ///             if (deflater == null)
        ///                 deflater = new ParallelDeflateOutputStream(outStream,
        ///                                                            CompressionLevel.Best,
        ///                                                            CompressionStrategy.Default,
        ///                                                            true);
        ///             deflater.Reset(outStream);
        ///
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 deflater.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public void Reset(Stream stream)
        {
            TraceOutput(TraceBits.Session, "-------------------------------------------------------");
            TraceOutput(TraceBits.Session, "Reset {0:X8} firstDone({1})", this.GetHashCode(), _firstWriteDone);

            if (!_firstWriteDone) return;

            // reset all status
            _toWrite.Clear();
            _toFill.Clear();
            foreach (var workitem in _pool)
            {
                _toFill.Enqueue(workitem.index);
                workitem.ordinal = -1;
            }

            _firstWriteDone = false;
            _totalBytesProcessed = 0L;
            _runningCrc = new Ionic.Crc.CRC32();
            _isClosed= false;
            _currentlyFilling = -1;
            _lastFilled = -1;
            _lastWritten = -1;
            _latestCompressed = -1;
            _outStream = stream;
        }




        private void EmitPendingBuffers(bool doAll, bool mustWait)
        {
            // When combining parallel deflation with a ZipSegmentedStream, it's
            // possible for the ZSS to throw from within this method.  In that
            // case, Close/Dispose will be called on this stream, if this stream
            // is employed within a using or try/finally pair as required. But
            // this stream is unaware of the pending exception, so the Close()
            // method invokes this method AGAIN.  This can lead to a deadlock.
            // Therefore, failfast if re-entering.

            if (emitting) return;
            emitting = true;
            if (doAll || mustWait)
                _newlyCompressedBlob.WaitOne();

            do
            {
                int firstSkip = -1;
                int millisecondsToWait = doAll ? 200 : (mustWait ? -1 : 0);
                int nextToWrite = -1;

                do
                {
                    if (Monitor.TryEnter(_toWrite, millisecondsToWait))
                    {
                        nextToWrite = -1;
                        try
                        {
                            if (_toWrite.Count > 0)
                                nextToWrite = _toWrite.Dequeue();
                        }
                        finally
                        {
                            Monitor.Exit(_toWrite);
                        }

                        if (nextToWrite >= 0)
                        {
                            WorkItem workitem = _pool[nextToWrite];
                            if (workitem.ordinal != _lastWritten + 1)
                            {
                                // out of order. requeue and try again.
                                TraceOutput(TraceBits.EmitSkip,
                                            "Emit     skip     wi({0}) ord({1}) lw({2}) fs({3})",
                                            workitem.index,
                                            workitem.ordinal,
                                            _lastWritten,
                                            firstSkip);

                                lock(_toWrite)
                                {
                                    _toWrite.Enqueue(nextToWrite);
                                }

                                if (firstSkip == nextToWrite)
                                {
                                    // We went around the list once.
                                    // None of the items in the list is the one we want.
                                    // Now wait for a compressor to signal again.
                                    _newlyCompressedBlob.WaitOne();
                                    firstSkip = -1;
                                }
                                else if (firstSkip == -1)
                                    firstSkip = nextToWrite;

                                continue;
                            }

                            firstSkip = -1;

                            TraceOutput(TraceBits.EmitBegin,
                                        "Emit     begin    wi({0}) ord({1})              cba({2})",
                                        workitem.index,
                                        workitem.ordinal,
                                        workitem.compressedBytesAvailable);

                            _outStream.Write(workitem.compressed, 0, workitem.compressedBytesAvailable);
                            _runningCrc.Combine(workitem.crc, workitem.inputBytesAvailable);
                            _totalBytesProcessed += workitem.inputBytesAvailable;
                            workitem.inputBytesAvailable = 0;

                            TraceOutput(TraceBits.EmitDone,
                                        "Emit     done     wi({0}) ord({1})              cba({2}) mtw({3})",
                                        workitem.index,
                                        workitem.ordinal,
                                        workitem.compressedBytesAvailable,
                                        millisecondsToWait);

                            _lastWritten = workitem.ordinal;
                            _toFill.Enqueue(workitem.index);

                            // don't wait next time through
                            if (millisecondsToWait == -1) millisecondsToWait = 0;
                        }
                    }
                    else
                        nextToWrite = -1;

                } while (nextToWrite >= 0);

            } while (doAll && (_lastWritten != _latestCompressed));

            emitting = false;
        }



#if OLD
        private void _PerpetualWriterMethod(object state)
        {
            TraceOutput(TraceBits.WriterThread, "_PerpetualWriterMethod START");

            try
            {
                do
                {
                    // wait for the next session
                    TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    _sessionReset.WaitOne(begin) PWM");
                    _sessionReset.WaitOne();
                    TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    _sessionReset.WaitOne(done)  PWM");

                    if (_isDisposed) break;

                    TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    _sessionReset.Reset()        PWM");
                    _sessionReset.Reset();

                    // repeatedly write buffers as they become ready
                    WorkItem workitem = null;
                    Ionic.Zlib.CRC32 c= new Ionic.Zlib.CRC32();
                    do
                    {
                        workitem = _pool[_nextToWrite % _pc];
                        lock(workitem)
                        {
                            if (_noMoreInputForThisSegment)
                                TraceOutput(TraceBits.Write,
                                               "Write    drain    wi({0}) stat({1}) canuse({2})  cba({3})",
                                               workitem.index,
                                               workitem.status,
                                               (workitem.status == (int)WorkItem.Status.Compressed),
                                               workitem.compressedBytesAvailable);

                            do
                            {
                                if (workitem.status == (int)WorkItem.Status.Compressed)
                                {
                                    TraceOutput(TraceBits.WriteBegin,
                                                   "Write    begin    wi({0}) stat({1})              cba({2})",
                                                   workitem.index,
                                                   workitem.status,
                                                   workitem.compressedBytesAvailable);

                                    workitem.status = (int)WorkItem.Status.Writing;
                                    _outStream.Write(workitem.compressed, 0, workitem.compressedBytesAvailable);
                                    c.Combine(workitem.crc, workitem.inputBytesAvailable);
                                    _totalBytesProcessed += workitem.inputBytesAvailable;
                                    _nextToWrite++;
                                    workitem.inputBytesAvailable= 0;
                                    workitem.status = (int)WorkItem.Status.Done;

                                    TraceOutput(TraceBits.WriteDone,
                                                   "Write    done     wi({0}) stat({1})              cba({2})",
                                                   workitem.index,
                                                   workitem.status,
                                                   workitem.compressedBytesAvailable);


                                    Monitor.Pulse(workitem);
                                    break;
                                }
                                else
                                {
                                    int wcycles = 0;
                                    // I've locked a workitem I cannot use.
                                    // Therefore, wake someone else up, and then release the lock.
                                    while (workitem.status != (int)WorkItem.Status.Compressed)
                                    {
                                        TraceOutput(TraceBits.WriteWait,
                                                       "Write    waiting  wi({0}) stat({1}) nw({2}) nf({3}) nomore({4})",
                                                       workitem.index,
                                                       workitem.status,
                                                       _nextToWrite, _nextToFill,
                                                       _noMoreInputForThisSegment );

                                        if (_noMoreInputForThisSegment && _nextToWrite == _nextToFill)
                                            break;

                                        wcycles++;

                                        // wake up someone else
                                        Monitor.Pulse(workitem);
                                        // release and wait
                                        Monitor.Wait(workitem);

                                        if (workitem.status == (int)WorkItem.Status.Compressed)
                                            TraceOutput(TraceBits.WriteWait,
                                                           "Write    A-OK     wi({0}) stat({1}) iba({2}) cba({3}) cyc({4})",
                                                           workitem.index,
                                                           workitem.status,
                                                           workitem.inputBytesAvailable,
                                                           workitem.compressedBytesAvailable,
                                                           wcycles);
                                    }

                                    if (_noMoreInputForThisSegment && _nextToWrite == _nextToFill)
                                        break;

                                }
                            }
                            while (true);
                        }

                        if (_noMoreInputForThisSegment)
                            TraceOutput(TraceBits.Write,
                                           "Write    nomore  nw({0}) nf({1}) break({2})",
                                           _nextToWrite, _nextToFill, (_nextToWrite == _nextToFill));

                        if (_noMoreInputForThisSegment && _nextToWrite == _nextToFill)
                            break;

                    } while (true);


                    // Finish:
                    // After writing a series of buffers, closing each one with
                    // Flush.Sync, we now write the final one as Flush.Finish, and
                    // then stop.
                    byte[] buffer = new byte[128];
                    ZlibCodec compressor = new ZlibCodec();
                    int rc = compressor.InitializeDeflate(_compressLevel, false);
                    compressor.InputBuffer = null;
                    compressor.NextIn = 0;
                    compressor.AvailableBytesIn = 0;
                    compressor.OutputBuffer = buffer;
                    compressor.NextOut = 0;
                    compressor.AvailableBytesOut = buffer.Length;
                    rc = compressor.Deflate(FlushType.Finish);

                    if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                        throw new Exception("deflating: " + compressor.Message);

                    if (buffer.Length - compressor.AvailableBytesOut > 0)
                    {
                        TraceOutput(TraceBits.WriteBegin,
                                       "Write    begin    flush bytes({0})",
                                       buffer.Length - compressor.AvailableBytesOut);

                        _outStream.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);

                        TraceOutput(TraceBits.WriteBegin,
                                       "Write    done     flush");
                    }

                    compressor.EndDeflate();

                    _Crc32 = c.Crc32Result;

                    // signal that writing is complete:
                    TraceOutput(TraceBits.Synch, "Synch    _writingDone.Set()           PWM");
                    _writingDone.Set();
                }
                while (true);
            }
            catch (System.Exception exc1)
            {
                lock(_eLock)
                {
                    // expose the exception to the main thread
                    if (_pendingException!=null)
                        _pendingException = exc1;
                }
            }

            TraceOutput(TraceBits.WriterThread, "_PerpetualWriterMethod FINIS");
        }
#endif




        private void _DeflateOne(Object wi)
        {
            // compress one buffer
            WorkItem workitem = (WorkItem) wi;
            try
            {
                int myItem = workitem.index;
                Ionic.Crc.CRC32 crc = new Ionic.Crc.CRC32();

                // calc CRC on the buffer
                crc.SlurpBlock(workitem.buffer, 0, workitem.inputBytesAvailable);

                // deflate it
                DeflateOneSegment(workitem);

                // update status
                workitem.crc = crc.Crc32Result;
                TraceOutput(TraceBits.Compress,
                            "Compress          wi({0}) ord({1}) len({2})",
                            workitem.index,
                            workitem.ordinal,
                            workitem.compressedBytesAvailable
                            );

                lock(_latestLock)
                {
                    if (workitem.ordinal > _latestCompressed)
                        _latestCompressed = workitem.ordinal;
                }
                lock (_toWrite)
                {
                    _toWrite.Enqueue(workitem.index);
                }
                _newlyCompressedBlob.Set();
            }
            catch (System.Exception exc1)
            {
                lock(_eLock)
                {
                    // expose the exception to the main thread
                    if (_pendingException!=null)
                        _pendingException = exc1;
                }
            }
        }




        private bool DeflateOneSegment(WorkItem workitem)
        {
            ZlibCodec compressor = workitem.compressor;
            int rc= 0;
            compressor.ResetDeflate();
            compressor.NextIn = 0;

            compressor.AvailableBytesIn = workitem.inputBytesAvailable;

            // step 1: deflate the buffer
            compressor.NextOut = 0;
            compressor.AvailableBytesOut =  workitem.compressed.Length;
            do
            {
                compressor.Deflate(FlushType.None);
            }
            while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);

            // step 2: flush (sync)
            rc = compressor.Deflate(FlushType.Sync);

            workitem.compressedBytesAvailable= (int) compressor.TotalBytesOut;
            return true;
        }


        [System.Diagnostics.ConditionalAttribute("Trace")]
        private void TraceOutput(TraceBits bits, string format, params object[] varParams)
        {
            if ((bits & _DesiredTrace) != 0)
            {
                lock(_outputLock)
                {
                    int tid = Thread.CurrentThread.GetHashCode();
#if !SILVERLIGHT
                    Console.ForegroundColor = (ConsoleColor) (tid % 8 + 8);
#endif
                    Console.Write("{0:000} PDOS ", tid);
                    Console.WriteLine(format, varParams);
#if !SILVERLIGHT
                    Console.ResetColor();
#endif
                }
            }
        }


        // used only when Trace is defined
        [Flags]
        enum TraceBits : uint
        {
            None         = 0,
            NotUsed1     = 1,
            EmitLock     = 2,
            EmitEnter    = 4,    // enter _EmitPending
            EmitBegin    = 8,    // begin to write out
            EmitDone     = 16,   // done writing out
            EmitSkip     = 32,   // writer skipping a workitem
            EmitAll      = 58,   // All Emit flags
            Flush        = 64,
            Lifecycle    = 128,  // constructor/disposer
            Session      = 256,  // Close/Reset
            Synch        = 512,  // thread synchronization
            Instance     = 1024, // instance settings
            Compress     = 2048,  // compress task
            Write        = 4096,    // filling buffers, when caller invokes Write()
            WriteEnter   = 8192,    // upon entry to Write()
            WriteTake    = 16384,    // on _toFill.Take()
            All          = 0xffffffff,
        }



        /// <summary>
        /// Indicates whether the stream supports Seek operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }


        /// <summary>
        /// Indicates whether the stream supports Read operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanRead
        {
            get {return false;}
        }

        /// <summary>
        /// Indicates whether the stream supports Write operations.
        /// </summary>
        /// <remarks>
        /// Returns true if the provided stream is writable.
        /// </remarks>
        public override bool CanWrite
        {
            get { return _outStream.CanWrite; }
        }

        /// <summary>
        /// Reading this property always throws a NotSupportedException.
        /// </summary>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Returns the current position of the output stream.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Because the output gets written by a background thread,
        ///     the value may change asynchronously.  Setting this
        ///     property always throws a NotSupportedException.
        ///   </para>
        /// </remarks>
        public override long Position
        {
            get { return _outStream.Position; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="buffer">
        ///   The buffer into which data would be read, IF THIS METHOD
        ///   ACTUALLY DID ANYTHING.
        /// </param>
        /// <param name="offset">
        ///   The offset within that data array at which to insert the
        ///   data that is read, IF THIS METHOD ACTUALLY DID
        ///   ANYTHING.
        /// </param>
        /// <param name="count">
        ///   The number of bytes to write, IF THIS METHOD ACTUALLY DID
        ///   ANYTHING.
        /// </param>
        /// <returns>nothing.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="offset">
        ///   The offset to seek to....
        ///   IF THIS METHOD ACTUALLY DID ANYTHING.
        /// </param>
        /// <param name="origin">
        ///   The reference specifying how to apply the offset....  IF
        ///   THIS METHOD ACTUALLY DID ANYTHING.
        /// </param>
        /// <returns>nothing. It always throws.</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        /// <param name="value">
        ///   The new value for the stream length....  IF
        ///   THIS METHOD ACTUALLY DID ANYTHING.
        /// </param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }

}


