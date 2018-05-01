#! /bin/bash
set -e
./build.sh
docker run -p 80:5000 dips/media-server