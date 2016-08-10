using Android.Graphics;
using Android.Widget;
using OpenNETCF.Controls;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LabelEx), typeof(OpenNETCF.Platform.Android.LabelExRenderer))]
namespace OpenNETCF.Platform.Android
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

            Typeface typeface;

            if (newControl.FontSource.IsNullOrEmpty())
            {
                typeface = Typeface.Default;
            }
            else
            {
                typeface = Typeface.CreateFromAsset(Context.Assets, newControl.FontSource);
            }

            var ctrl = Control as TextView;
            if (ctrl != null)
            {
                ctrl.Typeface = typeface;
            }
        }
    }
}
