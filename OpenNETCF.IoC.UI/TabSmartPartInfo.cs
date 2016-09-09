// LICENSE
// -------
// This software was originally authored by Christopher Tacke of OpenNETCF Consulting, LLC
// On March 10, 2009 is was placed in the public domain, meaning that all copyright has been disclaimed.
//
// You may use this code for any purpose, commercial or non-commercial, free or proprietary with no legal 
// obligation to acknowledge the use, copying or modification of the source.
//
// OpenNETCF will maintain an "official" version of this software at www.opennetcf.com and public 
// submissions of changes, fixes or updates are welcomed but not required
//

using System.ComponentModel;

namespace OpenNETCF.IoC.UI
{
    public enum TabPosition
    {
        // Summary:
        //     Place tab page at begining.
        Beginning = 0,
        //
        // Summary:
        //     Place tab page at end.
        End = 1,
    }

    public class TabSmartPartInfo : SmartPartInfo
    {
        public TabSmartPartInfo()
        {
        }

        /// <summary>
        /// Specifies whether the tab will get focus when shown.
        /// </summary>
        [DefaultValue(true)]
        public bool ActivateTab { get; set; }

        /// <summary>
        /// Specifies the position of the tab page.
        /// </summary>
        public TabPosition Position { get; set; }
    }
}
