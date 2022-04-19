using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrameFinder.FinderBuilder;

namespace FrameFinder
{
    public class Finder
    {
        readonly Queue<byte> _rawDataQueue;
        public int QueueCount =>_rawDataQueue.Count;
        private readonly List<SequenceItem> _sequenceItems = new();
        public List<SequenceItem> SequenceItems => _sequenceItems;
        CancellationTokenSource _finderTaskCts = new();
        public bool IsSearching { get; private set; } = false;
        private int _itemToFind_Index = 0;
        private BuilderOptions _options = 0;
        public bool FrameReady { get; private set; } = false;
        private List<byte> _readedSequenceBytes = new();
        private List<Frame> _readyFrames = new();
        public int FramesCount => _readyFrames.Count;
        // windows+; emoji
        public event EventHandler? FrameFound;
        ~Finder()
        {
            _rawDataQueue.Clear();
            StopFinderTask();
        }
        public Finder()
        {
            _rawDataQueue = new();
        }
        public Finder(List<SequenceItem> items, BuilderOptions options)
        {
            _rawDataQueue = new();
            _sequenceItems = new(items);
            _options = options;
        }



        public void FeedMe(byte data)
        {
            _rawDataQueue.Enqueue(data);
        }
        public void FeedMe(IEnumerable<byte> data)
        {
            data.ToList().ForEach(x => FeedMe(x));
        }



        public void RunFinderTask()
        {
            if (IsSearching)
                return;

            _finderTaskCts = new();
            IsSearching = true;
            Thread th = new(FinderTask);
            th.Start();

        }
        public void FinderTask()
        {
            while (!_finderTaskCts.IsCancellationRequested)
            {
                Task.Delay(1000).Wait();
                if (_rawDataQueue.Count > 0)
                {
                    Console.WriteLine("I got a data to proccess");

                    while (_rawDataQueue.Any())
                    {
                        byte bajcik = _rawDataQueue.Dequeue();
                        //Console.WriteLine(bajcik);
                        Analize(bajcik);
                    }

                }
            }
        }

        public void Analize(byte bajcik)
        {
            switch (_options)
            {
                case BuilderOptions.PrefixAndSuffix:
                    PrefixAndSuffixCase(bajcik);
                    break;
                case BuilderOptions.PrefixAndDynamicLenght:
                    PrefixAndDynamicLenght(bajcik);
                    break;

            }
        }
        public List<byte> _finderBuff = new();
        public void PrefixAndDynamicLenght(byte bajcik)
        {
            _finderBuff.Add(bajcik);

            if (_finderBuff.Count < _sequenceItems[_itemToFind_Index].Size)
            {
                return;
            }
            switch (_itemToFind_Index)
            {
                case 0:
                    var isEqualPrefix = CheckArrayElementsEquals(_sequenceItems[0].Value, _finderBuff.ToArray());

                    if (!isEqualPrefix)
                    {
                        _finderBuff.RemoveAt(0);
                        break;
                    }
                    ;
                    _readedSequenceBytes = new(_finderBuff);
                    _finderBuff.Clear();
                    _itemToFind_Index = 2;
                    break;
                case 1:
                    break;

            }
        }
        public void PrefixAndSuffixCase(byte bajcik)
        {
                _finderBuff.Add(bajcik);

            if (_finderBuff.Count < _sequenceItems[_itemToFind_Index].Size)
            {
                return;
            }

            switch (_itemToFind_Index)
            {
                case 0:
                    var isEqualPrefix= CheckArrayElementsEquals(_sequenceItems[0].Value, _finderBuff.ToArray());

                    if (!isEqualPrefix)
                    {
                        _finderBuff.RemoveAt(0);
                        break;
                    }
                    ;
                    _readedSequenceBytes = new(_finderBuff);
                    _finderBuff.Clear();
                    _itemToFind_Index = 2;
                    break;
                case 2:
                    var isEqualSuffix = CheckArrayElementsEquals(_sequenceItems[2].Value, _finderBuff.ToArray());

                    if (!isEqualSuffix)
                    {
                        _finderBuff.RemoveAt(0);
                        _readedSequenceBytes.Add(bajcik);
                        break;
                    }
                    ;
                    _readedSequenceBytes.AddRange(_finderBuff);
                    _finderBuff.Clear();
                    _itemToFind_Index = 0;
                    _readyFrames.Add(new(DateTime.Now, false, _readedSequenceBytes));
                    _readedSequenceBytes.Clear();
                    FrameFound?.Invoke(this, new ());
                    break;
            }
        }
        public Frame? GetFrame()
        {
            for (int i = 0; i < _readyFrames.Count; i++)
            {
                if(!_readyFrames[i].Readed)
                {
                    _readyFrames[i].Readed = true;
                    return _readyFrames[i];
                }
            }
            return  null;
        }

        public void StopFinderTask()
        {
            _finderTaskCts.Cancel();
            IsSearching = false;
        }

        private static bool CheckArrayElementsEquals(byte [] a1,byte [] a2)
        {
            if (a1.Length != a2.Length)
                return false;
            
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }

            return true;
        }

        public class Frame
        {
            public DateTime WhenFound { get; set; }
            public bool Readed { get; set; }
            public List<byte> Bytes { get; set; }

            public Frame(DateTime time,bool readed,List<byte> bytes)
            {
                this.WhenFound = time;
                this.Readed = readed;
                this.Bytes = new(bytes);
            }
            public string ToString(string? format=null)
            {
                if (format is null)
                    return "Finder.Frame";

                return $"Found: {WhenFound}, is readed: {Readed}, data count: {Bytes.Count}.";
            }

        }


    }
}
