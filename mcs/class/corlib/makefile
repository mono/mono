DIRS=System System.Collections System.Globalization System.Net System.Text

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /unsafe /target:library /out:corlib.dll /nowarn:1595 @list

unix:
	@echo "'make unix' is broken for now."

make-list:
	for i in $(DIRS); do						\
		cat $$i/common.src $$i/windows.src | sed "s/^/$$i\\\\/";	\
	done > list
