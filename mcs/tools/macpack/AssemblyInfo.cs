using System;
using System.Reflection;
using Mono.GetOptions;

// Attributes visible in " --help"
[assembly: AssemblyTitle ("macpack.exe")]
[assembly: AssemblyVersion ("1.0.*")]
[assembly: AssemblyDescription ("MacPack")]
[assembly: AssemblyCopyright ("MIT/X11")]

// This is text that goes after " [options]" in help output.
[assembly: Mono.UsageComplement ("")]

// Attributes visible in " -V"
[assembly: Mono.About("MacPack will take a managed assembly and prepare it in a Mac OSX compliant bundle to be run with mono")]
[assembly: Mono.Author ("kangaroo")]
