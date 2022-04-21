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
        public bool DateLenghtExist { get; private set; } = false;
        public FinderBuilder()
        {

        }

        public FinderBuilder AddPrefix(IEnumerable<byte> value)
        {
            //if (!IsTypeSupported(value))
            //    return this;

            if (PrefixExist)
                return this;


            var item = new SequenceItem("Prefix",
                    SequenceItemType.Constant,
                    InSequenceRole.Prefix,
                    value.ToArray());

            _sequenceItems.Add(item);

            if (_sequenceItems.Where(x => x.ItemRole == InSequenceRole.Prefix).Count() == 1) PrefixExist = true;

            return this;
        }
        public FinderBuilder AddSuffix(IEnumerable<byte> value)
        {
            if (SuffixExist)
                return this;

            //var sizee = Marshal.SizeOf(value);

            var item = new SequenceItem("Suffix",
                    SequenceItemType.Constant,
                    InSequenceRole.Suffix,
                    value.ToArray());

            _sequenceItems.Add(item);

            if (_sequenceItems.Where(x => x.ItemRole == InSequenceRole.Suffix).Count()==1) SuffixExist = true;

            return this;
        }

        public FinderBuilder AddDataField(string fieldName, uint variableSizeInByte)
        {

            if (_sequenceItems.Where(x => x.Name == fieldName).Any())
                throw new FinderBuilderException($"Sequence contains {fieldName} field.");

            var item = new SequenceItem(fieldName,
                   SequenceItemType.Data,
                   InSequenceRole.Data,
                   null,
                   variableSizeInByte);

            _sequenceItems.Add(item);

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
                   InSequenceRole.DataLenght,
                   null,
                   variableSizeInByte);

            _sequenceItems.Add(item);

            if (_sequenceItems.Find(x => x.ItemRole == InSequenceRole.DataLenght) is { })
                DateLenghtExist =true;

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
                    //SequenceItem item = new ("Data",
                    //    SequenceItemType.Data,
                    //    InSequenceRole.Data);
                    //if(_sequenceItems.Count!=2)
                    //{
                    //    throw new FinderBuilderException("Sequence contains more items than preffix and suffix.");
                    //}
                    //_sequenceItems.Insert(1,item);
                    break;

                case BuilderOptions.PrefixAndFixedLenght:
                    throw new NotImplementedException();

                case BuilderOptions.PrefixAndDynamicLenght:
                    if (!PrefixExist)
                    {
                        throw new FinderBuilderException("Sequence contains no prefix.");
                    }


                    break;

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
