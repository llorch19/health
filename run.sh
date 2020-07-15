#!/usr/bin/env bash
EXEC=dotnet
PORT=5005

( ps aux | grep ${EXEC} | grep zudan | grep -v grep | awk '{print $2}'| xargs kill -9 )   >/dev/null 2>&1
(pgrep $(basename $(realpath "$0")) | xargs kill -9)  > /dev/null 2>&1
nohup dotnet run --urls="http://*:$PORT" --project=$(realpath "$0") &
