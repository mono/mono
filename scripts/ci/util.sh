#!/bin/bash -e

function report_github_status {
    if [ -z "$1" ]; then echo "No status specified. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "$2" ]; then echo "No context specified. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "$3" ]; then echo "No description specified. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "${ghprbActualCommit}" ]; then echo "Not a pull request. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "${GITHUB_STATUS_AUTH_TOKEN}" ]; then echo "No auth token specified. Skipping GitHub manual status report."; return 1; fi;

    wget -qO- --header "Content-Type: application/json" --header "Authorization: token ${GITHUB_STATUS_AUTH_TOKEN}" --post-data "{\"state\": \"$1\", \"context\":\"$2\", \"description\": \"$3\", \"target_url\": \"$4\"}" "https://api.github.com/repos/mono/mono/statuses/${ghprbActualCommit}"
}

function helix_set_env_vars {
    if [[ ${CI_TAGS} != *'helix'* ]]; then return 0; fi;

    if   [[ ${CI_TAGS} == *'-i386'*  ]]; then export MONO_HELIX_ARCHITECTURE="x86";
    elif [[ ${CI_TAGS} == *'-amd64'* ]]; then export MONO_HELIX_ARCHITECTURE="x64";
    elif [[ ${CI_TAGS} == *'-arm64'* ]]; then export MONO_HELIX_ARCHITECTURE="arm64";
    elif [[ ${CI_TAGS} == *'-armel'* ]]; then export MONO_HELIX_ARCHITECTURE="armel";
    elif [[ ${CI_TAGS} == *'-armhf'* ]]; then export MONO_HELIX_ARCHITECTURE="armhf";
    else echo "Couldn't determine architecture for Helix."; return 1; fi

    if   [[ ${CI_TAGS} == *'linux-'* ]]; then export MONO_HELIX_OPERATINGSYSTEM="Debian 9";    export MONO_HELIX_TARGET_QUEUE="Debian.9.Amd64";
    elif [[ ${CI_TAGS} == *'osx-'*   ]]; then export MONO_HELIX_OPERATINGSYSTEM="macOS 10.12"; export MONO_HELIX_TARGET_QUEUE="OSX.1012.Amd64";
    elif [[ ${CI_TAGS} == *'win-'*   ]]; then export MONO_HELIX_OPERATINGSYSTEM="Windows 10";  export MONO_HELIX_TARGET_QUEUE="Windows.10.Amd64";
    else echo "Couldn't determine operating system and target queue for Helix."; return 1; fi

    if [[ ${CI_TAGS} == *'pull-request'* ]]; then
        export MONO_HELIX_CREATOR="$ghprbPullAuthorLogin"
        export MONO_HELIX_TARGET_QUEUE="${MONO_HELIX_TARGET_QUEUE}.Open"
        export MONO_HELIX_SOURCE="pr/jenkins/mono/mono/$ghprbTargetBranch/"
        export MONO_HELIX_BUILD_MONIKER="$(git rev-parse HEAD)"
    else
        version_number=$(grep AC_INIT configure.ac | sed -e 's/AC_INIT(mono, \[//' -e 's/\],//')
        major_ver=$(echo "$version_number" | cut -d . -f 1)
        minor_ver=$(echo "$version_number" | cut -d . -f 2)
        build_ver=$(echo "$version_number" | cut -d . -f 3)
        blame_rev=$(git blame configure.ac HEAD | grep AC_INIT | sed 's/ .*//')
        patch_ver=$(git log "$blame_rev"..HEAD --oneline | wc -l | sed 's/ //g')
        export MONO_HELIX_CREATOR="monojenkins"
        export MONO_HELIX_SOURCE="official/mono/mono/$MONO_BRANCH/"
        export MONO_HELIX_BUILD_MONIKER=$(printf %d.%d.%d.%d "$major_ver" "$minor_ver" "$build_ver" "$patch_ver")
    fi
}

function helix_send_build_start_event {
    if [[ ${CI_TAGS} != *'helix-telemetry'* ]]; then return 0; fi;
    if [ -z "$1" ]; then echo "No type."; return 1; fi;

    url="https://helix.dot.net/api/2018-03-14/telemetry/job"

    # we need an API key for non-PR builds
    if [[ "${MONO_HELIX_SOURCE}" != "pr/"* ]]; then
        if [ -z "$MONO_HELIX_API_KEY" ]; then echo "No Helix API key."; return 1; fi;
        url="${url}?access_token=${MONO_HELIX_API_KEY}"
    fi

    mkdir -p "helix-telemetry/${1}"
    wget -O- --method="POST" --header='Content-Type: application/json' --header='Accept: application/json' --body-data="{
        \"QueueId\": \"Build\",
        \"Source\": \"${MONO_HELIX_SOURCE}\",
        \"Type\": \"${1}\",
        \"Build\": \"${MONO_HELIX_BUILD_MONIKER}\",
        \"Properties\": { \"architecture\": \"${MONO_HELIX_ARCHITECTURE}\", \"operatingSystem\": \"${MONO_HELIX_OPERATINGSYSTEM}\"}
        }" "${url}" > "helix-telemetry/${1}/job-token.txt"
    helix_job_token=$(cat "helix-telemetry/${1}/job-token.txt" | sed 's/"//g')

    wget -O- --method="POST" --header='Accept: application/json' --header="X-Helix-Job-Token: ${helix_job_token}" "https://helix.dot.net/api/2018-03-14/telemetry/job/build?buildUri=${BUILD_URL//+/%2B}" > "helix-telemetry/${1}/build-id.txt"
}

function helix_send_build_done_event {
    if [[ ${CI_TAGS} != *'helix-telemetry'*  ]]; then return 0; fi;
    if [ -z "$1" ]; then echo "No type."; return 1; fi;
    if [ -z "$2" ]; then echo "No error count."; return 1; fi;

    helix_job_token=$(cat "helix-telemetry/${1}/job-token.txt" | sed 's/"//g')
    helix_build_id=$(cat "helix-telemetry/$1/build-id.txt" | sed 's/"//g')
    wget -O- --method="POST" --header='Accept: application/json' --header="X-Helix-Job-Token: ${helix_job_token}" "https://helix.dot.net/api/2018-03-14/telemetry/job/build/${helix_build_id}/finish?errorCount=${2}&warningCount=0"
}
