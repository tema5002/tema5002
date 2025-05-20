#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <limits.h>
#include "c_headers/levenshtein.h"
#include "c_headers/cstring_node.h"
#include "c_headers/exit_error.h"

int main(int argc, char** argv) {
    if (argc != 2) exit_error("Usage: %s <command>\n", argv[0]);

    cstring_node* commands = get_path_commands();
    if (!commands) exit_error("No commands found.\n");

    char* closest = NULL;
    int min_distance = INT_MAX;
    
    for (cstring_node* curr = commands; curr; curr = curr->next) {
        int distance = levenshtein(argv[1], curr->name);
        if (distance < min_distance) {
            min_distance = distance;
            free(closest);
            closest = strdup(curr->name);
        }
    }

    printf("Did you mean '%s'?\n", closest);

    free(closest);
    free_commands(commands);
    return 0;
}
