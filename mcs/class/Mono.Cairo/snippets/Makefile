CSC = mcs -debug
REFS = -r:Mono.Cairo.dll
SNIPPETS = snippets.exe
SNIPPETSGTK = snippets-gtk.exe
SNIPPETSSWF = snippets-swf.exe

all: $(SNIPPETS) $(SNIPPETSGTK) $(SNIPPETSSWF)

clean: 
	rm -f $(SNIPPETS) $(SNIPPETS).mdb
	rm -f $(SNIPPETSGTK) $(SNIPPETSGTK).mdb
	rm -f $(SNIPPETSSWF) $(SNIPPETSSWF).mdb

snippets.exe: Snippets.cs SnippetsPng.cs
	$(CSC) -out:$@ $(REFS) Snippets.cs SnippetsPng.cs 

snippets-gtk.exe: Snippets.cs SnippetsGtk.cs
	$(CSC) -out:$@ $(REFS) -pkg:gtk-sharp-2.0 Snippets.cs SnippetsGtk.cs

snippets-swf.exe: Snippets.cs SnippetsSwf.cs
	$(CSC) -out:$@ $(REFS) -r:System.Windows.Forms.dll -r:System.Drawing.dll Snippets.cs SnippetsSwf.cs

