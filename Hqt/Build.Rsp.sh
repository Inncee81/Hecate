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

# for debug purpose
if [ -n "$DEBUG" ]
then
    print_debug "Debug mode active!"
    set -x
    cd ../Hecate/Hecate/Hqt
    print_debug "Deleting rsp file"
    rm $RSPPATH
fi

# Add a single package to the rsp file
add_pkg() {
    path=$1 #write parameter to local variable
    echo -recurse:"$path"/*.cs                      >> $RSPPATH
}

# Check a directory for all packages with the newest version
# Those directories must have the format
# <filename>@<Version>
add_newest_versions() {
    declare -A file_dict
    # find the newest versions of all files
    for f in $package_path/*
    do
        print_debug "Found package $f"
        if [[ -d $f && $f =~ ^(.*)@(.*)$ ]]
        then
            file_dict[${BASH_REMATCH[1]}]=${BASH_REMATCH[2]}
        fi
    done
    for key in "${!file_dict[@]}";
    do
        add_pkg "$key@${file_dict[$key]}"
    done
}

# Build the rsp file from scratch
build_rsp() {
    # Adding general parameters
    print_info "Building new rsp file"
    echo -nologo                                        >  $RSPPATH
    echo -debug                                         >> $RSPPATH
    echo -nowarn:0436,1685                              >> $RSPPATH
    echo -out:../../Hqt.exe                             >> $RSPPATH
    echo -platform:x64                                  >> $RSPPATH
    echo -target:exe                                    >> $RSPPATH

    print_info "Adding packages"
    if [ -d ../../Sharp ]
    then
        package_path=../../Sharp/*
        for f in $package_path
        do
            print_debug "Found $f"
            if [[ -d $f ]];
            then
                add_pkg "$f"
            else
                print_debug "Skipping. $f is not a directory"
            fi
        done
    else
        package_path=$PKG_PATH
        if [[ -d $package_path ]];
        then
            add_newest_versions $package_path
        else
            print_error "Could not find $package_path"
        fi
    fi

    print_info "Adding Apollo"
    if [[ -d "../../Apollo/Package" ]];
    then
        echo -recurse:"../../Apollo/Package/*.cs"       >> $RSPPATH
    else
        print_warning "No such directory. Skipping"
    fi

    print_info "Adding local packages"
    echo -recurse:"*.cs"                                >> $RSPPATH

}

# check if rsp file exists
# if not, then build it
if [ ! -e $RSPPATH ]
then
    print_info "Could not find file $RSPPATH"
    build_rsp
fi
