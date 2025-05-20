#define _GNU_SOURCE
#include <stdio.h>
#include <signal.h>
#include <string.h>
#include <unistd.h>

void signal_handler(int sig) {
    printf("Caught signal SIG%s (%d) - %s\n", sigabbrev_np(sig), sig, strsignal(sig));
    fflush(stdout);
}

int main() {
    for (int i = 1; i < NSIG; signal(i++, signal_handler));
    puts("Listening for signals");
    while (pause());
}
