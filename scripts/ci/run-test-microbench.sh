#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests DebianShootoutMono.stamp

${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-BinaryTrees
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-Fannkuchredux
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-Fasta
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-NBody
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-Mandelbrot
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-RegexRedux
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-RevComp
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-SpectralNorm
${TESTCMD} --label=microbenchmark --timeout=40m make -C acceptance-tests run-microbench-KNucleotide

${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-BinaryTrees
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-Fannkuchredux
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-Fasta
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-NBody
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-Mandelbrot
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-RegexRedux
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-RevComp
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-SpectralNorm
${TESTCMD} --label=microbench-profiledmark --timeout=40m make -C acceptance-tests run-microbench-profiled-KNucleotide
