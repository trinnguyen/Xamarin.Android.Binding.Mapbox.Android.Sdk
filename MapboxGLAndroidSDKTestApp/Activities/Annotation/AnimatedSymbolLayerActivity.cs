using System;
using System.Collections.Generic;
using Android.Animation;
using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V7.App;
using Android.Views.Animations;
using Com.Mapbox.Geojson;
using Com.Mapbox.Geojson.Additions;
using Com.Mapbox.Mapboxsdk.Geometry;
using Com.Mapbox.Mapboxsdk.Maps;
using Com.Mapbox.Mapboxsdk.Style.Expressions;
using Com.Mapbox.Mapboxsdk.Style.Layers;
using Com.Mapbox.Mapboxsdk.Style.Sources;
using Com.Mapbox.Turf;
using GoogleGson;

namespace MapboxGLAndroidSDKTestApp.Activities.Annotation
{
    /*
     * Test activity showcasing animating MarkerViews.
     */
    [Activity(Name = "com.mapbox.mapboxsdk.testapp.Activities.Annotation.AnimatedSymbolLayerActivity")]
    public class AnimatedSymbolLayerActivity : AppCompatActivity,
    IOnMapReadyCallback
    {
        const string PASSENGER = "passenger";
        const string PASSENGER_LAYER = "passenger-layer";
        const string PASSENGER_SOURCE = "passenger-source";
        const string TAXI = "taxi";
        const string TAXI_LAYER = "taxi-layer";
        const string TAXI_SOURCE = "taxi-source";
        const string RANDOM_CAR_LAYER = "random-car-layer";
        const string RANDOM_CAR_SOURCE = "random-car-source";
        const string RANDOM_CAR_IMAGE_ID = "random-car";
        const string PROPERTY_BEARING = "bearing";
        const string WATERWAY_LAYER_ID = "waterway-label";
        const int DURATION_RANDOM_MAX = 1500;
        const int DURATION_BASE = 3000;

        Random random = new Random();

        MapView mapView;
        MapboxMap mapboxMap;

        List<Car> randomCars = new List<Car>();
        GeoJsonSource randomCarSource;
        Car taxi;
        GeoJsonSource taxiSource;
        LatLng passenger;

        List<Animator> animators = new List<Animator>();

        #region IOnMapReadyCallback
        public void OnMapReady(MapboxMap mapboxMap)
        {
            this.mapboxMap = mapboxMap;
            SetupCars();
            AnimateRandomRoutes();
            AnimateTaxi();
        }
        #endregion

        void SetupCars()
        {
            AddRandomCars();
            AddPassenger();
            AddMainCar();
        }

        void AnimateRandomRoutes()
        {
            Car longestDrive = GetLongestDrive();
            Random random = new Random();

            foreach (Car car in randomCars)
            {
                bool isLongestDrive = longestDrive.Equals(car);
                ValueAnimator valueAnimator = ValueAnimator.OfObject(new LatLngEvaluator(), car.current, car.next);
                valueAnimator.AddUpdateListener(new MyValueAnimatorAnimatorUpdateListener(this, car, isLongestDrive));

                if (isLongestDrive)
                {
                    valueAnimator.AddListener(new MyAnimatorListenerAdapter1(this));
                }

                valueAnimator.AddListener(new MyAnimatorListenerAdapter2(car));

                int offset = random.Next(2) == 0 ? 0 : random.Next(1000) + 250;
                valueAnimator.StartDelay = offset;
                valueAnimator.SetDuration(car.duration - offset);
                valueAnimator.SetInterpolator(new LinearInterpolator());
                valueAnimator.Start();

                animators.Add(valueAnimator);
            }
        }

        void AnimateTaxi()
        {
            ValueAnimator valueAnimator = ValueAnimator.OfObject(new LatLngEvaluator(), taxi.current, taxi.next);
            valueAnimator.AddUpdateListener(new TaxiAnimatorUpdateListener(this, taxi));
            valueAnimator.AddListener(new TaxiAnimatorListenerAdapter1(this));
            valueAnimator.AddListener(new TaxiAnimatorListenerAdapter2(taxi));
            valueAnimator.SetDuration((long)(7 * taxi.current.DistanceTo(taxi.next)));
            valueAnimator.SetInterpolator(new AccelerateDecelerateInterpolator());
            valueAnimator.Start();

            animators.Add(valueAnimator);
        }

        void UpdatePassenger()
        {
            passenger = GetLatLngInBounds();
            UpdatePassengerSource();
            taxi.next = passenger;
        }

        void UpdatePassengerSource()
        {
            GeoJsonSource source = mapboxMap.GetSourceAs(PASSENGER_SOURCE) as GeoJsonSource;
            FeatureCollection featureCollection = FeatureCollection.FromFeatures(new Feature[]
            {
                Feature.FromJson(new FeatureForJson
                {
                    Geometry = new DatasetGeometry
                    {
                        Coordinates = new List<object> { passenger.Longitude, passenger.Latitude },
                        Type = "Point"
                    }
                }.ToJson())
            });
            source.SetGeoJson(featureCollection);
        }

        void UpdateTaxiSource()
        {
            taxi.UpdateFeature();
            taxiSource.SetGeoJson(taxi.feature);
        }

        void UpdateRandomDestinations()
        {
            foreach (Car randomCar in randomCars)
            {
                randomCar.SetNext(GetLatLngInBounds());
            }
        }

        Car GetLongestDrive()
        {
            Car longestDrive = null;
            foreach (Car randomCar in randomCars)
            {
                if (longestDrive == null)
                {
                    longestDrive = randomCar;
                }
                else if (longestDrive.duration < randomCar.duration)
                {
                    longestDrive = randomCar;
                }
            }
            return longestDrive;
        }

        void UpdateRandomCarSource()
        {
            foreach (Car randomCarsRoute in randomCars)
            {
                randomCarsRoute.UpdateFeature();
            }
            randomCarSource.SetGeoJson(FeaturesFromRoutes());
        }

        FeatureCollection FeaturesFromRoutes()
        {
            List<Feature> features = new List<Feature>();
            foreach (Car randomCarsRoute in randomCars)
            {
                features.Add(randomCarsRoute.feature);
            }
            return FeatureCollection.FromFeatures(features);
        }

        long GetDuration()
        {
            return random.Next(DURATION_RANDOM_MAX) + DURATION_BASE;
        }

        void AddRandomCars()
        {
            LatLng latLng;
            LatLng next;

            for (int i = 0; i < 10; i++)
            {
                latLng = GetLatLngInBounds();
                next = GetLatLngInBounds();

                JsonObject properties = new JsonObject();
                properties.AddProperty(PROPERTY_BEARING, (Java.Lang.Number)Car.GetBearing(latLng, next));

                var feature = Feature.FromJson(new FeatureForJson
                {
                    Geometry = new DatasetGeometry
                    {
                        Coordinates = new List<object> { latLng.Longitude, latLng.Latitude },
                        Type = "Point"
                    },
                    Properties = properties
                }.ToJson());

                randomCars.Add(
                    new Car(feature, next, GetDuration(), Point.FromLngLat(latLng.Longitude, latLng.Latitude))
                );
            }

            randomCarSource = new GeoJsonSource(RANDOM_CAR_SOURCE, FeaturesFromRoutes());
            mapboxMap.AddSource(randomCarSource);
            mapboxMap.AddImage(RANDOM_CAR_IMAGE_ID,
              ((BitmapDrawable)Resources.GetDrawable(Resource.Drawable.ic_car_top)).Bitmap);

            SymbolLayer symbolLayer = new SymbolLayer(RANDOM_CAR_LAYER, RANDOM_CAR_SOURCE);
            symbolLayer.WithProperties(
                PropertyFactory.IconImage(RANDOM_CAR_IMAGE_ID),
                PropertyFactory.IconAllowOverlap((Java.Lang.Boolean)true),
                PropertyFactory.IconRotate(Expression.Get(PROPERTY_BEARING)),
                PropertyFactory.IconIgnorePlacement((Java.Lang.Boolean)true)
            );

            mapboxMap.AddLayerBelow(symbolLayer, WATERWAY_LAYER_ID);
        }

        void AddPassenger()
        {
            passenger = GetLatLngInBounds();
            var featureCollection = FeatureCollection.FromFeatures(new Feature[]
            {
                Feature.FromJson(new FeatureForJson
                {
                    Geometry = new DatasetGeometry
                    {
                        Coordinates = new List<object> { passenger.Longitude, passenger.Latitude },
                        Type = "Point"
                    }
                }.ToJson())
            });

            mapboxMap.AddImage(PASSENGER,
                               ((BitmapDrawable)Resources.GetDrawable(Resource.Drawable.icon_burned)).Bitmap);
            var geoJsonSource = new GeoJsonSource(PASSENGER_SOURCE, featureCollection);
            mapboxMap.AddSource(geoJsonSource);

            var symbolLayer = new SymbolLayer(PASSENGER_LAYER, PASSENGER_SOURCE);
            symbolLayer.WithProperties(
                PropertyFactory.IconImage(PASSENGER),
                PropertyFactory.IconIgnorePlacement((Java.Lang.Boolean)true),
                PropertyFactory.IconAllowOverlap((Java.Lang.Boolean)true)
            );
            mapboxMap.AddLayerBelow(symbolLayer, RANDOM_CAR_LAYER);
        }

        void AddMainCar()
        {
            LatLng latLng = GetLatLngInBounds();
            var properties = new JsonObject();
            properties.AddProperty(PROPERTY_BEARING, (Java.Lang.Number)Car.GetBearing(latLng, passenger));
            var feature = Feature.FromJson(new FeatureForJson
            {
                Geometry = new DatasetGeometry
                {
                    Coordinates = new List<object> { latLng.Longitude, latLng.Latitude },
                    Type = "Point"
                },
                Properties = properties
            }.ToJson());
            var featureCollection = FeatureCollection.FromFeatures(new Feature[] { feature });

            taxi = new Car(feature, passenger, GetDuration(), Point.FromLngLat(latLng.Longitude, latLng.Latitude));
            mapboxMap.AddImage(TAXI,
                               ((BitmapDrawable)Resources.GetDrawable(Resource.Drawable.ic_taxi_top)).Bitmap);
            taxiSource = new GeoJsonSource(TAXI_SOURCE, featureCollection);
            mapboxMap.AddSource(taxiSource);

            var symbolLayer = new SymbolLayer(TAXI_LAYER, TAXI_SOURCE);
            symbolLayer.WithProperties(
                PropertyFactory.IconImage(TAXI),
                PropertyFactory.IconRotate(Expression.Get(PROPERTY_BEARING)),
                PropertyFactory.IconAllowOverlap((Java.Lang.Boolean)true),
                PropertyFactory.IconIgnorePlacement((Java.Lang.Boolean)true)
            );
            mapboxMap.AddLayer(symbolLayer);
        }

        LatLng GetLatLngInBounds()
        {
            LatLngBounds bounds = mapboxMap.Projection.VisibleRegion.LatLngBounds;
            Random generator = new Random();
            double randomLat = bounds.LatSouth + generator.NextDouble()
              * (bounds.LatNorth - bounds.LatSouth);
            double randomLon = bounds.LonWest + generator.NextDouble()
              * (bounds.LonEast - bounds.LonWest);
            return new LatLng(randomLat, randomLon);
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

            foreach (Animator animator in animators)
            {
                if (animator != null)
                {
                    animator.RemoveAllListeners();
                    animator.Cancel();
                }
            }

            mapView.OnDestroy();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            mapView.OnLowMemory();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_animated_marker);

            mapView = FindViewById<MapView>(Resource.Id.mapView);
            mapView.OnCreate(savedInstanceState);
            mapView.GetMapAsync(this);
        }

