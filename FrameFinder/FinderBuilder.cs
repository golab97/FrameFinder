using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static FrameFinder.SequenceItem;
namespace FrameFinder
{
    public class FinderBuilder
    {
        readonly List<SequenceItem> _sequenceItems = new();
        public bool PrefixExist { get; private set; } = false;
        public bool SuffixExist { get; private set; } = false;
        public FinderBuilder()
        {

        }

        public FinderBuilder AddPrefix(byte[] value)
        {
            //if (!IsTypeSupported(value))
            //    return this;

            if (PrefixExist)
                return this;


            var item = new SequenceItem("Prefix",
                    SequenceItemType.Constant,
                    InSequenceRole.Prefix,
                    value);

            _sequenceItems.Add(item);

            if (_sequenceItems.Find(x => x.ItemRole == InSequenceRole.Prefix) is { }) PrefixExist = true;

            return this;
        }
        public FinderBuilder AddSuffix(byte[] value)
        {
            if (SuffixExist)
                return this;

            //var sizee = Marshal.SizeOf(value);

            var item = new SequenceItem("Suffix",
                    SequenceItemType.Constant,
                    InSequenceRole.Suffix,
                    value);

            _sequenceItems.Add(item);

            if (_sequenceItems.Find(x => x.ItemRole == InSequenceRole.Prefix) is { }) SuffixExist = true;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableSizeInByte">
        /// Size of lenght variable. ex.: 1= byte, 2 = uint16 data len, 4  = uint64 
        /// </param>
        /// <returns></returns>
        public FinderBuilder AddDynamicDataLenght(uint variableSizeInByte)
        {

            var item = new SequenceItem("DataLenghtItem",
                   SequenceItemType.Variable,
                   InSequenceRole.DataLenght);

            return this;
        }

        private static bool IsTypeSupported(object value)
        {
            TypeCode code = Type.GetTypeCode(value.GetType());

            if (!SupportedTypes.Contains(code))
                return false;
            //throw new ArgumentException("Value type is not supported.");

            return true;
        }
        public Finder? Build(BuilderOptions options)
        {
            if (!_sequenceItems.Any())
                //    return null;
                throw new IndexOutOfRangeException("Builder sequence contains no elements");

            switch (options)
            {
                case BuilderOptions.PrefixAndSuffix:
                    if (!PrefixExist)
                    {
                        throw new FinderBuilderException("Sequence contains no prefix.");
                    }
                    if (!SuffixExist)
                    {
                        throw new FinderBuilderException("Sequence contains no suffix.");
                    }
                    SequenceItem item = new ("Data",
                        SequenceItemType.Data,
                        InSequenceRole.Data);
                    if(_sequenceItems.Count!=2)
                    {
                        throw new FinderBuilderException("Sequence contains more items than preffix and suffix.");
                    }
                    _sequenceItems.Insert(1,item);
                    break;

                case BuilderOptions.PrefixAndFixedLenght:
                    throw new NotImplementedException();

                case BuilderOptions.PrefixAndDynamicLenght:
                    throw new NotImplementedException();
            }


            return new Finder(_sequenceItems,options);
        }
        public enum BuilderOptions
        {
            PrefixAndSuffix,
            PrefixAndFixedLenght,
            PrefixAndDynamicLenght
        }

        public class FinderBuilderException : Exception
        {
            public override string Message { get; } = "";
            public FinderBuilderException(string message)
            {
                this.Message = message;
            }
        }
    }

}
