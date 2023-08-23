using Landis.SpatialModeling;

namespace Landis.Ecoregions
{
    public class EcoregionPixel : SingleBandPixel<ushort>
    {
        //---------------------------------------------------------------------

        public EcoregionPixel()
            : base()
        {
        }

        public EcoregionPixel(ushort band0)
            : base(band0)
        {
        }
    }
}
