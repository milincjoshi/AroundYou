using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Around_You
{
    class FlickrImage
    {
        //for displaying small image
        public Uri Image320 { get; set; }

        //for displaying large image
        public Uri Image1024 { get; set; }

        //string flickrApiKey = "e424aad8bff9f275cdb417f0eebf3aad";


        private static string getBaseUrl(string FlickrApiKey,string topic, double latitude = double.NaN, double longitude = double.NaN, double radius = double.NaN)
        {
            /* About licenses
                <licenses>
                     <license id="0" name="All Rights Reserved" url="" />
                     <license id="1" name="Attribution-NonCommercial-ShareAlike License" url="http://creativecommons.org/licenses/by-nc-sa/2.0/" />
                     <license id="2" name="Attribution-NonCommercial License" url="http://creativecommons.org/licenses/by-nc/2.0/" />
                     <license id="3" name="Attribution-NonCommercial-NoDerivs License" url="http://creativecommons.org/licenses/by-nc-nd/2.0/" />
                     <license id="4" name="Attribution License" url="http://creativecommons.org/licenses/by/2.0/" />
                     <license id="5" name="Attribution-ShareAlike License" url="http://creativecommons.org/licenses/by-sa/2.0/" />
                     <license id="6" name="Attribution-NoDerivs License" url="http://creativecommons.org/licenses/by-nd/2.0/" />
                     <license id="7" name="No known copyright restrictions" url="http://flickr.com/commons/usage/" />
                     <license id="8" name="United States Government Work" url="http://www.usa.gov/copyright.shtml" />
               </licenses>
                */
            string[] licenses = { "4", "5", "6", "7" };
            string license = String.Join(",", licenses);
            license.Replace(",", "%2C");

            if (!double.IsNaN(latitude))   {latitude = Math.Round(latitude,5);}
            if (!double.IsNaN(longitude))  {longitude = Math.Round(longitude,5); }



            string url = "https://api.flickr.com/services/rest/" +
                "?method=flickr.photos.search" +
                "&license={0}" +
                "&api_key={1}" +
                //"&text={2}" +
               // "&lat={2}" +
               // "&lon={3}" +
               // "&topic={4}" +
               // "&radius=2" +
                "&format=json" +
                "&nojsoncallback=1";
            //"&api_sig=81b3c1ca981ea0f1ddea5c82afdc092d"+

            //api_key=57c6fbfb349da3a1217988416be05ab1



            string baseUrl = string.Format(url,
                license,
               FlickrApiKey,
               // latitude,
               // longitude,
                topic);

            //if (!string.IsNullOrWhiteSpace(topic)) { baseUrl += string.Format("&text=%22{0}%22",topic); }
            if (!double.IsNaN(latitude) && !double.IsNaN(longitude)) { baseUrl += string.Format("&lat={0}&lon={1}",latitude,longitude); }
            if (!double.IsNaN(radius)) { baseUrl += string.Format("&radius={0}", radius); }

            return baseUrl;
        }

        //getting image
        public async static Task<List<FlickrImage>> GetFlickrImages(string FlickrApiKey,string topic, double latitude = double.NaN, double longitude=double.NaN, double radius = double.NaN) {
            
            //1)creating http call client
            HttpClient client = new HttpClient();
            
            //2)creating a string for url
            string baseUrl = getBaseUrl(FlickrApiKey,topic,latitude,longitude,radius);
            
            //3)getting result url and storing it in flickrResult
            string FlickrResult = await client.GetStringAsync(baseUrl);
            
            //4)Conbrting flickrresult in FlickrData
            FlickrData apiData = JsonConvert.DeserializeObject<FlickrData>(FlickrResult);

            //creating a list for storing the result images
            List<FlickrImage> images = new List<FlickrImage>();

            if (apiData.stat == "ok")
            {
                //looping for all images
                foreach (Photo data in apiData.photos.photo)
                {
                    FlickrImage img = new FlickrImage();
                    //for retrieving single photo,use following code
                    //http://farm{farm-id}.staticflickr.com/{server-id}/{id}_{secret}{size}.jpg
                    string photourl = "http://farm{0}.staticflickr.com/{1}/{2}_{3}";
                    string baseflickrurl = string.Format(photourl,
                        data.farm,
                        data.server,
                        data.id,
                        data.secret);

                    //small size image
                    img.Image320 = new Uri(baseflickrurl + "_n.jpg");
                    
                    //llarge size image
                    img.Image1024 = new Uri(baseflickrurl + "_b.jpg");

                    //adding it to Images List of type FlickrImAGE
                    images.Add(img);
                }

            }
            //returning all images
            return images;
            
        }
    }
}
