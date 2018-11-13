#!/bin/bash -e

export TESTCMD=`dirname "${BASH_SOURCE[0]}"`/run-step.sh

${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks DebianShootoutMono.stamp
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-BinaryTrees
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-Fannkuchredux
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-Fasta
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-NBody
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-Mandelbrot
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-RegexRedux
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-RevComp
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-SpectralNorm
${TESTCMD} --label=microbenchmark --timeout=40m make -C mono/tests/microbenchmarks test-KNucleotide
