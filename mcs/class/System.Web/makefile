DIRS=	\
	System.Web					\
	System.Web.Caching				\
	System.Web.Configuration

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /target:library /out:System.Web.dll /nowarn:1595 @list

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
	if test -e System.Web.dll; then \
		rm System.Web.dll;  \
	fi; \