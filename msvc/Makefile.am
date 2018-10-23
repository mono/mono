
VCXPROJS = $(wildcard *.vcxproj)

EXTRA_DIST = \
	     $(VCXPROJS) \
	     README \
	     mono.props \
	     mono.sln \
	     monoposixhelper.def \
	     runmdesc.bat \
	     winsetup.bat

if WITH_MSVC
all-local:
	@echo "Building Mono runtime using Visual Studio."
	$(top_srcdir)/msvc/run-msbuild.sh "build" "$(host_cpu)"

clean-local:
	@echo "Clean Visual Studio build Mono runtime."
	$(top_srcdir)/msvc/run-msbuild.sh "clean" "$(host_cpu)"

install-exec-local:
	@echo "Install Visual Studio build Mono runtime."
endif
