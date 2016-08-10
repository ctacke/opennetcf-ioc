using OpenNETCF.Controls;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(EntryEx), typeof(OpenNETCF.Platform.iOS.EntryExRenderer))]
namespace OpenNETCF.Platform.iOS
{
    public class EntryExRenderer : EntryRenderer
    {
        public EntryExRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            var newControl = e.NewElement as EntryEx;

            UIFont font;

            if (newControl.FontSource.IsNullOrEmpty())
            {
                font = UIFont.SystemFontOfSize((nfloat)newControl.FontSize, UIFontWeight.Regular);
            }
            else
            {
                font = UIFont.FromName(newControl.FontSource, (nfloat)newControl.FontSize);
            }

            var ctrl = Control as UITextField;
            if (ctrl != null)
            {
                ctrl.Font = font;
            }
        }
    }
}
