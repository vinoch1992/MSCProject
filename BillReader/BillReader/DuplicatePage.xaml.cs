using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace BillReader
{
    public partial class DuplicatePage : ContentPage
    {
        List<ImageSource> DuplicateImages;
        public DuplicatePage(string image_id)
        {
            InitializeComponent();
            DuplicateImages = new List<ImageSource>();
            CheckProgressLabel.Text = "Your bill is being scanned for duplicate, please wait...";
            Task.Delay(1000).ContinueWith(t => CheckDuplicate(image_id));
        }

        private async void CheckDuplicate(string image_id)
        {
            HttpClient client = new HttpClient();

            string url = "http://" + MainPage.ip_address + ":5000/duplicate_bill/";
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new RequestDuplicate(new string[] { image_id });
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var result = response.Content.ReadAsStringAsync().Result;
            if (result != null)
            {
                JObject data = JObject.Parse(result);
                JArray array = (JArray)data["response"][image_id];

                for (int i = 0; i < array.Count; i++)
                {
                    Images stored_image = MainPage.StoredImages.Where(m => m.image_id == array[i].ToString()).FirstOrDefault();
                    if (stored_image != null)
                    {
                        var img = ImageSource.FromStream(
                        () => new MemoryStream(Convert.FromBase64String(stored_image.image)));
                        DuplicateImages.Add(img);
                    }
                }
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                if (DuplicateImages.Count > 0)
                {
                    MainListView.ItemsSource = DuplicateImages;
                    CheckProgressLabel.IsVisible = false;
                }
                else
                {
                    CheckProgressLabel.Text = "Duplicates not found.";
                }
            });
        }
    }

    public class RequestDuplicate
    {
        public String[] image_id_list;

        public RequestDuplicate(string[] image)
        {
            image_id_list = image;
        }
    }
}
