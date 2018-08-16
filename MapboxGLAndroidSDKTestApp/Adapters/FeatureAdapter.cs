using System;
using Android.Support.V7.Widget;
using Android.Views;
using System.Collections.Generic;
using MapboxGLAndroidSDKTestApp.Models.Activities;
using Android.Widget;
using Android.Graphics;
using MapboxGLAndroidSDKTestApp.Utils;

namespace MapboxGLAndroidSDKTestApp.Adapters
{
    /*
     * Adapter used for FeatureOverviewActivity.
     * <p>
     * Adapts a Feature to a visual representation to be shown in a RecyclerView.
     * </p>
     */
    public class FeatureAdapter : RecyclerView.Adapter
    {
        List<Feature> features;

        public FeatureAdapter(List<Feature> features)
        {
            this.features = features;
        }

        public override int ItemCount => features.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as ViewHolder;
            vh.labelView.Text = features[position].Label;
            vh.descriptionView.Text = features[position].Description;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_main_feature, parent, false);
            return new ViewHolder(view);
        }

        public class ViewHolder : RecyclerView.ViewHolder
        {
            public TextView labelView;
            public TextView descriptionView;

            public ViewHolder(View itemView) : base(itemView)
            {
                Typeface typeface = FontCache.Get("Roboto-Regular.ttf", itemView.Context);
                labelView = itemView.FindViewById<TextView>(Resource.Id.nameView);
                labelView.SetTypeface(typeface, TypefaceStyle.Normal);
                descriptionView = itemView.FindViewById<TextView>(Resource.Id.descriptionView);
                descriptionView.SetTypeface(typeface, TypefaceStyle.Normal);
            }
        }
    }
}
