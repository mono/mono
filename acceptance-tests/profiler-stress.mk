SYS_REFS = \
	System.dll \
	System.Core.dll \
	System.Data.dll \
	System.Runtime.Serialization.dll \
	System.Xml.dll \
	System.Xml.Linq.dll

check-profiler-stress:
	@$(MAKE) validate-benchmarker RESET_VERSIONS=1
	cd profiler-stress && $(MCS) -target:exe $(addprefix -r:, $(SYS_REFS)) -out:runner.exe @runner.exe.sources
	cd profiler-stress && $(RUNTIME) runner.exe
