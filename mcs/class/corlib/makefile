DIRS=	\
	System 						\
	System.Collections 				\
	System.Configuration.Assemblies 		\
	System.Diagnostics				\
	System.Globalization 				\
	System.IO					\
	System.Reflection 				\
	System.Runtime.CompilerServices			\
	System.Runtime.InteropServices			\
	System.Runtime.Serialization			\
	System.Security.Cryptography			\
	System.Security.Permissions			\
	System.Text					\
	System.Threading				\
	Unix

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /unsafe /target:library /out:corlibx.dll /nowarn:1595 @list

monolib: make-list
	$(CSC) /unsafe /nostdlib /target:library /out:corlib.dll /nowarn:1595 @listmono

monolib2: make-list
	$(CSC) /unsafe /target:library /out:corlib.dll /nowarn:1595 @listmono

unix:
	@echo "'make unix' is broken for now."

make-list:
	for i in $(DIRS); do						\
		cat $$i/common.src $$i/windows.src | sed "s/^/$$i\\\\/";	\
	done > list

