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

namespace OpenNETCF.IoC.UI
{
    public interface IWorkspace
    {
        event EventHandler<DataEventArgs<ISmartPart>> SmartPartActivated;
        event EventHandler<DataEventArgs<ISmartPart>> SmartPartDeactivated;
        event EventHandler<DataEventArgs<ISmartPart>> SmartPartClosing;

        void Show(ISmartPart smartPart);
        void Show(ISmartPart smartPart, ISmartPartInfo smartPartInfo);
        TSmartPart Show<TSmartPart>() where TSmartPart : class, ISmartPart;
        void Hide(ISmartPart smartPart);
        void Close(ISmartPart smartPart);
        void Activate(ISmartPart smartPart);
        void Deactivate(ISmartPart smartPart);

        ISmartPart ActiveSmartPart { get; }
        ISmartPartCollection SmartParts { get; }
    }
}