        /*
         * Evaluator for LatLng pairs
         */
        public class LatLngEvaluator : Java.Lang.Object, ITypeEvaluator
        {
            LatLng latLng = new LatLng();

            public Java.Lang.Object Evaluate(float fraction, Java.Lang.Object startValue, Java.Lang.Object endValue)
            {
                latLng.Latitude = (startValue as LatLng).Latitude +
                    (((endValue as LatLng).Latitude - (startValue as LatLng).Latitude) * fraction);
                latLng.Longitude = (startValue as LatLng).Longitude +
                    (((endValue as LatLng).Longitude - (startValue as LatLng).Longitude) * fraction);
                return latLng;
            }
        }

        public class Car
        {
            public Feature feature;
            public LatLng next;
            public LatLng current;
            public long duration;

            public Car(Feature feature, LatLng next, long duration, Point pt)
            {
                this.feature = feature;
                Point point = pt;
                this.current = new LatLng(point.Latitude(), point.Longitude());
                this.duration = duration;
                this.next = next;
            }

            public void SetNext(LatLng next)
            {
                this.next = next;
            }

            public void UpdateFeature()
            {
                var featureForJson = new FeatureForJson
                {
                    Geometry = new DatasetGeometry
                    {
                        Coordinates = new List<object> { current.Longitude, current.Latitude },
                        Type = "Point"
                    }
                };
                feature = Feature.FromJson(featureForJson.ToJson());
                feature.Properties().AddProperty("bearing", (Java.Lang.Number)GetBearing(current, next));
            }

