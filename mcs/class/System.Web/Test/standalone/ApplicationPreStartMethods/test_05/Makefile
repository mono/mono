thisdir = class/System.Web/Test/standalone/ApplicationPreStartMethods/test_05
include ../../../../../../build/rules.make

APPLICATION_ASSEMBLY = ApplicationPreStartMethods/bin/ApplicationPreStartMethods_test_05.dll

APPLICATION_ASSEMBLY_SOURCES = \
	ApplicationPreStartMethods/default.aspx.cs \
	ApplicationPreStartMethods/default.aspx.designer.cs \
	ApplicationPreStartMethods/Properties/AssemblyInfo.cs \
	ApplicationPreStartMethods/Tests/PreStartMethods.cs

APPLICATION_ASSEMBLY_MCS_FLAGS = \
	-debug:full \
	-r:System.Web.dll

VALID_PROFILE := $(filter 4.5, $(FRAMEWORK_VERSION))

ifndef VALID_PROFILE
all:
else
all: $(APPLICATION_ASSEMBLY)

$(APPLICATION_ASSEMBLY): ApplicationPreStartMethods/bin/.stamp $(APPLICATION_ASSEMBLY_SOURCES)
	$(CSCOMPILE) $(APPLICATION_ASSEMBLY_MCS_FLAGS) $(APPLICATION_ASSEMBLY_SOURCES) -target:library -out:$(APPLICATION_ASSEMBLY)

ApplicationPreStartMethods/bin/.stamp:
	install -d -m 755 ApplicationPreStartMethods/bin/
	touch ApplicationPreStartMethods/bin/.stamp
endif

clean:
	rm -rf ApplicationPreStartMethods/bin/
