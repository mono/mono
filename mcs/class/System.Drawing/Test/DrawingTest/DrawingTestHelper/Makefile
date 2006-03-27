MCS=mcs
sources=AssemblyInfo.cs DrawingTest.cs
EXOCORTEX=Exocortex.DSP.dll

DrawingTestHelper.dll: $(sources) $(EXOCORTEX)
	$(MCS) -t:library $(sources) -r:System.Windows.Forms -r:System.Drawing -r:$(EXOCORTEX) -r:nunit.framework -out:DrawingTestHelper.dll

$(EXOCORTEX):
	cd ../Exocortex.DSP/ && make
	cp ../Exocortex.DSP/*.dll .
