#!/bin/bash

while IFS= read -r line; do
    row=""
    for (( i=0; i<${#line}; i++ )); do
        char="${line:i:1}"
        row+="$char$char"
    done
    echo "$row"
    echo "$row"
done
