#pragma once

#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>

_Noreturn static inline void exit_error(const char* format, ...) {
    va_list args;
    va_start(args, format);
    vfprintf(stderr, format, args);
    va_end(args);
    exit(1);
}
