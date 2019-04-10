thisdir = class/System.Private.CoreLib
SUBDIRS =
#include ../../build/rules.make

all-local:
	dotnet build @CORETARGETS@ -p:BuildArch=@COREARCH@ -p:OutputPath=bin/@COREARCH@ -p:FeaturePortableTimer=true System.Private.CoreLib.csproj

dist-recursive: dist-local

dist-local:
	@:
