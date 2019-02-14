
# This file should only contain SUBDIRS variables
# Caution while renaming SUBDIRS variables as those are used outside mono repository.
# ie: macccore/builds/Makefile

common_SUBDIRS = System.Collections.Concurrent System.Collections System.ComponentModel.Annotations System.ComponentModel.EventBasedAsync System.ComponentModel \
System.Diagnostics.Contracts System.Diagnostics.Debug System.Diagnostics.Tracing System.Diagnostics.Tools System.Dynamic.Runtime System.Globalization System.IO System.Linq.Expressions \
System.Linq.Parallel System.Linq.Queryable System.Linq System.Net.NetworkInformation System.Net.Primitives System.Net.Requests System.ObjectModel \
System.Reflection.Extensions System.Reflection.Primitives System.Reflection System.Resources.ResourceManager System.Runtime.Extensions \
System.Runtime.InteropServices System.Runtime.InteropServices.WindowsRuntime System.Runtime.Numerics System.Runtime.Serialization.Json \
System.Runtime System.Security.Principal System.ServiceModel.Http \
System.ServiceModel.Security System.Text.Encoding.Extensions System.Text.Encoding System.Text.RegularExpressions System.Threading.Tasks.Parallel \
System.Threading.Tasks System.Threading.Timer System.Threading System.Xml.ReaderWriter System.Xml.XDocument System.Xml.XmlSerializer \
System.Runtime.Handles System.ServiceModel.Duplex System.ServiceModel.NetTcp \
Microsoft.Win32.Primitives Microsoft.Win32.Registry System.AppContext System.Collections.NonGeneric System.Collections.Specialized System.ComponentModel.Primitives \
System.ComponentModel.TypeConverter System.Console System.Data.SqlClient System.Diagnostics.FileVersionInfo \
System.Diagnostics.Process System.Diagnostics.TextWriterTraceListener System.Diagnostics.TraceEvent System.Diagnostics.TraceSource System.Globalization.Calendars \
System.IO.Compression.ZipFile System.IO.FileSystem System.IO.FileSystem.DriveInfo System.IO.FileSystem.Primitives \
System.IO.IsolatedStorage System.IO.MemoryMappedFiles System.IO.UnmanagedMemoryStream System.Net.AuthenticationManager System.Net.Cache \
System.Net.HttpListener System.Net.Mail System.Net.NameResolution System.Net.Security System.Net.ServicePoint System.Net.Sockets \
System.Net.Utilities System.Net.WebHeaderCollection System.Net.WebSockets System.Net.WebSockets.Client System.Resources.ReaderWriter System.Runtime.CompilerServices.VisualC \
System.Security.AccessControl System.Security.Claims System.Security.Cryptography.DeriveBytes System.Security.Cryptography.Encoding System.Security.Cryptography.Encryption \
System.Security.Cryptography.Encryption.Aes System.Security.Cryptography.Encryption.ECDiffieHellman System.Security.Cryptography.Encryption.ECDsa System.Security.Cryptography.Hashing \
System.Security.Cryptography.Hashing.Algorithms System.Security.Cryptography.RSA System.Security.Cryptography.RandomNumberGenerator \
System.Security.Principal.Windows System.Threading.Thread System.Threading.ThreadPool \
System.Xml.XPath System.Xml.XmlDocument System.Xml.Xsl.Primitives Microsoft.Win32.Registry.AccessControl \
System.IO.FileSystem.AccessControl System.Reflection.TypeExtensions System.Reflection.Emit.Lightweight System.Reflection.Emit.ILGeneration System.Reflection.Emit \
System.Threading.AccessControl System.ValueTuple \
System.Security.Cryptography.Primitives System.Text.Encoding.CodePages System.IO.FileSystem.Watcher \
System.Security.Cryptography.ProtectedData System.ServiceProcess.ServiceController System.IO.Pipes \
System.Net.Ping System.Resources.Reader System.Resources.Writer System.Runtime.Serialization.Formatters System.Security.Cryptography.Csp \
System.Runtime.InteropServices.RuntimeInformation System.Xml.XPath.XDocument System.Threading.Overlapped System.Security.SecureString \
System.Diagnostics.StackTrace System.Data.Common System.Globalization.Extensions System.Runtime.Serialization.Xml System.ServiceModel.Primitives \
System.Security.Cryptography.X509Certificates System.Runtime.Serialization.Primitives System.Security.Cryptography.Algorithms netstandard System.Drawing.Primitives \
System.Drawing.Common

