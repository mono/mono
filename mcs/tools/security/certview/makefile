RUNTIME=mono
CSC = $(RUNTIME) ../../../mcs/mcs.exe
CSCFLAGS=/win32icon:mono.ico /target:winexe /nologo /debug+ /debug:full /out:$@ /r:Mono.Security.dll /r:System.XML.dll
WINFLAGS=/res:Mono.Tools.CertView.CertificateViewer.resources /r:System.Windows.Forms.dll /r:System.Drawing.dll
LNXFLAGS=/unsafe /r:gdk-sharp.dll /r:glib-sharp.dll /r:glade-sharp.dll /r:gtk-sharp.dll

windows: certview.exe 

linux: gcertview.exe

certview.exe: CertificateViewer.cs CertificateFormatter.cs X509ExtensionsHandler.cs ..\AssemblyInfo.cs
	cp ../AssemblyInfo.cs AssemblyInfo.cs
	cp ../../../class/lib/Mono.Security.dll Mono.Security.dll
	resgen CertificateViewer.resx Mono.Tools.CertView.CertificateViewer.resources
	$(CSC) $(CSCFLAGS) $(WINFLAGS) CertificateViewer.cs CertificateFormatter.cs X509ExtensionsHandler.cs AssemblyInfo.cs

gcertview.exe: gcertview.cs CertificateFormatter.cs X509ExtensionsHandler.cs ..\AssemblyInfo.cs
	cp ../AssemblyInfo.cs AssemblyInfo.cs
	cp ../../../class/lib/Mono.Security.dll Mono.Security.dll
	$(CSC) $(CSCFLAGS) $(LNXFLAGS) gcertview.cs CertificateFormatter.cs X509ExtensionsHandler.cs AssemblyInfo.cs
	cp certview.exe.config gcertview.exe.config

clean:
	rm -f *.exe *.pdb *.dbg *.dll *.resources
	rm AssemblyInfo.cs gcertview.exe.config

dummy:

