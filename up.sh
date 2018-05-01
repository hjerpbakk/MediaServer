#! /bin/bash
set -e

./down.sh

export repo_pre=vt-optimus-solr02:5000/
export tag=:$(cat MediaServer/wwwroot/VERSION.txt)
export populate_tag=:$(cat SideCars/CacheWarmer/VERSION.txt)
export slack_tag=:$(cat SideCars/SlackIntegration/VERSION.txt)

docker-compose up -d