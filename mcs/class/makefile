SUBDIRS=corlib System
csc=/c/winnt/microsoft.net/framework/v1.0.2914/csc.exe

all: windows

windows:
	for subdir in $(SUBDIRS); do				\
		cd $$subdir; csc=$(csc) make windows; cd ..;	\
	done;
