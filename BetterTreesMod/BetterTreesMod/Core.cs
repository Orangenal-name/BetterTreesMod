using MelonLoader;

using RumbleModdingAPI;

using System.Text.Json;
using System.Collections;
using static UnityEngine.Rendering.Universal.UniversalRenderPipeline.Profiling.Pipeline;
using UnityEngine.Experimental.Rendering;
using UnityEngine;
using static RumbleModdingAPI.Calls;
using Il2CppPhoton.Compression;
using static UnityEngine.ParticleSystem;
using HarmonyLib;
using RumbleModUI;
using static Il2CppSystem.Linq.Expressions.Interpreter.NullableMethodCallInstruction;
using Il2CppSystem.Runtime.Remoting.Messaging;
using static MelonLoader.MelonLogger;
using UnityEngine.VFX;

[assembly: MelonInfo(typeof(BetterTreesMod.Core), "BetterTreesMod", "1.0.0", "f r o g", null)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]

namespace BetterTreesMod
{
    [HarmonyPatch(typeof(RUMBLECherryBlossoms.Core), "UpdateColours")]
    public class Core : MelonMod
    {
        //variables
        private List<GameObject> leafObjects = new List<GameObject>();
        private List<GameObject> rootObjects = new List<GameObject>();
        private List<GameObject> betterLeafObjects = new List<GameObject>();
        private List<ParticleSystemRenderer> particleSystemRenderers = new List<ParticleSystemRenderer>();
        public Color bt_Cherry = new Color(0.89f, 0.75f, 0.85f, 1f);
        public Color bt_Orange = new Color(0.69f, 0.48f, 0.34f, 1f);
        public Color bt_Leaves = new Color(0.48f, 0.57f, 0.40f, 1f);
        public Color bt_Red = new Color(0.7f, 0.4f, 0.42f, 1f);
        public Color bt_Yellow = new Color(0.74f, 0.58f, 0.49f, 1f);
        public Color bt_selectedColor;
        private int FLG = 1110;

        private GameObject btParentTreeObject;
        private GameObject btBasePrefab;

        public static Core bt_Core;

        private GameObject VFXsObject;
        private bool gotPropertyIDs;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        //Loading asset bundles
        public void LoadAsset()
        {
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("BetterTreesMod.LeavesAsset.leavesassetbundle"))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                Il2CppAssetBundle bundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                btParentTreeObject = GameObject.Instantiate(bundle.LoadAsset<GameObject>("AllMapsPrefab"));

