#!/bin/bash

# https://pico-8.fandom.com/wiki/Palette
colors=(
    "0 0 0"
    "29 43 83"
    "126 37 83"
    "0 135 81"
    "171 82 54"
    "95 87 79"
    "194 195 199"
    "255 241 232"
    "255 0 77"
    "255 163 0"
    "255 236 39"
    "0 228 54"
    "41 173 255"
    "131 118 156"
    "255 119 168"
    "255 204 170"
)

placeholders=("PK" "  " "  " "  " "  " "  " "PW" "  " "PR" "PO" "PY" "PG" "PB" "PS" "PM" "PE")


printf "\n%s\n\n" "PICO-8 сolor palette"

for i in "${!colors[@]}"; do
    IFS=' ' read -r r g b <<< "${colors[i]}"

    printf "#%02X%02X%02X " "$r" "$g" "$b"
    printf "\e[38;2;%d;%d;%dm" "$r" "$g" "$b"
    printf "████████████████"
    printf "\e[0m"
    printf " %-2s\n" "${placeholders[i]}"
done
