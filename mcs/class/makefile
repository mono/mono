DIRS=corlib System System.XML
ROOT=//$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	for i in $(DIRS); do				\
		(cd $$i; CSC=$(CSC) NUNIT_CONSOLE=NUnitConsole.exe PLATFORM=window make windows) || exit 1;\
	done;

unix:
	@echo "'make unix' is broken for now."
