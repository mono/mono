The CoreFxSources folder contains the implementation taken from MS CoreFx
repository at 905a1940bcda0afdca2f14ceb2b0161ebc4d1d02.

While we'd ideally not ship this assembly at all with Mono (it doesn't ship
with .NET Framework, there's only as a NuGet package), we shipped it in
the past and as such people might rely on it so we can't remove it.
