using System;
using Android.OS;
using Android.Runtime;

namespace MapboxGLAndroidSDKTestApp.Models.Activities
{
    public class Feature : Java.Lang.Object, IParcelable
    {
        public string Name { get; private set; }
        public string Category { get; private set; }

        public string SimpleName
        {
            get
            {
                string[] split = Name.Split(new string[] { "\\." }, StringSplitOptions.None);
                return split[split.Length - 1];
            }
        }

        string label;
        public string Label
        {
            private set { label = value; }
            get { return string.IsNullOrWhiteSpace(label) ? SimpleName : label; }
        }

        string description;
        public string Description
        {
            private set { description = value; }
            get { return string.IsNullOrWhiteSpace(description) ? "-" : description; }
        }

        public Feature(string name, string label, string description, string category)
        {
            Name = name;
            Label = label;
            Description = description;
            Category = category;
        }

        Feature(Parcel parcel)
        {
            Name = parcel.ReadString();
            Label = parcel.ReadString();
            Description = parcel.ReadString();
            Category = parcel.ReadString();
        }

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteString(Name);
            dest.WriteString(Label);
            dest.WriteString(Description);
            dest.WriteString(Category);
        }
    }
}
