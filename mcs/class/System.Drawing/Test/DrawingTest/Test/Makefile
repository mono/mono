MCS=mcs
NUNIT_FLAGS=/exclude=NotWorking

sources=AssemblyInfo.cs Bitmap.cs Brush.cs Graphics.cs Image.cs Pen.cs Region.cs

DRAWING_TEST_HELPER=DrawingTestHelper.dll
EXOCORTEX=Exocortex.DSP.dll

Test.dll: $(sources) $(DRAWING_TEST_HELPER)
	$(MCS) -t:library $(sources) -r:$(DRAWING_TEST_HELPER) -r:$(EXOCORTEX) -r:nunit.framework -r:System.Drawing -out:Test.dll

$(DRAWING_TEST_HELPER):
	cd ../DrawingTestHelper && make
	cp ../DrawingTestHelper/*.dll .

run-test: Test.dll
	MONO_PATH="../../../../../class/lib/net_1_1::$(MONO_PATH)" /home/andrews/monobuild/mono/runtime/mono-wrapper --debug ../../../../../class/lib/net_1_1/nunit-console.exe Test.dll $(NUNIT_FLAGS)
