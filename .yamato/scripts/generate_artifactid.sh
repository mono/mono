#!/bin/bash
set -euxo pipefail

revision=$(git rev-parse --short HEAD)
artifactsha=$(sha256sum $1 | cut -d " " -f1)

mkdir -p collectedbuilds

printf "MonoBleedingEdge/%s_%s.tar.zst\n" $revision $artifactsha > collectedbuilds/artifactid.txt

