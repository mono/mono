check-roslyn:
	@$(MAKE) validate-roslyn RESET_VERSIONS=1
	@if [ -z $$PREFIX ]; then echo "You need to set PREFIX to the prefix of the Mono installation that should be used for testing Roslyn." && exit 1; fi
	sed -i -e 's/\\4.5-api"/\\4.5"/g' $$PREFIX/lib/mono/xbuild-frameworks/.NETFramework/v4.5/RedistList/FrameworkList.xml; \
	export MSBuildExtensionsPath=$$PREFIX/lib/mono/xbuild; \
	MONO_DOTNET_PORTABLE_DIR=$$PREFIX/lib/mono/xbuild-frameworks/.NETPortable/; \
	if [ ! -d "$$MONO_DOTNET_PORTABLE_DIR/v4.6" ]; then \
		mkdir -p $$MONO_DOTNET_PORTABLE_DIR; \
		curl -SL "http://download.mono-project.com/third-party/RoslynBuildDependencies.zip" > /tmp/RoslynBuildDependencies.zip; \
		unzip -o /tmp/RoslynBuildDependencies.zip -d /tmp/RoslynBuildDependencies; \
		cp -r /tmp/RoslynBuildDependencies/PortableReferenceAssemblies/* $$MONO_DOTNET_PORTABLE_DIR; \
	fi; \
	cd $(ROSLYN_PATH); \
	sed -i -e 'N; s/bootstrapArg=".*\n.*"/bootstrapArg=""/g' cibuild.sh; \
	sed -i -e 's#-xml Binaries/\$$BUILD_CONFIGURATION/xUnitResults/#-nunit $(abs_top_builddir)/acceptance-tests/TestResult-#g' cibuild.sh; \
	./cibuild.sh --mono-path $$PREFIX/bin || EXIT_CODE=1; \
	sed -i -e 's/\\4.5"/\\4.5-api"/g' $$PREFIX/lib/mono/xbuild-frameworks/.NETFramework/v4.5/RedistList/FrameworkList.xml; \
	exit $$EXIT_CODE
