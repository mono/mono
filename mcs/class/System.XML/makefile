DIRS=System.Xml

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /target:library /out:System.XML.dll /nowarn:1595 @list

unix:
	@echo "'make unix' is broken for now."

make-list:
	for i in $(DIRS); do						\
		cat $$i/common.src $$i/windows.src | sed "s/^/$$i\\\\/";	\
	done > list

clean:
	if test -e list; then \
		rm list;  \
	fi; \
		\
	if test -e System.XML.dll; then \
		rm System.XML.dll;  \
	fi; \