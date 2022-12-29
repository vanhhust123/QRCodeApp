using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using Xamarin.Essentials;
using App.ViewModels;
using System.Collections.ObjectModel;
using App.Models;
using App.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using App.Services;

namespace App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TextPage : ContentPage
    {
        public int count = 1;
        public TextPage()
        {
            InitializeComponent();
            this.BindingContext = new TextPageViewModel() { TextPage = this };
            //localPath = Path.Combine(FileSystem.CacheDirectory, localFileName(count));

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var context = this.BindingContext as TextPageViewModel;
            context.LoadFiles();


        }

        private void Show_Text(object sender, EventArgs e)
        {

        }


        const string templateFileName = "FileSystemTemplate.txt";

        public string localFileName(int count) => $"TheFile_{count.ToString()}.txt";

        string localPath;

        private async void ButtonTemplate_Clicked(object sender, EventArgs e)
        {
            //await MainThread.InvokeOnMainThreadAsync(async () =>
            //{
            //    using (var stream = await FileSystem.OpenAppPackageFileAsync(templateFileName))
            //    {
            //        using (var reader = new StreamReader(stream))
            //        {
            //            labelContent.Text = await reader.ReadToEndAsync();
            //        }
            //    }
            //});


        }

        //private void Load(object sender, EventArgs e)
        //{
        //    //DirectoryInfo d = new DirectoryInfo(FileSystem.CacheDirectory); //Assuming Test is your Folder

        //    //FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files
        //    //string str = "";

        //    //foreach (FileInfo file in Files)
        //    //{
        //    //    str = str + ", " + file.Name;
        //    //}
        //    //CurrentContents.Text = File.ReadAllText(str);

        //    var localPath = Path.Combine(FileSystem.CacheDirectory, $"{CurrentContents.Text}.txt");
        //    labelContent.Text = File.ReadAllText(localPath);
        //}

        //private void Save(object sender, EventArgs e)
        //{
        //    count++;
        //    localPath = Path.Combine(FileSystem.CacheDirectory, localFileName(count));
        //    File.WriteAllText(localPath, CurrentContents.Text);
        //}

        //private void Open_Scan_Page(object sender, EventArgs e)
        //{
        //    Application.Current.MainPage = new NavigationPage(new MainPage());
        //}
    }


    public class TextPageViewModel : BaseViewModel
    {
        public TextPage TextPage { set; get; }
        public TextPageViewModel()
        {
            this.OpenScanPageCommandInitialize();
            this.SendFileCommandInitialize();
            this.ViewDetailCommandInitialize(); 
        }

        public ObservableCollection<DateFileInfo> dates { set; get; } = new ObservableCollection<DateFileInfo>();
        public ObservableCollection<DateFileInfo> Dates
        {
            get { return this.dates; }
            set { this.dates = value; this.OnPropertyChanged(); }
        }

        // Load file từ local, th
        public void LoadFiles()
        {
            this.Dates.Clear();
            var datesString = Preferences.Get(StaticConsts.Dates, "");
            if (string.IsNullOrEmpty(datesString))
            {
                return;
            }
            var dates = JsonConvert.DeserializeObject<string[]>(datesString);
            dates = dates.OrderBy(x => x).ToArray();

            if (dates.Count() != 0)
            {
                dates.ToList().ForEach(date =>
                {
                    DateFileInfo dateInfo = new DateFileInfo();
                    dateInfo.CreatedDate = $"{date.Substring(6, 2)}-{date.Substring(4, 2)}-{date.Substring(0, 4)}";
                    DirectoryInfo d = new DirectoryInfo(FileSystem.CacheDirectory); //Assuming Test is your Folder

                    FileInfo[] Files = d.GetFiles($"{StaticConsts.FileName}_{date}_*.txt"); //Getting Text files
                    foreach (FileInfo file in Files)
                    {
                        dateInfo.Files.Add(new TextFileInfo()
                        {
                            CreatedDate = file.CreationTime.ToString("dd-MM-yyyy"),
                            CreatedTime = file.CreationTime.ToString("HH:mm:ss"),
                            FileName = file.Name
                        });
                    }
                    try
                    {
                        dateInfo.Files.OrderByDescending(x => x.CreatedTime).ToList();
                        this.Dates.Add(dateInfo);
                    }
                    catch { }

                });
            }



        }

        public Command OpenScanPageCommand { set; get; }
        public void OpenScanPageCommandInitialize()
        {
            this.OpenScanPageCommand = new Command(async () =>
            {
                this.IsBusy = true;
                await this.TextPage.Navigation.PushAsync(new MainPage());
                this.IsBusy = false;
            });
        }

        public Command SendFileCommand { set; get; }
        public void SendFileCommandInitialize()
        {
            this.SendFileCommand = new Command(async (object file) =>
            {
                MultipartFormDataContent form = new MultipartFormDataContent();
                using (var stream = System.IO.File.Open(Path.Combine(FileSystem.CacheDirectory, file.ToString()), FileMode.Open))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        this.IsBusy = true;
                        stream.CopyTo(ms);
                        form.Add(new ByteArrayContent(ms.ToArray(), 0, ms.ToArray().Length), "file", file.ToString());

                        var service = new ApiService();
                        var result = await service.postMultipart($"{StaticConsts.Endpoint}/api/upload", form);

                        if (result is null)
                        {
                            this.IsBusy = false;
                            return;
                        }
                        if (result.IsSuccessStatusCode)
                        {
                            var localPath = Path.Combine(FileSystem.CacheDirectory, file.ToString());
                            File.Delete(localPath);
                            this.LoadFiles();

                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", await result.Content.ReadAsStringAsync(), "Ok");
                            this.IsBusy = false;
                        }

                        this.IsBusy = false;
                    }

                }


            });
        }

        public Command ViewDetailCommand { set; get; }
        public void ViewDetailCommandInitialize()
        {
            this.ViewDetailCommand = new Command(async (object fileName) =>
            {
                this.IsBusy = true;
                await this.TextPage.Navigation.PushAsync(new DetailPage(fileName.ToString()));
                this.IsBusy = false; 
            });
        }
    }
}