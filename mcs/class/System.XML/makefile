all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /target:library /out:System.XML.dll /nowarn:1595 @list

unix:
	@echo "'make unix' is broken for now."

make-list:
	cat $$i/common.src $$i/windows.src | sed "s/^/$$i\\\\/";

test: $(PLATFORM)