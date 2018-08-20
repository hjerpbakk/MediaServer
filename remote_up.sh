#! /bin/bash
set -e

# TODO: Remote folders must be added as part of this script
# TODO: Check that connectionstring is added as an environment variable

# Remote Folders
ssh root@vt-dipstube01 "mkdir -p ~/dipstube/MediaServer/wwwroot"
ssh root@vt-dipstube01 "mkdir -p ~/dipstube/SideCars/SlackIntegration"
ssh root@vt-dipstube01 "mkdir -p ~/dipstube/SideCars/CacheWarmer"

# Docker Compose
scp docker-compose.yml root@vt-dipstube01:dipstube

# Needed scripts
scp down.sh root@vt-dipstube01:dipstube
scp up.sh root@vt-dipstube01:dipstube

# Versions of containers
scp MediaServer/wwwroot/VERSION.txt root@vt-dipstube01:dipstube/MediaServer/wwwroot/VERSION.txt
scp SideCars/SlackIntegration/VERSION.txt root@vt-dipstube01:dipstube/SideCars/SlackIntegration/VERSION.txt
scp SideCars/CacheWarmer/VERSION.txt root@vt-dipstube01:dipstube/SideCars/CacheWarmer/VERSION.txt

ssh root@vt-dipstube01 "cd dipstube; ./up.sh"