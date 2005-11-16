CSC = mcs -debug
REFS = -r:Mono.Cairo.dll
SNIPPETS = snippets.exe

all: $(SNIPPETS)

clean: 
	rm -f $(SNIPPETS) $(SNIPPETS).mdb

snippets.exe: Snippets.cs SnippetsPng.cs
	$(CSC) -out:$@ $(REFS) Snippets.cs SnippetsPng.cs 

