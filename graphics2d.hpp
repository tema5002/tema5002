#pragma once

#include <SDL2/SDL.h>

struct RGBColor {
    uint8_t r = 255;
    uint8_t g = 255;
    uint8_t b = 255;
};

class Window {
    SDL_Window* window;
    SDL_Renderer* renderer;
public:
    Window(const char* name, const int w, const int h) {
        if (SDL_Init(SDL_INIT_EVERYTHING) < 0 ||
            !((window = SDL_CreateWindow(name, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, w, h, SDL_WINDOW_SHOWN))) ||
            !((renderer = SDL_CreateRenderer(window, -1, 0)))
        ) exit(1);
    }

    void scale(const float x, const float y) const {
        SDL_RenderSetScale(renderer, x, y);
    }

    void close() const {
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
        exit(0);
    }

    void rect(const int x, const int y, const int w, const int h, const RGBColor c) const {
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, 255);
        SDL_Rect rect_ = {x, y, w, h};
        SDL_RenderFillRect(renderer, &rect_);
    }

    void pixel(const int x, const int y, const RGBColor c) const {
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, 255);
        SDL_RenderDrawPoint(renderer, x, y);
    }

    void circle(const int x, const int y, const int radius, const RGBColor c) const {
        for (int i = 0; i < radius*2; i++) {
            for (int j = 0; j < radius*2; j++) {
                int dx = i - radius;
                int dy = j - radius;
                int distance = dx * dx + dy * dy;

                if ((radius - 1) * (radius - 1) <= distance && distance <= radius * radius) {
                    pixel(x - radius + i, y - radius + j, c);
                }
            }
        }
    }

    void line(const int x1, const int y1, const int x2, const int y2, const RGBColor c) const {
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, 255);
        SDL_RenderDrawLine(renderer, x1, y1, x2, y2);
    }

    void rect_outline(const int x1, const int y1, const int w, const int h, const RGBColor c) const {
        line(x1,     y1,     x1 + w, y1,     c);
        line(x1 + w, y1,     x1 + w, y1 + h, c);
        line(x1 + w, y1 + h, x1,     y1 + h, c);
        line(x1,     y1 + h, x1,     y1,     c);
    }

    void input() const {
        SDL_Event e;
        while (SDL_PollEvent(&e)) {
            if (e.type == SDL_QUIT) {
                this->close();
            }
        }
    }

    void render(const unsigned int ms) const {
        SDL_RenderPresent(renderer);
        SDL_Delay(ms);
    }

    void clear(const RGBColor c) const {
        SDL_SetRenderDrawColor(renderer, c.r, c.g, c.b, 255);
        SDL_RenderClear(renderer);
    }
};

/*
old line() code because i dont want to lose it:

int dx = x2 - x1;
int dy = y2 - y1;
float length = sqrt(dx * dx + dy * dy);

for (int i = 0; i <= length; i++) {
    int x = x1;
    int y = y1;
    if (length > 0) {
        x += i * dx / length;
        y += i * dy / length;
    }
    pixel(x, y, c);
}
*/
