CSC = mcs -debug
REFS = -r:Mono.Cairo.dll
SNIPPETS = snippets.exe
SNIPPETSGTK = snippets-gtk.exe

all: $(SNIPPETS)

clean: 
	rm -f $(SNIPPETS) $(SNIPPETS).mdb
	rm -f $(SNIPPETSGTK) $(SNIPPETSGTK).mdb

snippets.exe: Snippets.cs SnippetsPng.cs
	$(CSC) -out:$@ $(REFS) Snippets.cs SnippetsPng.cs 

# only for gtk 2.8
snippets-gtk.exe: Snippets.cs SnippetsGtk.cs
	$(CSC) -out:$@ $(REFS) -pkg:gtk-sharp-2.0 Snippets.cs SnippetsGtk.cs 

