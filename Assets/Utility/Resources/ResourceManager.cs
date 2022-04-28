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

    // Used to manage weapon sprites and effects for equipped weapons to the player
    public class PlayerWeaponMananger : Singleton<PlayerWeaponMananger>
    {
        // 1 handed sword sprite bundles
        AssetBundle oneHandSwordAtkStraightBundle;

        // (Optional) Prevent non-singleton constructor use.
        protected PlayerWeaponMananger() { }

        public void LoadOneHandSwordBundle()
        {
            if (oneHandSwordAtkStraightBundle != null)
            {
                Debug.LogWarning("Tried to load 1h sword attack straight asset bundle but it is already loaded!");
                return;
            }
            oneHandSwordAtkStraightBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "1hswordatkstraight"));
            if (oneHandSwordAtkStraightBundle == null)
            {
                Debug.LogError("Failed to load 1h sword attack straight asset bundle!");
                return;
            }
        }

        public Dictionary<AimDirection, Sprite[]> OneHandedSwordSprites
        {
            get
            {
                return new Dictionary<AimDirection, Sprite[]>{
                    { AimDirection.UP, null },
                    { AimDirection.UP_DIAG, null },
                    { AimDirection.STRAIGHT, oneHandSwordAtkStraightBundle.LoadAllAssets<Sprite>() },
                    { AimDirection.DOWN_DIAG, null },
                    { AimDirection.DOWN, null }
                };
            }
        }
    }
}
