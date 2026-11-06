using System.Collections.Generic;
using Abissal.Hazards;
using Abissal.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Espalha obstáculos, plantas e animais pelas três zonas, em posições fixas.
    /// Os perigos são prefabs: alterar um prefab atualiza todas as cópias.
    /// ATENÇÃO: gerar de novo substitui o objeto "Povoamento" da cena.
    /// </summary>
    public static class OceanPopulator
    {
        private const string GroupName = "Povoamento";
        private const string SpritesPath = "Assets/_Abissal/Art/Sprites/";
        private const string PrefabsPath = "Assets/_Abissal/Prefabs/";
        private const string MaterialsPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/";

        /// <summary>Distância máxima de busca por chão ou teto, para não entrar em túneis.</summary>
        private const int MaxSurfaceSearch = 20;

        private static readonly Color GlowColor = new Color(0.35f, 1f, 0.85f);

        private static Material litMaterial;
        private static Material unlitMaterial;
        private static HashSet<Vector2Int> blockedCells;

        /// <summary>Retorna o relatório de validação, com os perigos fixos tratados como obstáculos.</summary>
        public static string Populate()
        {
            litMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialsPath + "Sprite-Lit-Default.mat");
            unlitMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialsPath + "Sprite-Unlit-Default.mat");
            blockedCells = new HashSet<Vector2Int>();

            GameObject previous = GameObject.Find(GroupName);

            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }

            var prefabs = new HazardPrefabs
            {
                Spikes = SaveSpikesPrefab(),
                Jellyfish = SaveJellyfishPrefab("AguaViva", "Creatures/jellyfish", litMaterial, false),
                DeepJellyfish = SaveJellyfishPrefab("AguaVivaProfunda", "Creatures/jellyfish_deep", unlitMaterial, true),
                BigFish = SavePredatorPrefab("PeixeGrande", "Creatures/bigfish", new Vector2(2.3f, 1f), new Vector2(0.2f, 0f), litMaterial, false, 9f, 5f, 6.6f),
                Angler = SavePredatorPrefab("PeixeAbissal", "Creatures/angler", new Vector2(1.5f, 1.2f), new Vector2(0.1f, -0.2f), litMaterial, true, 7f, 6f, 5.6f),
                Vent = SaveVentPrefab()
            };

            Transform root = new GameObject(GroupName).transform;
            PopulateShallowZone(NewGroup(root, "Zona_Iluminada"), prefabs);
            PopulateTwilightZone(NewGroup(root, "Zona_Crepusculo"), prefabs);
            PopulateAbyssZone(NewGroup(root, "Zona_Abissal"), prefabs);

            AssetDatabase.SaveAssets();
            return LevelMapGenerator.Validate(blockedCells);
        }

        private class HazardPrefabs
        {
            public GameObject Spikes;
            public GameObject Jellyfish;
            public GameObject DeepJellyfish;
            public GameObject BigFish;
            public GameObject Angler;
            public GameObject Vent;
        }

        private static void PopulateShallowZone(Transform zone, HazardPrefabs prefabs)
        {
            Sprite[] kelp = LoadFrames("Plants/kelp", 4);
            Sprite[] seagrass = LoadFrames("Plants/seagrass", 4);

            foreach (int x in new[] { -34, -30, -25, -18, -12, -3, 12, 22, 28, 36 })
            {
                PlaceAnimatedPlant(zone, "Alga", kelp, x, -40, 3f, Color.white);
            }

            for (int x = 34; x <= 52; x += 2)
            {
                PlaceAnimatedPlant(zone, "Alga_Floresta", kelp, x, -50, 3f, Color.white);
            }

            foreach (int x in new[] { -32, -22, -8, 4, 18, 30, 44 })
            {
                PlaceAnimatedPlant(zone, "Capim", seagrass, x, -42, 3.5f, Color.white);
            }

            PlacePlant(zone, "Coral_Leque", "Plants/coral_fan", -15, -42, Color.white);
            PlacePlant(zone, "Coral_Tubos", "Plants/coral_tube", 9, -42, Color.white);
            PlacePlant(zone, "Coral_Leque", "Plants/coral_fan", 25, -42, Color.white);

            foreach (int x in new[] { -27, -10, 16, 32 })
            {
                PlaceSpikesOnFloor(zone, prefabs.Spikes, x, -42);
            }

            PlaceSpikesOnCeiling(zone, prefabs.Spikes, -20, -42);
            PlaceSpikesOnCeiling(zone, prefabs.Spikes, 30, -38);

            PlaceFish(zone, "Creatures/fish_orange", new Vector2(-20f, -36f), -36f, 20f, 1.4f);
            PlaceFish(zone, "Creatures/fish_orange", new Vector2(-2f, -49f), -30f, 14f, 1.2f);
            PlaceFish(zone, "Creatures/fish_yellow", new Vector2(30f, -36f), 18f, 46f, 1f);
            PlaceFish(zone, "Creatures/fish_yellow", new Vector2(42f, -50f), 34f, 52f, 1.1f);

            for (int i = 0; i < 5; i++)
            {
                PlaceFish(zone, "Creatures/fish_blue", new Vector2(-14f + i, -40f + i % 2), -34f, 16f, 1.9f);
            }

            foreach (Vector2 position in new[] { new Vector2(-26f, -38f), new Vector2(4f, -36f), new Vector2(24f, -41f), new Vector2(40f, -34f) })
            {
                PlaceJellyfish(zone, prefabs.Jellyfish, position);
            }

            PlacePatrol(zone, prefabs.BigFish, new Vector2(0f, -44f), -36f, 20f, 4f);
        }

        private static void PopulateTwilightZone(Transform zone, HazardPrefabs prefabs)
        {
            Sprite[] anemone = LoadFrames("Plants/anemone", 3);
            Sprite[] kelp = LoadFrames("Plants/kelp", 4);
            var dimKelp = new Color(0.55f, 0.7f, 0.85f);

            foreach (int x in new[] { -30, -20, -14, -2, 6, 16, 28 })
            {
                PlaceAnimatedPlant(zone, "Anemona", anemone, x, -92, 2.5f, Color.white);
            }

            foreach (int x in new[] { -42, -34, -24, -10 })
            {
                PlaceAnimatedPlant(zone, "Anemona", anemone, x, -122, 2.5f, Color.white);
            }

            foreach (int x in new[] { -26, -10, 12, 24 })
            {
                PlaceAnimatedPlant(zone, "Alga_Escura", kelp, x, -92, 2f, dimKelp);
            }

            foreach (int x in new[] { -28, -18, -6, 8, 20, 30 })
            {
                PlaceSpikesOnFloor(zone, prefabs.Spikes, x, -92);
            }

            foreach (int x in new[] { -24, -8, 14, 26 })
            {
                PlaceSpikesOnCeiling(zone, prefabs.Spikes, x, -92);
            }

            foreach (int x in new[] { -40, -30, -16, -4 })
            {
                PlaceSpikesOnFloor(zone, prefabs.Spikes, x, -122);
            }

            foreach (int x in new[] { -34, -14, -2 })
            {
                PlaceSpikesOnCeiling(zone, prefabs.Spikes, x, -122);
            }

            for (int i = 0; i < 6; i++)
            {
                PlaceFish(zone, "Creatures/fish_silver", new Vector2(-24f + i * 9f, -88f + i % 3 * 4f), -32f, 32f, 1.6f);
            }

            for (int i = 0; i < 4; i++)
            {
                PlaceFish(zone, "Creatures/fish_silver", new Vector2(-36f + i * 8f, -118f - i % 2 * 5f), -44f, 4f, 1.4f);
            }

            Vector2[] jellyfish =
            {
                new Vector2(-24f, -86f), new Vector2(-8f, -98f), new Vector2(12f, -84f), new Vector2(26f, -96f),
                new Vector2(-30f, -120f), new Vector2(-12f, -126f), new Vector2(0f, -118f)
            };

            foreach (Vector2 position in jellyfish)
            {
                PlaceJellyfish(zone, prefabs.Jellyfish, position);
            }

            PlacePatrol(zone, prefabs.BigFish, new Vector2(0f, -92f), -32f, 32f, 4.5f);
            PlacePatrol(zone, prefabs.BigFish, new Vector2(-20f, -122f), -44f, 4f, 5f);
        }

        private static void PopulateAbyssZone(Transform zone, HazardPrefabs prefabs)
        {
            foreach (int x in new[] { -21, -12, -2, 8, 17, 27, 38 })
            {
                PlaceGlowMushroom(zone, x, -158, x % 2 != 0);
            }

            foreach (int x in new[] { -14, -7, 7, 14 })
            {
                PlaceGlowMushroom(zone, x, -192, true);
            }

            foreach (int x in new[] { -44, -50 })
            {
                PlaceGlowMushroom(zone, x, -150, true);
            }

            foreach (int x in new[] { -18, -6, 12, 24, 36 })
            {
                PlaceSpikesOnFloor(zone, prefabs.Spikes, x, -158);
            }

            foreach (int x in new[] { -14, 2, 20, 34 })
            {
                PlaceSpikesOnCeiling(zone, prefabs.Spikes, x, -158);
            }

            foreach (int x in new[] { -16, 4, 22, 33 })
            {
                PlaceVent(zone, prefabs.Vent, x, -158);
            }

            foreach (int x in new[] { -11, 11 })
            {
                PlaceVent(zone, prefabs.Vent, x, -192);
            }

            for (int i = 0; i < 8; i++)
            {
                SpriteRenderer fish = PlaceFish(zone, "Creatures/fish_glow", new Vector2(-14f + i * 7f, -154f - i % 3 * 3f), -20f, 42f, 2f);
                fish.sharedMaterial = unlitMaterial;
            }

            Vector2[] jellyfish =
            {
                new Vector2(-12f, -160f), new Vector2(6f, -152f), new Vector2(22f, -162f), new Vector2(36f, -156f),
                new Vector2(-6f, -190f), new Vector2(8f, -188f),
                new Vector2(41f, -168f), new Vector2(44f, -172f), new Vector2(48f, -180f)
            };

            foreach (Vector2 position in jellyfish)
            {
                PlaceJellyfish(zone, prefabs.DeepJellyfish, position);
            }

            PlacePatrol(zone, prefabs.Angler, new Vector2(10f, -158f), -20f, 42f, 3.5f);
            PlacePatrol(zone, prefabs.Angler, new Vector2(20f, -150f), -14f, 38f, 3f);
        }

        private static int? FindFloor(int x, int fromY)
        {
            for (int y = fromY; y >= fromY - MaxSurfaceSearch; y--)
            {
                if (!LevelMapGenerator.IsWater(x, y))
                {
                    return y;
                }
            }

            return null;
        }

        private static int? FindCeiling(int x, int fromY)
        {
            for (int y = fromY; y <= fromY + MaxSurfaceSearch; y++)
            {
                if (!LevelMapGenerator.IsWater(x, y))
                {
                    return y;
                }
            }

            return null;
        }

        private static void PlaceAnimatedPlant(Transform parent, string name, Sprite[] frames, int x, int fromY, float fps, Color tint)
        {
            if (FindFloor(x, fromY) is int floor)
            {
                SpriteRenderer plant = CreateSprite(name, parent, frames[0], new Vector3(x + 0.5f, floor + 0.9f), "Scenery", 5, litMaterial);
                plant.color = tint;
                AddFrames(plant.gameObject, frames, fps);
            }
        }

        private static void PlacePlant(Transform parent, string name, string spritePath, int x, int fromY, Color tint)
        {
            if (FindFloor(x, fromY) is int floor)
            {
                SpriteRenderer plant = CreateSprite(name, parent, LoadSprite(spritePath), new Vector3(x + 0.5f, floor + 0.9f), "Scenery", 12, litMaterial);
                plant.color = tint;
            }
        }

        private static void PlaceGlowMushroom(Transform parent, int x, int fromY, bool hasLight)
        {
            if (FindFloor(x, fromY) is int floor)
            {
                SpriteRenderer mushroom = CreateSprite("Cogumelo_Brilhante", parent, LoadSprite("Plants/glow_mushroom"), new Vector3(x + 0.5f, floor + 0.9f), "Scenery", 12, unlitMaterial);

                if (hasLight)
                {
                    AddGlow(mushroom.gameObject, new Vector3(0f, 0.6f, 0f), 4f, 0.7f);
                }
            }
        }

        private static void PlaceSpikesOnFloor(Transform parent, GameObject prefab, int x, int fromY)
        {
            if (FindFloor(x, fromY) is int floor)
            {
                Instantiate(prefab, parent, new Vector3(x + 0.5f, floor + 0.95f));
                BlockAround(x, floor + 1);
                BlockAround(x + 1, floor + 1);
            }
        }

        private static void PlaceSpikesOnCeiling(Transform parent, GameObject prefab, int x, int fromY)
        {
            if (FindCeiling(x, fromY) is int ceiling)
            {
                GameObject spikes = Instantiate(prefab, parent, new Vector3(x + 0.5f, ceiling + 0.05f));
                spikes.transform.localScale = new Vector3(1f, -1f, 1f);
                BlockAround(x, ceiling - 1);
                BlockAround(x + 1, ceiling - 1);
            }
        }

        private static void PlaceVent(Transform parent, GameObject prefab, int x, int fromY)
        {
            if (FindFloor(x, fromY) is int floor)
            {
                Instantiate(prefab, parent, new Vector3(x + 0.5f, floor + 0.95f));

                for (int y = floor + 1; y <= floor + 5; y++)
                {
                    BlockAround(x, y);
                }
            }
        }

        private static void BlockAround(int x, int y)
        {
            blockedCells.Add(new Vector2Int(x, y));
            blockedCells.Add(new Vector2Int(x - 1, y));
        }

        private static SpriteRenderer PlaceFish(Transform parent, string framesPath, Vector2 position, float minX, float maxX, float speed)
        {
            Sprite[] frames = LoadFrames(framesPath, 2);
            SpriteRenderer fish = CreateSprite("Peixe", parent, frames[0], position, "Actors", 1, litMaterial);
            AddFrames(fish.gameObject, frames, 5f);
            ConfigureSwimmer(fish.gameObject.AddComponent<AmbientSwimmer>(), minX, maxX, speed);
            return fish;
        }

        private static void PlaceJellyfish(Transform parent, GameObject prefab, Vector2 position)
        {
            Instantiate(prefab, parent, position);
        }

        private static void PlacePatrol(Transform parent, GameObject prefab, Vector2 position, float minX, float maxX, float speed)
        {
            GameObject predator = Instantiate(prefab, parent, position);
            var serialized = new SerializedObject(predator.GetComponent<PredatorChase>());
            serialized.FindProperty("minX").floatValue = minX;
            serialized.FindProperty("maxX").floatValue = maxX;
            serialized.FindProperty("patrolSpeed").floatValue = speed;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureSwimmer(AmbientSwimmer swimmer, float minX, float maxX, float speed)
        {
            var serialized = new SerializedObject(swimmer);
            serialized.FindProperty("minX").floatValue = minX;
            serialized.FindProperty("maxX").floatValue = maxX;
            serialized.FindProperty("speed").floatValue = speed;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject SaveSpikesPrefab()
        {
            SpriteRenderer spikes = CreateSprite("RochaAfiada", null, LoadSprite("Props/spikes"), Vector3.zero, "Scenery", 11, litMaterial);
            var box = spikes.gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.3f, 0.8f);
            box.offset = new Vector2(0f, 0.45f);
            spikes.gameObject.AddComponent<Hazard>();
            return SavePrefab(spikes.gameObject);
        }

        private static GameObject SaveJellyfishPrefab(string name, string framesPath, Material material, bool glows)
        {
            Sprite[] frames = LoadFrames(framesPath, 4);
            SpriteRenderer jellyfish = CreateSprite(name, null, frames[0], Vector3.zero, "Actors", 2, material);
            AddFrames(jellyfish.gameObject, frames, 4f);
            jellyfish.gameObject.AddComponent<JellyfishMotion>();

            var body = jellyfish.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var circle = jellyfish.gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.55f;
            circle.offset = new Vector2(0f, 0.25f);
            jellyfish.gameObject.AddComponent<Hazard>();

            if (glows)
            {
                AddGlow(jellyfish.gameObject, Vector3.zero, 3.5f, 0.6f);
            }

            return SavePrefab(jellyfish.gameObject);
        }

        private static GameObject SavePredatorPrefab(string name, string framesPath, Vector2 colliderSize, Vector2 colliderOffset, Material material, bool hasLure, float detectionRange, float chaseSeconds, float chaseSpeed)
        {
            Sprite[] frames = LoadFrames(framesPath, 2);
            SpriteRenderer predator = CreateSprite(name, null, frames[0], Vector3.zero, "Actors", 3, material);
            AddFrames(predator.gameObject, frames, 4f);

            var chase = predator.gameObject.AddComponent<PredatorChase>();
            var chaseSettings = new SerializedObject(chase);
            chaseSettings.FindProperty("detectionRange").floatValue = detectionRange;
            chaseSettings.FindProperty("chaseSeconds").floatValue = chaseSeconds;
            chaseSettings.FindProperty("chaseSpeed").floatValue = chaseSpeed;
            chaseSettings.FindProperty("terrainMask").intValue = LayerMask.GetMask("Terrain");
            chaseSettings.ApplyModifiedPropertiesWithoutUndo();

            var body = predator.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;

            var capsule = predator.gameObject.AddComponent<CapsuleCollider2D>();
            capsule.isTrigger = true;
            capsule.direction = CapsuleDirection2D.Horizontal;
            capsule.size = colliderSize;
            capsule.offset = colliderOffset;
            predator.gameObject.AddComponent<Hazard>();

            if (hasLure)
            {
                AddGlow(predator.gameObject, new Vector3(0f, 0.6f, 0f), 5f, 0.9f);
            }

            return SavePrefab(predator.gameObject);
        }

        private static GameObject SaveVentPrefab()
        {
            SpriteRenderer vent = CreateSprite("FonteTermal", null, LoadSprite("Props/vent"), Vector3.zero, "Scenery", 11, litMaterial);

            var box = vent.gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.4f, 4.5f);
            box.offset = new Vector2(0f, 2.6f);
            vent.gameObject.AddComponent<Hazard>();

            var plumeObject = new GameObject("Jato");
            plumeObject.transform.SetParent(vent.transform, false);
            plumeObject.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var plume = plumeObject.AddComponent<ParticleSystem>();
            plume.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = plume.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.startColor = new Color(1f, 0.65f, 0.3f, 0.9f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;

            ParticleSystem.EmissionModule emission = plume.emission;
            emission.rateOverTime = 0f;

            ParticleSystem.ShapeModule shape = plume.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            ParticleSystem.VelocityOverLifetimeModule velocity = plume.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
            velocity.y = new ParticleSystem.MinMaxCurve(4.5f, 6f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            ParticleSystem.TextureSheetAnimationModule sheet = plume.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Sprites;
            sheet.SetSprite(0, LoadSprite("Effects/bubble"));

            var plumeRenderer = plumeObject.GetComponent<ParticleSystemRenderer>();
            plumeRenderer.sharedMaterial = unlitMaterial;
            plumeRenderer.sortingLayerName = "Foreground";

            var thermalVent = vent.gameObject.AddComponent<ThermalVent>();
            var serialized = new SerializedObject(thermalVent);
            serialized.FindProperty("damageZone").objectReferenceValue = box;
            serialized.FindProperty("plume").objectReferenceValue = plume;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(vent.gameObject);
        }

        private static GameObject SavePrefab(GameObject temporary)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temporary, PrefabsPath + temporary.name + ".prefab");
            Object.DestroyImmediate(temporary);
            return prefab;
        }

        private static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            return instance;
        }

        private static Transform NewGroup(Transform parent, string name)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static void AddGlow(GameObject target, Vector3 localPosition, float radius, float intensity)
        {
            var glowObject = new GameObject("Brilho");
            glowObject.transform.SetParent(target.transform, false);
            glowObject.transform.localPosition = localPosition;

            var light = glowObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.pointLightInnerRadius = 0.3f;
            light.pointLightOuterRadius = radius;
            light.color = GlowColor;
            light.intensity = intensity;

            var serialized = new SerializedObject(light);
            SerializedProperty targets = serialized.FindProperty("m_ApplyToSortingLayers");
            SortingLayer[] layers = SortingLayer.layers;
            targets.arraySize = layers.Length;

            for (int i = 0; i < layers.Length; i++)
            {
                targets.GetArrayElementAtIndex(i).intValue = layers[i].id;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent, Sprite sprite, Vector3 position, string sortingLayer, int order, Material material)
        {
            var spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent, false);
            spriteObject.transform.position = position;

            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void AddFrames(GameObject target, Sprite[] frames, float framesPerSecond)
        {
            var animator = target.AddComponent<SpriteFrameAnimator>();
            var serialized = new SerializedObject(animator);
            SerializedProperty list = serialized.FindProperty("frames");
            list.arraySize = frames.Length;

            for (int i = 0; i < frames.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }

            serialized.FindProperty("framesPerSecond").floatValue = framesPerSecond;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];

            for (int i = 0; i < count; i++)
            {
                frames[i] = LoadSprite($"{prefix}_{i}");
            }

            return frames;
        }

        private static Sprite LoadSprite(string relativePath)
        {
            string path = SpritesPath + relativePath + ".png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                throw new System.InvalidOperationException("Sprite não encontrado: " + path);
            }

            return sprite;
        }
    }
}
