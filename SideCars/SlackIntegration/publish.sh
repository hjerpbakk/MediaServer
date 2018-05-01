#! /bin/bash
set -e

VERSION=$(cat VERSION.txt)

./build.sh
docker tag dips/slack-integration:latest vt-optimus-solr02:5000/dips/slack-integration:$VERSION
docker push vt-optimus-solr02:5000/dips/slack-integration:$VERSION
