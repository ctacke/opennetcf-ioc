using OpenNETCF.Controls;
using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ButtonEx), typeof(OpenNETCF.Platform.iOS.ButtonExRenderer))]
namespace OpenNETCF.Platform.iOS
{
    public class ButtonExRenderer : ButtonRenderer
    {
        public ButtonExRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            var newControl = e.NewElement as ButtonEx;

            UIFont font;

            if (newControl.FontSource.IsNullOrEmpty())
            {
                font = UIFont.SystemFontOfSize((nfloat)newControl.FontSize, UIFontWeight.Regular);
            }
            else
            {
                font = UIFont.FromName(newControl.FontSource, (nfloat)newControl.FontSize);
            }

            var ctrl = Control as UIButton;
            if (ctrl != null)
            {
                ctrl.Font = font;
            }
        }
    }
}
