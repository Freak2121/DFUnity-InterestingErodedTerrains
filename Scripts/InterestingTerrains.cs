using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections;
using UnityEngine;
using Monobelisk.Compatibility;

namespace Monobelisk
{
    public class InterestingTerrains : MonoBehaviour
    {
        public static readonly TileDataCache tileDataCache = new TileDataCache();

        public static InterestingTerrains instance;
        public static Mod Mod { get; private set; }

        public static Settings settings = new Settings();
        public TerrainComputerParams csParams;
        public static Texture2D biomeMap;
        public static Texture2D derivMap;
        public static Texture2D tileableNoise;
        public static ComputeShader csPrototype;
        public static ComputeShader mainHeightComputer;

        #region Invoke
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Mod = initParams.Mod;
            var go = new GameObject(Mod.Title);
            instance = go.AddComponent<InterestingTerrains>();

            GameManager.Instance.StreamingWorld.TerrainScale = 1f;

            LoadAssetsAndParams();

            ModMessageHandler.Init();

            ConsoleHandler.RegisterConsoleCommands();
        }


        private static void LoadAssetsAndParams()
        {
            Mod.LoadAllAssetsFromBundle();

            biomeMap = Mod.GetAsset<Texture2D>("daggerfall_heightmap");
            derivMap = Mod.GetAsset<Texture2D>("daggerfall_deriv_map");
            tileableNoise = Mod.GetAsset<Texture2D>("tileable_noise");
            csPrototype = Mod.GetAsset<ComputeShader>("TerrainComputer");
            mainHeightComputer = Mod.GetAsset<ComputeShader>("MainHeightmapComputer");

#if UNITY_EDITOR
            instance.csParams = ScriptableObject.CreateInstance<TerrainComputerParams>();
#else
            instance.csParams = new TerrainComputerParams();
#endif

            var paramIni = Mod.GetAsset<TextAsset>("interesting_terrains");
            var ini = new IniParser.Parser.IniDataParser().Parse(paramIni.text);
            instance.csParams.FromIniData(ini);
            TerrainComputer.InitializeWoodsFileHeightmap();
        }
        #endregion

        private void Awake()
        {
            DaggerfallUnity.Instance.TerrainSampler = new InterestingTerrainSampler();

            DaggerfallUnity.Instance.TerrainTexturing = new InterestingTerrainTexturer();

            DaggerfallTerrain.OnPromoteTerrainData += tileDataCache.UncacheTileData;

            Mod.IsReady = true;
            Camera.main.farClipPlane = 10000f;
        }

        private void Start()
        {
            if (CompatibilityUtils.BasicRoadsLoaded)
                BasicRoadsUtils.Init();
        }

        private void OnDestroy()
        {
            TerrainComputer.Cleanup();
        }

        public IEnumerator ClearNoonRoutine()
        {
            DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.Hour = 12;
            DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.Minute = 0;
            DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.Second = 0;
            DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.Day += 1;

            yield return new WaitForSeconds(0.1f);

            Wenzil.Console.Console.ExecuteCommand("set_weather", "0");
            Wenzil.Console.Console.ExecuteCommand("killall");
        }
    }
}

