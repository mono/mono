using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpCompress.Common;

namespace SharpCompress.Archive
{
    internal abstract class AbstractWritableArchive<TEntry, TVolume> : AbstractArchive<TEntry, TVolume>
        where TEntry : IArchiveEntry
        where TVolume : IVolume
    {
        private readonly List<TEntry> newEntries = new List<TEntry>();
        private readonly List<TEntry> removedEntries = new List<TEntry>();

        private readonly List<TEntry> modifiedEntries = new List<TEntry>();
        private bool hasModifications;

        internal AbstractWritableArchive(ArchiveType type)
            : base(type)
        {
        }

        internal AbstractWritableArchive(ArchiveType type, Stream stream, Options options)
            : base(type, stream.AsEnumerable(), options, null)
        {
        }

#if !PORTABLE && !NETFX_CORE
        internal AbstractWritableArchive(ArchiveType type, FileInfo fileInfo, Options options)
            : base(type, fileInfo, options, null)
        {
        }
#endif

        public override ICollection<TEntry> Entries
        {
            get
            {
                if (hasModifications)
                {
                    return modifiedEntries;
                }
                return base.Entries;
            }
        }

        private void RebuildModifiedCollection()
        {
            hasModifications = true;
            newEntries.RemoveAll(v => removedEntries.Contains(v));
            modifiedEntries.Clear();
            modifiedEntries.AddRange(OldEntries.Concat(newEntries));
        }

        private IEnumerable<TEntry> OldEntries
        {
            get { return base.Entries.Where(x => !removedEntries.Contains(x)); }
        }

        public void RemoveEntry(TEntry entry)
        {
            if (!removedEntries.Contains(entry))
            {
                removedEntries.Add(entry);
                RebuildModifiedCollection();
            }
        }

        public TEntry AddEntry(string key, Stream source,
                             long size = 0, DateTime? modified = null)
        {
            return AddEntry(key, source, false, size, modified);
        }

        public TEntry AddEntry(string key, Stream source, bool closeStream,
                             long size = 0, DateTime? modified = null)
        {
            if (key.StartsWith("/")
                || key.StartsWith("\\"))
            {
                key = key.Substring(1);
            }
            if (DoesKeyMatchExisting(key))
            {
                throw new ArchiveException("Cannot add entry with duplicate key: " + key);
            }
            var entry = CreateEntry(key, source, size, modified, closeStream);
            newEntries.Add(entry);
            RebuildModifiedCollection();
            return entry;
        }

        private bool DoesKeyMatchExisting(string key)
        {
            foreach (var path in Entries.Select(x => x.Key))
            {
                var p = path.Replace('/','\\');
                if (p.StartsWith("\\"))
                {
                    p = p.Substring(1);
                }
                return string.Equals(p, key, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public void SaveTo(Stream stream, CompressionInfo compressionType, Encoding encoding = null)
        {
            //reset streams of new entries
            newEntries.Cast<IWritableArchiveEntry>().ForEach(x => x.Stream.Seek(0, SeekOrigin.Begin));
            SaveTo(stream, compressionType, encoding ?? ArchiveEncoding.Default, OldEntries, newEntries);
        }

        protected TEntry CreateEntry(string key, Stream source, long size, DateTime? modified,
            bool closeStream)
        {
            if (!source.CanRead || !source.CanSeek)
            {
                throw new ArgumentException("Streams must be readable and seekable to use the Writing Archive API");
            }
            return CreateEntryInternal(key, source, size, modified, closeStream);
        }

        protected abstract TEntry CreateEntryInternal(string key, Stream source, long size, DateTime? modified,
                                              bool closeStream);

        protected abstract void SaveTo(Stream stream, CompressionInfo compressionType, Encoding encoding,
                                       IEnumerable<TEntry> oldEntries, IEnumerable<TEntry> newEntries);

        public override void Dispose()
        {
            base.Dispose();
            newEntries.Cast<Entry>().ForEach(x => x.Close());
            removedEntries.Cast<Entry>().ForEach(x => x.Close());
            modifiedEntries.Cast<Entry>().ForEach(x => x.Close());
        }
    }
}