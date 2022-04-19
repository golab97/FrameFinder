// See https://aka.ms/new-console-template for more information
using FrameFinder;
using static FrameFinder.SequenceItem;

Console.WriteLine("Hello, World!");
var dat = DateTime.Now;

Console.WriteLine(dat.ToString("O"));

byte[] frame = { 0xa5, 0x55, 0x10, 0xaa, 10, 0xfa, 0xfa };

Finder? finder = new FinderBuilder()
    .AddPrefix(frame.ToList().Take(4).ToArray())
    .AddSuffix(frame.ToList().TakeLast(3).ToArray())
    .Build(FinderBuilder.BuilderOptions.PrefixAndSuffix);

if (finder is null)
    return;

finder.FrameFound += (s, e) =>
 {
     var frame = finder.GetFrame();
     if (frame is null)
         return;

     global::System.Console.WriteLine(frame.ToString(""));
 };
;

List<byte> bytesToFeed = new();

Random rng = new();
int randomFrameCount = rng.Next(0,byte.MaxValue);
for (int i = 0; i < randomFrameCount; i++)
{
    int randomBytesBetween=rng.Next(0,byte.MaxValue);

    for (int ii = 0; ii < randomBytesBetween; ii++)
    {
        bytesToFeed.Add((byte)rng.Next(0, byte.MaxValue));
    }

    var randomFrame = RandomThings.Get_PrefixAndSuffixFrame(frame.ToList().Take(4).ToArray(), frame.ToList().TakeLast(3).ToArray(), (uint)rng.Next(0, byte.MaxValue));

    bytesToFeed.AddRange(randomFrame.ToArray());

}

finder.FeedMe(bytesToFeed);
finder.RunFinderTask();

while (true)
{
    var readed = Console.ReadLine();
    ;
}
