#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests DebianShootoutMono.stamp

if [ -z "$MONO_BENCH_GIST_URL"]; then
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-BinaryTrees
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-Fannkuchredux
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-Fasta
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-NBody
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-Mandelbrot
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-RegexRedux
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-RevComp
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-SpectralNorm
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-KNucleotide
else
	${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-GistBenchmark
fi

if [[ ${CI_TAGS} == *'linux-'* ]]; then
	if [ -z "$MONO_BENCH_GIST_URL"]; then
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-BinaryTrees
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-Fannkuchredux
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-Fasta
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-NBody
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-Mandelbrot
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-RegexRedux
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-RevComp
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-SpectralNorm
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-KNucleotide
	else
		${TESTCMD} --label=microbench-profiled --timeout=40m make -C acceptance-tests run-microbench-profiled-GistBenchmark
	fi

	${TESTCMD} --label=microbench-report --timeout=40m make -C acceptance-tests perf-report
fi

