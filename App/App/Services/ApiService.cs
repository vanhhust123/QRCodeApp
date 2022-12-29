using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App.Services
{
    public class ApiService
    {

        // Multil part form-data
        public async Task<HttpResponseMessage> postMultipart(string url, MultipartFormDataContent data)
        {
            // Tạo Httpheader.
            try
            {
                using (HttpClient client = new HttpClient()
                {
                    Timeout = Timeout.InfiniteTimeSpan
                })
                {
                    var result = await client.PostAsync(url, data);
                    return result;
                };
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Low speed of Internet", "It takes too long to upload, please try again later", "OK");
                return null;
            }
        }

    }
}
