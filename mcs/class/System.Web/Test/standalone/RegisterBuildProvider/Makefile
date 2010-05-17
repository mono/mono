thisdir = class/System.Web/Test/standalone/RegisterBuildProvider
include ../../../../../build/rules.make

APPLICATION_ASSEMBLY = bin/RegisterBuildProvider.dll

APPLICATION_ASSEMBLY_SOURCES = \
	default.aspx.cs \
	default.aspx.designer.cs \
	Properties/AssemblyInfo.cs \
	Test/FooBuildProvider.cs \
	Test/Log.cs

APPLICATION_ASSEMBLY_MCS_FLAGS = \
	-debug:full \
	-r:System.Web.dll

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))

ifndef VALID_PROFILE
all:
else
all: $(APPLICATION_ASSEMBLY)

$(APPLICATION_ASSEMBLY): bin/.stamp $(APPLICATION_ASSEMBLY_SOURCES)
	$(CSCOMPILE) $(APPLICATION_ASSEMBLY_MCS_FLAGS) $(APPLICATION_ASSEMBLY_SOURCES) -target:library -out:$(APPLICATION_ASSEMBLY)

bin/.stamp:
	install -d -m 755 ApplicationPreStartMethods/bin/
	touch ApplicationPreStartMethods/bin/.stamp
endif

clean:
	rm -rf bin/
