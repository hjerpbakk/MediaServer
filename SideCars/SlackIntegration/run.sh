#! /bin/bash
set -e
./build.sh
docker run -p 80:1338 dips/slack-integration