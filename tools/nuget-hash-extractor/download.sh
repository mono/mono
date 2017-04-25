mkdir nugets

#System.Runtime.InteropServices.RuntimeInformation
wget https://www.nuget.org/api/v2/package/System.Runtime.InteropServices.RuntimeInformation/4.3.0 -O nugets/system.runtime.interopservices.runtimeinformation.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Runtime.InteropServices.RuntimeInformation/4.0.0 -O nugets/system.runtime.interopservices.runtimeinformation.4.0.0.nupkg

#System.Globalization.Extensions
wget https://www.nuget.org/api/v2/package/System.Globalization.Extensions/4.3.0 -O nugets/system.globalization.extensions.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Globalization.Extensions/4.0.1 -O nugets/system.globalization.extensions.4.0.1.nupkg
wget https://www.nuget.org/api/v2/package/System.Globalization.Extensions/4.0.0 -O nugets/system.globalization.extensions.4.0.0.nupkg

#System.IO.Compression
wget https://www.nuget.org/api/v2/package/System.IO.Compression/4.3.0 -O nugets/system.io.compression.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.IO.Compression/4.1.0 -O nugets/system.io.compression.4.1.0.nupkg
wget https://www.nuget.org/api/v2/package/System.IO.Compression/4.0.0 -O nugets/system.io.compression.4.0.0.nupkg

#System.Net.Http
wget https://www.nuget.org/api/v2/package/System.Net.Http/4.3.1 -O nugets/system.net.http.4.3.1.nupkg
wget https://www.nuget.org/api/v2/package/System.Net.Http/4.3.0 -O nugets/system.net.http.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Net.Http/4.1.1 -O nugets/system.net.http.4.1.1.nupkg
wget https://www.nuget.org/api/v2/package/System.Net.Http/4.1.0 -O nugets/system.net.http.4.1.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Net.Http/4.0.0 -O nugets/system.net.http.4.0.0.nupkg

#System.Text.Encoding.CodePages
wget https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.3.0 -O nugets/system.text.encoding.codepages.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.0.1 -O nugets/system.text.encoding.codepages.4.0.1.nupkg
wget https://www.nuget.org/api/v2/package/System.Text.Encoding.CodePages/4.0.0 -O nugets/system.text.encoding.codepages.4.0.0.nupkg

#System.Reflection.DispatchProxy
wget https://www.nuget.org/api/v2/package/System.Reflection.DispatchProxy/4.3.0 -O nugets/system.reflection.dispatchproxy.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Reflection.DispatchProxy/4.0.1 -O nugets/system.reflection.dispatchproxy.4.0.1.nupkg
wget https://www.nuget.org/api/v2/package/System.Reflection.DispatchProxy/4.0.0 -O nugets/system.reflection.dispatchproxy.4.0.0.nupkg

#System.ValueTuple
wget https://www.nuget.org/api/v2/package/System.ValueTuple/4.3.0 -O nugets/system.valuetuple.4.3.0.nupkg

#System.Threading.Overlapped
wget https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.3.0 -O nugets/system.threading.overlapped.4.3.0.nupkg
wget https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.0.1 -O nugets/system.threading.overlapped.4.0.1.nupkg
wget https://www.nuget.org/api/v2/package/System.Threading.Overlapped/4.0.0 -O nugets/system.threading.overlapped.4.0.0.nupkg

#System.Security.Cryptography.OpenSsl when .net 4.6.2 + 1 is out

touch .download_stamp_file
