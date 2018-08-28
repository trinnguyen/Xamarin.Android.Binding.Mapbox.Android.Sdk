using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content.Res;
using Android.Support.V4.Graphics.Drawable;
using Com.Mapbox.Mapboxsdk.Annotations;

namespace MapboxGLAndroidSDKTestApp.Utils
{
    public static class IconUtils
    {
        /*
         * Demonstrates converting any Drawable to an Icon, for use as a marker icon.
         */
        public static Icon DrawableToIcon(Context context, int id, int colorRes)
        {
            Android.Graphics.Drawables.Drawable vectorDrawable = ResourcesCompat.GetDrawable(context.Resources, id, context.Theme);
            Bitmap bitmap = Bitmap.CreateBitmap(vectorDrawable.IntrinsicWidth,
                                                vectorDrawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);
            vectorDrawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            DrawableCompat.SetTint(vectorDrawable, colorRes);
            vectorDrawable.Draw(canvas);
            return IconFactory.GetInstance(context).FromBitmap(bitmap);
        }
    }
}
