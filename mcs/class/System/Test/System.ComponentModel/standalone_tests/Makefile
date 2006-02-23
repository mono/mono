.SUFFIXES: .cs .exe

RUNTIME = mono
CSCOMPILE = mcs

MONO="mono"

test: TypeDescriptorTest.exe

run-test: test
	$(MONO) TypeDescriptorTest.exe

clean:
	rm -f TypeDescriptorTest.exe

TypeDescriptorTest.exe: TypeDescriptorTest.cs
	$(CSCOMPILE) TypeDescriptorTest.cs -r:System.Web
