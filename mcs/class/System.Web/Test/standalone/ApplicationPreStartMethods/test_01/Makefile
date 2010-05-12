thisdir = class/System.Web/Test/standalone/ApplicationPreStartMethods/test_01
include ../../../../../../build/rules.make

EXTERNAL_ASSEMBLY = ApplicationPreStartMethods/ExternalAssemblies/ExternalAssembly1.dll

EXTERNAL_ASSEMBLY_SOURCES = \
	ExternalAssembly1/ExternalAssemblyPreStartMethods.cs \
	ExternalAssembly1/Properties/AssemblyInfo.cs

EXTERNAL_ASSEMBLY_MCS_FLAGS = \
	-debug:full \
	-r:System.Web.dll


APPLICATION_ASSEMBLY = ApplicationPreStartMethods/bin/ApplicationPreStartMethods_test_01.dll

APPLICATION_ASSEMBLY_SOURCES = \
	ApplicationPreStartMethods/default.aspx.cs \
	ApplicationPreStartMethods/default.aspx.designer.cs \
	ApplicationPreStartMethods/Properties/AssemblyInfo.cs \
	ApplicationPreStartMethods/Tests/PreStartMethods.cs

APPLICATION_ASSEMBLY_MCS_FLAGS = \
	-debug:full \
	-r:System.Web.dll

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))

ifndef VALID_PROFILE
all:
else
all: $(EXTERNAL_ASSEMBLY) $(APPLICATION_ASSEMBLY)

$(EXTERNAL_ASSEMBLY): ApplicationPreStartMethods/ExternalAssemblies/.stamp $(EXTERNAL_ASSEMBLY_SOURCES)
	$(CSCOMPILE) $(EXTERNAL_ASSEMBLY_MCS_FLAGS) $(EXTERNAL_ASSEMBLY_SOURCES) -target:library -out:$(EXTERNAL_ASSEMBLY)

$(APPLICATION_ASSEMBLY): ApplicationPreStartMethods/bin/.stamp $(APPLICATION_ASSEMBLY_SOURCES)
	$(CSCOMPILE) $(APPLICATION_ASSEMBLY_MCS_FLAGS) $(APPLICATION_ASSEMBLY_SOURCES) -target:library -out:$(APPLICATION_ASSEMBLY)

ApplicationPreStartMethods/ExternalAssemblies/.stamp:
	install -d -m 755 ApplicationPreStartMethods/ExternalAssemblies/
	touch ApplicationPreStartMethods/ExternalAssemblies/.stamp

ApplicationPreStartMethods/bin/.stamp:
	install -d -m 755 ApplicationPreStartMethods/bin/
	touch ApplicationPreStartMethods/bin/.stamp
endif

clean:
	rm -rf ApplicationPreStartMethods/ExternalAssemblies/
	rm -rf ApplicationPreStartMethods/bin/
