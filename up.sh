#! /bin/bash
set -e

./down.sh

export repo_pre=vt-optimus-solr02:5000/
export tag=:$(cat MediaServer/wwwroot/VERSION.txt)
export populate_tag=:0.0.4

docker-compose up -d