# common_SUBDIRS dependencies
common_DEPS_SUBDIRS :=

monotouch_SUBDIRS = $(common_DEPS_SUBDIRS)
monotouch_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

monodroid_SUBDIRS = $(common_DEPS_SUBDIRS) System.Drawing.Common
monodroid_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

net_4_x_SUBDIRS = $(common_DEPS_SUBDIRS)
net_4_x_PARALLEL_SUBDIRS = $(common_SUBDIRS) System.Net.Http.Rtc

build_PARALLEL_SUBDIRS = System.Runtime System.Reflection System.Collections System.Resources.ResourceManager System.Globalization \
System.Threading.Tasks System.Collections.Concurrent System.Text.Encoding System.IO System.Threading System.Diagnostics.Debug \
System.Linq.Expressions System.Dynamic.Runtime System.Linq System.Threading.Tasks.Parallel System.Xml.ReaderWriter \
System.Diagnostics.Tools System.Reflection.Primitives System.Runtime.Extensions System.Runtime.InteropServices System.Text.Encoding.Extensions \
System.Runtime.Numerics System.Xml.XDocument System.Reflection.Extensions System.IO.FileSystem.Primitives System.IO.FileSystem \
System.Diagnostics.FileVersionInfo System.Security.Cryptography.Primitives System.Security.Cryptography.Algorithms System.ValueTuple \
System.Text.Encoding.CodePages System.Text.RegularExpressions System.Diagnostics.Contracts \
System.Diagnostics.Tracing System.Net.Primitives netstandard

xammac_SUBDIRS = $(monotouch_SUBDIRS)
xammac_PARALLEL_SUBDIRS = $(monotouch_PARALLEL_SUBDIRS)

xammac_net_4_5_SUBDIRS = $(net_4_x_SUBDIRS) System.Drawing.Primitives
xammac_net_4_5_PARALLEL_SUBDIRS = $(net_4_x_PARALLEL_SUBDIRS)

monotouch_watch_SUBDIRS = $(monotouch_SUBDIRS)
monotouch_watch_PARALLEL_SUBDIRS = $(monotouch_PARALLEL_SUBDIRS)

monotouch_tv_SUBDIRS = $(monotouch_SUBDIRS)
monotouch_tv_PARALLEL_SUBDIRS = $(monotouch_PARALLEL_SUBDIRS)

winaot_SUBDIRS = $(common_DEPS_SUBDIRS)
winaot_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

orbis_SUBDIRS = $(common_DEPS_SUBDIRS)
orbis_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

unreal_SUBDIRS = $(common_DEPS_SUBDIRS)
unreal_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

wasm_SUBDIRS = $(common_DEPS_SUBDIRS)
wasm_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

testing_aot_full_SUBDIRS = $(common_DEPS_SUBDIRS)
testing_aot_full_PARALLEL_SUBDIRS = $(common_SUBDIRS) $(mobile_only_SUBDIRS)

mobile_only_SUBDIRS = System.Security.Cryptography.Pkcs System.Security.Cryptography.OpenSsl System.Threading.Tasks.Extensions \
System.Security.Cryptography.Cng System.Runtime.Loader System.Xml.XPath.XmlDocument System.Reflection.DispatchProxy System.Memory \
System.Buffers

PROFILE_PARALLEL_SUBDIRS = $(net_4_x_PARALLEL_SUBDIRS)
