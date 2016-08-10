using Android.Graphics;
using Android.Widget;
using OpenNETCF.Controls;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(EntryEx), typeof(OpenNETCF.Platform.Android.EntryExRenderer))]
namespace OpenNETCF.Platform.Android
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
