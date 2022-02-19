using System.IO;
using UnityEngine;

namespace ResourceManager
{
    public class EffectsManager : Singleton<EffectsManager>
    {
        AssetBundle effectsBundle;
        // (Optional) Prevent non-singleton constructor use.
        protected EffectsManager() { }

        public void LoadEffectsBundle()
        {
            if (effectsBundle != null)
            {
                Debug.LogWarning("Tried to load effects asset bundle but it is already loaded!");
                return;
            }
            effectsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "effects"));
            if (effectsBundle == null)
            {
                Debug.LogError("Failed to load effects asset bundle!");
                return;
            }
        }

        public Material BloodiedMaterial
        {
            get { return effectsBundle.LoadAsset<Material>("Bloodied"); }
        }

        public GameObject BloodSplash
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Splash"); }
        }

        public GameObject BloodSpurt
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Spurt"); }
        }


        public GameObject Bleeding
        {
            get { return effectsBundle.LoadAsset<GameObject>("Bleeding"); }
        }

        public Material BurningMaterial
        {
            get { return effectsBundle.LoadAsset<Material>("Burning"); }
        }

        public Material PoisonedMaterial
        {
            get { return effectsBundle.LoadAsset<Material>("Poisoned"); }
        }

        public GameObject FireBurning
        {
            get { return effectsBundle.LoadAsset<GameObject>("Fire_Burning"); }
        }

        public GameObject Roar
        {
            get { return effectsBundle.LoadAsset<GameObject>("Roar"); }
        }
    }
}
