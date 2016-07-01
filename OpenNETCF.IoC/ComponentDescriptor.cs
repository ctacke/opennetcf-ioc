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

namespace OpenNETCF.IoC
{
    public class ComponentDescriptor : IEquatable<ComponentDescriptor>
    {
        public string Name { get; set; }
        public object Instance { get; set; }
        public Type ClassType { get; set; }
        public Type RegistrationType { get; set; }

        public static implicit operator KeyValuePair<Type, object>(ComponentDescriptor cd)
        {
            return new KeyValuePair<Type, object>(cd.RegistrationType, cd.Instance);
        }

        public bool Equals(ComponentDescriptor other)
        {
            if (Name != other.Name) return false;
            if (ClassType != other.ClassType) return false;
            if (RegistrationType != other.RegistrationType) return false;

            return true;
        }
    }
}
