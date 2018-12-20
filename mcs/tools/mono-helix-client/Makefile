thisdir = tools/mono-helix-client
SUBDIRS = 
include ../../build/rules.make

PROGRAM = mono-helix-client.exe
NO_INSTALL = yes

LIB_REFS = System System.Net.Http Facades/netstandard Facades/System.Runtime Facades/System.Threading.Tasks
helix_binaries = $(topdir)/../external/helix-binaries
LOCAL_MCS_FLAGS = -r:$(helix_binaries)/Microsoft.DotNet.Helix.Client.dll -r:$(helix_binaries)/Microsoft.DotNet.Helix.JobSender.dll -r:$(helix_binaries)/Microsoft.Rest.ClientRuntime.dll

with_helix_client = MONO_PATH="$(helix_binaries)$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)/mono-helix-client.exe

upload-to-helix:
	MONO_HELIX_XUNIT_REPORTER_PATH="$(abspath $(helix_binaries)/xunit-reporter.py)" $(with_helix_client) --tests="$(MONO_HELIX_TYPE)" --correlationIdFile="$(MONO_HELIX_CORRELATION_ID_FILE)"

wait-for-job-completion:
	$(with_helix_client) --wait="$(MONO_HELIX_CORRELATION_ID)"

include ../../build/executable.make
