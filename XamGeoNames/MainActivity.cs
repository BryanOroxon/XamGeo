using Android.App;
using Android.Widget;
using Android.OS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Json;
using System.Net;
using Plugin.Geolocator;
using System;
using System.Threading.Tasks;
using System.IO;
using Android.Graphics;

namespace XamGeoNames
{
    public class Country
    {
        public string continent;
        public string capital;
        public string languages;
        public string geonameId;
        public string south;
        public string isoAlpha3;
        public string north;
        public string fipsCode;
        public string population;
        public string east;
        public string isoNumeric;
        public string areaInSqKm;
        public string countryCode;
        public string west;
        public string countryName;
        public string continentName;
        public string currencyCode;
    }

    public class CountryCodeResult
    {
        public string countryCode;
        public string countryName;
        public string distance;
        public string languages;
    }

    public class CountryInfoResult
    {
        [JsonProperty("geonames")]
        public Country[] country;
    }

    [Activity(Label = "XamGeoNames", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //Instance of webclient for async processing
        WebClient webClient;
        ImageView imageView;
        string url;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            FindViewById<Button>(Resource.Id.getGeoRef).Click += (s, e) =>
            {
                LocateMe();
                
            };
        }

        private async void LocateMe()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;
                var position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);
                if (position == null)
                    return;
                string latitude = position.Latitude.ToString();
                string longitude = position.Longitude.ToString();
                Country country = await GetCountryInfo(latitude.Replace(",", "."), longitude.Replace(",", "."));
                var dPais=FindViewById<TextView>(Resource.Id.DataPais).Text = country.countryName; 
                var dCapital = FindViewById<TextView>(Resource.Id.DataCapital).Text = country.capital;                
                var dContinente = FindViewById<TextView>(Resource.Id.DataContinente).Text = country.continentName;
                var dCountryCode = FindViewById<TextView>(Resource.Id.DataCountryCode).Text = country.countryCode;
                var dLanguagues = FindViewById<TextView>(Resource.Id.DataLanguages).Text = country.languages;
                var dPopulation= FindViewById<TextView>(Resource.Id.DataPopulation).Text = country.population;               
                var im1 = FindViewById<ImageView>(Resource.Id.imageView1);
                var imageBitmap = GetImageBitmapFromUrl("http://flags.fmcdn.net/data/flags/w580/"+country.countryCode.ToLower() + ".png".ToString());
                im1.SetImageBitmap(imageBitmap);
            }
            catch (Exception e)
            {
                Android.Util.Log.Debug("La aplicación no puede Geolocalizar tu Posición", e.ToString());
            }
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            return imageBitmap;
        }

        private async Task<Country> GetCountryInfo(string latitude, string longitude)
        {
            string country = await GetCountryCode(latitude, longitude);
            string url = "http://api.geonames.org/countryInfoJSON?formatted=true&lang=es&country=" + country + "&username=bryanoroxon&style=full";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue json = await Task.Run(() => JsonObject.Load(stream));
                    CountryInfoResult result = JsonConvert.DeserializeObject<CountryInfoResult>(json.ToString());
                    return result.country[0];
                }
            }
        }

        private async Task<string> GetCountryCode(string latitude, string longitude)
        {
            string url = "http://api.geonames.org/countryCodeJSON?formatted=true&lat=" + latitude + "&lng=" + longitude + "&username=rdomingo86&style=full";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    JsonValue json = await Task.Run(() => JsonObject.Load(stream));
                    CountryCodeResult result = JsonConvert.DeserializeObject<CountryCodeResult>(json.ToString());
                    return result.countryCode.ToString();
                }
            }
        }
    }
}

