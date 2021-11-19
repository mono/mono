#!/bin/bash -e

function report_github_status {
    if [ -z "$1" ]; then echo "No status specified. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "$2" ]; then echo "No context specified. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "$3" ]; then echo "No description specified. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "${ghprbActualCommit}" ]; then echo "Not a pull request. Skipping GitHub manual status report."; return 1; fi;
    if [ -z "${GITHUB_STATUS_AUTH_TOKEN}" ]; then echo "No auth token specified. Skipping GitHub manual status report."; return 1; fi;

    wget -qO- --header "Content-Type: application/json" --header "Authorization: token ${GITHUB_STATUS_AUTH_TOKEN}" --post-data "{\"state\": \"$1\", \"context\":\"$2\", \"description\": \"$3\", \"target_url\": \"$4\"}" "https://api.github.com/repos/mono/mono/statuses/${ghprbActualCommit}"
}
