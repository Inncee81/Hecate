#!/bin/bash
: <<'code'
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
code

set -e
cd "`dirname "$0"`"

rm -f Build.rsp
sh ./Build.sh