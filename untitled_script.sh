#!/bin/bash

while IFS= read -r line; do
    row1=""
    row2=""
    for (( i=0; i<${#line}; i++ )); do
        char="${line:i:1}"
        row1+="$char$char"
        row2+="$char$char"
    done
    echo "$row1"
    echo "$row2"
done

