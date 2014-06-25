using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace SharpCompress.Archive
{
    internal abstract class AbstractArchive<TEntry, TVolume> : IArchive, IArchiveExtractionListener
        where TEntry : IArchiveEntry
        where TVolume : IVolume
    {
        private readonly LazyReadOnlyCollection<TVolume> lazyVolumes;
        private readonly LazyReadOnlyCollection<TEntry> lazyEntries;

        public event EventHandler<ArchiveExtractionEventArgs<IArchiveEntry>> EntryExtractionBegin;
        public event EventHandler<ArchiveExtractionEventArgs<IArchiveEntry>> EntryExtractionEnd;

        public event EventHandler<CompressedBytesReadEventArgs> CompressedBytesRead;
        public event EventHandler<FilePartExtractionBeginEventArgs> FilePartExtractionBegin;

        protected string Password { get; private set; }

#if !PORTABLE && !NETFX_CORE
        internal AbstractArchive(ArchiveType type, FileInfo fileInfo, Options options, string password)
        {
            Type = type;
            Password = password;
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("File does not exist: " + fileInfo.FullName);
            }
            options = (Options) FlagUtility.SetFlag(options, Options.KeepStreamsOpen, false);
            lazyVolumes = new LazyReadOnlyCollection<TVolume>(LoadVolumes(fileInfo, options));
            lazyEntries = new LazyReadOnlyCollection<TEntry>(LoadEntries(Volumes));
        }


        protected abstract IEnumerable<TVolume> LoadVolumes(FileInfo file, Options options);
#endif

        internal AbstractArchive(ArchiveType type, IEnumerable<Stream> streams, Options options, string password)
        {
            Type = type;
            Password = password;
            lazyVolumes = new LazyReadOnlyCollection<TVolume>(LoadVolumes(streams.Select(CheckStreams), options));
            lazyEntries = new LazyReadOnlyCollection<TEntry>(LoadEntries(Volumes));
        }

        internal AbstractArchive(ArchiveType type)
        {
            Type = type;
            lazyVolumes = new LazyReadOnlyCollection<TVolume>(Enumerable.Empty<TVolume>());
            lazyEntries = new LazyReadOnlyCollection<TEntry>(Enumerable.Empty<TEntry>());
        }
        public ArchiveType Type { get; private set; }

        void IArchiveExtractionListener.FireEntryExtractionBegin(IArchiveEntry entry)
        {
            if (EntryExtractionBegin != null)
            {
                EntryExtractionBegin(this, new ArchiveExtractionEventArgs<IArchiveEntry>(entry));
            }
        }

        void IArchiveExtractionListener.FireEntryExtractionEnd(IArchiveEntry entry)
        {
            if (EntryExtractionEnd != null)
            {
                EntryExtractionEnd(this, new ArchiveExtractionEventArgs<IArchiveEntry>(entry));
            }
        }

        private static Stream CheckStreams(Stream stream)
        {
            if (!stream.CanSeek || !stream.CanRead)
            {
                throw new ArgumentException("Archive streams must be Readable and Seekable");
            }
            return stream;
        }

        /// <summary>
        /// Returns an ReadOnlyCollection of all the RarArchiveEntries across the one or many parts of the RarArchive.
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<TEntry> Entries
        {
            get { return lazyEntries; }
        }

        /// <summary>
        /// Returns an ReadOnlyCollection of all the RarArchiveVolumes across the one or many parts of the RarArchive.
        /// </summary>
        /// <returns></returns>
        public ICollection<TVolume> Volumes
        {
            get { return lazyVolumes; }
        }

        /// <summary>
        /// The total size of the files compressed in the archive.
        /// </summary>
        public long TotalSize
        {
            get { return Entries.Aggregate(0L, (total, cf) => total + cf.CompressedSize); }
        }

        protected abstract IEnumerable<TVolume> LoadVolumes(IEnumerable<Stream> streams, Options options);
        protected abstract IEnumerable<TEntry> LoadEntries(IEnumerable<TVolume> volumes);

        IEnumerable<IArchiveEntry> IArchive.Entries
        {
            get { return Entries.Cast<IArchiveEntry>(); }
        }

        IEnumerable<IVolume> IArchive.Volumes
        {
            get { return lazyVolumes.Cast<IVolume>(); }
        }

        private bool disposed;

        public virtual void Dispose()
        {
            if (!disposed)
            {
                lazyVolumes.ForEach(v => v.Dispose());
                lazyEntries.GetLoaded().Cast<Entry>().ForEach(x => x.Close());
                disposed = true;
            }
        }

        void IArchiveExtractionListener.EnsureEntriesLoaded()
        {
            lazyEntries.EnsureFullyLoaded();
            lazyVolumes.EnsureFullyLoaded();
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

        /// <summary>
        /// Use this method to extract all entries in an archive in order.
        /// This is primarily for SOLID Rar Archives or 7Zip Archives as they need to be 
        /// extracted sequentially for the best performance.
        /// 
        /// This method will load all entry information from the archive.
        /// 
        /// WARNING: this will reuse the underlying stream for the archive.  Errors may 
        /// occur if this is used at the same time as other extraction methods on this instance.
        /// </summary>
        /// <returns></returns>
        public IReader ExtractAllEntries()
        {
            ((IArchiveExtractionListener)this).EnsureEntriesLoaded();
            return CreateReaderForSolidExtraction();
        }

        protected abstract IReader CreateReaderForSolidExtraction();

        /// <summary>
        /// Archive is SOLID (this means the Archive saved bytes by reusing information which helps for archives containing many small files).
        /// </summary>
        public virtual bool IsSolid
        {
            get { return false; }
        }


        /// <summary>
        /// The archive can find all the parts of the archive needed to fully extract the archive.  This forces the parsing of the entire archive.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                ((IArchiveExtractionListener)this).EnsureEntriesLoaded();
                return Entries.All(x => x.IsComplete);
            }
        }
    }
}