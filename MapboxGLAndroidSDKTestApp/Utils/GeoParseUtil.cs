using System;
using Android.Content;
using Android.Text;
using System.IO;
using System.Collections.Generic;
using Com.Mapbox.Mapboxsdk.Geometry;
using Com.Mapbox.Geojson;
using Com.Mapbox.Geojson.Additions;
using GoogleGson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace MapboxGLAndroidSDKTestApp.Utils
{
    public static class GeoParseUtil
    {
        public static string LoadStringFormatAssets(Context context, string fileName)
        {
            string result = string.Empty;

            if (TextUtils.IsEmpty(fileName))
            {
                throw new NullReferenceException("No GeoJSON File Name passed in.");
            }

            using (var stream = context.Assets.Open(fileName))
            {
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            return result;
        }

        public static List<LatLng> ParseGeoJsonCoordinates(string geojsonStr)
        {
            var latLngs = new List<LatLng>();
            var featureCollection = FeatureCollection.FromJson(geojsonStr);

            foreach (var feature in featureCollection.Features())
            {
                if (string.Equals(feature.Geometry().Type(), "Point"))
                {
                    var ff = JsonConvert.DeserializeObject<FeatureForJson>(feature.ToJson());
                    latLngs.Add(new LatLng(Convert.ToDouble(ff.Geometry.Coordinates[1]),
                                           Convert.ToDouble(ff.Geometry.Coordinates[0])));
                }
            }

            return latLngs;
        }
    }
}
