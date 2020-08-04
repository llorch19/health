#!/usr/bin/env bash
# This file is subject to the terms and conditions defined in
# file 'LICENSE.txt', which is part of this source code package.

set -x
PORT=25000
IDIR=$(dirname $(realpath "$0"))
IPROJ=$(basename ${IDIR})

while lsof -i:$PORT
do
	(while ps ux | grep ${PORT} |  grep -v grep | awk '{print $2}'| xargs kill ; do sleep 1; done  )  
	(while pgrep $IPROJ | xargs kill ; do sleep 1; done  )  
	sleep 2
done
nohup dotnet run --urls="http://*:$PORT"  --project=$IDIR  > /dev/null 2>&1 &
