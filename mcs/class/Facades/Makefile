MCS_BUILD_DIR = ../../build

thisdir = class/Facades

monotouch_SUBDIRS = System.Collections.Concurrent System.Collections System.ComponentModel.Annotations System.ComponentModel.EventBasedAsync System.ComponentModel \
	System.Diagnostics.Contracts System.Diagnostics.Debug System.Diagnostics.Tracing System.Diagnostics.Tools System.Dynamic.Runtime System.Globalization System.IO System.Linq.Expressions \
	System.Linq.Parallel System.Linq.Queryable System.Linq System.Net.NetworkInformation System.Net.Primitives System.Net.Requests System.ObjectModel \
	System.Reflection.Extensions System.Reflection.Primitives System.Reflection System.Resources.ResourceManager System.Runtime.Extensions \
	System.Runtime.InteropServices System.Runtime.InteropServices.WindowsRuntime System.Runtime.Numerics System.Runtime.Serialization.Json \
	System.Runtime.Serialization.Primitives System.Runtime.Serialization.Xml System.Runtime System.Security.Principal System.ServiceModel.Http \
	System.ServiceModel.Primitives System.Text.Encoding.Extensions System.Text.Encoding System.Text.RegularExpressions System.Threading.Tasks.Parallel \
	System.Threading.Tasks System.Threading.Timer System.Threading System.Xml.ReaderWriter System.Xml.XDocument System.Xml.XmlSerializer

mobile_static_SUBDIRS = $(monotouch_SUBDIRS)

net_4_5_SUBDIRS = $(monotouch_SUBDIRS) System.Reflection.Emit.ILGeneration System.Reflection.Emit.Lightweight System.Reflection.Emit

monodroid_SUBDIRS = $(net_4_5_SUBDIRS)

xammac_SUBDIRS = $(net_4_5_SUBDIRS)

SUBDIRS = $(net_4_5_SUBDIRS)

include $(MCS_BUILD_DIR)/rules.make

dist-local: dist-default


doc-update-local:
	@echo "not doing docs"

doc-update-recursive:
	@echo "do not recurse the Facades folder"