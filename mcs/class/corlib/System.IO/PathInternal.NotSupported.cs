namespace System.IO
{
    internal static partial class PathInternal
    {
        internal const char DirectorySeparatorChar = '\0';
        internal const char AltDirectorySeparatorChar = '\0';
        internal const char VolumeSeparatorChar = '\0';
        internal const char PathSeparator = '\0';

        internal const string DirectorySeparatorCharAsString = "";
        internal const string ParentDirectoryPrefix = "";

        internal static int GetRootLength(ReadOnlySpan<char> path) => throw new PlatformNotSupportedException ();
        internal static bool IsDirectorySeparator(char c) => throw new PlatformNotSupportedException ();
        internal static string NormalizeDirectorySeparators(string path) => throw new PlatformNotSupportedException ();
        internal static bool IsPartiallyQualified(ReadOnlySpan<char> path) => throw new PlatformNotSupportedException ();
        internal static bool IsEffectivelyEmpty(string path) => throw new PlatformNotSupportedException ();
        internal static bool IsEffectivelyEmpty(ReadOnlySpan<char> path) => throw new PlatformNotSupportedException ();
    }
}
