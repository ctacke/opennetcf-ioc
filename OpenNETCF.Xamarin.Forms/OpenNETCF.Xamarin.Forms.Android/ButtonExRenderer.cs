using Android.Graphics;
using OpenNETCF.Controls;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using AWB = Android.Widget.Button; // due to 'Android' namespace collision

[assembly: ExportRenderer(typeof(ButtonEx), typeof(OpenNETCF.Platform.Android.ButtonExRenderer))]
namespace OpenNETCF.Platform.Android
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

            Typeface typeface;

            if (newControl.FontSource.IsNullOrEmpty())
            {
                typeface = Typeface.Default;
            }
            else
            {
                typeface = Typeface.CreateFromAsset(Context.Assets, newControl.FontSource);
            }

            var ctrl = Control as AWB;
            if (ctrl != null)
            {
                ctrl.Typeface = typeface;
            }
        }
    }
}
