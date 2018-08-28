
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Com.Mapbox.Mapboxsdk.Maps;
using Com.Mapbox.Mapboxsdk.Geometry;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Content.Res;
using Com.Mapbox.Mapboxsdk.Annotations;
using Android.Support.V4.View;
using Java.Text;
using System;
using System.Collections;
using Android.Support.V4.Content.Res;
using MapboxGLAndroidSDKTestApp.Utils;
using System.Linq;
using Java.Util;
using Java.Lang;
using TimberLog;

namespace MapboxGLAndroidSDKTestApp.Activities.Annotation
{
    [Activity(Name = "com.mapbox.mapboxsdk.testapp.Activities.Annotation.BulkMarkerActivity")]
    public class BulkMarkerActivity : AppCompatActivity, IOnMapReadyCallback
    {
        MapboxMap mapboxMap;
        MapView mapView;
        bool customMarkerView;
        List<LatLng> locations;
        ProgressDialog progressDialog;

        #region IOnMapReadyCallback
        public void OnMapReady(MapboxMap p0)
        {
            mapboxMap = p0;
        }
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_marker_bulk);

            mapView = FindViewById<MapView>(Resource.Id.mapView);
            mapView.OnCreate(savedInstanceState);
            mapView.GetMapAsync(this);

            var fab = FindViewById<View>(Resource.Id.fab);
            if (fab != null)
                fab.SetOnClickListener(new FabClickListener(this));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var spinnerAdapter = ArrayAdapter.CreateFromResource(this,
                                                                 Resource.Array.bulk_marker_list,
                                                                 Android.Resource.Layout.SimpleSpinnerItem);
            spinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            MenuInflater.Inflate(Resource.Menu.menu_bulk_marker, menu);
            var item = menu.FindItem(Resource.Id.spinner);
            var spinner = MenuItemCompat.GetActionView(item) as Spinner;
            spinner.Adapter = spinnerAdapter;
            spinner.ItemSelected += (sender, e) =>
            {
                int amount = int.Parse(Resources.GetStringArray(Resource.Array.bulk_marker_list)[e.Position]);
                if (locations == null)
                {
                    progressDialog = ProgressDialog.Show(this, "Loading", "Fetching markers", false);
                    new LoadLocationTask(this, amount).Execute();
                }
                else
                {
                    ShowMarkers(amount);
                }
            };
            return true;
        }

        void OnLatLngListLoaded(List<LatLng> latLngs, int amount)
        {
            progressDialog.Hide();
            locations = latLngs;
            ShowMarkers(amount);
        }

        void ShowMarkers(int amount)
        {
            if (mapboxMap == null || locations == null || mapView == null)
                return;

            mapboxMap.Clear();

            if (locations.Count < amount)
                amount = locations.Count;

            if (customMarkerView)
                ShowViewMarkers(amount);
            else
                ShowGlMarkers(amount);
        }

        void ShowViewMarkers(int amount)
        {
            DecimalFormat formatter = new DecimalFormat("#,#####");
            var random = new System.Random();
            int randomIndex = 0;

            int color = ResourcesCompat.GetColor(Resources, Resource.Color.redAccent, Theme);
            var icon = IconUtils.DrawableToIcon(this, Resource.Drawable.ic_droppin, color);

            var markerOptionsList = new List<BaseMarkerViewOptions>();
            for (int i = 0; i < amount; i++)
            {
                randomIndex = random.Next(locations.Count);
                var latLng = locations[randomIndex];
                var markerOptions = new MarkerViewOptions()
                    .InvokePosition(latLng)
                    .InvokeIcon(icon)
                    .InvokeTitle(i.ToString())
                    .InvokeSnippet($"{formatter.Format(latLng.Latitude)}, {formatter.Format(latLng.Longitude)}");
                markerOptionsList.Add(markerOptions);
            }
            mapboxMap.AddMarkerViews(markerOptionsList);
        }

        void ShowGlMarkers(int amount)
        {
            var markerOptionsList = new List<BaseMarkerOptions>();
            DecimalFormat formatter = new DecimalFormat("#,#####");
            var random = new System.Random();
            int randomIndex;

            for (int i = 0; i < amount; i++)
            {
                randomIndex = random.Next(locations.Count);
                var latLng = locations[randomIndex];
                markerOptionsList.Add(new MarkerOptions()
                                      .SetPosition(latLng)
                                      .SetTitle(i.ToString())
                                      .SetSnippet($"{formatter.Format(latLng.Latitude)}, {formatter.Format(latLng.Longitude)}"));
            }
            mapboxMap.AddMarkers(markerOptionsList);
        }

        protected override void OnStart()
        {
            base.OnStart();
            mapView.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();
            mapView.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            mapView.OnPause();
        }

        protected override void OnStop()
        {
            base.OnStop();
            mapView.OnStop();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            mapView.OnSaveInstanceState(outState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            mapView.OnDestroy();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            mapView.OnLowMemory();
        }

        public class FabClickListener : Java.Lang.Object, View.IOnClickListener
        {
            TextView viewCountView;
            BulkMarkerActivity parent;

            public FabClickListener(BulkMarkerActivity parent)
            {
                this.parent = parent;
            }

            public void OnClick(View v)
            {
                if (parent.mapboxMap != null)
                    parent.customMarkerView = true;

                // remove fab
                v.Animate().Alpha(0).SetListener(new MyAnimatorListenerAdapter(v))
                 .Start();

                // reload markers
                Spinner spinner = parent.FindViewById<Spinner>(Resource.Id.spinner);
                if (spinner != null)
                {
                    int amount = int.Parse(parent
                                           .Resources
                                           .GetStringArray(Resource.Array.bulk_marker_list)[spinner.SelectedItemPosition]);
                    parent.ShowMarkers(amount);
                }

                viewCountView = parent.FindViewById<TextView>(Resource.Id.countView);

                parent.mapView.AddOnMapChangedListener(new MyOnMapChangedListener(parent, viewCountView));
                parent.mapboxMap.MarkerViewManager.MarkerViewClick += (sender, e) =>
                {
                    Toast.MakeText(parent, $"Hello {(e.P0 as Marker).Id}", ToastLength.Short).Show();
                };

            }
        }

        public class MyOnMapChangedListener : Java.Lang.Object, MapView.IOnMapChangedListener
        {
            BulkMarkerActivity parent;
            TextView viewCountView;

            public MyOnMapChangedListener(BulkMarkerActivity parent, TextView viewCountView)
            {
                this.parent = parent;
                this.viewCountView = viewCountView;
            }

            public void OnMapChanged(int change)
            {
                if (change == MapView.RegionIsChanging || change == MapView.RegionDidChange)
                {
                    if (parent.mapboxMap.MarkerViewManager.MarkerViewAdapters.Any())
                    {
                        viewCountView.Text = Java.Lang.String.Format(Locale.Default, "ViewCache size %d",
                                                                     parent.mapboxMap.MarkerViewManager.MarkerViewContainer.ChildCount);
                    }
                }
            }
        }

        public class MyAnimatorListenerAdapter : AnimatorListenerAdapter
        {
            View v;

            public MyAnimatorListenerAdapter(View v)
            {
                this.v = v;
            }

            public override void OnAnimationEnd(Animator animation)
            {
                base.OnAnimationEnd(animation);
                v.Visibility = ViewStates.Gone;
            }
        }

        public class LoadLocationTask : AsyncTask<Java.Lang.Void, Integer, List<LatLng>>
        {
            BulkMarkerActivity activity;
            int amount;

            public LoadLocationTask(BulkMarkerActivity activity, int amount)
            {
                this.amount = amount;
                this.activity = activity;
            }

            protected override List<LatLng> RunInBackground(params Java.Lang.Void[] @params)
            {
                if (activity != null)
                {
                    string json = null;
                    try
                    {
                        json = GeoParseUtil.LoadStringFormatAssets(activity, "points.geojson");
                    }
                    catch (System.Exception)
                    {
                        Timber.E("Coud not add markers");
                    }

                    if (json != null)
                    {
                        var locations = GeoParseUtil.ParseGeoJsonCoordinates(json);
                        activity.RunOnUiThread(() => activity.OnLatLngListLoaded(locations, amount));

                        return locations;
                    }
                }

                activity.RunOnUiThread(() => activity.OnLatLngListLoaded(null, amount));
                return null;
            }
        }
    }
}
