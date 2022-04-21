using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using static FrameFinder.SequenceItem;
namespace FrameFinder.Tests
{
    public class FinderBuilderTests
    {
        [Fact]
        public void Build_NoSequenceItems_ReturnsNull()
        {

        }
        [Fact]
        public void Finder_SuffixAndPredix_RandomCount()
        {
            byte[] frame = { 0xa5, 0x55, 0x10, 0xaa, 10, 0xfa, 0xfa };

            Finder? finder = new FinderBuilder()
                .AddPrefix(frame.ToList().Take(4).ToArray())
                .AddSuffix(frame.ToList().TakeLast(3).ToArray())
                .Build(FinderBuilder.BuilderOptions.PrefixAndSuffix);

            if (finder is null)
                return;


            List<byte> bytesToFeed = new();

            Random rng = new();
            int randomFrameCount = rng.Next(0, byte.MaxValue);
            for (int i = 0; i < randomFrameCount; i++)
            {
                int randomBytesBetween = rng.Next(0, byte.MaxValue);

                for (int ii = 0; ii < randomBytesBetween; ii++)
                {
                    bytesToFeed.Add((byte)rng.Next(0, byte.MaxValue));
                }

                var randomFrame = RandomThings.Get_PrefixAndSuffixFrame(frame.ToList().Take(4).ToArray(), frame.ToList().TakeLast(3).ToArray(), (uint)rng.Next(0, byte.MaxValue));

                bytesToFeed.AddRange(randomFrame.ToArray());

            }

            finder.FeedMe(bytesToFeed);
            DateTime now= DateTime.Now;
            finder.RunFinderTask();

            while (finder.NoDataInQueue ==false)
            {

            }
            ;
            Trace.WriteLine($"test time: {DateTime.Now - now}, scanned bytes: {bytesToFeed.Count}, found frames/expected:{finder.FramesCount}/{randomFrameCount}");
            Assert.True(finder.FramesCount == randomFrameCount);
        }
        [Fact]
        public void Finder_PrefixAndDynamicLen_RandomCount()
        {


            byte[] frame = { 10, 50, 2, 0xa5, 0x55, 0x10, 0xaa, 0x10, 1, 4, 0, 0, 1, 2, 3, 0xfa, 0xfa ,
                10, 50, 2, 0xa5, 0x50, 0x10, 0xaa, 0x10, 1, 4, 0, 0, 1, 2, 3, 0xfa, 0xfa,
                10, 50, 2, 0xa5, 0x55, 0x10, 0xaa, 0x10, 1, 5, 0, 0, 1, 2, 3,4, 0xfa, 0xfa ,
            };

            var finder = new FinderBuilder()
                .AddPrefix(frame.ToList().Skip(3).Take(4))
                .AddDataField("Sender", 1)
                .AddDataField("Receiver", 1)
                .AddDynamicDataLenght(2)
                .AddDataField("Crc16", 2)
                .Build(FinderBuilder.BuilderOptions.PrefixAndDynamicLenght);



          

            if (finder is null)
                return;

            finder.FeedMe(frame);
            DateTime now = DateTime.Now;
            finder.RunFinderTask();

            while (finder.NoDataInQueue==false)
            {

            }

            Trace.WriteLine($"test time: {DateTime.Now - now}, scanned bytes: {frame.Length}, found frames/expected:{finder.FramesCount}/{2}");
            Assert.True(finder.FramesCount==2);
        }
    }
}