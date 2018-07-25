using System;
using System.Collections.Generic;
using System.Text;

namespace BazarGallery
{
    public static partial class Constants
    {
        public const string Time_AM0 = "شەو";
        public const string Time_AM1 = "بەیانی";
        public const string Time_PM0 = "نیوەڕۆ";
        public const string Time_PM1 = "ئێوارە";
        public static readonly List<string> Days = new List<string>() { "شەممە", "١ شەممە", "٢ شەممە", "٣ شەممە", "٤ شەممە", "٥ شەممە", "هەینی" };
        public static List<string> TimeRanges
        {
            get
            {
                if (_TimeRanges == null)
                {
                    _TimeRanges = new List<string>(48);

                    for (float i = 0; i < 24; i += 0.5f)
                    {
                        string t = String.Empty;
                        string ext = String.Empty;
                        if (i < 4 || i >= 20)
                            ext = Time_AM0;
                        else if (i < 12)
                            ext = Time_AM1;
                        else if (i < 16)
                            ext = Time_PM0;
                        else if (i < 20)
                            ext = Time_PM1;

                        float index = i;
                        if (index >= 13)
                            index -= 12;

                        string min = ((int)((index - ((int)index)) * 60)).ToString("00");
                        if (i < 1)
                            t = $"12:{min} {ext}";
                        else
                            t = $"{(int)index}:{min} {ext}";

                        _TimeRanges.Add(t);
                    }
                }
                return _TimeRanges;
            }
        }
        private static List<string> _TimeRanges;
    }
}
