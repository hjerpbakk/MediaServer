#! /bin/bash
set -e

export repo_pre=
export tag=
export populate_tag=
export slack_tag=

docker-compose stop cache-populator
docker-compose stop media-server
docker-compose stop slack-integration
docker-compose rm --force