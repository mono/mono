mkdir nugets

##
## Following are nugets which have net4* implementation but that implementation is Windows specific and won’t work on Mono or
## with any profile derived from Mono net_4_x profile like Xamarin.Mac. This is due to no TFM for Mono or Xamarin.Mac which
## would allow us to customize the behaviors.
##
## We don’t want to fix all broken nugets we only focus on few system-like that are likely to be used by broad audience and
## we have working implementation available in one of Mono assemblies.
##
## PLEASE keep this in sync with mcs/tools/xbuild/data/deniedAssembliesList.txt
##
## If any nugets are added or removed here, then make sure to regenerate the above file with:
##
##   $ mono nuget-hash-extractor.exe nugets guids_for_msbuild > ../../mcs/tools/xbuild/data/deniedAssembliesList.txt
##

#System.Runtime.InteropServices.RuntimeInformation
curl -L https://www.nuget.org/api/v2/package/System.Runtime.InteropServices.RuntimeInformation/4.3.0 -o nugets/system.runtime.interopservices.runtimeinformation.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Runtime.InteropServices.RuntimeInformation/4.0.0 -o nugets/system.runtime.interopservices.runtimeinformation.4.0.0.nupkg

#System.Globalization.Extensions
curl -L https://www.nuget.org/api/v2/package/System.Globalization.Extensions/4.3.0 -o nugets/system.globalization.extensions.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Globalization.Extensions/4.0.1 -o nugets/system.globalization.extensions.4.0.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Globalization.Extensions/4.0.0 -o nugets/system.globalization.extensions.4.0.0.nupkg

#System.IO.Compression
curl -L https://www.nuget.org/api/v2/package/System.IO.Compression/4.3.0 -o nugets/system.io.compression.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.IO.Compression/4.1.0 -o nugets/system.io.compression.4.1.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.IO.Compression/4.0.0 -o nugets/system.io.compression.4.0.0.nupkg

#System.Net.Http
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.3 -o nugets/system.net.http.4.3.3.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.2 -o nugets/system.net.http.4.3.2.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.1 -o nugets/system.net.http.4.3.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.0 -o nugets/system.net.http.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.1.1 -o nugets/system.net.http.4.1.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.1.0 -o nugets/system.net.http.4.1.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.0.0 -o nugets/system.net.http.4.0.0.nupkg

#System.Text.Encoding.CodePages
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.4.0 -o nugets/system.text.encoding.codepages.4.4.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.3.0 -o nugets/system.text.encoding.codepages.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.0.1 -o nugets/system.text.encoding.codepages.4.0.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.0.0 -o nugets/system.text.encoding.codepages.4.0.0.nupkg

#System.Threading.Overlapped
curl -L https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.3.0 -o nugets/system.threading.overlapped.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.0.1 -o nugets/system.threading.overlapped.4.0.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.0.0 -o nugets/system.threading.overlapped.4.0.0.nupkg

touch .download_stamp_file
