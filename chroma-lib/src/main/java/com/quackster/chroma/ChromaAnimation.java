package com.quackster.chroma;

import java.util.TreeMap;

/**
 * Represents an animation for a Chroma furniture item
 */
public class ChromaAnimation {
    private TreeMap<Integer, ChromaFrame> states;

    public ChromaAnimation() {
        this.states = new TreeMap<>();
    }

    public TreeMap<Integer, ChromaFrame> getStates() {
        return states;
    }

    public void setStates(TreeMap<Integer, ChromaFrame> states) {
        this.states = states;
    }
}
