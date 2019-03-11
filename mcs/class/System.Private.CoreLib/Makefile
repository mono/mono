thisdir = class/System.Private.CoreLib
SUBDIRS =
include ../../build/rules.make

all-local:
	dotnet build -p:TargetsUnix=true -p:TargetsOSX=true -p:BuildArch=x64 -p:OutputPath=bin/x64 -p:FeaturePortableTimer=true System.Private.CoreLib.csproj

dist-local:
	@:
