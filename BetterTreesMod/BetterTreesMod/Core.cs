using HarmonyLib;
using MelonLoader;
using RumbleModdingAPI;
using RumbleModUI;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.VFX;

[assembly: MelonInfo(typeof(BetterTreesMod.Core), "BetterTreesMod", "1.0.0", "f r o g", null)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]

namespace BetterTreesMod
{
    [HarmonyPatch(typeof(RumbleTrees.Core), "UpdateColours")]
    public class Core : MelonMod
    {
        //variables
        internal static List<GameObject> leafObjects = new List<GameObject>();
        private List<GameObject> rootObjects = new List<GameObject>();
        private List<GameObject> betterLeafObjects = new List<GameObject>();
        private static List<ParticleSystemRenderer> particleSystemRenderers = new List<ParticleSystemRenderer>();
        public static Color bt_Cherry = new Color(0.89f, 0.75f, 0.85f, 1f);
        public static Color bt_Orange = new Color(0.69f, 0.48f, 0.34f, 1f);
        public static Color bt_Leaves = new Color(0.48f, 0.57f, 0.40f, 1f);
        public static Color bt_Red = new Color(0.7f, 0.4f, 0.42f, 1f);
        public static Color bt_Yellow = new Color(0.74f, 0.58f, 0.49f, 1f);
        public static Color bt_selectedColor;
        private int FLG = 1110;

        private GameObject btParentTreeObject;
        private GameObject btBasePrefab;

        private GameObject VFXsObject;
        private bool gotPropertyIDs = false;

        public override void OnInitializeMelon()
        {
            VFXsObject = null;
            if (!gotPropertyIDs)
            {

                FLG = Shader.PropertyToID("Leaf Color Gradient");
                gotPropertyIDs = true;
            }
            LoggerInstance.Msg("Initialized.");
        }

        //Loading asset bundles
        public void LoadAsset()
        {
            AssetBundle bundle = Calls.LoadAssetBundleFromStream(this, "BetterTreesMod.LeavesAsset.leavesassetbundle");
            btParentTreeObject = GameObject.Instantiate(bundle.LoadAsset<GameObject>("AllMapsPrefab"));
            GameObject.DontDestroyOnLoad(btParentTreeObject);
        }
        public override void OnLateInitializeMelon()
        {
            if (btParentTreeObject == null)
                LoadAsset();
        }

        //Runs when a unity scene is loaded, finds all tree locations and then runs a function to disable the original tree and create a particle system in its place

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Loader") return;
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

                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves_001"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves_002"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Foliage/Root_leaves_003"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Gymarena/Leave_sphere__23_"));
                leafObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Main static group/Gymarena/Leave_sphere__24_"));
                GameObject roots = GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group/Scene_roots/Test_root_1_middetail/");
                for (int i = 0; i < roots.transform.childCount; i++)
                {
                    GameObject child = roots.transform.GetChild(i).gameObject;
                    if (child.name != "GymCompRoot") // Because for SOME REASON there is a random empty gameobject here
                        rootObjects.Add(child);
                }
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group(buildings)/Rumble_station/Root"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group(buildings)/School/Cylinder_011"));
                rootObjects.Add(GameObject.Find("--------------SCENE--------------/Gym_Production/Sub static group(buildings)/School/Cylinder_003"));
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

        public void BT_UpdateVFXsColor(Color colorToChangeTo)
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
        }

        public static void BT_UpdateLeafColor(Color colorToChangeTo)
        {
            foreach (ParticleSystemRenderer p in particleSystemRenderers)
            {
                p.material.SetColor("_Color", colorToChangeTo);
            }
        }
    }


    [HarmonyPatch(typeof(RumbleTrees.Core), "UpdateLeafColour")]
    public static class UpdateColorPatch
    {
        private static bool Prefix(ref Color colour)
        {
            Core.BT_UpdateLeafColor(colour);
            return false;
        }
    }

    //Replaces the rumble trees color set code with basically tyhe same thing but using different presets 
    [HarmonyPatch(typeof(RumbleTrees.Core), "setSelectedLeafColour")]
    public static class SelectedColorPatch
    {

        private static bool Prefix(string colour, ref RumbleTrees.Core __instance)
        {
            var rainbowLeafCoroutine = typeof(RumbleTrees.Core).GetField("rainbowLeafCoroutine", BindingFlags.NonPublic | BindingFlags.Instance);
            var isRainbow = typeof(RumbleTrees.Core).GetField("isRainbow", BindingFlags.NonPublic | BindingFlags.Instance);
            var leavesEnabled = typeof(RumbleTrees.Core).GetField("enabled", BindingFlags.NonPublic | BindingFlags.Instance);
            int sceneID = (int)typeof(RumbleTrees.Core).GetField("sceneID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            var SwapLightmap = typeof(RumbleTrees.Core).GetMethod("SwapLightmap", BindingFlags.NonPublic | BindingFlags.Instance);
            var selectedLeafMaterial = typeof(RumbleTrees.Core).GetField("selectedLeafMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
            var selectedLeafColour = typeof(RumbleTrees.Core).GetField("selectedLeafColour", BindingFlags.NonPublic | BindingFlags.Instance);
            MelonLogger.Msg(selectedLeafColour == null);
            var RAINBOWLEAVES = typeof(RumbleTrees.Core).GetMethod("RAINBOWLEAVES", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!(bool)leavesEnabled.GetValue(__instance) && (sceneID == 2 || sceneID == 4))
            {
                foreach (GameObject tree in Core.leafObjects)
                {
                    MeshRenderer renderer = tree.GetComponent<MeshRenderer>();
                    string sceneName = Calls.Scene.GetSceneName();
                    MelonCoroutines.Start(SwapLightmap.Invoke(__instance, new object[] { sceneName, renderer, false }) as IEnumerator);
                }
            }
            MelonLogger.Msg("Got here 3");
            leavesEnabled.SetValue(__instance, true);
            selectedLeafMaterial.SetValue(__instance, "vanilla");
            switch (colour.ToLower())
            {
                case "cherry":
                    {
                        selectedLeafColour.SetValue(__instance, Core.bt_Cherry);
                        break;
                    }
                case "orange":
                    {
                        selectedLeafColour.SetValue(__instance, Core.bt_Orange);
                        break;
                    }
                case "vanilla":
                    {
                        selectedLeafColour.SetValue(__instance, Core.bt_Leaves);
                        break;
                    }
                case "yellow":
                    {
                        selectedLeafColour.SetValue(__instance, Core.bt_Yellow);
                        break;
                    }
                case "red":
                    {
                        selectedLeafColour.SetValue(__instance, Core.bt_Red);
                        break;
                    }
                case "rainbow":
                    isRainbow.SetValue(__instance, true);
                    if (sceneID != -1)
                    {
                        rainbowLeafCoroutine.SetValue(__instance, MelonCoroutines.Start(RAINBOWLEAVES.Invoke(__instance, new object[] { }) as IEnumerator));
                    }
                    break;
            }
            MelonLogger.Msg("Got here 4");
            return false;
        }
    }
    [HarmonyPatch(typeof(RumbleTrees.Core), nameof(RumbleTrees.Core.OnSceneWasLoaded))]
    public static class SceneLoadPatch
    {
        //This reloads the colors from settings on sceneload after bettertrees has loaded the new leaves
        //There is a delay timer because this postfix and the loading of the new trees run at almost exactly the same time, which means occasionally this function runs
        //before rumbletrees has created the leaf models, causing the leaves to never change color 
        private static float waitTime = 0.5f;
        private static void Postfix(int buildIndex, string sceneName, ref RumbleTrees.Core __instance)
        {
            MelonCoroutines.Start(WaitForSceneColors(__instance));
        }


        static IEnumerator WaitForSceneColors(RumbleTrees.Core instance)
        {
            yield return new WaitForSeconds(waitTime);

            if (Calls.Scene.GetSceneName() == "Loader") yield break;
            var field = typeof(RumbleTrees.Core).GetField("RumbleTrees", BindingFlags.NonPublic | BindingFlags.Instance);
            var RumbleTreesSettings = (Mod)field.GetValue(instance);
            var setSelectedLeafColour = typeof(RumbleTrees.Core).GetMethod("setSelectedLeafColour", BindingFlags.NonPublic | BindingFlags.Instance);
            var selectedLeafColour = typeof(RumbleTrees.Core).GetField("selectedLeafColour", BindingFlags.NonPublic | BindingFlags.Instance);

            string setting = (string)RumbleTreesSettings.Settings[6].SavedValue;
            setSelectedLeafColour.Invoke(instance, new object[] { setting });
            MelonLogger.Msg("Finished invoke");
            Core.BT_UpdateLeafColor((Color)selectedLeafColour.GetValue(instance));
        }
    }
}
