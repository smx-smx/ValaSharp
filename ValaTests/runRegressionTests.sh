#!/bin/bash
cd "$(dirname "$0")"
cpus=`grep processor /proc/cpuinfo | wc -l`
find . -type f -iname "*.vala" -print0 | xargs -0 --max-procs=${cpus} -L 1 php regressionTests.php
