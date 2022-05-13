using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ResourceManager
{
    // Used to capture effects applied in the game, not typically tied to anything affecting gameplay. More for feedback flavor
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

        public GameObject BloodSplashLarge
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Splash Large"); }
        }

        public GameObject BloodSplashSmall
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Splash Small"); }
        }

        public GameObject BloodSpurt
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Spurt"); }
        }

        public GameObject Smolder
        {
            get { return effectsBundle.LoadAsset<GameObject>("Smolder"); }
        }

        public GameObject Spark
        {
            get { return effectsBundle.LoadAsset<GameObject>("Spark"); }
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

        public GameObject PoisonPuff
        {
            get { return effectsBundle.LoadAsset<GameObject>("Poison_Puff"); }
        }

        public GameObject FireBurning
        {
            get { return effectsBundle.LoadAsset<GameObject>("Fire_Burning"); }
        }

        public GameObject FireExplosion
        {
            get { return effectsBundle.LoadAsset<GameObject>("Fire_Explosion"); }
        }

        public Material WeaponShine
        {
            get { return effectsBundle.LoadAsset<Material>("Shine"); }
        }

        public Material WeaponIronMaterial
        {
            get { return effectsBundle.LoadAsset<Material>("Iron"); }
        }

        public Material WeaponWarmMaterial
        {
            get { return effectsBundle.LoadAsset<Material>("Warm"); }
        }

        public Material WeaponRustyMaterial
        {
            get { return effectsBundle.LoadAsset<Material>("Rusty"); }
        }
    }

    // Used to manage projectiles produced in the game
    public class ProjectileMananger : Singleton<ProjectileMananger>
    {
        AssetBundle projectileBundle;
        // (Optional) Prevent non-singleton constructor use.
        protected ProjectileMananger() { }

        public void LoadProjectileBundle()
        {
            if (projectileBundle != null)
            {
                Debug.LogWarning("Tried to load projectiles asset bundle but it is already loaded!");
                return;
            }
            projectileBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "projectiles"));
            if (projectileBundle == null)
            {
                Debug.LogError("Failed to load projectiles asset bundle!");
                return;
            }
        }

        public GameObject Roar
        {
            get { return projectileBundle.LoadAsset<GameObject>("Roar"); }
        }

        public GameObject Flame
        {
            get { return projectileBundle.LoadAsset<GameObject>("Flame"); }
        }
    }

    // Used to manage items equipped to the player
    public class PlayerItemMananger : Singleton<PlayerItemMananger>
    {
        AssetBundle playerItemBundle;
        // (Optional) Prevent non-singleton constructor use.
        protected PlayerItemMananger() { }

        public void LoadPlayerItemBundle()
        {
            if (playerItemBundle != null)
            {
                Debug.LogWarning("Tried to load player items asset bundle but it is already loaded!");
                return;
            }
            playerItemBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "playeritems"));
            if (playerItemBundle == null)
            {
                Debug.LogError("Failed to load player items asset bundle!");
                return;
            }
        }

        public GameObject Medicine
        {
            get { return playerItemBundle.LoadAsset<GameObject>("Medicine"); }
        }

        public GameObject FireBomb
        {
            get { return playerItemBundle.LoadAsset<GameObject>("Firebomb"); }
        }
    }
}
