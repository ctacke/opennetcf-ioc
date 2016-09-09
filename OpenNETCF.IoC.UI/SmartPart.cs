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

using System.Windows.Forms;
using System;

namespace OpenNETCF.IoC.UI
{
    public class SmartPart : UserControl, ISmartPart
    {
        public IWorkspace Workspace { get; set; }

        public SmartPart()
        {
#if !WindowsCE
            this.SuspendLayout();
            // 
            // SmartPart
            // 
            this.DoubleBuffered = true;
            this.Name = "SmartPart";
            this.Size = new System.Drawing.Size(171, 169);
            this.ResumeLayout(false);
#endif
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                GC.ReRegisterForFinalize(this);
            }
        }

        public virtual void OnActivated() 
        {
            this.Focus();
        }
        
        public virtual void OnDeactivated() 
        { 
        }
        
        public new void Hide()
        {
            var wasVisible = this.Visible;

            SetVisibleCore(false);

            if (wasVisible)
            {
                OnDeactivated();
            }
        }

        public new void Show()
        {
            var wasVisible = this.Visible;

            SetVisibleCore(true);

            if (!wasVisible)
            {
                OnActivated();
            }
        }

        public new bool Visible
        {
            get { return base.Visible; }
            set
            {
                if (Visible == value) return;

                SetVisibleCore(value);
                
                if (value)
                {
                    OnActivated();
                }
                else
                {
                    OnDeactivated();
                }
            }
        }

#if WindowsCE
        private void SetVisibleCore(bool value)
        {
            if(value)
            {
                // To "show", all parents must be visible
                // In the CF, the parent Form's Visible property isn't set until after the ctor has run
                // so things like Focus and SelectAll on a control inside the SmartPart will fail on first construction
                // if we don't verify parent visibility
                SetParentVisibility(this, true);

                base.Show();
            }
            else
            {
                base.Hide();
            }
        }

        private void SetParentVisibility(Control child, bool visible)
        {
            if (child.Parent == null)
            {
                child.Visible = visible;
            }
            else
            {
                SetParentVisibility(child.Parent, visible);
            }
        }
#endif
    }
}