            public static float GetBearing(LatLng from, LatLng to)
            {
                return (float)TurfMeasurement.Bearing(
                    Point.FromLngLat(from.Longitude, from.Latitude),
                    Point.FromLngLat(to.Longitude, to.Latitude)
                );
            }
        }

        public class TaxiAnimatorUpdateListener
            : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
        {
            LatLng latLng;
            AnimatedSymbolLayerActivity parent;
            Car taxi;

            public TaxiAnimatorUpdateListener(AnimatedSymbolLayerActivity parent,
                                             Car taxi)
            {
                this.parent = parent;
                this.taxi = taxi;
            }

            public void OnAnimationUpdate(ValueAnimator animation)
            {
                latLng = animation.AnimatedValue as LatLng;
                taxi.current = latLng;
                parent.UpdateTaxiSource();
            }
        }

        public class MyValueAnimatorAnimatorUpdateListener
            : Java.Lang.Object, ValueAnimator.IAnimatorUpdateListener
        {
            LatLng latLng;
            AnimatedSymbolLayerActivity parent;
            Car car;
            bool isLongestDrive;

            public MyValueAnimatorAnimatorUpdateListener(AnimatedSymbolLayerActivity parent,
                                                         Car car,
                                                         bool isLongestDrive)
            {
                this.parent = parent;
                this.car = car;
                this.isLongestDrive = isLongestDrive;
            }

            public void OnAnimationUpdate(ValueAnimator animation)
            {
                latLng = animation.AnimatedValue as LatLng;
                car.current = latLng;
                if (isLongestDrive)
                    parent.UpdateRandomCarSource();
            }
        }

