all: Program1.exe

run-test: Program1.exe
	mono Program1.exe

dist-clean: clean
	rm -rf bin My.master PageWithMaster.aspx

clean:
	rm -f Program1.exe Class1.dll

Program1.exe: Program1.cs Class1.dll
	gmcs -out:$@ -r:System.Web -r:Class1 Program1.cs

Class1.dll: Class1.cs
	gmcs -out:$@ -r:System.Web Class1.cs -target:library
