using System.Collections.Generic;

namespace Chroma
{
    public class ChromaAnimation
    {
        public SortedDictionary<int, ChromaFrame> States;

        public ChromaAnimation()
        {
            States = new SortedDictionary<int, ChromaFrame>();
        }
    }
}