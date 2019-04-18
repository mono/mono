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
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.4 -o nugets/system.net.http.4.3.4.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.3 -o nugets/system.net.http.4.3.3.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.2 -o nugets/system.net.http.4.3.2.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.1 -o nugets/system.net.http.4.3.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.3.0 -o nugets/system.net.http.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.1.1 -o nugets/system.net.http.4.1.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.1.0 -o nugets/system.net.http.4.1.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Net.Http/4.0.0 -o nugets/system.net.http.4.0.0.nupkg

#System.Text.Encoding.CodePages
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.5.1 -o nugets/system.text.encoding.codepages.4.5.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.5.0 -o nugets/system.text.encoding.codepages.4.5.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.4.0 -o nugets/system.text.encoding.codepages.4.4.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.3.0 -o nugets/system.text.encoding.codepages.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.0.1 -o nugets/system.text.encoding.codepages.4.0.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.0.0 -o nugets/system.text.encoding.codepages.4.0.0.nupkg

#System.Threading.Overlapped
curl -L https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.3.0 -o nugets/system.threading.overlapped.4.3.0.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.0.1 -o nugets/system.threading.overlapped.4.0.1.nupkg
curl -L https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.0.0 -o nugets/system.threading.overlapped.4.0.0.nupkg

# Assemblies from Microsoft.NET.Build.Extensions are bundled with msbuild and they contain net4x assemblies, some of which
# are incompatible with mono

MS_EXTN_VERSIONS="2.0.0-preview3-20170622-1" # https://github.com/dotnet/cli/blob/7c928a9f18f81001e586e5bf0411f9bfa92e30d4/build/DependencyVersions.props
MS_EXTN_VERSIONS="$MS_EXTN_VERSIONS 15.5.0-preview-20171027-2" # https://github.com/dotnet/cli/blob/ed916bb13e798a470855fb4f60acd3cabb2765fc/build/DependencyVersions.props
MS_EXTN_VERSIONS="$MS_EXTN_VERSIONS 2.1.0-preview1-62414-02" # https://github.com/dotnet/cli/blob/501e11d928c21608999c934f0a7078570b688c6c/build/DependencyVersions.props
MS_EXTN_VERSIONS="$MS_EXTN_VERSIONS 2.1.100-preview-62617-01" # https://github.com/dotnet/cli/blob/b9e74c65201ef39c74b6d75cedbb605a88cd26ec/build/DependencyVersions.props

for ver in $MS_EXTN_VERSIONS; do
	curl -L https://dotnet.myget.org/F/dotnet-core/api/v2/package/Microsoft.NET.Build.Extensions/${ver} -o nugets/microsoft.net.build.extensions.${ver}.nupkg
done

MS_EXTN_VERSIONS_DOTNETFEED="2.1.300-preview3-62804-06" 			    # https://github.com/dotnet/cli/blob/8c937a0db08e56660aca456ac088f2d0e70735ab/build/DependencyVersions.props
MS_EXTN_VERSIONS_DOTNETFEED="$MS_EXTN_VERSIONS_DOTNETFEED 2.1.600-preview-63821-02" # https://github.com/dotnet/cli/blob/4b9d6502f8061db4a56b730d3d4e65262c72ad5c/build/DependencyVersions.props

for ver in $MS_EXTN_VERSIONS_DOTNETFEED; do
	curl https://dotnetfeed.blob.core.windows.net/dotnet-core/flatcontainer/microsoft.net.build.extensions/${ver}/microsoft.net.build.extensions.${ver}.nupkg -o nugets//microsoft.net.build.extensions.${ver}.nupkg
done

touch .download_stamp_file
