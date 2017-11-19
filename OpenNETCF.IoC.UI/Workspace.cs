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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OpenNETCF.IoC.UI
{
    public delegate void GestureHandler(IWorkspace workspace, GestureDirection direction);

    public class Workspace : ContainerControl, IWorkspace
    {
        public event EventHandler<DataEventArgs<ISmartPart>> SmartPartActivated;
        public event EventHandler<DataEventArgs<ISmartPart>> SmartPartDeactivated;
        public event EventHandler<DataEventArgs<ISmartPart>> SmartPartClosing;
        public event GestureHandler GestureReceived;

        private SmartPartCollection m_smartParts;
        private Point m_lastDown;
        private int m_lastDownTime;
        private bool m_gestureEnabled;
        private const int GestureTime = 500;
        private const int DefaultGestureThreshold = 20;

        public int GestureThreshold { get; set; }

        public virtual ISmartPart ActiveSmartPart { get; protected set; }

        public Workspace()
        {
            GesturesEnabled = false;
            GestureThreshold = DefaultGestureThreshold;
            m_smartParts = new SmartPartCollection();
        }

        public ISmartPartCollection SmartParts 
        {
            get { return m_smartParts; }
        }

        public TSmartPart Show<TSmartPart>()
            where TSmartPart : class, ISmartPart
        {
            var existing = SmartParts.FirstOrDefault(s => s is TSmartPart) as TSmartPart;
            if(existing == null)
            {
                existing = RootWorkItem.SmartParts.AddNew<TSmartPart>();
            }

            Show(existing, null);

            return existing;
        }

        public void Show(ISmartPart smartPart)
        {
            this.Show(smartPart, null);
        }

        public void Show(ISmartPart smartPart, ISmartPartInfo smartPartInfo)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            this.BeginInvokeIfRequired(() =>
                {
                    OnShow(smartPart, smartPartInfo);
                });
        }

        /// <summary>
        /// Adds a SmartPart to the Workspace without showing it
        /// </summary>
        /// <param name="smartPart"></param>
        public virtual void Add(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            this.InvokeIfRequired(() =>
                {
                    var control = smartPart as Control;
                    if (control == null) throw new ArgumentException("smartPart must be a Control");

                    if (!SmartParts.Contains(smartPart))
                    {
                        smartPart.Workspace = this;
                        m_smartParts.Add(smartPart);
                        if (!RootWorkItem.SmartParts.ContainsObject(smartPart))
                        {
                            RootWorkItem.SmartParts.Add(smartPart, Guid.NewGuid().ToString());
                        }
                        if (!this.Controls.Contains(control))
                        {
                            this.Controls.Add(control);
                        }

                        EnableGesturesForControl(smartPart, true);                    }
                });
        }

        protected void EnableGesturesForControl(ISmartPart smartPart, bool enable)
        {
            if (GesturesEnabled)
            {
                HookClicks(smartPart as Control, enable);
            }
        }

        protected virtual void OnShow(ISmartPart smartPart, ISmartPartInfo smartPartInfo)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            var control = smartPart as Control;
            if (control == null) throw new ArgumentException("smartPart must be a Control");
            control.Dock = DockStyle.Fill;

            Add(smartPart);
            Activate(smartPart);
        }

        public void Hide(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            CheckSmartPartExists(smartPart);

            this.InvokeIfRequired(() =>
                {
                    OnHide(smartPart);

                    RaiseSmartPartDeactivated(smartPart);
                });
        }

        protected virtual void OnHide(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            Deactivate(smartPart);
        }

        public void Close(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            CheckSmartPartExists(smartPart);

            this.InvokeIfRequired(() =>
                {
                    OnClose(smartPart);
                });
        }

        protected virtual void OnClose(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            Control control = smartPart as Control;
            if (control == null) throw new ArgumentException("smartPart must be a Control");

            RaiseSmartPartDeactivated(smartPart);
            RaiseSmartPartClosing(smartPart);

            smartPart.Visible = false;
            smartPart.Workspace = null;

            this.Controls.Remove(control);

            EnableGesturesForControl(smartPart, false);

            m_smartParts.Remove(smartPart);

            // this call will also call Dispose on any IDisposable items
            RootWorkItem.SmartParts.Remove(smartPart);

            if(smartPart == ActiveSmartPart)
            {
                ActiveSmartPart = null;
            }

            smartPart = null;
        }

        protected void RaiseSmartPartClosing(ISmartPart smartPart)
        {
            if (SmartPartClosing == null) return;

            SmartPartClosing(this, new DataEventArgs<ISmartPart>(smartPart));
        }

        public void Activate(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            this.InvokeIfRequired(() =>
                {
                    AddSmartPartToCollectionIfRequired(smartPart);

                    if (smartPart == ActiveSmartPart) return;

                    if (ActiveSmartPart != null) OnDeactivate(ActiveSmartPart);
                    ActiveSmartPart = smartPart;
                    OnActivate(smartPart);
                });
        }

        protected virtual void OnActivate(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            if (!smartPart.Visible)
            {
                // this will call OnActivated for us
                smartPart.Visible = true;
            }
            else
            {
                // for activation of an already-visible SmartPart
                smartPart.OnActivated();
            }

            RaiseSmartPartActivated(smartPart);

            smartPart.BringToFront();
            smartPart.Focus();
        }

        public void Deactivate(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            this.InvokeIfRequired(() =>
                {
                    CheckSmartPartExists(smartPart);

                    OnDeactivate(smartPart);
                });
        }

        protected virtual void OnDeactivate(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");

            if (smartPart.Visible)
            {
                smartPart.Visible = false;
            }
            else
            {
                smartPart.OnDeactivated();
            }

            if (smartPart == ActiveSmartPart)
            {
                ActiveSmartPart = null;
            }

            RaiseSmartPartDeactivated(smartPart);            
        }

        protected internal void RaiseSmartPartActivated(ISmartPart smartPart)
        {
            if (SmartPartActivated == null) return;

            SmartPartActivated(this, new DataEventArgs<ISmartPart>(smartPart));
        }

        protected void RaiseSmartPartDeactivated(ISmartPart smartPart)
        {
            if (SmartPartDeactivated == null) return;

            SmartPartDeactivated(this, new DataEventArgs<ISmartPart>(smartPart));
        }

        private void CheckSmartPartExists(ISmartPart smartPart)
        {
            if (smartPart == null) throw new ArgumentNullException("smartPart");
            var control = smartPart as Control;
            if (control == null) throw new ArgumentException("smartPart must be a Control");

            if (!SmartParts.Contains(smartPart))
            {                
                // check to see if it's the RootWorkItem SmartParts collection (i.e. added to the RootWorkItem.SmartParts directly)
                if (RootWorkItem.SmartParts.ContainsObject(smartPart))
                {
                    m_smartParts.Add(smartPart);
                    this.Controls.Add(control);
                }
                else
                {
                    throw new Exception("ISmartPart not in Workspace");
                }
            }
        }

        protected void AddSmartPartToCollectionIfRequired(ISmartPart smartPart)
        {
            if (!SmartParts.Contains(smartPart))
            {
                m_smartParts.Add(smartPart);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (IsDesignTime)
            {
                base.OnPaintBackground(e);

                Color c = Color.FromArgb(69, 69, 69);
                e.Graphics.DrawRectangle(
                    new Pen(c),
                    new Rectangle(1, 1, this.Width - 2, this.Height - 2));

                string text = this.GetType().Name;
                SizeF size = e.Graphics.MeasureString(text, this.Font);
                int left = (int)(this.Width - size.Width) / 2;
                int top = (int)(this.Height - size.Height) / 2;
                e.Graphics.DrawString(text, this.Font, new SolidBrush(c), left, top);
            }
            else
            {
                base.OnPaintBackground(e);
            }
        }

        protected bool IsDesignTime
        {
            get
            {
                // Determine if this instance is running against .NET Framework by using the MSCoreLib PublicKeyToken
                System.Reflection.Assembly mscorlibAssembly = typeof(int).Assembly;
                if ((mscorlibAssembly != null))
                {
                    if (mscorlibAssembly.FullName.ToUpper().EndsWith("B77A5C561934E089"))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool GesturesEnabled 
        {
            get { return m_gestureEnabled; }
            set
            {
                if (m_gestureEnabled == value) return;

                // Yes, this would be way easier with an IMEssageFilter, but the CF doesn't support them.
                // I could get support by adding a reference to the SDF, but I'm trying to keep IoC self-contained
                HookClicks(this, value);

                m_gestureEnabled = value;
            }
        }

        private void HookClicks(Control control, bool hook)
        {
            lock (this)
            {
                if (hook)
                {
                    control.MouseDown += control_MouseDown;
                    control.MouseUp += control_MouseUp;
                }
                else
                {
                    control.MouseDown -= control_MouseDown;
                    control.MouseUp -= control_MouseUp;
                }

                foreach (var child in control.Controls)
                {
                    HookClicks(child as Control, hook);
                }
            }
        }
        void control_MouseUp(object sender, MouseEventArgs e)
        {
            if (GesturesEnabled)
            {
                var p = (sender as Control).PointToScreen(new Point(e.X, e.Y));
                DetectGesture(m_lastDown, p, Environment.TickCount - m_lastDownTime);
            }
        }

        void control_MouseDown(object sender, MouseEventArgs e)
        {
            if (GesturesEnabled)
            {
                m_lastDown = (sender as Control).PointToScreen(new Point(e.X, e.Y));
                m_lastDownTime = Environment.TickCount;
            }
        }

        private void RaiseGestureReceived(GestureDirection direction)
        {
            var handler = GestureReceived;
            if (handler == null) return;
            handler(this, direction);
        }

        private void DetectGesture(Point start, Point end, int dt)
        {
            // don't bother calculating if no one is listening
            if (GestureReceived == null) return;

            // does it fit the time domain of a gesture?
            if (dt > 500) return;

            var dx = end.X - start.X;
            var dy = end.Y - start.Y;

            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                // check for no gesture
                if (Math.Abs(dx) < GestureThreshold) return;

                // gesture in the X axis
                if (dx > 0)
                {
                    RaiseGestureReceived(GestureDirection.Right);
                }
                else
                {
                    RaiseGestureReceived(GestureDirection.Left);
                }
            }
            else
            {
                // check for no gesture
                if (Math.Abs(dy) < GestureThreshold) return;

                // gesture in the Y axis
                if (dy > 0)
                {
                    RaiseGestureReceived(GestureDirection.Down);
                }
                else
                {
                    RaiseGestureReceived(GestureDirection.Up);
                }
            }
        }
    }
}
