using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;

#if PORTABLE
using SharpCompress.Common.Rar.Headers;
#endif

namespace SharpCompress.Reader
{
    /// <summary>
    /// A generic push reader that reads unseekable comrpessed streams.
    /// </summary>
    internal abstract class AbstractReader<TEntry, TVolume> : IReader, IExtractionListener
        where TEntry : Entry
        where TVolume : Volume
    {
        private bool completed;
        private IEnumerator<TEntry> entriesForCurrentReadStream;
        private bool wroteCurrentEntry;

        public event EventHandler<CompressedBytesReadEventArgs> CompressedBytesRead;
        public event EventHandler<FilePartExtractionBeginEventArgs> FilePartExtractionBegin;

        internal AbstractReader(Options options, ArchiveType archiveType)
        {
            ArchiveType = archiveType;
            Options = options;
        }

        internal Options Options { get; private set; }

        public ArchiveType ArchiveType { get; private set; }

        /// <summary>
        /// Current volume that the current entry resides in
        /// </summary>
        public abstract TVolume Volume { get; }

        /// <summary>
        /// Current file entry 
        /// </summary>
        public TEntry Entry
        {
            get { return entriesForCurrentReadStream.Current; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (entriesForCurrentReadStream != null)
            {
                entriesForCurrentReadStream.Dispose();
            }
            Volume.Dispose();
        }

        #endregion

        public bool MoveToNextEntry()
        {
            if (completed)
            {
                return false;
            }
            if (entriesForCurrentReadStream == null)
            {
                return LoadStreamForReading(RequestInitialStream());
            }
            if (!wroteCurrentEntry)
            {
                SkipEntry();
            }
            wroteCurrentEntry = false;
            if (NextEntryForCurrentStream())
            {
                return true;
            }
            completed = true;
            return false;
        }

        internal bool LoadStreamForReading(Stream stream)
        {
            if (entriesForCurrentReadStream != null)
            {
                entriesForCurrentReadStream.Dispose();
            }
            if ((stream == null) || (!stream.CanRead))
            {
                throw new MultipartStreamRequiredException("File is split into multiple archives: '"
                                                           + Entry.Key +
                                                           "'. A new readable stream is required.  Use Cancel if it was intended.");
            }
            entriesForCurrentReadStream = GetEntries(stream).GetEnumerator();
            if (entriesForCurrentReadStream.MoveNext())
            {
                return true;
            }
            return false;
        }

        internal virtual Stream RequestInitialStream()
        {
            return Volume.Stream;
        }

        internal virtual bool NextEntryForCurrentStream()
        {
            return entriesForCurrentReadStream.MoveNext();
        }

        internal abstract IEnumerable<TEntry> GetEntries(Stream stream);

        #region Entry Skip/Write

        private void SkipEntry()
        {
            if (!Entry.IsDirectory)
            {
                Skip();
            }
        }

        private readonly byte[] skipBuffer = new byte[4096];

        private void Skip()
        {
            if (!Entry.IsSolid)
            {
                var rawStream = Entry.Parts.First().GetRawStream();

                if (rawStream != null)
                {
                    var bytesToAdvance = Entry.CompressedSize;
                    for (var i = 0; i < bytesToAdvance / skipBuffer.Length; i++)
                    {
                        rawStream.Read(skipBuffer, 0, skipBuffer.Length);
                    }
                    rawStream.Read(skipBuffer, 0, (int)(bytesToAdvance % skipBuffer.Length));
                    return;
                }
            }
            using (var s = OpenEntryStream())
            {
                while (s.Read(skipBuffer, 0, skipBuffer.Length) > 0)
                {
                }
            }
        }

        public void WriteEntryTo(Stream writableStream)
        {
            if (wroteCurrentEntry)
            {
                throw new ArgumentException("WriteEntryTo or OpenEntryStream can only be called once.");
            }
            if ((writableStream == null) || (!writableStream.CanWrite))
            {
                throw new ArgumentNullException(
                    "A writable Stream was required.  Use Cancel if that was intended.");
            }
            Write(writableStream);
            wroteCurrentEntry = true;
        }

        internal void Write(Stream writeStream)
        {
            using (Stream s = OpenEntryStream())
            {
                s.TransferTo(writeStream);
            }
        }

        public EntryStream OpenEntryStream()
        {
            if (wroteCurrentEntry)
            {
                throw new ArgumentException("WriteEntryTo or OpenEntryStream can only be called once.");
            }
            var stream = GetEntryStream();
            wroteCurrentEntry = true;
            return stream;
        }

        protected virtual EntryStream GetEntryStream()
        {
            return new EntryStream(Entry.Parts.First().GetCompressedStream());
        }

        #endregion

        IEntry IReader.Entry
        {
            get { return Entry; }
        }

        void IExtractionListener.FireCompressedBytesRead(long currentPartCompressedBytes, long compressedReadBytes)
        {
            if (CompressedBytesRead != null)
            {
                CompressedBytesRead(this, new CompressedBytesReadEventArgs()
                                              {
                                                  CurrentFilePartCompressedBytesRead = currentPartCompressedBytes,
                                                  CompressedBytesRead = compressedReadBytes
                                              });
            }
        }

        void IExtractionListener.FireFilePartExtractionBegin(string name, long size, long compressedSize)
        {
            if (FilePartExtractionBegin != null)
            {
                FilePartExtractionBegin(this, new FilePartExtractionBeginEventArgs()
                                                  {
                                                      CompressedSize = compressedSize,
                                                      Size = size,
                                                      Name = name,
                                                  });
            }
        }
    }
}