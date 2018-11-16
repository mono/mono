check-microbench: DebianShootoutMono.stamp
	@$(MAKE) test-run-microbench

DebianShootoutMono.stamp: 
	@$(MAKE) validate-DebianShootoutMono RESET_VERSIONS=1
	@$(MAKE) prepare-dlls
	@touch $@

abs_top_srcdir = $(abspath $(top_srcdir))
TEST_EXE_PATH=$(abs_top_srcdir)/acceptance-tests/external/DebianShootoutMono/release/
NET_4_X_RUNTIME=MONO_PATH=$(TEST_EXE_PATH):$(abs_top_srcdir)/mcs/class/lib/net_4_x $(abs_top_srcdir)/runtime/mono-wrapper
FULL_AOT_RUNTIME=MONO_PATH=$(abs_top_srcdir)/mcs/class/lib/testing_aot_full $(abs_top_srcdir)/runtime/mono-wrapper

define BenchmarkDotNetTemplate
run-microbench-$(1):: DebianShootoutMono.stamp
	MONO_BENCH_AOT_RUN="$(AOT_RUN_FLAGS)"\
	MONO_BENCH_AOT_BUILD="$(AOT_BUILD_FLAGS)"\
	MONO_BENCH_PROFILE_PREFIX="$(PROFILE_TOOL)"\
	MONO_BENCH_EXECUTABLE="$(abs_top_srcdir)/runtime/mono-wrapper" \
	MONO_BENCH_PATH="$(abs_top_srcdir)/mcs/class/lib/$(TEST_PROFILE)" \
	$(NET_4_X_RUNTIME) \
	$(TEST_EXE_PATH)/DebianShootoutMono.exe $(1)

test-run-microbench:: run-microbench-$(1)

if HOST_LINUX
run-microbench-profiled-$(1):: DebianShootoutMono.stamp
	MONO_EXECUTABLE="perf record -F 99 -a -g -- $(abs_top_srcdir)/mono/mini/mono-sgen " \
	MONO_BENCH_EXECUTABLE="$(abs_top_srcdir)/runtime/mono-wrapper" \
	MONO_BENCH_AOT_RUN="$(AOT_RUN_FLAGS)"\
	MONO_BENCH_AOT_BUILD="$(AOT_BUILD_FLAGS)"\
	MONO_BENCH_PROFILE_PREFIX="$(PROFILE_TOOL)"\
	MONO_BENCH_PATH="$(abs_top_srcdir)/mcs/class/lib/$(TEST_PROFILE)" \
	$(NET_4_X_RUNTIME) \
	$(TEST_EXE_PATH)/DebianShootoutMono.exe $(1)
	perf script > $(1).out.perf

test-run-microbench-profiled:: run-microbench-profiled-$(1)
endif

endef

.PHONY: prepare-dlls

if FULL_AOT_TESTS
prepare-dlls: 
	$(FULL_AOT_RUNTIME) $(AOT_BUILD_FLAGS) $(TEST_EXE_PATH)/*.{dll,exe}

else

prepare-dlls:

endif

$(eval $(call BenchmarkDotNetTemplate,BinaryTrees))
$(eval $(call BenchmarkDotNetTemplate,NBody))
$(eval $(call BenchmarkDotNetTemplate,Mandelbrot))
$(eval $(call BenchmarkDotNetTemplate,RegexRedux))
$(eval $(call BenchmarkDotNetTemplate,SpectralNorm))
$(eval $(call BenchmarkDotNetTemplate,Fannkuchredux))
$(eval $(call BenchmarkDotNetTemplate,Fasta))
$(eval $(call BenchmarkDotNetTemplate,KNucleotide))
$(eval $(call BenchmarkDotNetTemplate,RevComp))