                GameObject.DontDestroyOnLoad(btParentTreeObject);
            }

        }
        public override void OnLateInitializeMelon()
        {
            Core.bt_Core = this;
        }

        //Runs when a unity scene is loaded, finds all tree locations and then runs a function to disable the original tree and create a particle system in its place

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            VFXsObject = null;
            if (gotPropertyIDs)
            {

                FLG = Shader.PropertyToID("Leaf Color Gradient");
                gotPropertyIDs = true;
            }

                if (btParentTreeObject == null)
                LoadAsset();
            for (int i = 0; i < btParentTreeObject.transform.childCount; i++)
            {
                btParentTreeObject.transform.GetChild(i).gameObject.SetActive(false);
            }
            betterLeafObjects.Clear();
            particleSystemRenderers.Clear();

            leafObjects = new List<GameObject>();
            rootObjects = new List<GameObject>();
            if (sceneName == "Map0")
            {
                //Sets currently used leaves object, for bettertrees, to the Map0 one
                btBasePrefab = btParentTreeObject.transform.GetChild(0).gameObject;

                leafObjects.Add(GameObject.Find("Map0_production/Main static group/leave"));
                rootObjects.Add(GameObject.Find("Map0_production/Main static group/Root"));
            }
            else if (sceneName == "Map1")
            {
                //Sets currently used leaves object, for bettertrees, to the Map1 one
                btBasePrefab = btParentTreeObject.transform.GetChild(1).gameObject;
                leafObjects.Add(GameObject.Find("Map1_production/Main static group/Leaves_Map2"));
                VFXsObject = GameObject.Find("Lighting & Effects/Visual Effects/Falling Leaf VFXs");
            }
            else if (sceneName == "Gym")
            {
                //Sets currently used leaves object, for bettertrees, to the gym one
                btBasePrefab = btParentTreeObject.transform.GetChild(2).gameObject;
                VFXsObject = GameObject.Find("--------------SCENE--------------/Lighting and effects/Visual Effects/Falling Leaf VFXs");

                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves (1)"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves_001 (1)"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves_002 (1)"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves_003 (1)"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Gymarena/Leave_sphere__23_"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Gymarena/Leave_sphere__24_"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group/Scene_roots/Test_root_1_middetail/Cylinder_014__6_"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group/Scene_roots/Test_root_1_middetail/Cylinder_015__1_"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group/Scene_roots/Test_root_1_middetail/Cylinder_015__4__1"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group/Scene_roots/Test_root_1_middetail/Cylinder_018__2_"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group(buildings)/Rumble_station/Root"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group(buildings)/School/Cylinder_011"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Gymarena/Cylinder_015__4_"));
                //sceneID = 3;
            }
            else
            {
                if (!(sceneName == "Park"))
                {
                    return;
                }
                //Sets currently used leaves object, for bettertrees, to the park one
                btBasePrefab = btParentTreeObject.transform.GetChild(3).gameObject;

                leafObjects.Add(GameObject.Find("________________SCENE_________________/Park/Main static group/Leaves/Leave_sphere_park"));
                leafObjects.Add(GameObject.Find("________________SCENE_________________/Park/Main static group/Leaves/Leave_sphere_park_001"));
                leafObjects.Add(GameObject.Find("________________SCENE_________________/Park/Main static group/Leaves/Leave_sphere_park_002"));
                leafObjects.Add(GameObject.Find("________________SCENE_________________/Park/Main static group/Leaves/Leave_sphere_park_003"));
                VFXsObject = GameObject.Find("Lighting and effects/Visual Effects/Falling Leaf VFXs");

                GameObject gameObject = GameObject.Find("________________SCENE_________________/Park/Main static group/Root/");
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    rootObjects.Add(gameObject.transform.GetChild(i).gameObject);
                }
            }
            ReplaceAllTrees();
        }

        //Disables original trees and moves new trees to that new position
        public void ReplaceAllTrees()
        {
            btBasePrefab.active = true;
            if (leafObjects.Count != 0)
            {
                for (int i = 0; i < leafObjects.Count; i++)
                {
                    if (i >= btBasePrefab.transform.childCount)
                        break;
                    betterLeafObjects.Add(btBasePrefab.transform.GetChild(i).gameObject);

                    betterLeafObjects[i].transform.position = leafObjects[i].transform.position;
                    betterLeafObjects[i].transform.rotation = leafObjects[i].transform.rotation;
                    betterLeafObjects[i].transform.localScale = leafObjects[i].transform.localScale;
                }
                //this should be in the other loop but it was giving me trouble with unity explorer so i just moved it here lol
                foreach (GameObject leaf in betterLeafObjects)
                {

                    for (int i = 0; i < leaf.transform.childCount; i++)
                    {
                        particleSystemRenderers.Add(leaf.transform.GetChild(i).gameObject.GetComponent<ParticleSystemRenderer>());
                    }
                }
                for (int i = 0; i < leafObjects.Count; i++)
                {
                    leafObjects[i].GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }

        public void BT_UpdateColors(Color colorToChangeTo)
        {
            if (this.VFXsObject != null)
            {
                for (int i = 0; i < this.VFXsObject.transform.GetChildCount(); i++)
                {
                    VisualEffect vfx = VFXsObject.transform.GetChild(i).gameObject.GetComponent<VisualEffect>();
                    var gradient = new Gradient();

                    var colors = new GradientColorKey[1];
                    colors[0] = new GradientColorKey(colorToChangeTo, 0.0f);


                    var alphas = new GradientAlphaKey[1];
                    alphas[0] = new GradientAlphaKey(1.0f, 0.0f);

                    gradient.SetKeys(colors, alphas);

                    vfx.SetGradient(FLG, gradient);
                }
            }

                    foreach (ParticleSystemRenderer p in particleSystemRenderers)
            {
                p.material.SetColor("_Color", colorToChangeTo);
            }
        }
    }


    [HarmonyPatch(typeof(RUMBLECherryBlossoms.Core), nameof(RUMBLECherryBlossoms.Core.UpdateColours))]
    public static class UpdateColorPatch
    {
        private static bool Prefix(bool reset, string type, RUMBLECherryBlossoms.Core __instance)
        {
            Core.bt_Core.BT_UpdateColors(__instance.selectedLeafColour);
            return false;
        }
    }

    //Replaces the rumble trees color set code with basically tyhe same thing but using different presets 
    [HarmonyPatch(typeof(RUMBLECherryBlossoms.Core), nameof(RUMBLECherryBlossoms.Core.setSelectedColour))]
    public static class SelectedColorPatch
    {

        private static bool Prefix(string colour, bool custom, RUMBLECherryBlossoms.Core __instance)
        {
            if (__instance.rainbowCoroutine != null)
            {
                MelonCoroutines.Stop(__instance.rainbowCoroutine);
            }
            __instance.isRainbow = false;
            if (!__instance.leavesEnabled && (__instance.sceneID == 2 || __instance.sceneID == 4))
            {
                MelonCoroutines.Start(__instance.SwapLightmap(false));
            }
            __instance.leavesEnabled = true;
            __instance.stoneLeaves = "none";
            switch (colour.ToLower())
            {
                case "cherry":
                    {
                        __instance.selectedLeafColour = Core.bt_Core.bt_Cherry;
                        break;
                    }
                case "orange":
                    {
                        __instance.selectedLeafColour = Core.bt_Core.bt_Orange;
                        break;
                    }
                case "vanilla":
                    {
                        __instance.selectedLeafColour = Core.bt_Core.bt_Leaves;
                        break;
                    }
                case "yellow":
                    {
                        __instance.selectedLeafColour = Core.bt_Core.bt_Yellow;
                        break;
                    }
                case "red":
                    {
                        __instance.selectedLeafColour = Core.bt_Core.bt_Red;
                        break;
                    }
                case "rainbow":
                    __instance.isRainbow = true;
                    if (__instance.sceneID != -1)
                    {
                        __instance.rainbowCoroutine = MelonCoroutines.Start(__instance.RAINBOW());
                    }
                    break;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(RUMBLECherryBlossoms.Core), nameof(RUMBLECherryBlossoms.Core.OnSceneWasLoaded))]
    public static class SceneLoadPatch
    {
        //This reloads the colors from settings on sceneload after bettertrees has loaded the new leaves
        //There is a delay timer because this postfix and the loading of the new trees run at almost exactly the same time, which means occasionally this function runs
        //before rumbletrees has created the leaf models, causing the leaves to never change color 
        private static float waitTime = 0.5f;
        private static void Postfix(int buildIndex, string sceneName, RUMBLECherryBlossoms.Core __instance)
        {
            MelonCoroutines.Start(WaitForSceneColors(__instance));
        }


        static IEnumerator WaitForSceneColors(RUMBLECherryBlossoms.Core instance)
        {
            
            yield return new WaitForSeconds(waitTime);
            string setting = (string)instance.RumbleTrees.Settings[6].SavedValue;
            if (instance.checkCustom(setting))
            {
                instance.setCustom(setting, "leaves");
            }
            else
            {
                instance.setSelectedColour(setting, false);
            }
            Core.bt_Core.BT_UpdateColors(instance.selectedLeafColour);
        }
    }
}
