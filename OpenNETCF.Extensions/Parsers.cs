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
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF
{
    public static class Parsers
    {
        private static bool TryParseImpl(string s, int start, ref int value)
        {
            if (start == s.Length) return false;
            unchecked
            {
                int i = start;
                do
                {
                    int newvalue = value * 10 + '0' - s[i++];
                    if (value != newvalue / 10)
                    {
                        value = 0;
                        return false;
                    }

                    value = newvalue;
                } while (i < s.Length);
                if (start == 0)
                {
                    if (value == int.MinValue)
                    {
                        value = 0;
                        return false;
                    }
                    value = -value;
                }
            }
            return true;
        }

        public static bool TryParse(string s, out int value)
        {
            value = 0;
            if (s == null) return false;
            s = s.Trim();
            if (s.Length == 0) return false;
            return TryParseImpl(s, (s[0] == '-') ? 1 : 0, ref value);
        }

        public static bool TryParse(string s, out short value)
        {
            value = 0;
            if (s == null) return false;
            s = s.Trim();
            if (s.Length == 0) return false;
            try
            {
                value = short.Parse(s);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        public static bool TryParse(string s, out bool value)
        {
            try
            {
                value = bool.Parse(s);
                return true;
            }
            catch
            {
                value = false;
                return false;
            }
        }

        public static bool TryParse(string s, out double value)
        {
            value = 0;
            if (s.IsNullOrEmpty()) return false;

            try
            {
                value = double.Parse(s);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        public static bool TryParse(string s, out long value)
        {
            try
            {
                value = long.Parse(s);
                return true;
            }
            catch
            {
                value = 0;
                return false;
            }
        }

        public static bool TryParse(string s, out DateTime value)
        {
            try
            {
                value = DateTime.Parse(s);
                return true;
            }
            catch
            {
                value = DateTime.Now;
                return false;
            }
        }
    }
}
