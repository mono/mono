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
	System.Security.Permissions			\
	System.Text					\
	System.Threading

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /unsafe /target:library /out:corlibx.dll /nowarn:1595 @list
#	$(CSC) /unsafe /nostdlib /target:library /out:corlib.dll /nowarn:1595 System\\Object.cs System\\ValueType.cs @list

unix:
	@echo "'make unix' is broken for now."

make-list:
	for i in $(DIRS); do						\
		cat $$i/common.src $$i/windows.src | sed "s/^/$$i\\\\/";	\
	done > list
