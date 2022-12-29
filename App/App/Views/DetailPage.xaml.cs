using App.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailPage : ContentPage
    {
        public string FileName { set; get; }
        public DetailPage(string fileName)
        {
            InitializeComponent();
            this.FileName = fileName;
            this.BindingContext = new DetailPageViewModel(fileName) { DetailPage = this };
        }
        protected override void OnAppearing()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var context = this.BindingContext as DetailPageViewModel;
                context.LoadText(this.FileName);
                context.FileName = this.FileName;
            });

            base.OnAppearing();

        }
    }
    public class DetailPageViewModel : BaseViewModel
    {
        public DetailPage DetailPage { set; get; }
        public string FileNamez { set; get; }
        public DetailPageViewModel(string fileName)
        {
            this.FileNamez = fileName;
        }

        public string fileName { set; get; }
        public string FileName
        {
            get { return this.fileName; }
            set
            {
                this.fileName = value;
                this.OnPropertyChanged();
            }
        }

        public string text { set; get; }
        public string Text { get { return this.text; } set { this.text = value; this.OnPropertyChanged(); } }
        public void LoadText(string fileName)
        {
            this.DetailPage.title.Text = fileName;
            var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            var content = "";
            if (File.Exists(localPath))
            {
                content = File.ReadAllText(localPath, Encoding.UTF8);
            }
            this.DetailPage.detail.Text = content;
        }
    }
}