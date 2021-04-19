#!/bin/bash
: <<'code'
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
code

# Parameter
RSPPATH=Build.rsp
PKG_PATH=../../Packages
compiler=csc

# DEBUG STUFF
# disable debug with empty variable
DEBUG=

# Print error message
print_error() {
    echo "[ERRO] $1"
}

# Print warning message
print_warning() {
    echo "[WARN] $1"
}

# Print info message
print_info() {
    echo "[INFO] $1"
}

# Debug output
print_debug() {
    if [ -n "$DEBUG" ]
    then
        echo "[DEBU] $1"
    fi
}

# Compile Hecate using the rsp file
compile_rsp() {
    print_info "Adding preprocessor definition"
    echo "-define:net45;NET45;NET_4_5;NET_FRAMEWORK"    >> $RSPPATH
    print_info "Adding references"
    echo "-reference:System.Net.Http.dll"               >> $RSPPATH

    print_info "Compiling Hecate"
    if $compiler @$RSPPATH;
    then
        print_info "Compilation done!"
    else
        print_error "Compilation failed!"
    fi
}

# Check
print_info "Building for mono (.NET4)"
if [[ ! -f $RSPPATH ]];
then
    sh ./Build.Rsp.sh
fi

# compiling
print_info "Compiling $RSPPATH"
compile_rsp
