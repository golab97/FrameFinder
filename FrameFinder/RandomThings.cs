using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameFinder;
namespace FrameFinder
{
    public class RandomThings
    {
        public static List<byte> Get_PrefixAndSuffixFrame(byte [] prefix,byte[] suffix, uint dataCount=99)
        {
            List<byte> result = new();
            result.AddRange(prefix);

            Random rng = new();

            for (int i = 0; i < dataCount; i++)
            {
                result.Add((byte)rng.Next(0, 255));
            };
            result.AddRange(suffix);
            return result;
        }
    }
}
