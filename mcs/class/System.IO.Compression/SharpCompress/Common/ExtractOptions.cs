using System;

namespace SharpCompress.Common
{
    [Flags]
    internal enum ExtractOptions
    {
        None,

        /// <summary>
        /// overwrite target if it exists
        /// </summary>
        Overwrite,

        /// <summary>
        /// extract with internal directory structure
        /// </summary>
        ExtractFullPath,
    }
}