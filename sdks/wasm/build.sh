#!/usr/bin/env bash

# Stop script if unbound variable found (use ${var:-} if intentional)
set -u

# Stop script if command returns non-zero exit code.
# Prevents hidden errors caused by missing error code propagation.
set -e

usage()
{
  echo "Common settings:"
  echo "  --configuration <value>    Build configuration: 'Debug' or 'Release' (short: -c)"
  echo "  --help                     Print help and exit (short: -h)"
  echo ""

  echo "Actions:"
  echo "  --clean                    Clean up the clean targets"
  echo "  --cxx                      Enable CXX"
  echo "  --reconfigure              Force provision and configure"  
  echo "  --test                     Run all tests (short: -t)"
  echo "  --thread                   Enable WASM threads"
  echo "  --dynamic                  Enable Dynamic Runtime"  
  echo "  --win                      Enable Windows cross build"
  echo ""

  echo "Command line arguments starting with '/p:' are passed through to MSBuild."
  echo "Arguments can also be passed in with a single hyphen."
}

cleanall=false
cxx=false
configuration='Release'
force_reconfigure=false
test=false
thread=false
dynamic=false
win=false

while [[ $# > 0 ]]; do
  opt="$(echo "${1/#--/-}" | awk '{print tolower($0)}')"
  case "$opt" in
    -help|-h)
      usage
      exit 0
      ;;
    -clean)
      cleanall=true
      ;;
    -cxx)
      cxx=true
      ;;
    -configuration|-c)
      configuration=$2
      shift
      ;;
    -dynamic)
      dynamic=true
      ;;
    -reconfigure)
      force_reconfigure=true
      ;;
    -test|-t)
      test=true
      ;;
    -thread)
      thread=true
      ;;
    -win)
      win=true
      ;;
    *)
      echo "Invalid argument: $1"
      usage
      exit 1
      ;;
  esac

  shift
done

CPU_COUNT=$(getconf _NPROCESSORS_ONLN || echo 4)

# clean all 
if [ "$cleanall" = "true" ]; then
  make clean
  exit 0;
fi

# provision and configuration
if [[ "$force_reconfigure" == "true" || ! -f .configured ]]; then
  # re-create Make.config
  echo "ENABLE_WASM=1" > ../Make.config

  if [ "$win" == "true" ]; then
    echo "ENABLE_WINDOWS=1" >> ../Make.config
  fi
  
  if [ "$cxx" == "true" ]; then
    echo "ENABLE_CXX=1" >> ../Make.config
  fi

  if [ "$dynamic" == "true" ]; then
    echo "ENABLE_WASM_DYNAMIC_RUNTIME=1" >> ../Make.config
  fi

  if [ "$thread" == "true" ]; then
    echo "ENABLE_WASM_THREADS=1" >> ../Make.config
  fi
  
  if [[ "$configuration" == "Debug" ]]; then
    echo "CONFIGURATION=debug" >> ../Make.config
  fi

  make -C ../builds provision-wasm
  make -j ${CPU_COUNT} -C ../builds configure-wasm NINJA=
  touch .configured
fi

make -j ${CPU_COUNT} -C ../builds archive-wasm NINJA=
make -C ../wasm runtime

# run all tests
if [ "$test" = "true" ]; then
  export aot_test_suites="System.Core"
  export mixed_test_suites="System.Core"
  export xunit_test_suites="System.Core corlib"

  make -j ${CPU_COUNT} build
  make run-all-mini
  make run-all-corlib
  #The following tests are not passing yet, so enabling them would make us perma-red
  #make run-all-System
  make run-all-System.Core
  for suite in ${xunit_test_suites}; do make run-${suite}-xunit; done
  # disable for now until https://github.com/mono/mono/pull/13622 goes in
  #make run-debugger-tests
  make run-browser-tests
  #make run-browser-threads-tests
  make -j ${CPU_COUNT} run-aot-mini
  make -j ${CPU_COUNT} build-aot-all
  for suite in ${aot_test_suites}; do make run-aot-${suite}; done
  for suite in ${mixed_test_suites}; do make run-aot-mixed-${suite}; done
  #make check-aot
  make package
fi

exit 0