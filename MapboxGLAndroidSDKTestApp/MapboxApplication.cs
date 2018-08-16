using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Com.Mapbox.Mapboxsdk;
using Com.Mapbox.Mapboxsdk.Maps;
using MapboxGLAndroidSDKTestApp.Utils;
using Square.LeakCanary;
using TimberLog;

namespace MapboxGLAndroidSDKTestApp
{
    /*
     * Application class of the test application.
     * <p>
     * Initialises components as LeakCanary, Strictmode, Timber and Mapbox
     * </p>
     */
#if DEBUG
    [Application(Debuggable = true)]
#else
    [Application(Debuggable = false)]
#endif
    public class MapboxApplication : Application
    {
        const string DEFAULT_MAPBOX_ACCESS_TOKEN = "YOUR_MAPBOX_ACCESS_TOKEN_GOES_HERE";
        const string ACCESS_TOKEN_NOT_SET_MESSAGE = "In order to run the Test App you need to set a valid "
    + "access token. During development, you can set the MAPBOX_ACCESS_TOKEN environment variable for the SDK to "
    + "automatically include it in the Test App. Otherwise, you can manually include it in the "
    + "res/values/developer-config.xml file in the MapboxGLAndroidSDKTestApp folder.";

        protected MapboxApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            if (!InitializeLeakCanary())
                return;
            InitializeLogger();
            InitializeStrictMode();
            InitializeMapbox();
        }

        bool InitializeLeakCanary()
        {
            if (LeakCanaryXamarin.IsInAnalyzerProcess(this))
            {
                // This process is dedicated to LeakCanary for heap analysis.
                // You should not init your app in this process.
                return false;
            }
            LeakCanaryXamarin.Install(this);
            return true;
        }

        void InitializeLogger()
        {
            if (Debug.IsDebuggerConnected)
                Timber.Plant(new Timber.DebugTree());
        }

        void InitializeStrictMode()
        {
            StrictMode.SetThreadPolicy(new StrictMode.ThreadPolicy.Builder()
                                       .DetectDiskReads()
                                       .DetectDiskWrites()
                                       .DetectNetwork()
                                       .PenaltyLog()
                                       .Build());
            StrictMode.SetVmPolicy(new StrictMode.VmPolicy.Builder()
                                   .DetectLeakedSqlLiteObjects()
                                   .PenaltyLog()
                                   .PenaltyDeath()
                                   .Build());
        }

        void InitializeMapbox()
        {
            string accessToken = TokenUtils.GetMapboxAccessToken(ApplicationContext);
            ValidateAccessToken(accessToken);
            Mapbox.GetInstance(ApplicationContext, accessToken);
            Telemetry.UpdateDebugLoggingEnabled(true);
        }

        void ValidateAccessToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken) || Equals(accessToken, DEFAULT_MAPBOX_ACCESS_TOKEN))
                Timber.E(ACCESS_TOKEN_NOT_SET_MESSAGE);
        }
    }
}
