using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using MapboxGLAndroidSDKTestApp.Utils;

namespace MapboxGLAndroidSDKTestApp.Adapters
{
    public class FeatureSectionAdapter : RecyclerView.Adapter
    {
        const int SECTION_TYPE = 0;

        Context context;
        SparseArray<Section> sections;
        public RecyclerView.Adapter Adapter;

        int sectionRes;
        int textRes;
        public bool Valid = true;

        public FeatureSectionAdapter(Context ctx, int sectionResourceId, int textResourceId, RecyclerView.Adapter baseAdapter)
        {
            context = ctx;
            sectionRes = sectionResourceId;
            textRes = textResourceId;
            Adapter = baseAdapter;
            sections = new SparseArray<Section>();
            Adapter.RegisterAdapterDataObserver(new MyRecyclerViewAdapterDataObserver(this));
        }

        public override int ItemCount => Valid ? Adapter.ItemCount + sections.Size() : 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (IsSectionHeaderPosition(position))
            {
                string cleanTitle = sections.Get(position).Title.Replace("_", " ");
                (holder as SectionViewHolder).Title.Text = cleanTitle;
            }
            else
            {
                Adapter.OnBindViewHolder(holder, GetConvertedPosition(position));
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == SECTION_TYPE)
            {
                View view = LayoutInflater.From(context).Inflate(sectionRes, parent, false);
                return new SectionViewHolder(view, textRes);
            }
            else
            {
                return Adapter.OnCreateViewHolder(parent, viewType - 1);
            }
        }

        public override int GetItemViewType(int position)
        {
            return IsSectionHeaderPosition(position) ? SECTION_TYPE : Adapter.GetItemViewType(GetConvertedPosition(position)) + 1;
        }

        public override long GetItemId(int position)
        {
            return IsSectionHeaderPosition(position)
                ? int.MaxValue - sections.IndexOfKey(position)
                         : Adapter.GetItemId(GetConvertedPosition(position));
        }

        public void SetSections(Section[] sections)
        {
            this.sections.Clear();
            Array.Sort(sections, (x, y) => (x.FirstPosition == y.FirstPosition)
                       ? 0
                       : ((x.FirstPosition < y.FirstPosition) ? -1 : 1));

            int offset = 0;
            foreach (var section in sections)
            {
                section.SectionedPosition = section.FirstPosition + offset;
                this.sections.Append(section.SectionedPosition, section);
                ++offset;
            }

            NotifyDataSetChanged();
        }

        public int GetConvertedPosition(int sectionedPosition)
        {
            if (IsSectionHeaderPosition(sectionedPosition))
                return RecyclerView.NoPosition;

            int offset = 0;
            for (int i = 0; i < sections.Size(); i++)
            {
                if (sections.ValueAt(i).SectionedPosition > sectionedPosition)
                    break;
                --offset;
            }

            return sectionedPosition + offset;
        }

        public bool IsSectionHeaderPosition(int position)
        {
            return sections.Get(position) != null;
        }
    }

    public class SectionViewHolder : RecyclerView.ViewHolder
    {
        public TextView Title;

        public SectionViewHolder(View view, int textRes) : base(view)
        {
            Title = view.FindViewById<TextView>(textRes);
            Title.SetTypeface(FontCache.Get("Roboto-Medium.ttf", view.Context), Android.Graphics.TypefaceStyle.Normal);
        }
    }

    public class Section
    {
        public int FirstPosition;
        public int SectionedPosition;

        public string Title { get; private set; }

        public Section(int firstPosition, string title)
        {
            this.FirstPosition = firstPosition;
            Title = title;
        }
    }

    public class MyRecyclerViewAdapterDataObserver : RecyclerView.AdapterDataObserver
    {
        FeatureSectionAdapter parent;

        public MyRecyclerViewAdapterDataObserver(FeatureSectionAdapter parent)
        {
            this.parent = parent;
        }

        public override void OnChanged()
        {
            parent.Valid = parent.Adapter.ItemCount > 0;
            parent.Adapter.NotifyDataSetChanged();
        }

        public override void OnItemRangeChanged(int positionStart, int itemCount)
        {
            parent.Valid = parent.Adapter.ItemCount > 0;
            parent.Adapter.NotifyItemRangeChanged(positionStart, itemCount);
        }

        public override void OnItemRangeInserted(int positionStart, int itemCount)
        {
            parent.Valid = parent.Adapter.ItemCount > 0;
            parent.Adapter.NotifyItemRangeInserted(positionStart, itemCount);
        }

        public override void OnItemRangeRemoved(int positionStart, int itemCount)
        {
            parent.Valid = parent.Adapter.ItemCount > 0;
            parent.Adapter.NotifyItemRangeRemoved(positionStart, itemCount);
        }
    }
}
