mono_mozilladir=$(pkglibdir)
mono_mozilla_SCRIPTS = Mono.Mozilla.dll

EXTRA_DIST = $(mono_mozilla_sources) $(mono_mozilla_sources_in)
CLEANFILES = Mono.Mozilla.dll
DISTCLEANFILES = AssemblyInfo.cs Makefile.in

mono_mozilla_sources_in = AssemblyInfo.cs.in
mono_mozilla_generated_sources = $(mono_mozilla_sources_in:.in=)
mono_mozilla_sources = \
	Base.cs \
	IWebBrowser.cs \
	WebBrowser.cs \
	ICallback.cs	\
	Callback.cs \
	Manager.cs
	
	

mono_mozilla_build_sources = $(addprefix $(srcdir)/, $(mono_mozilla_sources))
mono_mozilla_build_sources += $(mono_mozilla_generated_sources)

Mono.Mozilla.dll: $(mono_mozilla_build_sources)
	gmcs -debug -target:library -out:$@ $(mono_mozilla_build_sources)
