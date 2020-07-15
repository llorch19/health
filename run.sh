#!/usr/bin/env bash
set -x
EXEC=dotnet
PORT=5005

( ps aux | grep ${EXEC} | grep zudan | grep -v grep | awk '{print $2}'| xargs kill -9 )   >/dev/null 2>&1
(pgrep $(basename $(realpath "$0")) | xargs kill -9)  > /dev/null 2>&1
dotnet run --urls="http://*:$PORT"  --project=$(dirname $(realpath "$0"))
