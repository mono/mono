check-ms-test-suite:
	@if $(MAKE) validate-ms-test-suite RESET_VERSIONS=1; then \
		$(MAKE) -C $(MSTESTSUITE_PATH)/conformance build MCS="$(MCS) -debug -t:library -warn:1 -r:nunit.framework" && \
		$(MAKE) -C $(MSTESTSUITE_PATH)/conformance run NUNIT-CONSOLE="$(RUNTIME) $(CLASS)/nunit-console.exe -nologo -exclude=MonoBug,BadTest" NUNIT_XML_RESULT=$(abs_top_builddir)/acceptance-tests/TestResult-ms-test-suite-conformance.xml || EXIT_CODE=1; \
		$(MAKE) -C $(MSTESTSUITE_PATH)/systemruntimebringup build MCS="$(MCS) -debug -warn:1" && \
		$(MAKE) -C $(MSTESTSUITE_PATH)/systemruntimebringup run MONO="$(RUNTIME)" || EXIT_CODE=1; \
		exit $$EXIT_CODE; \
	else \
		echo "*** [ms-test-suite] Getting the repository failed, you probably don't have access to this Xamarin-internal resource. Skipping."; \
	fi
