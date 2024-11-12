/*
 * requires ffmpeg
 *
 * compiles with:
 * gcc audiosort.c -o audiosort -O3 -Wall -Wextra -Werror -Wno-unused-result -lm
 *
 * usage:
 * ./audiosort <input_file> <output_file>
 */

#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <tgmath.h>

inline size_t escapedstrlen(const char* s) {
    size_t len = 0;
    for (; *s; s++, len += 1 + (*s == ' ')) {}
    return len;
}

FILE* get_input_pipe(const char* const filename) {
    char command[escapedstrlen(filename) + 38];
    strncpy(command, "ffmpeg -i ", 11);

    char* ptr = command + 10;

    for (const char* c = filename; *c; c++) {
        if (*c == ' ') *ptr++ = '\\';
        *ptr++ = *c;
    }

    strncpy(ptr, " -f s16le -ar 48000 -ac 2 -", 28);
    return popen(command, "r");
}

FILE* get_output_pipe(const char* const filename) {
    char command[escapedstrlen(filename) + 43];
    strncpy(command, "ffmpeg -f s16le -ar 48k -ac 2 -i pipe: -y ", 43);

    char* ptr = command + 42;

    for (const char* c = filename; *c; c++) {
        if (*c == ' ') *ptr++ = '\\';
        *ptr++ = *c;
    }

    *ptr = '\0';
    return popen(command, "w");
}

long filesize(const char* const file_name) {
    FILE* f = fopen(file_name, "r");
    if (f == NULL) exit(1);

    fseek(f, 0L, SEEK_END);
    const long res = ftell(f);
    fclose(f);

    return res;
}

#define BUFFER_SIZE 1200
typedef short sample_t;

inline void* safe_malloc(const size_t bytes) {
    void* ptr = malloc(bytes);
    if (ptr == NULL) exit(3);
    return ptr;
}

int sample_sorter(const void* const a, const void* const b) {
    int sum = 0;
    for (int i = 0; i < BUFFER_SIZE; i++) {
        sum -= abs(*(sample_t*)a);
        sum += abs(*(sample_t*)b);
    }
    return sum;
}

int main(const int argc, const char* argv[]) {
    if (argc < 3) {
        fprintf(stderr, "Usage: %s <input_file> <output_file>\n", argv[0]);
        return 1;
    }
    const int pcm_data_size = ceil((float)filesize(argv[1]) / (BUFFER_SIZE * sizeof(sample_t)));
    FILE* input = get_input_pipe(argv[1]);
    if (input == NULL) exit(1);

    sample_t** pcm = safe_malloc(sizeof(sample_t*) * pcm_data_size);
    for (int i = 0; i < pcm_data_size; i++) {
        pcm[i] = safe_malloc(sizeof(sample_t) * BUFFER_SIZE);
        fread(pcm[i], sizeof(sample_t), BUFFER_SIZE, input);
    }
    pclose(input);

    qsort(pcm, pcm_data_size, sizeof(sample_t*), sample_sorter);

    FILE* output = get_output_pipe(argv[2]);
    if (output == NULL) exit(2);
    for (int i = 0; i < pcm_data_size; i++) {
        fwrite(pcm[i], sizeof(sample_t), BUFFER_SIZE, output);
        free(pcm[i]);
    }
    pclose(output);
    free(pcm);
    return 0;
}
