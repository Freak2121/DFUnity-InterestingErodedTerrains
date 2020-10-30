using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monobelisk
{
    public class InterestingTerrains : MonoBehaviour
    {
        public static readonly Dictionary<string, byte[]> tileDataStorage = new Dictionary<string, byte[]>();

        public static InterestingTerrains instance;

        #region Invoke
        public static Mod Mod { get; private set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Mod = initParams.Mod;
            var go = new GameObject(Mod.Title);
            instance = go.AddComponent<InterestingTerrains>();

            GameManager.Instance.StreamingWorld.TerrainScale = 1f;

            LoadAssetsAndParams();

            DaggerfallUnity.Instance.TerrainSampler = new InterestingTerrainSampler();
            DaggerfallUnity.Instance.TerrainTexturing = new InterestingTerrainTexturer();

            ModMessageHandler.Init();

            ConsoleHandler.RegisterConsoleCommands();
        }


        private static void LoadAssetsAndParams()
        {
            Mod.LoadAllAssetsFromBundle();

            biomeMap = Mod.GetAsset<Texture2D>("daggerfall_heightmap");
            derivMap = Mod.GetAsset<Texture2D>("daggerfall_deriv_map");
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

        public static Settings settings = new Settings();
        public TerrainComputerParams csParams;

        public static Texture2D biomeMap;
        public static Texture2D derivMap;
        public static ComputeShader csPrototype;
        public static ComputeShader mainHeightComputer;

        private void Awake()
        {
            Mod.IsReady = true;
            Camera.main.farClipPlane = 10000f;
        }


        private void OnDestroy()
        {
            TerrainComputer.Cleanup();
        }

        public static byte[] GetTileData(int mapPixelX, int mapPixelY)
        {
            var pos = new DFPosition(mapPixelX, mapPixelY).ToString().Trim();

            if (tileDataStorage.ContainsKey(pos))
            {
                var td = tileDataStorage[pos];
                tileDataStorage.Remove(pos);

                return td;
            }

            Debug.LogWarning("==> Interesting Terrains: No tileData found for map pixel " + mapPixelX + "x" + mapPixelY);

            return null;
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

