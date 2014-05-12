using System;

namespace SharpCompress.Common
{
    internal class ArchiveExtractionEventArgs<T> : EventArgs
    {
        internal ArchiveExtractionEventArgs(T entry)
        {
            Item = entry;
        }

        public T Item { get; private set; }
    }
}