using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrameFinder.FinderBuilder;

namespace FrameFinder
{
    public class Finder
    {
        readonly Queue<byte> _rawDataQueue;
        public int QueueCount => _rawDataQueue.Count;
        private readonly List<SequenceItem> _sequenceItems = new();
        public List<SequenceItem> SequenceItems => _sequenceItems;
        CancellationTokenSource _finderTaskCts = new();
        public bool IsSearching { get; private set; } = false;
        private int _itemToFind_Index = 0;
        private int _dataLenght = 0;
        private BuilderOptions _options = 0;
        public bool FrameReady { get; private set; } = false;
        public bool NoDataInQueue { get; private set; } = true;
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
            NoDataInQueue = false;
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
                    DateTime now = DateTime.Now;
                if (_rawDataQueue.Count > 0)
                {
                    Console.WriteLine("I got a data to proccess");
                    while (_rawDataQueue.Any())
                    {
                        byte bajcik = _rawDataQueue.Dequeue();
                        //Console.WriteLine(bajcik);
                        if(_options== BuilderOptions.PrefixAndDynamicLenght)
                        {
                            Analize2(bajcik);
                        }
                        else if(_options== BuilderOptions.PrefixAndSuffix)
                        {
                            PrefixAndSuffixCase(bajcik);
                        }
                    }
                    NoDataInQueue = true;
                    var dif=DateTime.Now - now;

                    Trace.WriteLine(dif.ToString());

                }
            }
        }


                    
              

            
        
        private SequenceItem? _currentItem = null;
        private uint _currentItemIndex = 0;

        private void Analize2(byte bajcik)
        {
            if (_currentItem is null)
            {
                if (_currentItemIndex > _sequenceItems.Count - 1)
                {
                    _currentItemIndex = 0;
                    _readyFrames.Add(new Frame(DateTime.Now, false, _readedSequenceBytes));
                    _readedSequenceBytes.Clear();
                }

                _currentItem = _sequenceItems[(int)_currentItemIndex];
            }

            _finderBuff.Add(bajcik);


            switch (_currentItem.ItemRole)
            {
                case SequenceItem.InSequenceRole.Prefix:
                    if (_finderBuff.Count < _currentItem.Size)
                    {
                        break;
                    }
                    if (_finderBuff.Count > _currentItem.Size)
                    {
                        throw new FinderException("Impossible but finderbuff count larger than item size.");
                    }
                    ;

                    var isEqualPrefix = CheckArrayElementsEquals(_currentItem.Value, _finderBuff.ToArray());

                    if (!isEqualPrefix)
                    {
                        _finderBuff.RemoveAt(0);
                        break;
                    }
                    _readedSequenceBytes.AddRange(_finderBuff);
                    _finderBuff.Clear();
                    _currentItemIndex++;
                    _currentItem = null;
                    break;

                case SequenceItem.InSequenceRole.Data:
                    ;
                    if (_finderBuff.Count == _currentItem.Size)
                    {
                        _readedSequenceBytes.AddRange(_finderBuff);
                        _finderBuff.Clear();
                        _currentItemIndex++;
                        _currentItem = null;
                    }

                    break;
                case SequenceItem.InSequenceRole.DataLenght:
                    ;
                    if (_finderBuff.Count == _currentItem.Size)
                    {
                        _readedSequenceBytes.AddRange(_finderBuff);
                        //_currentItemIndex++;
                        uint len = _currentItem.Size switch
                        {
                            0 => 0,
                            1 => _finderBuff.First(),
                            2 => BitConverter.ToUInt16(_finderBuff.Take(2).ToArray()),
                            3 => GetUint32ValueFrom3bytes(_finderBuff),
                            4 => BitConverter.ToUInt16(_finderBuff.Take(4).ToArray()),
                            _ => throw new NotImplementedException("Finder DataLenght Item can't have more than 4 bytes size")

                        };

                        _finderBuff.Clear();
                        _currentItem = new SequenceItem(
                            "",
                            SequenceItem.SequenceItemType.Data,
                            SequenceItem.InSequenceRole.Data,
                            null,
                            len);
                    }
                    break;

                case SequenceItem.InSequenceRole.Suffix:

                    if (_finderBuff.Count < _currentItem.Size)
                    {
                        break;
                    }
                    if (_finderBuff.Count > _currentItem.Size)
                    {
                        throw new FinderException("Impossible but finderbuff count larger than item size.");
                    }
                    ;

                    var isEqualSuffix = CheckArrayElementsEquals(_currentItem.Value, _finderBuff.ToArray());

                    if (!isEqualSuffix)
                    {
                        _finderBuff.RemoveAt(0);
                        break;
                    }
                    _readedSequenceBytes.AddRange(_finderBuff);
                    _finderBuff.Clear();
                    _currentItemIndex++;
                    _currentItem = null;

                    break;

            }
             
            if (_currentItem is null)
            {
                if (_currentItemIndex > _sequenceItems.Count - 1)
                {
                    _currentItemIndex = 0;
                    _readyFrames.Add(new Frame(DateTime.Now, false, _readedSequenceBytes));
                    _readedSequenceBytes.Clear();
                }
            }

        }
        uint GetUint32ValueFrom3bytes(IEnumerable<byte> list)
        {
            var list2 = new List<byte>(list);
            list2.Add(0);
            return (uint)BitConverter.ToUInt32(_finderBuff.Take(4).ToArray());
        }
        public List<byte> _finderBuff = new();

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
                    var isEqualPrefix = CheckArrayElementsEquals(_sequenceItems[0].Value, _finderBuff.ToArray());

                    if (!isEqualPrefix)
                    {
                        _finderBuff.RemoveAt(0);
                        break;
                    }
                    ;
                    _readedSequenceBytes = new(_finderBuff);
                    _finderBuff.Clear();
                    _itemToFind_Index = 1;
                    break;
                case 1:
                    var isEqualSuffix = CheckArrayElementsEquals(_sequenceItems[1].Value, _finderBuff.ToArray());

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
                    FrameFound?.Invoke(this, new());
                    break;
            }
        }
        public Frame? GetFrame()
        {
            for (int i = 0; i < _readyFrames.Count; i++)
            {
                if (!_readyFrames[i].Readed)
                {
                    _readyFrames[i].Readed = true;
                    return _readyFrames[i];
                }
            }
            return null;
        }

        public void StopFinderTask()
        {
            _finderTaskCts.Cancel();
            IsSearching = false;
        }

        private static bool CheckArrayElementsEquals(byte[] a1, byte[] a2)
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
        public class FinderException : Exception
        {
            public override string Message { get; } = "";
            public FinderException(string message)
            {
                this.Message = message;
            }
        }
        public class Frame
        {
            public DateTime WhenFound { get; set; }
            public bool Readed { get; set; }
            public List<byte> Bytes { get; set; }

            public Frame(DateTime time, bool readed, List<byte> bytes)
            {
                this.WhenFound = time;
                this.Readed = readed;
                this.Bytes = new(bytes);
            }
            public string ToString(string? format = null)
            {
                if (format is null)
                    return "Finder.Frame";

                return $"Found: {WhenFound}, is readed: {Readed}, data count: {Bytes.Count}.";
            }

        }


    }
}
