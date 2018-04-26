#! /bin/bash
set -e

VERSION=$(cat MediaServer/wwwroot/VERSION.txt)

./build.sh
docker tag dips/media-server:latest vt-optimus-solr02:5000/dips/media-server:$VERSION
docker push vt-optimus-solr02:5000/dips/media-server:$VERSION
