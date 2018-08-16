using Android.Content;
using Com.Mapbox.Mapboxsdk;

namespace MapboxGLAndroidSDKTestApp.Utils
{
    public static class TokenUtils
    {
        /*
         * <p>
         * Returns the Mapbox access token set in the app resources.
         * </p>
         * It will first search for a token in the Mapbox object. If not found it
         * will then attempt to load the access token from the
         * {@code res/values/dev.xml} development file.
         *
         * @param context The {@link Context} of the {@link android.app.Activity} or {@link android.app.Fragment}.
         * @return The Mapbox access token or null if not found.
         */
        public static string GetMapboxAccessToken(Context context)
        {
            return context.GetString(Resource.String.mapbox_access_token);
        }
    }
}
