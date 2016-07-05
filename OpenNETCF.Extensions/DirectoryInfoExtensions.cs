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

#if !PCL
namespace OpenNETCF
{
    public static class DirectoryInfoExtensions
    {
        public static void Copy(this DirectoryInfo source, string destination)
        {
            Copy(source, new DirectoryInfo(destination));
        }

        public static void Copy(this DirectoryInfo source, DirectoryInfo destination)
        {
            // Check if the destination directory exists, if not, create it.
            if (Directory.Exists(destination.FullName) == false)
            {
                Directory.CreateDirectory(destination.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                string fileDestination = Path.Combine(destination.ToString(), fi.Name);
                if (!File.Exists(fileDestination))
                {
                    fi.CopyTo(fileDestination, true);
                }
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextdestinationSubDir =
                    destination.CreateSubdirectory(diSourceSubDir.Name);
                Copy(diSourceSubDir, nextdestinationSubDir);
            }
        }
    }
}
#endif