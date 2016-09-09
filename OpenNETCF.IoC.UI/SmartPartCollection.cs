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

using System;
using System.Collections.Generic;

namespace OpenNETCF.IoC.UI
{
    public class SmartPartCollection : ISmartPartCollection
    {
        private List<ISmartPart> m_smartParts = new List<ISmartPart>();

        internal SmartPartCollection()
        {
        }

        internal void Add(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException();

            m_smartParts.Add(smartPart);
        }

        internal void Remove(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException();

            m_smartParts.Remove(smartPart);
        }

        public IEnumerator<ISmartPart> GetEnumerator()
        {
            return m_smartParts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_smartParts.GetEnumerator();
        }

        public int Count
        {
            get { return m_smartParts.Count; }
        }

    }
}
