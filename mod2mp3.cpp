/* not sure will this run on windows
 *
 * you can actually use any audio format not just mp3 i had no idea how to call it
 *
 * compiles with:
 * g++ mod2mp3.cpp -o mod2mp3 -O3 -lopenmpt
 *
 * usage: ./mod2mp3 <input.(xm/mod/stm/s3m/it/etc.)> <output.(mp3/wav/ogg/etc.)>
 */

#include <bits/stdc++.h>
#include <libopenmpt/libopenmpt.hpp>

#define BUFFER_SIZE 480
#define SAMPLE_RATE 48000

int main(int argc, char* argv[]) {
    if (argc < 3) {
        std::cout << "nouuuuuuuuu\n";
        return 1;
    }
    std::ifstream file(argv[1], std::ios::binary);
    openmpt::module mod(file);

    std::string command = "ffmpeg -f s16le -ar " + std::to_string(SAMPLE_RATE) + " -ac 2 -i pipe: ";
    for (char* c = argv[2]; *c; c++) {
        if (*c == ' ') command += '\\';
        command += *c;
    }
    std::cout << command << '\n';
    FILE* pipe = popen(command.c_str(), "w");
    if (!pipe) {
        std::cerr << "Error: failed to open pipe\n";
        return 1;
    }

    mod.set_render_param(openmpt::module::render_param::RENDER_INTERPOLATIONFILTER_LENGTH, 1); // no interpolation
    mod.set_render_param(openmpt::module::render_param::RENDER_VOLUMERAMPING_STRENGTH, 0); // no volume ramping

    try {
        int16_t interleaved_buffer[BUFFER_SIZE * 2];
        while (true) {
            int16_t pcm_buffer_right[BUFFER_SIZE];
            int16_t pcm_buffer_left[BUFFER_SIZE];
            std::size_t count = mod.read(SAMPLE_RATE, BUFFER_SIZE, pcm_buffer_left, pcm_buffer_right);
            if (count == 0) break;
            for (size_t i = 0; i < count; i++) {
                interleaved_buffer[2 * i] = pcm_buffer_left[i];
                interleaved_buffer[2 * i + 1] = pcm_buffer_right[i];
            }
            fwrite(interleaved_buffer, sizeof(int16_t), count * 2, pipe);
        }
        pclose(pipe);
    }
    catch (const std::exception& e) {
        std::cerr << "Error: " << std::string(e.what() ? e.what() : "unknown error") << '\n';
        return 1;
    }

    return 0;
}
