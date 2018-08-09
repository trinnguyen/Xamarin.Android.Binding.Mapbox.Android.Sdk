# Mapbox Map binding library for Xamarin.Android

### Latest Mapbox library version: ![Maven](https://maven-badges.herokuapp.com/maven-central/com.mapbox.mapboxsdk/mapbox-android-sdk/badge.svg)
### Xamarin Binding library version: ![Nuget](https://img.shields.io/nuget/v/Kwon.Mapbox.Android.Sdk.svg)

## How to get it
Find below package on Nuget.org. Other dependent packages will come with this package.
> Kwon.Mapbox.Android.Sdk

## Guide
First of all, you need an access token to make use of the Mapbox map. Therefore, singing up on Mapbox and getting the access token is necessary. Then, you can copy and paste it into your project.

For example in sample code,
> access_token string value in Resources/values/String.xml

Please see **[Mapbox docs](https://www.mapbox.com/android-docs/maps/overview/)** and **`sample source code`** then you would get used to how to use it.

## Restrictions
Impossible to call Com.Mapbox.Geojson.Feature.FromGeometry() method due to differences between Java and C#. I couldn't find a way to map a Generic java interface to C# code. Instead, you can use FromJson() method for all cases and I provide a FeatureForJson class to easily create feature instances.

For example,
```csharp
var featureForJson = new FeatureForJson
{
    Geometry = new DatasetGeometry
    {
        Coordinates = new List<object> { 1.0, 2.0 },
        Type = "Point"
    }
};

var feature = Feature.FromJson(featureForJson.ToJson());
```

## Contribution
Any contribution is welcome and if you would like to maintain this together, please ask me. Any messages are welcome :)
