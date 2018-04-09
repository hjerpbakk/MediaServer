#!/bin/bash
docker run -d -p 5000:80 mediaserver --mount source=/Users/sankra/Downloads/videos,target=/app/wwwroot/videos