        public class MyAnimatorListenerAdapter1 : AnimatorListenerAdapter
        {
            AnimatedSymbolLayerActivity parent;

            public MyAnimatorListenerAdapter1(AnimatedSymbolLayerActivity parent)
            {
                this.parent = parent;
            }

            public override void OnAnimationEnd(Animator animation)
            {
                base.OnAnimationEnd(animation);
                parent.UpdateRandomDestinations();
                parent.AnimateRandomRoutes();
            }
        }

        public class MyAnimatorListenerAdapter2 : AnimatorListenerAdapter
        {
            Car car;

            public MyAnimatorListenerAdapter2(Car car)
            {
                this.car = car;
            }

            public override void OnAnimationStart(Animator animation)
            {
                base.OnAnimationStart(animation);
                car.feature.Properties().AddProperty("bearing", (Java.Lang.Number)Car.GetBearing(car.current, car.next));
            }
        }

        public class TaxiAnimatorListenerAdapter1 : AnimatorListenerAdapter
        {
            AnimatedSymbolLayerActivity parent;

            public TaxiAnimatorListenerAdapter1(AnimatedSymbolLayerActivity parent)
            {
                this.parent = parent;
            }

            public override void OnAnimationEnd(Animator animation)
            {
                base.OnAnimationEnd(animation);
                parent.UpdatePassenger();
                parent.AnimateTaxi();
            }
        }

        public class TaxiAnimatorListenerAdapter2 : AnimatorListenerAdapter
        {
            Car taxi;

            public TaxiAnimatorListenerAdapter2(Car taxi)
            {
                this.taxi = taxi;
            }

            public override void OnAnimationStart(Animator animation)
            {
                base.OnAnimationStart(animation);
                taxi.feature.Properties().AddProperty("bearing", (Java.Lang.Number)Car.GetBearing(taxi.current, taxi.next));
            }
        }
    }
}
