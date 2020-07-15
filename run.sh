#!/usr/bin/env bash
# This file is subject to the terms and conditions defined in
# file 'LICENSE.txt', which is part of this source code package.

set -x
EXEC=dotnet
PORT=5005

( ps aux | grep ${EXEC} | grep zudan | grep -v grep | awk '{print $2}'| xargs kill -9 )   >/dev/null 2>&1
(pgrep $(basename $(realpath "$0")) | xargs kill -9)  > /dev/null 2>&1
nohup dotnet run --urls="http://*:$PORT"  --project=$(dirname $(realpath "$0")) &
