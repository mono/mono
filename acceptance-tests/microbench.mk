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

PERF_BINARY=$(if $(MONO_PERF_BINARY),$(MONO_PERF_BINARY),perf)
PERF_RUNTIME=$(abs_top_srcdir)/acceptance-tests/microbench-perf.sh

define BenchmarkDotNetTemplate

run-microbench-$(1):: DebianShootoutMono.stamp
	MONO_BENCH_AOT_RUN="$(AOT_RUN_FLAGS)" \
	MONO_BENCH_AOT_BUILD="$(AOT_BUILD_FLAGS)" \
	MONO_BENCH_EXECUTABLE="$(abs_top_srcdir)/runtime/mono-wrapper" \
	MONO_BENCH_PATH="$(abs_top_srcdir)/mcs/class/lib/$(TEST_PROFILE)" \
	MONO_BENCH_INPUT="$(2)" \
	$(NET_4_X_RUNTIME) \
	$(TEST_EXE_PATH)/DebianShootoutMono.exe $(1)

test-run-microbench:: run-microbench-$(1)

run-microbench-debug-$(1):: DebianShootoutMono.stamp
	echo MONO_PATH="$(abs_top_srcdir)/mcs/class/lib/$(TEST_PROFILE)" $(abs_top_srcdir)/runtime/mono-wrapper $(AOT_BUILD_FLAGS) $(TEST_EXE_PATH)/DebianShootoutMono.exe
	MONO_BENCH_INPUT="$(2)" \
	MONO_PATH="$(abs_top_srcdir)/mcs/class/lib/$(TEST_PROFILE)" \
	$(abs_top_srcdir)/runtime/mono-wrapper $(AOT_RUN_FLAGS) $(TEST_EXE_PATH)/DebianShootoutMono.exe Run $(1)

test-run-microbench-debug:: run-microbench-debug-$(1)

if HOST_LINUX
run-microbench-profiled-$(1):: microbench-results/$(1).perf.data 

microbench-results/$(1).perf.data: DebianShootoutMono.stamp 
	mkdir -p microbench-results
	MONO_PERF_BINARY="$(PERF_BINARY)" \
	MONO_BENCH_EXECUTABLE="$(PERF_RUNTIME)" \
	MONO_BENCH_AOT_RUN="$(AOT_RUN_FLAGS)"\
	MONO_BENCH_AOT_BUILD="$(AOT_BUILD_FLAGS)"\
	MONO_BENCH_PATH="$(abs_top_srcdir)/mcs/class/lib/$(TEST_PROFILE)" \
	MONO_BENCH_INPUT="$(2)" \
	$(NET_4_X_RUNTIME) \
	$(TEST_EXE_PATH)/DebianShootoutMono.exe $(1) $(2)
	mv perf.data microbench-results/$(1).perf.data

microbench-results/$(1).tmp.perf: microbench-results/$(1).perf.data
	$(PERF_BINARY) script -i microbench-results/$(1).perf.data > microbench-results/$(1).tmp.perf

microbench-results/$(1).perf-flame.svg: microbench-results/$(1).tmp.perf
	cat microbench-results/$(1).tmp.perf | ./external/DebianShootoutMono/FlameGraph/stackcollapse-perf.pl > microbench-results/$(1).perf-folded
	./external/DebianShootoutMono/FlameGraph/flamegraph.pl microbench-results/$(1).perf-folded > microbench-results/$(1).perf-flame.svg
	rm microbench-results/$(1).tmp.perf
	rm microbench-results/$(1).perf-folded

MONO_PERF_FLAGS=--show-cpu-utilization -n --hierarchy -T $(MONO_PERF_ADDITIONAL_FLAGS)

microbench-results/$(1).perf.report: microbench-results/$(1).perf.data
	$(PERF_BINARY) report -i microbench-results/$(1).perf.data $(MONO_PERF_FLAGS) > microbench-results/$(1).perf.report 

test-run-microbench-profiled:: run-microbench-profiled-$(1)

test-run-microbench-publish-collect:: microbench-results/$(1).perf.data microbench-results/$(1).perf.report microbench-results/$(1).perf-flame.svg

endif

endef

if HOST_LINUX

test-run-microbench-perf-check:
	$(PERF_BINARY) record -a -o perf.data -- echo "testing"
	rm perf.data

microbench-results/perf-data.zip:
	zip microbench-results/perf-data.zip microbench-results/*.perf.data
	rm microbench-results/*.perf.data

perf-report: microbench-results/perf-data.zip

perf-report-total: test-run-microbench-publish-collect
	@$(MAKE) perf-report

endif

.PHONY: prepare-dlls

if FULL_AOT_TESTS
prepare-dlls: 
	$(FULL_AOT_RUNTIME) $(AOT_BUILD_FLAGS) $(TEST_EXE_PATH)/*.{dll,exe}

else

prepare-dlls:

endif

FIXTURE_DIR=$(abs_top_srcdir)/acceptance-tests/external/DebianShootoutMono/fixtures

$(eval $(call BenchmarkDotNetTemplate,Mandelbrot,))
$(eval $(call BenchmarkDotNetTemplate,RegexRedux,$(FIXTURE_DIR)/regexredux-input.txt))
$(eval $(call BenchmarkDotNetTemplate,KNucleotide,$(FIXTURE_DIR)/knucleotide-input.txt))

$(eval $(call BenchmarkDotNetTemplate,BinaryTrees,))
$(eval $(call BenchmarkDotNetTemplate,NBodyTest,))
$(eval $(call BenchmarkDotNetTemplate,SpectralNorm,))
$(eval $(call BenchmarkDotNetTemplate,Fannkuchredux,))
$(eval $(call BenchmarkDotNetTemplate,Fasta,))

$(eval $(call BenchmarkDotNetTemplate,RevComp,$(FIXTURE_DIR)/revcomp-input.txt)) 

#$(eval $(call BenchmarkDotNetTemplate,GistBenchmark,$(MONO_BENCH_GIST_URL))) broken in BDN, bug filed

