using System;
using System.Collections.Generic;
using System.Text;

namespace App.Helpers
{
    public static class StaticConsts
    {
        public static string Times { set; get; } = "Times";
        public static string Dates { set; get; } = "Dates";
        public static string FileName { set; get; } = "Scanned";

        static public string Endpoint = "http://104.237.5.116:5500"; // api/upload
    }
}
