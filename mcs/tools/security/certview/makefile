RUNTIME=mono
CSC = $(RUNTIME) ../../../mcs/mcs.exe
CSCFLAGS=/target:winexe /nologo /debug+ /debug:full /out:$@ /r:Mono.Security.dll /r:System.Xml.dll
WINFLAGS=/res:Mono.Tools.CertView.CertificateViewer.resources /r:System.Windows.Forms.dll /r:System.Drawing.dll /win32icon:mono.ico
LNXFLAGS=/unsafe /r:gdk-sharp.dll /r:glib-sharp.dll /r:glade-sharp.dll /r:gtk-sharp.dll
RESGEN=monoresgen

windows: certview.exe 

linux: gcertview.exe

certview.exe: CertificateViewer.cs CertificateFormatter.cs X509ExtensionsHandler.cs ../AssemblyInfo.cs
	cp ../AssemblyInfo.cs AssemblyInfo.cs
	cp ../../../class/lib/Mono.Security.dll Mono.Security.dll
	$(RESGEN) CertificateViewer.resx Mono.Tools.CertView.CertificateViewer.resources
	$(CSC) $(CSCFLAGS) $(WINFLAGS) CertificateViewer.cs CertificateFormatter.cs X509ExtensionsHandler.cs AssemblyInfo.cs
	rm AssemblyInfo.cs 

gcertview.exe: gcertview.cs CertificateFormatter.cs X509ExtensionsHandler.cs ../AssemblyInfo.cs
	cp ../AssemblyInfo.cs AssemblyInfo.cs
	cp ../../../class/lib/Mono.Security.dll Mono.Security.dll
	$(CSC) $(CSCFLAGS) $(LNXFLAGS) gcertview.cs CertificateFormatter.cs X509ExtensionsHandler.cs AssemblyInfo.cs
	cp certview.exe.config gcertview.exe.config
	rm AssemblyInfo.cs 

clean:
	rm -f *.exe *.pdb *.dbg *.dll *.resources

dummy:

