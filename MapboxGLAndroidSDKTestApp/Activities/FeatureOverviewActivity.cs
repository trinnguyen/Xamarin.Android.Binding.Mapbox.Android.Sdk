using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Java.Lang;
using MapboxGLAndroidSDKTestApp.Adapters;
using MapboxGLAndroidSDKTestApp.Models.Activities;
using MapboxGLAndroidSDKTestApp.Utils;
using TimberLog;

namespace MapboxGLAndroidSDKTestApp.Activities
{
    /*
     * Activity shown when application is started
     * <p>
     * This activity  will generate data for RecyclerView based on the AndroidManifest entries.
     * It uses tags as category and description to order the different entries.
     * </p>
     */
    [Activity(Label = "FeatureOverviewActivity",
              Name = "com.mapbox.mapboxsdk.testapp.Activities.FeatureOverviewActivity",
              MainLauncher = true)]
    public class FeatureOverviewActivity : AppCompatActivity
    {
        const string KEY_STATE_FEATURES = "featureList";

        RecyclerView recyclerView;
        public FeatureSectionAdapter SectionAdapter;
        public List<Feature> Features;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_feature_overview);

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            recyclerView.AddOnItemTouchListener(new RecyclerView.SimpleOnItemTouchListener());
            recyclerView.HasFixedSize = true;

            ItemClickSupport.AddTo(recyclerView).SetOnItemClickListener(new MyOnItemClickListener(this));

            if (savedInstanceState == null)
            {
                LoadFeatures();
            }
            else
            {
                Features = savedInstanceState.GetParcelableArrayList(KEY_STATE_FEATURES).Cast<Feature>().ToList();
                OnFeaturesLoaded(Features);
            }
        }

        void LoadFeatures()
        {
            try
            {
                new LoadFeatureTask(this).Execute(
                    PackageManager.GetPackageInfo(PackageName,
                                                  PackageInfoFlags.Activities | PackageInfoFlags.MetaData));
            }
            catch (PackageManager.NameNotFoundException exception)
            {
                Timber.E(exception, "Could not resolve package info");
            }
        }

        public void OnFeaturesLoaded(List<Feature> featuresList)
        {
            Features = featuresList;
            if (featuresList == null || !featuresList.Any())
                return;

            List<Section> sections = new List<Section>();
            string currentCat = string.Empty;
            for (int i = 0; i < Features.Count; i++)
            {
                string category = Features[i].Category;
                if (!Equals(currentCat, category))
                {
                    sections.Add(new Section(i, category));
                    currentCat = category;
                }
            }

            Section[] dummy = new Section[sections.Count];
            SectionAdapter = new FeatureSectionAdapter(this,
                                                       Resource.Layout.section_main_layout,
                                                       Resource.Id.section_text,
                                                      new FeatureAdapter(Features));
            SectionAdapter.SetSections(sections.ToArray());
            recyclerView.SetAdapter(SectionAdapter);
        }

        public void StartFeature(Feature feature)
        {
            var intent = new Intent();
            intent.SetComponent(new ComponentName(PackageName, feature.Name));
            StartActivity(intent);
        }
    }

    public class LoadFeatureTask : AsyncTask<PackageInfo, Java.Lang.Void, List<Feature>>
    {
        Context context;
        FeatureOverviewActivity parent;

        public LoadFeatureTask(Context context)
        {
            this.context = context;
            parent = context as FeatureOverviewActivity;
        }

        protected override List<Feature> RunInBackground(params PackageInfo[] @params)
        {
            List<Feature> features = new List<Feature>();
            PackageInfo app = @params[0];

            string packageName = context.PackageName;
            string metaDataKey = context.GetString(Resource.String.category);
            foreach (var info in app.Activities)
            {
                if (info.LabelRes != 0 && info.Name.StartsWith(packageName, StringComparison.Ordinal)
                    && !Equals(info.Name, Class.FromType(typeof(FeatureOverviewActivity)).Class.Name))
                {
                    string label = context.GetString(info.LabelRes);
                    string description = ResolveString(info.DescriptionRes);
                    string category = ResolveMetaData(info.MetaData, metaDataKey);
                    features.Add(new Feature(info.Name, label, description, category));
                }
            }

            if (features.Any())
            {
                features.Sort((x, y) =>
                {
                    int result = string.Compare(x.Category, y.Category, true);
                    if (result == 0)
                        result = string.Compare(x.Label, y.Label, true);
                    return result;
                });
            }

            parent.RunOnUiThread(() => parent.OnFeaturesLoaded(features));

            return features;
        }

        string ResolveMetaData(Bundle bundle, string key)
        {
            string category = string.Empty;
            if (bundle != null)
                category = bundle.GetString(key);
            return category;
        }

        string ResolveString(int stringRes)
        {
            try
            {
                return context.GetString(stringRes);
            }
            catch (System.Exception)
            {
                return "-";
            }
        }

        protected override void OnPostExecute(List<Feature> result)
        {
            base.OnPostExecute(result);
            //parent.OnFeaturesLoaded(result);
        }
    }

    public class MyOnItemClickListener : Java.Lang.Object, IOnItemClickListener
    {
        FeatureOverviewActivity parent;

        public MyOnItemClickListener(FeatureOverviewActivity parent)
        {
            this.parent = parent;
        }

        public void OnItemClicked(RecyclerView recyclerView, int position, View view)
        {
            if (!parent.SectionAdapter.IsSectionHeaderPosition(position))
            {
                int itemPosition = parent.SectionAdapter.GetConvertedPosition(position);
                Feature feature = parent.Features[itemPosition];
                parent.StartFeature(feature);
            }
        }
    }
}
