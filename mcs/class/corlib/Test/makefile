NUNIT_HOME=C:\Program Files\NUnit
all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: make-list
	$(CSC) /debug+ /lib:"$(NUNIT_HOME)\bin" /r:NUnitCore.dll /target:library /out:Test.dll /nowarn:1595 @list;

unix:
	@echo "'make unix' is broken for now."

make-list:
	cat common.src windows.src > list

clean:
	if test -e list; then \
		rm list;  \
	fi; \
		\
	if test -e Test.dll; then \
		rm Test.dll;  \
	fi; \
		\
	if test -e Test.pdb; then \
		rm Test.pdb;  \
	fi; \
	
runtests: windows
	"$(NUNIT_HOME)\bin\NUnitConsole.exe" Ximian.Mono.Tests.AllTests,Test.dll