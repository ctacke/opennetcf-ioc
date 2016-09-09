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


namespace OpenNETCF.IoC.UI
{
    public interface IPresenter<TView> 
        where TView : ISmartPart
    {
        void Run();
    }

    public class Presenter<TView>
    {
        public TView View { get; set; }
        public virtual void OnViewReady()
        {
        }
        public virtual void OnCloseView()
        {
        }
    }
}
