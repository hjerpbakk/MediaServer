#! /bin/bash
set -e
./build.sh
docker run -p 80:1337 dips/cache-populator