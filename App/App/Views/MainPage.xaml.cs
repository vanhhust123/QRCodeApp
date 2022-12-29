using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using ZXing.Net.Mobile.Forms;
using static ZXing.Net.Mobile.Forms.ZXingScannerPage;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Essentials;
using Newtonsoft.Json;
using App.Helpers;
using System.IO;

namespace App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public string Code { set; get; } = "";
        public DateTime Date { set; get; } = DateTime.Now;
        StackLayout stkMainlayout;
        ZXingScannerPage scanPage;
        public MainPage()
        {
            InitializeComponent();
            this.Times = Preferences.Get(StaticConsts.Times, 1);
            Button btnScan = new Button
            {
                Text = "Scan",
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            btnScan.Clicked += async (a, s) =>
            {
                // Cũ 
                scanPage = new ZXingScannerPage(new ZXing.Mobile.MobileBarcodeScanningOptions()
                {
                    DelayBetweenContinuousScans = 0,
                    PossibleFormats = new List<BarcodeFormat>()
                    {
                        BarcodeFormat.CODE_93,
                        BarcodeFormat.CODE_128,
                        BarcodeFormat.CODE_39,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.UPC_EAN_EXTENSION,
                        BarcodeFormat.ITF,
                        BarcodeFormat.QR_CODE
                    },
                    PureBarcode = true, 
                    //UseNativeScanning = true
                });
                this.Times = Preferences.Get(StaticConsts.Times, 1);
                this.Times++;
                Preferences.Set(StaticConsts.Times, this.Times);
                // Add event loop
                scanPage.OnScanResult += this.OnResult;
                await this.Navigation.PushAsync(scanPage);


                // Mới

                //var zxing = new ZXingScannerView
                //{
                //    HorizontalOptions = LayoutOptions.FillAndExpand,
                //    VerticalOptions = LayoutOptions.FillAndExpand,
                //    AutomationId = "zxingScannerView",

                //    Scale = 0.7
                //};
                //zxing.AutoFocus(); 
                //zxing.OnScanResult += this.OnResult;

                //zxing.IsAnalyzing = true;
                //zxing.IsScanning = true; 

                //var overlay = new ZXingDefaultOverlay
                //{


                //    ShowFlashButton = true,
                //    AutomationId = "zxingDefaultOverlay",
                //    Scale = 0.70

                //};
                //overlay.FlashButtonClicked += (sender, e) =>
                //{
                //    zxing.IsTorchOn = !zxing.IsTorchOn;
                //};


                //var grid = new Grid
                //{
                //    VerticalOptions = LayoutOptions.FillAndExpand,
                //    HorizontalOptions = LayoutOptions.FillAndExpand,
                //};

                //grid.Children.Add(zxing);
                //grid.Children.Add(overlay);

                //var page = new ContentPage() { BackgroundColor = Color.Black };
                //page.Content = grid;

                //await this.Navigation.PushAsync(page);
            };



            this.root.Children.Add(btnScan);
        }

        public void OnResult(ZXing.Result result)
        {
            //IBarcodeReader reader = new BarcodeReader();
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (result.Text != this.Code)
                {
                    // Display Snackbar
                    var option = new SnackBarOptions()
                    {
                        MessageOptions = new MessageOptions()
                        {
                            Foreground = Color.White,
                            Message = $"Code: {result.Text} - Asset: {result.Text}"
                        },
                        BackgroundColor = Color.FromHex("#2196F3"),
                        Duration = TimeSpan.FromSeconds(2),
                        Actions = new[] { new SnackBarActionOptions(){
                        Action = () =>{ return Task.CompletedTask; }
                    } }
                    };
                    this.Code = result.Text;
                    this.Date = System.DateTime.Now;

                    // Save file
                    try
                    {
                        this.SaveFile(result.Text);
                    }
                    catch
                    {
                    }
                    try
                    {
                        // Use default vibration length
                        // Or use specified time
                        var duration = TimeSpan.FromSeconds(0.3);
                        Vibration.Vibrate(duration);
                    }
                    catch (FeatureNotSupportedException ex)
                    {
                        // Feature not supported on device
                    }
                    catch (Exception ex)
                    {
                        // Other error has occurred.
                    }
                    await this.DisplaySnackBarAsync(option);





                }
                else if (result.Text == this.Code && DateTime.Now.Subtract(this.Date).TotalSeconds > 2.5)
                {
                    // Display Snackbar
                    var option = new SnackBarOptions()
                    {
                        MessageOptions = new MessageOptions()
                        {
                            Foreground = Color.White,
                            Message = $"Code: {result.Text} - Asset: {result.Text}"
                        },
                        BackgroundColor = Color.FromHex("#2196F3"),
                        Duration = TimeSpan.FromSeconds(2),
                        Actions = new[] { new SnackBarActionOptions(){
                        Action = () =>{ return Task.CompletedTask; }
                    } }
                    };
                    this.Code = result.Text;
                    this.Date = System.DateTime.Now;
                    await this.DisplaySnackBarAsync(option);

                    try
                    {
                        // Use default vibration length
                        // Or use specified time
                        var duration = TimeSpan.FromSeconds(0.3);
                        //Vibration.Vibrate(duration);
                    }
                    catch (FeatureNotSupportedException ex)
                    {
                        // Feature not supported on device
                    }
                    catch (Exception ex)
                    {
                        // Other error has occurred.
                    }
                }

            });
        }


        // Các biến để lưu file. 
        public int Times { set; get; }

        public void SaveFile(string text)
        {
            var datesString = Preferences.Get(StaticConsts.Dates, "");
            if (string.IsNullOrEmpty(datesString))
            {
                datesString = JsonConvert.SerializeObject(new string[] { DateTime.Now.ToString("yyyyMMdd") });
                Preferences.Set(StaticConsts.Dates, datesString);
            }
            var dates = JsonConvert.DeserializeObject<string[]>(datesString);
            dates = dates.OrderByDescending(x => Convert.ToInt32(x)).ToArray();

            // Lấy ra max date, nếu không phải ngày hôm nay thì set max là hôm nay
            var currentDate = dates[0];
            if (currentDate != DateTime.Now.ToString("yyyyMMdd"))
            {
                var list = dates.ToList();
                list.Add(DateTime.Now.ToString("yyyyMMdd"));
                dates = list.ToArray();
                datesString = JsonConvert.SerializeObject(dates);
                currentDate = DateTime.Now.ToString("yyyyMMdd");
                Preferences.Set(StaticConsts.Dates, datesString);
                Preferences.Set(StaticConsts.Times, 1);
                this.Times = 1;
            }


            // Xử lý file 
            var localPath = Path.Combine(FileSystem.CacheDirectory, localFileName(currentDate, this.Times));
            var content = "";
            if (File.Exists(localPath))
            {
                content = File.ReadAllText(localPath, Encoding.UTF8);
            }
            if (!content.Contains($"Code: {text} - Asset: {text}"))
            {
                content += $"Code: {text} - Asset: {text}" + "\n";
                File.WriteAllText(localPath, content, Encoding.UTF8);
            }



        }
        private void Save(object sender, EventArgs e)
        {

        }

        public string localFileName(string date, int times) => $"{StaticConsts.FileName}_{date}_{times.ToString()}.txt";
    }
}