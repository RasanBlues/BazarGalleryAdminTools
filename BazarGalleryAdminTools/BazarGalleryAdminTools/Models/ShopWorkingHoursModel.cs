using System;
using System.Collections.Generic;
using System.Text;

namespace BazarGallery.Models
{
    public partial class ShopWorkingHours
    {
        public string ID { get; set; }
        public int OldID { get; set; }
        public string ShopID { get; set; }

        public string SatFrom { get; set; }
        public string SatTo { get; set; }

        public string SunFrom { get; set; }
        public string SunTo { get; set; }

        public string MonFrom { get; set; }
        public string MonTo { get; set; }

        public string TueFrom { get; set; }
        public string TueTo { get; set; }

        public string WedFrom { get; set; }
        public string WedTo { get; set; }

        public string ThuFrom { get; set; }
        public string ThuTo { get; set; }

        public string FriFrom { get; set; }
        public string FriTo { get; set; }

        public int this[int index]
        {
            get
            {
                TimeSpan span = new TimeSpan(0, 0, 0);

                switch (index)
                {
                    case -1:
                        if (string.IsNullOrEmpty(SatFrom))
                            return 16;
                        span = TimeSpan.Parse(SatFrom);
                        break;
                    case 1:
                        if (string.IsNullOrEmpty(SatTo))
                            return 32;
                        span = TimeSpan.Parse(SatTo);
                        break;
                    case -2:
                        if (string.IsNullOrEmpty(SunFrom))
                            return 16;
                        span = TimeSpan.Parse(SunFrom);
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(SunTo))
                            return 32;
                        span = TimeSpan.Parse(SunTo);
                        break;
                    case -3:
                        if (string.IsNullOrEmpty(MonFrom))
                            return 16;
                        span = TimeSpan.Parse(MonFrom);
                        break;
                    case 3:
                        if (string.IsNullOrEmpty(MonTo))
                            return 32;
                        span = TimeSpan.Parse(MonTo);
                        break;

                    case -4:
                        if (string.IsNullOrEmpty(TueFrom))
                            return 16;
                        span = TimeSpan.Parse(TueFrom);
                        break;
                    case 4:
                        if (string.IsNullOrEmpty(TueTo))
                            return 32;
                        span = TimeSpan.Parse(TueTo);
                        break;

                    case -5:
                        if (string.IsNullOrEmpty(WedFrom))
                            return 16;
                        span = TimeSpan.Parse(WedFrom);
                        break;
                    case 5:
                        if (string.IsNullOrEmpty(WedTo))
                            return 32;
                        span = TimeSpan.Parse(WedTo);
                        break;

                    case -6:
                        if (string.IsNullOrEmpty(ThuFrom))
                            return 16;
                        span = TimeSpan.Parse(ThuFrom);
                        break;
                    case 6:
                        if (string.IsNullOrEmpty(ThuTo))
                            return 32;
                        span = TimeSpan.Parse(ThuTo);
                        break;
                    case -7:
                        if (string.IsNullOrEmpty(FriFrom))
                            return 16;
                        span = TimeSpan.Parse(FriFrom);
                        break;
                    case 7:
                        if (string.IsNullOrEmpty(FriTo))
                            return 32;
                        span = TimeSpan.Parse(FriTo);
                        break;
                }

                return (span.Hours * 2) + (span.Minutes / 30);
            }
        }
    }
}
