NANT	= ../nant/NAnt.exe

all: build

linux: build

windows: build

build: framework/Transform.resources
	$(NANT) build

framework/Transform.resources: 
	resgen framework/Transform.resX

clean:
	$(NANT) clean

