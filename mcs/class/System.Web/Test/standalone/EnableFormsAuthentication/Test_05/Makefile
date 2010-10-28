TESTNUM=05
thisdir = class/System.Web/Test/standalone/EnableFormsAuthentication/Test_$(TESTNUM)
include ../../../../../../build/rules.make

APPLICATION_ASSEMBLY = bin/EnableFormsAuthentication_Test_$(TESTNUM).dll

APPLICATION_ASSEMBLY_SOURCES = \
	Properties/AssemblyInfo.cs \
	Tests/PreStart.cs

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
	install -d -m 755 bin/
	touch bin/.stamp
endif

clean:
	rm -rf bin/
