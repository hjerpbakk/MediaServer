#! /bin/bash
set -e

./down.sh

export repo_pre=
export tag=
export populate_tag=

docker-compose up -d