package com.quackster.chroma;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a frame in a Chroma animation sequence
 */
public class ChromaFrame {
    private int loop = -1;
    private int framesPerSecond = -1;
    private List<String> frames;

    public ChromaFrame() {
        this.frames = new ArrayList<>();
    }

    public int getLoop() {
        return loop;
    }

    public void setLoop(int loop) {
        this.loop = loop;
    }

    public int getFramesPerSecond() {
        return framesPerSecond;
    }

    public void setFramesPerSecond(int framesPerSecond) {
        this.framesPerSecond = framesPerSecond;
    }

    public List<String> getFrames() {
        return frames;
    }

    public void setFrames(List<String> frames) {
        this.frames = frames;
    }
}
