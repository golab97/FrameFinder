using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameFinder
{
    public class Finder
    {
        readonly Queue<byte> _rawDataQueue;

        public Finder()
        {
            _rawDataQueue = new();
        }
        public void AddToQueue(byte data)
        {
            _rawDataQueue.Enqueue(data);
        }
        ~Finder() 
        {
            _rawDataQueue.Clear();
        }
    }
}
