CPPSHARP_DIR = CppSharp

CPPSHARP_REFS = -r:$(CPPSHARP_DIR)/CppSharp.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.AST.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Parser.CSharp.dll \
	-r:$(CPPSHARP_DIR)/CppSharp.Generator.dll

SRC_ROOT = ../..

MONO_OPTIONS_SRC = $(SRC_ROOT)/mcs/class/Mono.Options/Mono.Options/Options.cs

.stamp-clone:
	@if [ ! -d $(CPPSHARP_DIR) ]; then \
		git clone git://github.com/xamarin/CppSharpBinaries.git $(CPPSHARP_DIR) && touch $@; \
	fi

MonoAotOffsetsDumper.exe: .stamp-clone MonoAotOffsetsDumper.cs $(MONO_OPTIONS_SRC)
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