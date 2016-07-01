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
using System.IO;

namespace System
{

    public struct PackedBCD : IComparable, IComparable<PackedBCD>, IEquatable<PackedBCD>
    {
        public const int MaxValue = 0x79999999;
        public const int MinValue = 0x00000000;

        private int m_value;

        private PackedBCD(int value)
        {
            m_value = value;
        }

        public static explicit operator PackedBCD(int value)
        {
            if(value > MaxValue) throw new ArgumentException();
            if(value < 0) throw new ArgumentException();

            int bcd = 0; 
            for (int digit = 0; digit < 8; ++digit) 
            { 
            int nibble = value % 10; 
            bcd |= nibble << (digit * 4); 
            value /= 10;
            }
            return new PackedBCD(bcd);
        }

        public static explicit operator int(PackedBCD value)
        {
            var data = BitConverter.GetBytes(value.m_value);

            int i = 0;
            int scale = 1;

            foreach (var b in data)
            {
                i += ((b & 0x0f) * scale);
                scale *= 10;
                i += ((b >> 4) * scale);
                scale *= 10;
            }

            return i;
        }

        public static PackedBCD FromBytes(byte[] data, int offset)
        {
            var val = 0;
            var scale = 0;

            for (int i = 3; i >= 0; i--)
            {
                val += ((data[offset + i] & 0xf) << scale);
                val += ((data[offset + i] >> 4) << (scale + 4));
                scale += 8;
            }

            return new PackedBCD(val);
        }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(m_value).Reverse().ToArray();
        }

        public override string ToString()
        {
            var bytes = BitConverter.GetBytes(m_value).Reverse();

            var keep = false;
            var s = string.Empty;

            foreach (var b in bytes)
            {
                var right = b & 0x0f;
                var left = b >> 4;

                if (left == 0)
                {
                    if (keep)
                    {
                        s += left.ToString();
                        s += right.ToString();
                    }
                    else
                    {
                        if (right != 0)
                        {
                            keep = true;
                            s += right.ToString();
                        }
                    }
                }
                else
                {
                    keep = true;
                    s += left.ToString();
                    s += right.ToString();
                }
            }

            return s;
        }

        public static PackedBCD Parse(string s)
        {
            var i = int.Parse(s);
            return new PackedBCD(i);
        }

        public static bool TryParse(string s, out PackedBCD result)
        {
            try
            {
                result = PackedBCD.Parse(s);
                return true;
            }
            catch
            {
                result = new PackedBCD(0);
                return false;
            }
        }

        public int CompareTo(PackedBCD other)
        {
            return this.m_value.CompareTo(other.m_value);
        }

        public int CompareTo(object value)
        {

            return ((int)this).CompareTo((int)value);
        }

        public bool Equals(PackedBCD obj)
        {
            return this.m_value.Equals(obj.m_value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PackedBCD)) throw new ArgumentException("object must be of type BCD");

            return this.m_value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }
    }

    public struct BCD : IComparable, IComparable<BCD>, IEquatable<BCD>
    {
        public const int MaxValue = 0x9999;
        public const int MinValue = 0x00000000;

        private int m_value;

        private BCD(int value)
        {
            m_value = value;
        }

        public static explicit operator BCD(int value)
        {
            if(value > MaxValue) throw new ArgumentException();
            if(value < 0) throw new ArgumentException();

            var data = new byte[4];

            var tempval = value;
            data[3] = (byte)(value / 1000);
            tempval %= 1000;
            data[2] = (byte)(tempval / 100);
            tempval %= 100;
            data[1] = (byte)(tempval / 10);
            data[0] = (byte)(tempval % 10);

            return new BCD(BitConverter.ToInt32(data, 0));
        }

        public static explicit operator int(BCD value)
        {
            var data = BitConverter.GetBytes(value.m_value);

            int i = data[3] * 1000
              + data[2] * 100
              + data[1] * 10
              + data[0];

            return i;
        }

        public static BCD FromBytes(byte[] data, int offset)
        {
            var val = 0;
            var scale = 0;

            for (int i = 3; i >= 0 ; i--)
            {
                val += ((data[offset + i] & 0xf) << scale);
                scale += 8;
            }

            return new BCD(val);
        }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(m_value).Reverse().ToArray();
        }

        public override string ToString()
        {
            var bytes = BitConverter.GetBytes(m_value).Reverse();

            var keep = false;
            var s = string.Empty;

            foreach (var b in bytes)
            {
                if (b == 0)
                {
                    if (keep)
                    {
                        s += b.ToString();
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    keep = true;
                    s += b.ToString();
                }
            }

            return s;
        }

        public static BCD Parse(string s)
        {
            var i = int.Parse(s);
            return new BCD(i);
        }

        public static bool TryParse(string s, out BCD result)
        {
            try
            {
                result = BCD.Parse(s);
                return true;
            }
            catch
            {
                result = new BCD(0);
                return false;
            }
        }

        public int CompareTo(BCD other)
        {
            return this.m_value.CompareTo(other.m_value);
        }

        public int CompareTo(object value)
        {

            return ((int)this).CompareTo((int)value);
        }

        public bool Equals(BCD obj)
        {
            return this.m_value.Equals(obj.m_value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BCD)) throw new ArgumentException("object must be of type BCD");

            return this.m_value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }
    }
}
