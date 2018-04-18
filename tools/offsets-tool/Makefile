CPPSHARP_BASE_DIR = CppSharp

ifeq ($(OS),Windows_NT)
	CPPSHARP_OS=windows
	ifeq ($(PROCESSOR_ARCHITEW6432),AMD64)
		CPPSHARP_ARCH=64
	else
		ifeq ($(PROCESSOR_ARCHITECTURE),AMD64)
			CPPSHARP_ARCH=64
		endif
		ifeq ($(PROCESSOR_ARCHITECTURE),x86)
			CPPSHARP_ARCH=32
		endif
	endif
else
	UNAME_S := $(shell uname -s)
	ifeq ($(UNAME_S),Linux)
		CPPSHARP_OS=linux
		CPPSHARP_ARCH=64
	endif
	ifeq ($(UNAME_S),Darwin)
		CPPSHARP_OS=osx
		CPPSHARP_ARCH=64
	endif
endif

CPPSHARP_DIR = $(CPPSHARP_BASE_DIR)/$(CPPSHARP_OS)_$(CPPSHARP_ARCH)

CPPSHARP_REFS = -r:$(CPPSHARP_DIR)/CppSharp.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.AST.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Parser.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Parser.CSharp.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Generator.dll

SRC_ROOT = ../..

MONO_OPTIONS_SRC = $(SRC_ROOT)/mcs/class/Mono.Options/Mono.Options/Options.cs

HASH=9dc880dd
.stamp-clone-$(HASH):
	rm -Rf $(CPPSHARP_BASE_DIR)
	git clone -b $(HASH) --depth 1 git://github.com/xamarin/CppSharpBinaries.git $(CPPSHARP_BASE_DIR) && touch $@

MonoAotOffsetsDumper.exe: .stamp-clone-$(HASH) MonoAotOffsetsDumper.cs $(MONO_OPTIONS_SRC)
	mcs MonoAotOffsetsDumper.cs /debug /nowarn:0436 $(MONO_OPTIONS_SRC) $(CPPSHARP_REFS)

.PHONY: clean
clean:
	rm MonoAotOffsetsDumper.exe

dump: MonoAotOffsetsDumper.exe 
	MONO_PATH=$(CPPSHARP_DIR) mono MonoAotOffsetsDumper.exe

update:
	@if [ -f object-offsets.h ]; then rm object-offsets.h; fi;
	@for f in *.h; do \
		echo "Processing $$f.."; \
		echo "#include \"$$f\"" >> object-offsets1.h; \
	done
	@cp *.h ../mono/metadata

gen-proj:
	$(CPPSHARP_DIR)/premake5 vs2012

all: MonoAotOffsetsDumper.exe
