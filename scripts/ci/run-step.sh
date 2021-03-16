#!/bin/bash -e
TIMEOUTCMD=`dirname "${BASH_SOURCE[0]}"`/babysitter
if ! ${TIMEOUTCMD} -h >/dev/null 2>&1; then
    TIMEOUTCMD=timeout  # fall back to timeout if babysitter doesn't work (e.g. python not installed or wrong version)
fi

export MONO_BABYSITTER_LOG_FILE=babysitter_report.json_lines

helptext ()
{
    echo "run-step.sh {--label=LABEL} {--skip|--timeout=TIMEOUT [--fatal]} command to run with arguments"
}

for i in "$@"
do
case $i in
    --help)
    helptext
    exit 0
    ;;
    --label=*)
    LABEL="${i#*=}"
    shift # past argument=value
    ;;
    --timeout=*)
    TIMEOUT="${i#*=}"
    shift # past argument=value
    ;;
    --fatal)
    FATAL="true"
    shift # past argument
    ;;
    --skip)
    SKIP="true"
    shift # past argument
    ;;
    *)
            # unknown option, assume just part of cmdline
    ;;
esac
done
if [ -n "${SKIP}" ] && [ -z "${LABEL}" ]
    then helptext
    exit 1
fi
if [ -n "${SKIP}" ]
    then echo -e "*** start: ${LABEL}\n*** end(0): ${LABEL}: \e[45mSkipped\e[0m"
    exit 0
fi
if [ -z "${LABEL}" ] || [ -z "${TIMEOUT}" ]
    then helptext
    exit 1
fi
STARTTIME=`date +%s`
echo "*** start: ${LABEL}"
if [ -n "${FATAL}" ]; then
    ${TIMEOUTCMD} --signal=ABRT --kill-after=60s ${TIMEOUT} "$@" && echo -e "*** end($(echo $(date +%s) - ${STARTTIME} | bc)): ${LABEL}: \e[42mPassed\e[0m" || (echo -e "*** end($(echo $(date +%s) - ${STARTTIME} | bc)): ${LABEL}: \e[41mFailed\e[0m" && echo "##vso[task.logissue type=error] ${LABEL} step failed." && exit 1)
else
    ${TIMEOUTCMD} --signal=ABRT --kill-after=60s ${TIMEOUT} "$@" && echo -e "*** end($(echo $(date +%s) - ${STARTTIME} | bc)): ${LABEL}: \e[42mPassed\e[0m" || (echo -e "*** end($(echo $(date +%s) - ${STARTTIME} | bc)): ${LABEL}: \e[43mUnstable\e[0m" && echo "##vso[task.logissue type=error] ${LABEL} step failed (unstable)." && echo "##vso[task.setvariable variable=BuildHasUnstableSteps]true")
fi
