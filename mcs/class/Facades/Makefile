MCS_BUILD_DIR = ../../build

thisdir = class/Facades

monotouch_SUBDIRS = System.Collections.Concurrent System.Collections System.ComponentModel.Annotations System.ComponentModel.EventBasedAsync System.ComponentModel \
	System.Diagnostics.Contracts System.Diagnostics.Debug System.Diagnostics.Tracing System.Diagnostics.Tools System.Dynamic.Runtime System.Globalization System.IO System.Linq.Expressions \
	System.Linq.Parallel System.Linq.Queryable System.Linq System.Net.NetworkInformation System.Net.Primitives System.Net.Requests System.ObjectModel \
	System.Reflection.Extensions System.Reflection.Primitives System.Reflection System.Resources.ResourceManager System.Runtime.Extensions \
	System.Runtime.InteropServices System.Runtime.InteropServices.WindowsRuntime System.Runtime.Numerics System.Runtime.Serialization.Json \
	System.Runtime.Serialization.Primitives System.Runtime.Serialization.Xml System.Runtime System.Security.Principal System.ServiceModel.Http \
	System.ServiceModel.Primitives System.ServiceModel.Security System.Text.Encoding.Extensions System.Text.Encoding System.Text.RegularExpressions System.Threading.Tasks.Parallel \
	System.Threading.Tasks System.Threading.Timer System.Threading System.Xml.ReaderWriter System.Xml.XDocument System.Xml.XmlSerializer \
	System.Runtime.Handles System.ServiceModel.Duplex System.ServiceModel.NetTcp \
	Microsoft.Win32.Primitives Microsoft.Win32.Registry System.AppContext System.Collections.NonGeneric System.Collections.Specialized System.ComponentModel.Primitives \
	System.ComponentModel.TypeConverter System.Console System.Data.Common System.Data.SqlClient System.Diagnostics.FileVersionInfo \
	System.Diagnostics.Process System.Diagnostics.TextWriterTraceListener System.Diagnostics.TraceEvent System.Diagnostics.TraceSource System.Globalization.Calendars \
	System.IO.Compression System.IO.Compression.ZipFile System.IO.FileSystem System.IO.FileSystem.DriveInfo System.IO.FileSystem.Primitives \
	System.IO.IsolatedStorage System.IO.MemoryMappedFiles System.IO.UnmanagedMemoryStream System.Net.AuthenticationManager System.Net.Cache \
	System.Net.Http.WebRequestHandler System.Net.HttpListener System.Net.Mail System.Net.NameResolution System.Net.Security System.Net.ServicePoint System.Net.Sockets \
	System.Net.Utilities System.Net.WebHeaderCollection System.Net.WebSockets System.Net.WebSockets.Client System.Resources.ReaderWriter System.Runtime.CompilerServices.VisualC \
	System.Security.AccessControl System.Security.Claims System.Security.Cryptography.DeriveBytes System.Security.Cryptography.Encoding System.Security.Cryptography.Encryption \
	System.Security.Cryptography.Encryption.Aes System.Security.Cryptography.Encryption.ECDiffieHellman System.Security.Cryptography.Encryption.ECDsa System.Security.Cryptography.Hashing \
	System.Security.Cryptography.Hashing.Algorithms System.Security.Cryptography.RSA System.Security.Cryptography.RandomNumberGenerator \
	System.Security.Cryptography.X509Certificates System.Security.Principal.Windows System.Threading.Thread System.Threading.ThreadPool \
	System.Xml.XPath System.Xml.XmlDocument System.Xml.Xsl.Primitives Microsoft.Win32.Registry.AccessControl System.Diagnostics.StackTrace System.Globalization.Extensions \
	System.IO.FileSystem.AccessControl System.Private.CoreLib.InteropServices System.Private.CoreLib.Threading System.Reflection.TypeExtensions \
	System.Security.SecureString System.Threading.AccessControl System.Threading.Overlapped System.Xml.XPath.XDocument

reflection_SUBDIRS = System.Reflection.Emit.ILGeneration System.Reflection.Emit.Lightweight System.Reflection.Emit

mobile_static_SUBDIRS = $(monotouch_SUBDIRS)

net_4_5_SUBDIRS = $(monotouch_SUBDIRS) $(reflection_SUBDIRS) System.Diagnostics.PerformanceCounter \
	System.IO.FileSystem.Watcher System.IO.Pipes System.Security.Cryptography.ProtectedData System.ServiceProcess.ServiceController 

monodroid_SUBDIRS = $(monotouch_SUBDIRS) $(reflection_SUBDIRS)

xammac_SUBDIRS = $(net_4_5_SUBDIRS)
xammac_net_4_5_SUBDIRS = $(net_4_5_SUBDIRS)

monotouch_watch_SUBDIRS = $(monotouch_SUBDIRS)

PROFILE_SUBDIRS = $(net_4_5_SUBDIRS)

#OVERRIDE_TARGET_ALL = yes

# 
# The output directory for the facade assemblies
#
target_dir = $(topdir)/class/lib/$(PROFILE)/Facades

#
# Contains the expanded version of the target dll for each profile
#
target_libraries = $(patsubst %,$(target_dir)/%.dll,$(SUBDIRS))

#
# To build each target, go into the subdirectory
#
$(target_dir)/%.dll: 
	(cd `basename $@ .dll`; $(MAKE))

#all.override: $(target_libraries)

include $(MCS_BUILD_DIR)/rules.make

dist-local: dist-default

SUBDIRS = $(net_4_5_SUBDIRS)

doc-update-local:
	@echo "not doing docs"

doc-update-recursive:
	@echo "do not recurse the Facades folder"
