#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests DebianShootoutMono.stamp

${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-BinaryTrees
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-Fannkuchredux
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-Fasta
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-NBody
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-Mandelbrot
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-RegexRedux
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-RevComp
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-SpectralNorm
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests test-run-microbench-KNucleotide

${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-BinaryTrees
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-Fannkuchredux
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-Fasta
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-NBody
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-Mandelbrot
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-RegexRedux
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-RevComp
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-SpectralNorm
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests test-run-microbench-profiled-KNucleotide
