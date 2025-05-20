#pragma once

#include <stdlib.h>
#include <string.h>

#ifndef min
#define min(a, b) ((a) < (b) ? (a) : (b))
#endif

#ifndef min3
#define min3(a, b, c) (min(min(a, b), c))
#endif

static inline int levenshtein(const char* s1, const char* s2) {
    int len1 = strlen(s1);
    int len2 = strlen(s2);

    int* row = malloc((len2 + 1) * sizeof(int));
    if (!row) return -1;

    for (int j = 0; j <= len2; j++)
        row[j] = j;

    for (int i = 1; i <= len1; i++) {
        int prev_diag = row[0];
        row[0] = i;
        for (int j = 1; j <= len2; j++) {
            int temp = row[j];
            row[j] = min3(
                row[j] + 1,                      // deletion
                row[j-1] + 1,                    // insertion
                prev_diag + (s1[i-1] != s2[j-1]) // substitution
            );
            prev_diag = temp;
        }
    }

    int result = row[len2];
    free(row);
    return result;
}
