using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FrameFinder
{
    public class SequenceItem
    {
        

        public static List<TypeCode> _supportedTypes = new();
       
        public int Size { get; private set; }
        public string Name { get; private set; } = "";
        public SequenceItemType ItemType { get; private set; }
        public InSequenceRole ItemRole { get; private set; }

        public byte[] Value = new byte[0];

        public SequenceItem(string name, SequenceItemType type, InSequenceRole role, byte[]? value=null)
        {
            this.Name = name;
            
            this.ItemType = type;
            this.ItemRole = role;

            if (this.ItemRole == InSequenceRole.Suffix
                || this.ItemRole == InSequenceRole.Prefix)
            {
                if (value is null)
                    throw new ArgumentNullException("Value is null. See inner exception", new ArgumentException("When type equals Prefix or Suffix value have to be an object"));

                this.Size = value.Length;
                this.Value= new byte[value.Length];
                Array.Copy(value, 0, this.Value, 0, value.Length);
            }
            else if(this.ItemRole== InSequenceRole.DataLenght)
            {

            }
            else if(this.ItemRole== InSequenceRole.Data)
            {

            }
        }

        public static List<TypeCode> UnsupportedTypes = new()
        {
            TypeCode.Object,
            TypeCode.Empty,
            TypeCode.Boolean,
            TypeCode.DateTime,
            TypeCode.DBNull
        };
        public static List<TypeCode> SupportedTypes
        {
            get
            {
                if (!_supportedTypes.Any())
                {
                    _supportedTypes = Enum.GetValues<TypeCode>().
                    Cast<TypeCode>().
                    ToList().SkipWhile(x =>
                    {
                        if (UnsupportedTypes.Contains(x))
                            return true;

                        return false;
                    }).ToList();
                }
                return _supportedTypes;
            }
        }
        public enum SequenceItemType
        {
            Constant,
            Data,
            Variable
        }
        public enum InSequenceRole
        {
            Prefix,
            Suffix,
            DataLenght,
            Data
        }
    }
}
