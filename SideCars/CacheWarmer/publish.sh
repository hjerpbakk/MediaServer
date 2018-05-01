#! /bin/bash
set -e

VERSION=$(cat VERSION.txt)

./build.sh
docker tag dips/cache-populator:latest vt-optimus-solr02:5000/dips/cache-populator:$VERSION
docker push vt-optimus-solr02:5000/dips/cache-populator:$VERSION
