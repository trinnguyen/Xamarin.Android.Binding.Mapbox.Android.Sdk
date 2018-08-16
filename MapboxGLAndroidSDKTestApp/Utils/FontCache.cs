using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Content;
using TimberLog;
namespace MapboxGLAndroidSDKTestApp.Utils
{
    public static class FontCache
    {
        static Dictionary<string, Typeface> fontCache = new Dictionary<string, Typeface>();

        public static Typeface Get(string name, Context context)
        {
            Typeface tf = null;
            fontCache.TryGetValue(name, out tf);

            if (tf == null)
            {
                try
                {
                    tf = Typeface.CreateFromAsset(context.Assets, name);
                    fontCache.Add(name, tf);
                }
                catch (Exception)
                {
                    Timber.E("Font not found");
                }
            }

            return tf;
        }
    }
}
