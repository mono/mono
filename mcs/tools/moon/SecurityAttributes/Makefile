securitymodel_sources = CecilRocks.cs PlatformCode.cs

CECIL=-r:../../../class/lib/net_1_1/Mono.Cecil.dll
LIBS=$(CECIL) -r:Moonlight.SecurityModel.dll
MONO_PLATFORM_CODE=../../../class/lib/net_2_1/
MOONLIGHT_PLATFORM_CODE=../../../../moon/class/lib/2.1/
SILVERLIGHT_PLATFORM_CODE="/cygdrive/c/Program\ Files/Microsoft\ Silverlight/2.0.31005.0/"

all: merge

Moonlight.SecurityModel.dll: $(securitymodel_sources)
	gmcs $(CECIL) -r:System.Core.dll -t:library -out:$@ $(securitymodel_sources)

find-sc.exe: find-sc.cs Moonlight.SecurityModel.dll
	gmcs find-sc.cs -out:$@ $(LIBS)

compat: find-sc.exe
	mkdir -p compatibility
	mono find-sc.exe $(SILVERLIGHT_PLATFORM_CODE) compatibility

detect-sc.exe: detect-sc.cs Moonlight.SecurityModel.dll
	gmcs detect-sc.cs -out:$@ $(LIBS)

detect-ssc.exe: detect-ssc.cs Moonlight.SecurityModel.dll
	gmcs detect-ssc.cs -out:$@ $(LIBS)

detect: detect-sc.exe detect-ssc.exe
	mkdir -p automatic
	mono detect-sc.exe $(MONO_PLATFORM_CODE) $(MOONLIGHT_PLATFORM_CODE) automatic
	mono detect-ssc.exe $(MONO_PLATFORM_CODE) $(MOONLIGHT_PLATFORM_CODE) automatic

merge.exe: merge.cs Moonlight.SecurityModel.dll
	gmcs merge.cs -out:$@ $(LIBS)

merge: merge.exe
	mono merge.exe . .

validate: find-sc.exe
	mkdir -p moonlight
	mono find-sc.exe $(MOONLIGHT_PLATFORM_CODE) moonlight
	diff compatibility/*.compat.sc moonlight/*.compat.sc

clean-data:
	rm -f *.secattr automatic/*.auto.sc automatic/*.auto.ssc

clean: clean-data
	rm -f find-sc.exe detect-sc.exe detect-ssc.exe merge.exe Moonlight.SecurityModel.dll
