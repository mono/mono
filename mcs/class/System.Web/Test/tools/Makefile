thisdir = class/System.Web/Test/tools

include ../../../../build/rules.make

CLASSLIB_DIR = $(topdir)/class/lib/$(PROFILE)
STANDALONE_RUNNER_SUPPORT_ASSEMBLY = $(CLASSLIB_DIR)/standalone-runner-support.dll

STANDALONE_RUNNER_SOURCES = \
	standalone-runner.cs \
	../../../Mono.Options/Mono.Options/Options.cs \

STANDALONE_RUNNER_REFERENCES = \
	-lib:$(CLASSLIB_DIR) \
	-r:$(STANDALONE_RUNNER_SUPPORT_ASSEMBLY) \
	-r:System.Web.dll

all-local: HtmlWriter.dll standalone-runner.exe

HtmlWriter.dll: HtmlWriter.cs
	$(MCS) -t:library -r:System.Web.dll $<

standalone-runner.exe: deps $(STANDALONE_RUNNER_SOURCES)
	$(MCS) -debug:full $(STANDALONE_RUNNER_REFERENCES) -out:$@ $(STANDALONE_RUNNER_SOURCES)

deps:
ifndef STANDALONE_SUPPORT_BUILT
	$(MAKE) -C ../../ standalone-runner-support
endif

clean:
	rm -f HtmlWriter.dll HtmlWriter.*db
	rm -f standalone-runner.exe standalone-runner.*db
