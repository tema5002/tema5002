#pragma once

#include <stdlib.h>
#include <string.h>
#include <dirent.h>

typedef struct cstring_node {
    char* name;
    struct cstring_node* next;
} cstring_node;

static inline void insert_command(cstring_node** head, const char* command) {
    for (cstring_node* curr = *head; curr != NULL; curr = curr->next) {
        if (strcmp(curr->name, command) == 0) return;
    }

    cstring_node* node = (cstring_node*)malloc(sizeof(cstring_node));
    if (!node) return;

    node->name = strdup(command);
    if (!node->name) {
        free(node);
        return;
    }

    node->next = *head;
    *head = node;
}

static inline void free_commands(cstring_node* head) {
    while (head) {
        cstring_node* next = head->next;
        free(head->name);
        free(head);
        head = next;
    }
}

static inline cstring_node* get_path_commands() {
    char* path_env = getenv("PATH");
    if (!path_env) return NULL;

    cstring_node* head = NULL;
    char* path_copy = strdup(path_env);

    for (char* dir = strtok(path_copy, ":"); dir != NULL; dir = strtok(NULL, ":")) {
        DIR* d = opendir(dir);
        if (!d) continue;

        struct dirent* entry;
        while ((entry = readdir(d)) != NULL) {
            if (strcmp(entry->d_name, ".") == 0 || strcmp(entry->d_name, "..") == 0) continue;
            insert_command(&head, entry->d_name);
        }
        closedir(d);
    }

    free(path_copy);
    return head;
}
