#!/bin/sh

BUILD_CONFIG=$1
BUILD_PROJ_DIR=$2
BUILD_TARGET_DIR=$3

if [ -z "$1" ]; then
    BUILD_CONFIG=Release
    BUILD_PROJ_DIR=/Users/midiway/Documents/IconDrop/IconDrop
    BUILD_TARGET_DIR=/Users/midiway/Documents/IconDrop/IconDrop/bin/Release
fi

CONTENTS_DIR=$BUILD_TARGET_DIR/IconDrop.app/Contents
SHARED_DIR=$BUILD_TARGET_DIR/IconDrop.app/Contents/Shared

find SHARED_DIR -name '.DS_Store' -type f -delete

mkdir -p $CONTENTS_DIR
#if [ ! -d "$SHARED_DIR" ]; then
    cp -r $BUILD_PROJ_DIR/Shared $CONTENTS_DIR
#fi

# IN OSX, COMPRESS FILES IN /res FOLDER TO ArchiveResource.cs C# FILE
if [ "$BUILD_CONFIG" == "Release" ]; then
    cd "$(dirname "$0")"
    chmod +x packfolder
    ./packfolder ../res ../ArchiveResource.cs -csharp -x "*IconBundler*;*sciter.dll;.DS_store"
fi