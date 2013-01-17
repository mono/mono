// Compiler options: -target:module

using System.Reflection;

[assembly:AssemblyKeyFile("test-715.snk")]
// Have to install the container first but Mono's sn is broken
// [assembly:AssemblyKeyName("foo")]
[assembly:AssemblyDelaySign(false)]
