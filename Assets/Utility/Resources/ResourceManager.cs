﻿using System.IO;
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

        public GameObject BloodSplash
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Splash"); }
        }

        public GameObject BloodSpurt
        {
            get { return effectsBundle.LoadAsset<GameObject>("Blood Spurt"); }
        }

        public GameObject Smolder
        {
            get { return effectsBundle.LoadAsset<GameObject>("Smolder"); }
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

        public GameObject FireExplosion
        {
            get { return effectsBundle.LoadAsset<GameObject>("Fire_Explosion"); }
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

        public GameObject FireBomb
        {
            get { return projectileBundle.LoadAsset<GameObject>("Firebomb"); }
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
}
