using SharpCompress.Common;

namespace SharpCompress.Archive
{
    internal interface IArchiveExtractionListener : IExtractionListener
    {
        void EnsureEntriesLoaded();
        void FireEntryExtractionBegin(IArchiveEntry entry);
        void FireEntryExtractionEnd(IArchiveEntry entry);
    }
}