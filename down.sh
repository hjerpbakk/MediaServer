#! /bin/bash
set -e

export repo_pre=
export tag=
export populate_tag=

docker-compose stop
docker-compose rm --force