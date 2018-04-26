#! /bin/bash
set -e

export repo_pre=
export tag=

docker-compose stop
docker-compose rm --force