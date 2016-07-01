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
    public struct BitVector8
    {
        private byte data;

        public BitVector8(byte data)
        {
            this.data = data;
        }

        public BitVector8(BitVector8 value)
        {
            this.data = value.Data;
        }

        public byte Data
        {
            get { return data; }
        }

        public bool this[int bit]
        {
            get            
            {
                return ((this.Data & bit) == ((uint)bit));
            }
            set
            {
                if (value)
                {
                    this.data |= (byte)bit;
                }
                else
                {
                    this.data &= (byte)~bit;
                }
            }
        }

        private static short CountBitsSet(byte mask)
        {
            byte num = 0;
            while ((mask & 1) != 0)
            {
                num = (byte)(num + 1);
                mask = (byte)(mask >> 1);
            }
            return num;
        }

        public static short CreateMask()
        {
            return CreateMask(0);
        }

        public static short CreateMask(byte previous)
        {
            if (previous == 0)
            {
                return 1;
            }
            if (previous == byte.MaxValue)
            {
                throw new InvalidOperationException();
            }
            return (short)(previous << 1);
        }

        public override bool Equals(object o)
        {
            return ((o is BitVector16) && (this.Data == ((BitVector8)o).data));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string ToString(BitVector8 value)
        {
            StringBuilder builder = new StringBuilder(0x2d);
            builder.Append("BitVector8{");
            int data = (int)value.Data;
            for (int i = 0; i < 0x8; i++)
            {
                if ((data & 0x80L) != 0L)
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

        public static implicit operator byte(BitVector8 vector)
        {
            return vector.Data;
        }

        public static implicit operator BitVector8(byte data)
        {
            return new BitVector8(data);
        }
    }
}
