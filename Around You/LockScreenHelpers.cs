using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace Around_You
{
    class LockScreenHelpers
    {
        private const string Iconroot = "Shared/ShellContent/";
        private const string Backgroundroot = "Images/";
        public const string LockScreenData = "LockScreenData.json";

        public static void CleanStorage() {
           
            using (IsolatedStorageFile storagefolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                TryToDeleteAllFiles(storagefolder, Backgroundroot);
                TryToDeleteAllFiles(storagefolder, Iconroot);

                
            }
        }


        public static async Task SetRandomImageFromLocalstorage() {

            string filedata;
            using (IsolatedStorageFile storagefolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storagefolder.FileExists(LockScreenData))
                    return;

                using (IsolatedStorageFileStream stream = storagefolder.OpenFile(LockScreenData,FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        filedata = reader.ReadToEnd();
                    }                    
                }
            }

            List<FlickrImage> images = JsonConvert.DeserializeObject<List<FlickrImage>>(filedata);
            if (images!=null)
            {
                Random rnd = new Random();
                int i = rnd.Next(images.Count);
                Debug.WriteLine(i+"::"+images[i].Image1024);
                await SetImage(images[i].Image1024);
            }
        }

        public static async Task SetImage(Uri uri) { 
        string filename = uri.Segments[uri.Segments.Length-1];
        string imagename = Backgroundroot + filename;
        string iconname = Iconroot + filename;
        using (IsolatedStorageFile storagefolder = IsolatedStorageFile.GetUserStoreForApplication())
        {
            if (!storagefolder.DirectoryExists(Backgroundroot))
                storagefolder.CreateDirectory(Backgroundroot);

            if (!storagefolder.FileExists(imagename))
            {
                using (IsolatedStorageFileStream stream = storagefolder.CreateFile(imagename))
                {
                    HttpClient client = new HttpClient();
                    byte[] flickrresult = await client.GetByteArrayAsync(uri);
                    await stream.WriteAsync(flickrresult,0,flickrresult.Length);
                }
                storagefolder.CopyFile(imagename,iconname);
            }
        }
        await SetLockScreen(filename);
        }


        private static async Task SetLockScreen(string filename) {

            //http://msdn.microsoft.com/en-us/library/windows/apps/jj206968(v=vs.105).aspx


            /*
            <Extensions>
                <Extension ExtensionName="LockScreen_Background" ConsumerID="{111DFF24-AA15-4A96-8006-2BFF8122084F}" TaskID="_default" />
            </Extensions>
            */
            //place above code from above url in "WMAppManifest file" which should be opened with "xml editor" with "Encoding" and "Autodetecting"
            
            //This makes our app a Lockscreen background Provider
 
            //Lockscreen design guidelines
            //http://msdn.microsoft.com/en-us/library/windows/apps/jj662927(v=vs.105).aspx

            bool hasAccessForLockScreen = LockScreenManager.IsProvidedByCurrentApplication;

            if (!hasAccessForLockScreen)
            {
                //if request is not given , this will call prompt the user for permission
                var accessRequested = await LockScreenManager.RequestAccessAsync();

                //only do further work if the access is granted
                hasAccessForLockScreen = (accessRequested == LockScreenRequestResult.Granted);
            }

            if (hasAccessForLockScreen)
            {
                Uri imgUri = new Uri("ms-appdata:///local/" + Backgroundroot + filename,UriKind.Absolute);
                LockScreen.SetImageUri(imgUri);
            }


            var mainTille = ShellTile.ActiveTiles.FirstOrDefault();

            //setting main tile
            if (null != mainTille)
            {
                Uri iconUri = new Uri("isostore:///" + Iconroot +filename,UriKind.Absolute);
                var imgs = new List<Uri>();
                imgs.Add(iconUri);

                CycleTileData tileData = new CycleTileData();
                tileData.CycleImages = imgs;
                //tileData.IconImage = imgUri;

                mainTille.Update(tileData);

            }

        }

        private static void TryToDeleteAllFiles( IsolatedStorageFile storagefolder, string Directory) {
            if (storagefolder.DirectoryExists(Directory))
            {
                try
                {
                    string[] files = storagefolder.GetFileNames(Directory);
                    foreach (var item in files)
                    {
                        storagefolder.DeleteFile(Directory+item); 
                    }
                }
                catch (Exception)
                {
                    //In Use
                    throw;
                }
            }
        
        }

        public static void SaveSelectedBackgroundscreens(List<FlickrImage> data) {
            
            var stringdata = JsonConvert.SerializeObject(data);
            using (var storagefolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storagefolder.CreateFile(LockScreenData))
                {
                    using (StreamWriter writer = new StreamWriter(stream) )
                    {
                        writer.Write(stringdata);
                    }
                }
            }
        }
    }
}
