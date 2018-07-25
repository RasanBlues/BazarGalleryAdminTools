using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace BazarGallery.Controls
{
    public class CustomMap : Map
    {
        public List<Position> RouteCoordinates { get; private set; }

        public CustomMap()
        {
            RouteCoordinates = new List<Position>();
        }
    }
}
