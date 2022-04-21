// See https://aka.ms/new-console-template for more information
using FrameFinder;
using static FrameFinder.SequenceItem;

Console.WriteLine("Hello, World!");
var dat = DateTime.Now;

Console.WriteLine(dat.ToString("O"));



#region prefixandsuffix test


//byte[] frame = { 0xa5, 0x55, 0x10, 0xaa, 10 ,10, 0xfa, 0xfa };

//Finder? finder = new FinderBuilder()
//    .AddPrefix(frame.ToList().Take(4).ToArray())
//    .AddSuffix(frame.ToList().TakeLast(3).ToArray())
//    .Build(FinderBuilder.BuilderOptions.PrefixAndSuffix);

//if (finder is null)
//    return;


//finder.FeedMe(frame);
//finder.RunFinderTask();
#endregion


#region prefixAndDynamicLen test
byte[] frame = { 10, 50, 2, 0xa5, 0x55, 0x10, 0xaa, 0x10, 1, 4, 0, 0, 1, 2, 3, 0xfa, 0xfa };

var finder = new FinderBuilder()
    .AddPrefix(frame.ToList().Skip(3).Take(4))
    .AddDataField("Sender", 1)
    .AddDataField("Receiver", 1)
    .AddDynamicDataLenght(2)
    .AddDataField("Crc16", 2)
    .Build(FinderBuilder.BuilderOptions.PrefixAndDynamicLenght);

finder.FeedMe(frame);
finder.RunFinderTask();

#endregion
while (true)
{
    var readed = Console.ReadLine();
    ;
}
