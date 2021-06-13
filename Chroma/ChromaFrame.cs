using System.Collections.Generic;

namespace Chroma
{
    public class ChromaFrame
    {
        public int Loop = -1;
        public int FramesPerSecond = -1;
        public List<string> Frames;

        public ChromaFrame()
        {
            this.Frames = new List<string>();
        }
    }
}