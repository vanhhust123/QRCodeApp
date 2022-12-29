using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace App.Models
{
    public class TextFileInfo
    {
        public string FileName { set; get; }
        public string CreatedTime { set; get; }
        public string CreatedDate { set; get; }
    }

    public class DateFileInfo
    {
        public string CreatedDate{set;get; }
        public ObservableCollection<TextFileInfo> Files { set; get; } = new ObservableCollection<TextFileInfo>(); 
    }
}
