#! /bin/bash
set -e

./down.sh

export repo_pre=
export tag=

docker-compose up -d