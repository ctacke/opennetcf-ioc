using OpenNETCF.Controls;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(LabelEx), typeof(OpenNETCF.Platform.iOS.LabelExRenderer))]
namespace OpenNETCF.Platform.iOS
{
    public class LabelExRenderer : LabelRenderer
    {
        public LabelExRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            var newControl = e.NewElement as LabelEx;

            UIFont font;

            if (newControl.FontSource.IsNullOrEmpty())
            {
                font = UIFont.SystemFontOfSize((nfloat)newControl.FontSize, UIFontWeight.Regular);
            }
            else
            {
                font = UIFont.FromName(newControl.FontSource, (nfloat)newControl.FontSize);
            }

            var ctrl = Control as UILabel;
            if (ctrl != null)
            {
                ctrl.Font = font;
            }
        }
    }
}
