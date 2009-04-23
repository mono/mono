all: pdb2mdb.exe

pdb2mdb.exe:
	gmcs -r:`pkg-config --variable=Libraries cecil` -r:Mono.CompilerServices.SymbolWriter.dll -out:pdb2mdb.exe *.cs

clean:
	rm -f pdb2mdb.exe
