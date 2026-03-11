#!/bin/bash

function parseSetCommand(){
    local result=$(cat "$1" | grep "$2" | sed 's/[^0-9]*//g')
    echo $result
}

# Get to the root folder
pathScript=$(dirname "$0")
cd $pathScript/..

# Create a list of files to archive
listArchive=$(ls -a --ignore=. --ignore=.. | grep -Ev "archive|.vs|help|x64")

# Create an archive
dirArchive="archive"
tempNameArchive="tempArchive.zip"
echo $listArchive | xargs zip -r "$dirArchive/$tempNameArchive" 

# Set the archive name
cd $dirArchive
nameArchive="$(date +"%Y.%m.%d").zip"
if [[ ! -f $nameArchive ]]; then
    mv $tempNameArchive "$nameArchive"
    echo "$nameArchive was successfully created"
else
    echo "$nameArchive already exists. The name of the created archive is $tempNameArchive"
fi