using DaggerfallWorkshop;
using Unity.Jobs;

namespace Monobelisk
{
    public class InterestingTerrainSampler : TerrainSampler
    {
        public InterestingTerrainSampler()
        {
            HeightmapDimension = defaultHeightmapDimension;
            MaxTerrainHeight = 5000f;
            MeanTerrainHeightScale = 5000f / 255f;
            OceanElevation = 99f;
            BeachElevation = 112.8f;
        }

        public override int Version
        {
            get { return 2; }
        }

        public override bool IsLocationTerrainBlended()
        {
            return true;
        }

        public override void GenerateSamples(ref MapPixelData mapPixel)
        {
            mapPixel.maxHeight = MaxTerrainHeight;
            var computer = TerrainComputer.Create(mapPixel, this);            
            computer.DispatchAndProcess(InterestingTerrains.csPrototype, ref mapPixel, InterestingTerrains.instance.csParams);
        }

        public override JobHandle ScheduleGenerateSamplesJob(ref MapPixelData mapPixel)
        {
            GenerateSamples(ref mapPixel);


            return new JobHandle();
        }
    }
}