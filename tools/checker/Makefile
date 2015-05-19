CPPSHARP_DIR = CppSharp

CPPSHARP_REFS = -r:$(CPPSHARP_DIR)/CppSharp.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.AST.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Parser.CSharp.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Generator.dll

NEWTONSOFT_DIR = Newtonsoft.Json.6.0.8/lib/net45
DEPS_REFS = -r:$(NEWTONSOFT_DIR)/Newtonsoft.Json.dll

SRC_ROOT = ../..

.stamp-clone:
	@if [ ! -d $(CPPSHARP_DIR) ]; then \
		git clone git@github.com:tritao/CppSharpBinaries.git $(CPPSHARP_DIR); \
		touch $@; \
	fi

.stamp-deps:
	nuget install Newtonsoft.Json
	touch $@;

MonoChecker.exe: .stamp-clone .stamp-deps MonoChecker.cs 
	mcs MonoChecker.cs $(CPPSHARP_REFS) $(DEPS_REFS)
	cp $(NEWTONSOFT_DIR)/*.dll .

check: MonoChecker.exe 
	MONO_PATH=$(CPPSHARP_DIR) mono MonoChecker.exe

gen-proj:
	$(CPPSHARP_DIR)/premake5 vs2012

clean:
	rm MonoChecker.exe

all: MonoChecker.exe