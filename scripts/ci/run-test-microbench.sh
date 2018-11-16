#!/bin/bash -e


export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

if [[ ${CI_TAGS} == *'win-'* ]]; then
	exit 0
fi

${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests DebianShootoutMono.stamp

if [ -z "$MONO_BENCH_GIST_URL"]; then
	${TESTCMD} --label=microbenchmark-BinaryTrees --timeout=40m make -C acceptance-tests run-microbench-BinaryTrees
	${TESTCMD} --label=microbenchmark-Fannkuchredux --timeout=40m make -C acceptance-tests run-microbench-Fannkuchredux
	${TESTCMD} --label=microbenchmark-Fasta --timeout=40m make -C acceptance-tests run-microbench-Fasta
	${TESTCMD} --label=microbenchmark-NBodyTest --timeout=40m make -C acceptance-tests run-microbench-NBodyTest
	${TESTCMD} --label=microbenchmark-Mandelbrot --timeout=40m make -C acceptance-tests run-microbench-Mandelbrot
	${TESTCMD} --label=microbenchmark-RegexRedux --timeout=40m make -C acceptance-tests run-microbench-RegexRedux
	${TESTCMD} --label=microbenchmark-RevComp --timeout=40m make -C acceptance-tests run-microbench-RevComp
	${TESTCMD} --label=microbenchmark-SpectralNorm --timeout=40m make -C acceptance-tests run-microbench-SpectralNorm
	${TESTCMD} --label=microbenchmark-KNucleotide --timeout=40m make -C acceptance-tests run-microbench-KNucleotide
else
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-GistBenchmark
fi

if [[ ${CI_TAGS} == *'linux-'* ]]; then
	export MONO_PERF_BINARY=perf_4.9
	${TESTCMD} --label=microbench-profiler-check --timeout=40m make -C acceptance-tests test-run-microbench-perf-check

	if [ -z "$MONO_BENCH_GIST_URL"]; then
		${TESTCMD} --label=microbench-profiled-BinaryTrees   --timeout=40m make -C acceptance-tests run-microbench-profiled-BinaryTrees
		${TESTCMD} --label=microbench-profiled-Fannkuchredux --timeout=40m make -C acceptance-tests run-microbench-profiled-Fannkuchredux
		${TESTCMD} --label=microbench-profiled-Fasta         --timeout=40m make -C acceptance-tests run-microbench-profiled-Fasta
		${TESTCMD} --label=microbench-profiled-NBodyTest         --timeout=40m make -C acceptance-tests run-microbench-profiled-NBodyTest
		${TESTCMD} --label=microbench-profiled-Mandelbrot    --timeout=40m make -C acceptance-tests run-microbench-profiled-Mandelbrot
		${TESTCMD} --label=microbench-profiled-RegexRedux    --timeout=40m make -C acceptance-tests run-microbench-profiled-RegexRedux
		${TESTCMD} --label=microbench-profiled-RevComp       --timeout=40m make -C acceptance-tests run-microbench-profiled-RevComp
		${TESTCMD} --label=microbench-profiled-SpectralNorm  --timeout=40m make -C acceptance-tests run-microbench-profiled-SpectralNorm
		${TESTCMD} --label=microbench-profiled-KNucleotide   --timeout=40m make -C acceptance-tests run-microbench-profiled-KNucleotide
	else
		${TESTCMD} --label=microbench-profiled-Gist --timeout=40m make -C acceptance-tests run-microbench-profiled-GistBenchmark
	fi

	${TESTCMD} --label=microbench-report --timeout=40m make -C acceptance-tests perf-report-total
fi

