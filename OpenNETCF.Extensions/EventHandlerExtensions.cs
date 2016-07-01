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
using System.ComponentModel;
using OpenNETCF;

namespace System
{
    public static class EventHandlerExtensions
    {
        public static void Fire(this EventHandler h, object sender)
        {
            var handler = h;
            if (handler == null) return;
            handler(sender, EventArgs.Empty);
        }

        public static void Fire(this EventHandler h, object sender, EventArgs args)
        {
            var handler = h;
            if (handler == null) return;
            handler(sender, args);
        }

        public static void Fire<T>(this EventHandler<T> h, object sender, T args) where T : EventArgs
        {
            var handler = h;
            if (handler == null) return;
            handler(sender, args);
        }

        public static void Fire<T>(this EventHandler<GenericEventArgs<T>> h, object sender, T args)
        {
            var handler = h;
            if (handler == null) return;
            handler(sender, new GenericEventArgs<T>(args));
        }

        public static void Fire(this PropertyChangedEventHandler h, object sender, string propertyName)
        {
            var handler = h;
            if (handler == null) return;
            handler(sender, new PropertyChangedEventArgs(propertyName));
        }
    }


}
