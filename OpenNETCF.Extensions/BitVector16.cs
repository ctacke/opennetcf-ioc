// -------------------------------------------------------------------------------------------------------
// LICENSE INFORMATION
//
// - This software is licensed under the MIT shared source license.
// - The "official" source code for this project is maintained at http://oncfext.codeplex.com
//
// Copyright (c) 2010 OpenNETCF Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
// associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// -------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Collections.Specialized
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BitVector16
    {
        private ushort data;

        public BitVector16(short data)
        {
            this.data = (ushort)data;
        }

        public BitVector16(BitVector16 value)
        {
            this.data = value.data;
        }

        public bool this[int bit]
        {
            get
            {
                return ((this.data & bit) == ((uint)bit));
            }
            set
            {
                if (value)
                {
                    this.data |= (ushort)bit;
                }
                else
                {
                    this.data &= (ushort)~bit;
                }
            }
        }

        public short this[Section section]
        {
            get
            {
                return (short)((this.data & (section.Mask << (section.Offset & 0x1f))) >> (section.Offset & 0x1f));
            }
            set
            {
                value = (short)(value << section.Offset);
                int num = (0xff & section.Mask) << section.Offset;
                this.data = (ushort)((this.data & ~num) | (value & num));
            }
        }

        public short Data
        {
            get
            {
                return (short)this.data;
            }
        }

        private static short CountBitsSet(short mask)
        {
            short num = 0;
            while ((mask & 1) != 0)
            {
                num = (short)(num + 1);
                mask = (short)(mask >> 1);
            }
            return num;
        }

        public static short CreateMask()
        {
            return CreateMask(0);
        }

        public static short CreateMask(short previous)
        {
            if (previous == 0)
            {
                return 1;
            }
            if (previous == short.MinValue)
            {
                throw new InvalidOperationException();
            }
            return (short)(previous << 1);
        }

        private static byte CreateMaskFromHighValue(byte highValue)
        {
            byte num = 0x10;
            while ((highValue & 0x8000) == 0)
            {
                num = (byte)(num - 1);
                highValue = (byte)(highValue << 1);
            }
            byte num2 = 0;
            while (num > 0)
            {
                num = (byte)(num - 1);
                num2 = (byte)(num2 << 1);
                num2 = (byte)(num2 | 1);
            }
            return (byte)num2;
        }

        public static Section CreateSection(byte maxValue)
        {
            return CreateSectionHelper(maxValue, 0, 0);
        }

        public static Section CreateSection(byte maxValue, Section previous)
        {
            return CreateSectionHelper(maxValue, previous.Mask, previous.Offset);
        }

        private static Section CreateSectionHelper(byte maxValue, byte priorMask, byte priorOffset)
        {
            if (maxValue < 1)
            {
                throw new ArgumentException();
            }
            byte offset = (byte)(priorOffset + CountBitsSet(priorMask));
            if (offset >= 0x20)
            {
                throw new InvalidOperationException();
            }
            return new Section(CreateMaskFromHighValue(maxValue), offset);
        }

        public override bool Equals(object o)
        {
            return ((o is BitVector16) && (this.data == ((BitVector16)o).data));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string ToString(BitVector16 value)
        {
            StringBuilder builder = new StringBuilder(0x2d);
            builder.Append("BitVector16{");
            int data = (int)value.data;
            for (int i = 0; i < 0x10; i++)
            {
                if ((data & 0x8000L) != 0L)
                {
                    builder.Append("1");
                }
                else
                {
                    builder.Append("0");
                }
                data = data << 1;
            }
            builder.Append("}");
            return builder.ToString();
        }

        public override string ToString()
        {
            return ToString(this);
        }
        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        public struct Section
        {
            private readonly byte mask;
            private readonly byte offset;
            internal Section(byte mask, byte offset)
            {
                this.mask = mask;
                this.offset = offset;
            }

            public byte Mask
            {
                get
                {
                    return this.mask;
                }
            }
            public byte Offset
            {
                get
                {
                    return this.offset;
                }
            }
            public override bool Equals(object o)
            {
                return ((o is BitVector16.Section) && this.Equals((BitVector16.Section)o));
            }

            public bool Equals(BitVector16.Section obj)
            {
                return ((obj.mask == this.mask) && (obj.offset == this.offset));
            }

            public static bool operator ==(BitVector16.Section a, BitVector16.Section b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(BitVector16.Section a, BitVector16.Section b)
            {
                return !(a == b);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static string ToString(BitVector16.Section value)
            {
                return ("Section{0x" + Convert.ToString(value.Mask, 0x10) + ", 0x" + Convert.ToString(value.Offset, 0x10) + "}");
            }

            public override string ToString()
            {
                return ToString(this);
            }
        }
    }

}
