DIRS=corlib System
CSC=//c/winnt/microsoft.net/framework/v1.0.2914/csc.exe

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	for i in $(DIRS); do				\
		(cd $$i; CSC=$(CSC) NUNIT_CONSOLE=NUnitConsole.exe PLATFORM=window make windows)\
	done;

unix:
	@echo "'make unix' is broken for now."
