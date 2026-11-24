using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using W = LaTranquera.EditorTools.WorldParts;

namespace LaTranquera.EditorTools
{
    /// <summary>
    /// Menú "La Tranquera > Construir escena Fase 1".
    /// Arma TODA la escena jugable con un clic. Después podés mover y cambiar lo que quieras a mano.
    ///
    /// Plano (metros). La ruta corre de oeste a este sobre Z = 0. Al norte de la ruta:
    ///  - Casa de campo (X -10 a 11, Z 16 a 28) con galería al frente:
    ///      adelante al centro el ALMACÉN, a la izquierda el DORMITORIO, a la derecha el DEPÓSITO;
    ///      atrás el COMEDOR, la COCINA y el BAÑO. Atrás de la casa, el patio con el aljibe.
    ///  - Al oeste, los terrenos para sembrar (Lote 1 es tuyo; 2 y 3 se compran).
    ///  - Al este, la tranquera y el camino de tierra que va al galpón del tambo (al fondo).
    /// </summary>
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/LaTranquera/Scenes/Fase1.unity";
        const float FL = 0.12f;            // altura del piso de la casa y la galería
        const float CT = FL + 0.93f;       // tapa del mostrador
        const float WH = 3.4f;             // altura de las paredes (techos altos de casa vieja)

        static Transform estatico, interactivo;
        static System.Random rnd;
        static readonly List<Light> nightLights = new List<Light>();
        static readonly List<Rect> noGrass = new List<Rect>();

        [MenuItem("La Tranquera/Construir escena Fase 1")]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog("La Tranquera", "Se va a crear una escena nueva (Fase1.unity) con todo el mapa.\nSi ya existe, se reemplaza. ¿Seguimos?", "Sí, construir", "Cancelar"))
                return;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            // Lo que pusiste vos a mano dentro de "MIS COSAS" se guarda y se vuelve a poner
            const string mineName = "MIS COSAS", minePath = "Assets/LaTranquera/MisCosas.prefab";
            var mineOld = GameObject.Find(mineName);
            if (mineOld != null) PrefabUtility.SaveAsPrefabAsset(mineOld, minePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Mats.EnsureFolder(Mats.Folder);
            Mats.EnsureFolder("Assets/LaTranquera/Scenes");
            rnd = new System.Random(42);
            nightLights.Clear();
            noGrass.Clear();

            estatico = new GameObject("Mundo (estático)").transform;
            interactivo = new GameObject("Interactivo").transform;

            try
            {
                Progress("Cielo y luz...", 0.03f);
                var sun = Lighting();
                Progress("Terreno, ruta y caminos...", 0.08f);
                Ground();
                Road();
                Paths();
                Progress("Casa de campo...", 0.2f);
                House();
                Progress("Almacén...", 0.35f);
                var store = Store();
                Progress("Depósito...", 0.45f);
                var dep = Deposito();
                Progress("Comedor, cocina, dormitorio y baño...", 0.5f);
                var bed = Rooms();
                Progress("Patio y aljibe...", 0.6f);
                Patio();
                Progress("Terrenos para sembrar...", 0.65f);
                var crateSpots = Farmland();
                Progress("Camino de tierra, tambo y corrales...", 0.72f);
                var gate = BackField();
                Progress("Alambrados y árboles...", 0.8f);
                Fences();
                Trees();
                Hills();
                Grass();
                Progress("Sistemas y jugador...", 0.9f);
                Systems(store, dep, bed, crateSpots);
                Player();
                sun.GetComponent<DayNightCycle>().nightLights = new List<Light>(nightLights);

                foreach (Transform t in estatico) SetStatic(t.gameObject);

                var minePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(minePath);
                GameObject mine;
                if (minePrefab != null)
                {
                    mine = (GameObject)PrefabUtility.InstantiatePrefab(minePrefab);
                    PrefabUtility.UnpackPrefabInstance(mine, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    mine.name = mineName;
                }
                else mine = new GameObject(mineName);

                EditorSceneManager.SaveScene(scene, ScenePath);
                var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                scenes.RemoveAll(s => s.path == ScenePath);
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            EditorUtility.DisplayDialog("La Tranquera", "¡Listo! Escena guardada en " + ScenePath + ".\nApretá Play para jugar.", "Dale");
        }

        [MenuItem("La Tranquera/Borrar partida guardada")]
        public static void DeleteSave()
        {
            SaveSystem.Delete();
            Debug.Log("Partida borrada: " + SaveSystem.PathFile);
        }

        static void Progress(string msg, float k) => EditorUtility.DisplayProgressBar("La Tranquera", msg, k);

        static void SetStatic(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            foreach (Transform c in go.transform) SetStatic(c.gameObject);
        }

        static Rect R(float x0, float z0, float x1, float z1) => new Rect(x0, z0, x1 - x0, z1 - z0);

        /// <summary>Capa plana en el piso (camino, ripio) con textura.</summary>
        static void Flat(string name, float x0, float z0, float x1, float z1, Material m, float h = 0.02f, bool grassFree = true)
        {
            W.TBoxB(estatico, name, new Vector3((x0 + x1) / 2, 0, (z0 + z1) / 2), new Vector3(x1 - x0, h, z1 - z0), m, 0, false);
            if (grassFree) noGrass.Add(R(x0 - 0.3f, z0 - 0.3f, x1 + 0.3f, z1 + 0.3f));
        }

        static float Rf => (float)rnd.NextDouble();

        /// <summary>Pone un modelo de Resources/Modelos (si existe). Devuelve null si no está.</summary>
        static GameObject Mdl(Transform p, string res, string name, Vector3 pos, float yaw, bool collider = false, float scale = 1f)
        {
            var g = ModelLib.Place(p, "Modelos/" + res, name, pos, yaw, scale);
            if (g != null && collider) ModelLib.FitCollider(g);
            return g;
        }

        static void Fan(Transform p, Vector3 pos)
        {
            var f = Mdl(p, "VentiladorTecho", "VentiladorDeTecho", pos, 0);
            if (f != null) f.AddComponent<Spin>().degreesPerSecond = 200f;
        }

        // ------------------------------------------------------------------
        static GameObject Lighting()
        {
            var sky = Mats.Folder + "/Cielo.mat";
            var skyMat = AssetDatabase.LoadAssetAtPath<Material>(sky);
            if (skyMat == null)
            {
                skyMat = new Material(Shader.Find("Skybox/Procedural"));
                skyMat.SetFloat("_SunSize", 0.04f);
                skyMat.SetFloat("_AtmosphereThickness", 1.0f);
                skyMat.SetColor("_SkyTint", new Color(0.5f, 0.6f, 0.75f));
                skyMat.SetFloat("_Exposure", 1.2f);
                AssetDatabase.CreateAsset(skyMat, sky);
            }
            RenderSettings.skybox = skyMat;

            var ls = new LightingSettings { bakedGI = false, realtimeGI = false };
            var lsPath = "Assets/LaTranquera/Generated/Iluminacion.lighting";
            AssetDatabase.DeleteAsset(lsPath);
            AssetDatabase.CreateAsset(ls, lsPath);
            Lightmapping.lightingSettings = ls;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.72f);

            var sun = new GameObject("Sol");
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.3f;
            l.shadows = LightShadows.Soft;
            l.color = new Color(1f, 0.96f, 0.9f);
            sun.transform.rotation = Quaternion.Euler(50, 30, 0);
            sun.AddComponent<DayNightCycle>();
            RenderSettings.sun = l;
            return sun;
        }

        static void Ground()
        {
            var g = W.TBoxB(estatico, "Piso", new Vector3(0, -0.2f, 0), new Vector3(1400, 0.2f, 1400), TexKit.Mat("pasto", Color.white), 0, true);
            g.name = "Piso";
            // Campo sembrado del otro lado de la ruta (surcos de soja)
            var campo = Shapes.Empty(estatico, "CampoSembrado", Vector3.zero).transform;
            W.TBoxB(campo, "Tierra", new Vector3(0, 0, -55), new Vector3(240, 0.02f, 70), TexKit.Mat("tierraArada", Color.white), 0, false);
            for (int i = 0; i < 34; i++)
                Shapes.Box(campo, "Surco", new Vector3(0, 0.02f, -22f - i * 2f), new Vector3(238, 0.22f, 0.9f), new Color(0.33f, 0.52f, 0.2f), false);
            noGrass.Add(R(-120, -90, 120, -20));
        }

        static void Road()
        {
            var road = Shapes.Empty(estatico, "Ruta", Vector3.zero).transform;
            // asfalto con las líneas pintadas, huellas, grietas y parches (una sola textura a lo ancho)
            W.TBoxB(road, "Asfalto", Vector3.zero, new Vector3(801, 0.04f, 7.2f), TexKit.MatST("ruta", Color.white, new Vector2(9f, 7.2f), 0.25f), 0, false);
            ElectricLine(road);
            Mojon(road, new Vector3(-24, 0, 6.0f), "KM\n214");
            Mojon(road, new Vector3(76, 0, 6.0f), "KM\n215");
            RoadSign(road, new Vector3(-40, 0, 6.2f), 180, "60", true);
            RoadSign(road, new Vector3(42, 0, -6.2f), 0, "60", true);
            RoadSign(road, new Vector3(-28, 0, -6.2f), 0, "ALMACÉN\nA 200 m", false);
            W.TBoxB(road, "BanquinaN", new Vector3(0, 0, 4.6f), new Vector3(800, 0.03f, 2f), TexKit.Mat("ripio", Color.white), 0, false);
            W.TBoxB(road, "BanquinaS", new Vector3(0, 0, -4.6f), new Vector3(800, 0.03f, 2f), TexKit.Mat("ripio", Color.white), 0, false);
            noGrass.Add(R(-400, -5.7f, 400, 5.7f));

            foreach (float x in new float[] { -90, -60, -30, 0, 30, 60, 90 })
            {
                var l = W.StreetLight(road, new Vector3(x, 0, 5.4f), Vector3.back, Mathf.Abs(x) <= 30);
                if (l != null) nightLights.Add(l);
            }
            W.Sign(road, new Vector3(-16, 0, 5.2f), 180, "ALMACÉN LA TRANQUERA\nVerduras frescas · 50 m", new Vector2(2.4f, 0.9f));
            W.Sign(road, new Vector3(24, 0, -5.2f), 0, "RUTA PROVINCIAL 5", new Vector2(1.8f, 0.5f));
        }

        /// <summary>Tendido eléctrico rural: postes de madera con cruceta, aisladores y 3 cables que cuelgan.</summary>
        static void ElectricLine(Transform p)
        {
            var t = Shapes.Empty(p, "Tendido electrico", Vector3.zero).transform;
            var wood = TexKit.Mat("madera", new Color(0.7f, 0.62f, 0.52f));
            var wire = new Color(0.08f, 0.08f, 0.08f);
            float z = -7f, h = 8.5f;
            float prev = float.NaN;
            for (float x = -270; x <= 270; x += 45)
            {
                var b = new MeshKit.Builder(1);
                MeshKit.AddCylinder(b, 0, Matrix4x4.identity, 0.16f, 0.11f, h, 8);
                MeshKit.AddBox(b, 0, new Vector3(0, h - 0.4f, 0), new Vector3(0.12f, 0.12f, 1.8f));
                MeshKit.Object(t, "Poste", MeshKit.Persist("posteLuz", () => b.Build()), new[] { wood }, new Vector3(x, 0, z));
                foreach (var dz in new[] { -0.75f, 0f, 0.75f })
                    Shapes.Cyl(t, "Aislador", new Vector3(x, h - 0.34f, z + dz), 0.07f, 0.12f, new Color(0.75f, 0.85f, 0.75f), false);
                if (!float.IsNaN(prev))
                    foreach (var dz in new[] { -0.75f, 0f, 0.75f })
                    {
                        Vector3 last = new Vector3(prev, h - 0.2f, z + dz);
                        for (int k = 1; k <= 6; k++)
                        {
                            float u = k / 6f;
                            var pt = new Vector3(Mathf.Lerp(prev, x, u), h - 0.2f - Mathf.Sin(u * Mathf.PI) * 0.9f, z + dz);
                            Shapes.Beam(t, "Cable", last, pt, 0.02f, wire);
                            last = pt;
                        }
                    }
                prev = x;
            }
        }

        static void Mojon(Transform p, Vector3 pos, string text)
        {
            var t = Shapes.Empty(p, "Mojon", pos, new Vector3(0, 180, 0)).transform;
            W.TBoxB(t, "Cuerpo", Vector3.zero, new Vector3(0.35f, 0.9f, 0.18f), W.Cemento);
            Shapes.Box(t, "Franja", new Vector3(0, 0.62f, 0), new Vector3(0.36f, 0.25f, 0.19f), Color.white, false);
            WorldLabel.Create(t, text, new Vector3(0, 0.74f, 0.1f), new Vector2(0.33f, 0.24f), 80, Color.black, Vector3.forward);
            WorldLabel.Create(t, text, new Vector3(0, 0.74f, -0.1f), new Vector2(0.33f, 0.24f), 80, Color.black, Vector3.back);
        }

        /// <summary>Cartel vial en caño: redondo de velocidad máxima o rectangular informativo.</summary>
        static void RoadSign(Transform p, Vector3 pos, float yaw, string text, bool speed)
        {
            var t = Shapes.Empty(p, "Señal", pos, new Vector3(0, yaw, 0)).transform;
            Shapes.Cyl(t, "Caño", Vector3.zero, 0.07f, 2.4f, new Color(0.6f, 0.6f, 0.62f), true);
            if (speed)
            {
                var red = Shapes.Cyl(t, "Aro", new Vector3(0, 2.4f, 0), 0.75f, 0.03f, new Color(0.8f, 0.1f, 0.1f), false);
                red.transform.localPosition = new Vector3(0, 2.4f, 0.05f); red.transform.localEulerAngles = new Vector3(90, 0, 0);
                var white = Shapes.Cyl(t, "Fondo", new Vector3(0, 2.4f, 0), 0.6f, 0.03f, Color.white, false);
                white.transform.localPosition = new Vector3(0, 2.4f, 0.07f); white.transform.localEulerAngles = new Vector3(90, 0, 0);
                WorldLabel.Create(t, text, new Vector3(0, 2.4f, 0.09f), new Vector2(0.55f, 0.4f), 300, Color.black, Vector3.forward);
            }
            else
            {
                Shapes.BoxC(t, "Chapa", new Vector3(0, 2.4f, 0.05f), new Vector3(1.4f, 0.8f, 0.03f), new Color(0.1f, 0.35f, 0.65f), false);
                WorldLabel.Create(t, text, new Vector3(0, 2.4f, 0.07f), new Vector2(1.3f, 0.7f), 200, Color.white, Vector3.forward);
            }
        }

        static void Paths()
        {
            var tierra = TexKit.Mat("tierra", Color.white);
            var ripio = TexKit.Mat("ripio", Color.white);
            Flat("Estacionamiento (ripio)", -10f, 5.6f, 14f, 13.2f, ripio, 0.03f);
            Flat("Camino a los terrenos", -15f, 13.6f, -10.5f, 15.2f, tierra);
            Flat("Camino a los terrenos 2", -15f, 15.2f, -13.6f, 23.5f, tierra);
            Flat("Camino de tierra (tranquera)", 16f, 3.6f, 20f, 64f, tierra, 0.025f);
            Flat("Playón del depósito", 11.2f, 17f, 16f, 26f, tierra, 0.02f);
            Flat("Patio de tierra", -10.5f, 28.3f, 11.5f, 38f, tierra, 0.015f);
            Flat("Playón del tambo", 8f, 50f, 32f, 56f, tierra, 0.02f);
        }

        // ------------------------------------------------------------------
        /// <summary>El cascarón de la casa: pisos, paredes, techo de chapa a dos aguas, galería con columnas.</summary>
        static void House()
        {
            var h = Shapes.Empty(estatico, "Casa de campo", Vector3.zero).transform;
            var rev = W.Revoque;
            noGrass.Add(R(-11f, 12.8f, 12f, 28.6f));

            // Pisos (todos a la altura FL)
            W.TBoxB(h, "Piso almacén", new Vector3(0, 0, 19), new Vector3(8, FL, 6), W.Calcareo);
            W.TBoxB(h, "Piso dormitorio", new Vector3(-7, 0, 19), new Vector3(6, FL, 6), W.Madera);
            W.TBoxB(h, "Piso comedor", new Vector3(0, 0, 25), new Vector3(8, FL, 6), W.Calcareo);
            W.TBoxB(h, "Piso cocina", new Vector3(-7, 0, 25), new Vector3(6, FL, 6), W.Calcareo);
            W.TBoxB(h, "Piso depósito", new Vector3(7.5f, 0, 22), new Vector3(7, FL, 12), W.Cemento);
            W.TBoxB(h, "Piso galería", new Vector3(0.5f, 0, 14.6f), new Vector3(21.6f, FL, 2.8f), TexKit.Mat("calcareo", new Color(0.9f, 0.85f, 0.85f)));
            W.TBoxB(h, "Escalón", new Vector3(0.5f, 0, 13.1f), new Vector3(21.6f, FL * 0.5f, 0.3f), W.Ladrillo);

            float y0 = FL;
            Vector3 P(float x, float z) => new Vector3(x, y0, z);
            const float T = 0.25f; // paredes gruesas de ladrillo
            // Frente (de este a oeste para que la derecha sea "afuera" = sur)
            W.Wall(h, "Frente depósito", P(11, 16), P(4, 16), WH, T, rev, false, new W.Opening(2f, 4f, 1.0f, 2.3f, true));
            W.Wall(h, "Frente almacén", P(4, 16), P(-4, 16), WH, T, rev, false,
                new W.Opening(0.8f, 2.8f, 1.0f, 2.4f, true), new W.Opening(3.3f, 4.7f, 0, 2.6f), new W.Opening(5.2f, 7.2f, 1.0f, 2.4f, true));
            W.Wall(h, "Frente dormitorio", P(-4, 16), P(-10, 16), WH, T, rev, false, new W.Opening(2.0f, 3.6f, 1.0f, 2.4f, true));
            // Contrafrente (atrás)
            W.Wall(h, "Fondo", P(-10, 28), P(11, 28), WH, T, rev, false,
                new W.Opening(1.0f, 2.0f, 1.3f, 2.2f, true), new W.Opening(4.2f, 5.6f, 1.15f, 2.4f, true), new W.Opening(9.5f, 10.5f, 0, 2.4f), new W.Opening(18.5f, 20f, 1.1f, 2.3f, true));
            // Costados
            W.Wall(h, "Costado oeste", P(-10, 28), P(-10, 16), WH, T, rev, true, new W.Opening(1.0f, 1.9f, 1.4f, 2.2f, true), new W.Opening(8.2f, 9.6f, 1.2f, 2.4f, true));
            W.Wall(h, "Costado este (portón)", P(11, 16), P(11, 28), WH, T, rev, true, new W.Opening(3.5f, 7.5f, 0, 3.0f));
            // Paredes interiores
            W.Wall(h, "Almacén|Depósito", P(4, 16), P(4, 28), WH, 0.2f, rev, true, new W.Opening(3.7f, 4.6f, 0, 2.3f), new W.Opening(8.6f, 9.6f, 0, 2.3f));
            W.Wall(h, "Almacén|Dormitorio", P(-4, 16), P(-4, 28), WH, 0.2f, rev, true, new W.Opening(8.6f, 9.6f, 0, 2.3f));
            W.Wall(h, "Almacén|Comedor", P(-4, 22), P(4, 22), WH, 0.2f, rev, true, new W.Opening(0.2f, 1.1f, 0, 2.3f));
            W.Wall(h, "Dormitorio|Cocina", P(-10, 22), P(-4, 22), WH, 0.2f, rev, true, new W.Opening(2.6f, 3.5f, 0, 2.3f));
            W.Wall(h, "Baño", P(-10, 25.4f), P(-7.6f, 25.4f), WH, 0.15f, rev, true);
            W.Wall(h, "Baño puerta", P(-7.6f, 25.4f), P(-7.6f, 28), WH, 0.15f, rev, true, new W.Opening(0.9f, 1.7f, 0, 2.2f));

            // Zócalo de ladrillo visto en el frente y una cornisa
            // (cortado en la puerta del almacén para que no la tape)
            W.TBoxB(h, "Zócalo", new Vector3((-10.1f - 0.72f) / 2, y0, 15.85f), new Vector3(10.1f - 0.72f, 0.75f, 0.08f), W.Ladrillo, 0, false);
            W.TBoxB(h, "Zócalo", new Vector3((11.1f + 0.72f) / 2, y0, 15.85f), new Vector3(11.1f - 0.72f, 0.75f, 0.08f), W.Ladrillo, 0, false);
            W.TBoxB(h, "Cornisa", new Vector3(0.5f, y0 + WH - 0.25f, 15.85f), new Vector3(21.4f, 0.25f, 0.12f), rev, 0, false);

            // Cielorraso de machimbre
            W.TBoxB(h, "Cielorraso", new Vector3(0.5f, y0 + WH, 22), new Vector3(21, 0.05f, 12), W.Machimbre, 0, false);

            // Techo de chapa a dos aguas (cumbrera a lo largo de X en Z = 22)
            float eave = y0 + WH + 0.05f, ridge = eave + 2.2f;
            W.ChapaSlope(h, "Techo sur", new Vector3(0.5f, eave, 15.85f), new Vector3(0.5f, ridge, 22), 22.2f, 0.5f);
            W.ChapaSlope(h, "Techo norte", new Vector3(0.5f, eave, 28.15f), new Vector3(0.5f, ridge, 22), 22.2f, 0.5f);
            Shapes.BoxC(h, "Cumbrera", new Vector3(0.5f, ridge + 0.05f, 22), new Vector3(22.3f, 0.08f, 0.35f), Mats.Get(Mats.Chapa, 0.5f, 0.6f), false);
            // Mojinetes (triángulos de pared en los costados)
            foreach (float x in new[] { -10f, 11f })
            {
                var b = new MeshKit.Builder(1);
                MeshKit.AddExtrusion(b, 0, new List<Vector2> { new Vector2(15.9f, eave), new Vector2(28.1f, eave), new Vector2(22, ridge - 0.05f) }, 0.25f);
                MeshKit.Object(h, "Mojinete", MeshKit.Persist("mojinete", () => b.Build()), new[] { rev }, new Vector3(x, 0, 0));
            }
            // Chimenea de la cocina a leña
            W.TBoxB(h, "Chimenea", new Vector3(-9.3f, eave + 0.4f, 23.2f), new Vector3(0.6f, 2.4f, 0.6f), W.Ladrillo, 0, false);

            // Galería: techo de chapa más bajo sobre columnas de madera
            float galHigh = y0 + WH - 0.15f, galLow = y0 + 2.6f;
            W.ChapaSlope(h, "Techo galería", new Vector3(0.5f, galLow, 13.2f), new Vector3(0.5f, galHigh, 15.9f), 22.2f, 0.25f);
            W.TBoxB(h, "Viga galería", new Vector3(0.5f, galLow - 0.2f, 13.35f), new Vector3(21.8f, 0.2f, 0.15f), W.Madera, 0, false);
            for (float x = -10.2f; x <= 11.3f; x += 3.07f)
            {
                W.TBoxB(h, "Columna", new Vector3(x, y0, 13.35f), new Vector3(0.16f, galLow - y0 - 0.2f, 0.16f), W.Madera);
                Shapes.Beam(h, "Zapata", new Vector3(x - 0.35f, galLow - 0.2f, 13.35f), new Vector3(x, galLow - 0.55f, 13.35f), 0.08f, Mats.Madera);
                Shapes.Beam(h, "Zapata", new Vector3(x + 0.35f, galLow - 0.2f, 13.35f), new Vector3(x, galLow - 0.55f, 13.35f), 0.08f, Mats.Madera);
            }
            // Faroles de la galería
            foreach (float x in new[] { -5.5f, 5.5f })
            {
                Shapes.BoxC(h, "Farol", new Vector3(x, galLow - 0.45f, 13.6f), new Vector3(0.18f, 0.28f, 0.18f), Mats.Emissive(new Color(1f, 0.85f, 0.55f), 1.5f), false);
                nightLights.Add(W.PointLight(h, new Vector3(x, galLow - 0.6f, 13.9f), 9f, 1.6f, new Color(1f, 0.8f, 0.5f)));
            }
            // Cartel grande arriba del alero
            var sign = Shapes.Empty(h, "Cartel almacén", new Vector3(0, galLow + 0.15f, 13.25f)).transform;
            W.TBoxB(sign, "Tabla", Vector3.zero, new Vector3(9f, 1.0f, 0.06f), TexKit.Mat("madera", new Color(0.95f, 0.9f, 0.75f)), 0, false);
            W.TBoxB(sign, "Pata", new Vector3(-4f, -0.3f, 0.05f), new Vector3(0.1f, 0.5f, 0.1f), W.Madera, 0, false);
            W.TBoxB(sign, "Pata", new Vector3(4f, -0.3f, 0.05f), new Vector3(0.1f, 0.5f, 0.1f), W.Madera, 0, false);
            WorldLabel.Create(sign, "ALMACÉN DE RAMOS GENERALES\n\"LA TRANQUERA\"", new Vector3(0, 0.5f, -0.04f), new Vector2(8.8f, 0.95f), 340, new Color(0.45f, 0.12f, 0.08f), Vector3.back);
            // Banco y macetas en la galería
            W.TBoxB(h, "Banco", new Vector3(-6.5f, y0 + 0.42f, 15.4f), new Vector3(1.8f, 0.06f, 0.4f), W.Madera);
            W.TBoxB(h, "PataBanco", new Vector3(-7.2f, y0, 15.4f), new Vector3(0.08f, 0.42f, 0.35f), W.Madera, 0, false);
            W.TBoxB(h, "PataBanco", new Vector3(-5.8f, y0, 15.4f), new Vector3(0.08f, 0.42f, 0.35f), W.Madera, 0, false);
            foreach (float x in new[] { -1.4f, 1.4f, 9f })
            {
                if (Mdl(h, "Maceta", "Maceta", new Vector3(x, y0, 15.3f), Rf * 360, true) != null) continue;
                Shapes.Cyl(h, "Maceta", new Vector3(x, y0, 15.3f), 0.45f, 0.45f, new Color(0.7f, 0.35f, 0.2f), true);
                NatureKit.Place(NatureKit.Tree.Arbusto, h, new Vector3(x, y0 + 0.35f, 15.3f), rnd.Next(), Rf * 360, 0.45f);
            }
        }

        // ------------------------------------------------------------------
        public class StoreRefs
        {
            public ToolHook labelerHook;
        }

        static StoreRefs Store()
        {
            var s = new StoreRefs();
            var inter = Shapes.Empty(interactivo, "Almacen", Vector3.zero).transform;

            // Puertas dobles del almacén (siempre abiertas para los clientes)
            var verde = W.MaderaVerde;
            var dl = W.MakeDoor(inter, "Puerta del almacén", new Vector3(-0.7f, FL, 16.05f), 0, 0.7f, 2.6f, true, -100f, verde);
            var dr = W.MakeDoor(inter, "Puerta del almacén", new Vector3(0.7f, FL, 16.05f), 180, 0.7f, 2.6f, true, 100f, verde);
            // cortina de tiras
            Color[] strips = { new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.5f, 0.9f), new Color(0.95f, 0.85f, 0.2f), new Color(0.2f, 0.7f, 0.3f) };
            for (int i = 0; i < 13; i++)
                Shapes.Box(estatico, "Tira", new Vector3(-0.6f + i * 0.1f, FL + 1.25f, 16.0f), new Vector3(0.04f, 1.35f, 0.01f), strips[i % 4], false);

            WorldParts.RoomLight(estatico, new Vector3(0, FL + WH - 0.65f, 19f), 10f, 2.2f);
            WorldParts.RoomLight(estatico, new Vector3(2.9f, FL + WH - 0.65f, 18.5f), 6f, 1.2f);

            // --- Estanterías ---
            ShelfObj(inter, "Estanteria_1", "estanteria_1", new Vector3(-3.6f, FL, 19f), 90, 2.4f, 0.42f, 4, 3, 0.12f, 0.44f, false, new Vector3(-2.6f, FL, 19f));
            ShelfObj(inter, "Estanteria_2", "estanteria_2", new Vector3(-1.05f, FL, 21.65f), 180, 2.0f, 0.42f, 4, 3, 0.12f, 0.44f, false, new Vector3(-1.05f, FL, 20.75f));
            if (ModelLib.Has("Modelos/HeladeraExhibidora"))
            {
                // dos heladeras exhibidoras con puerta de vidrio (modelo 3D)
                for (int i = 0; i < 2; i++)
                {
                    float x = 1.0f + i * 0.86f;
                    var model = ModelLib.Place(estatico, "Modelos/HeladeraExhibidora", "HeladeraExhibidora", new Vector3(x, FL, 21.47f), 180);
                    var sh = ShelfObj(inter, "Heladera_" + (i + 1), "heladera_" + (i + 1), new Vector3(x, FL, 21.47f), 180, 0.62f, 0.5f, 4, 1, 0.32f, 0.38f, true, new Vector3(x, FL, 20.5f), false);
                    sh.displayName = "Heladera";
                    // paredes invisibles para no atravesarla
                    var col = model.AddComponent<BoxCollider>(); col.center = new Vector3(0, 0.97f, -0.32f); col.size = new Vector3(0.82f, 1.95f, 0.16f);
                    var colI = model.AddComponent<BoxCollider>(); colI.center = new Vector3(-0.38f, 0.97f, 0); colI.size = new Vector3(0.06f, 1.95f, 0.8f);
                    var colD = model.AddComponent<BoxCollider>(); colD.center = new Vector3(0.38f, 0.97f, 0); colD.size = new Vector3(0.06f, 1.95f, 0.8f);
                }
                WorldLabel.Create(inter, "LÁCTEOS", new Vector3(1.43f, FL + 2.05f, 21.05f), new Vector2(1.6f, 0.15f), 110, new Color(0.15f, 0.3f, 0.7f), Vector3.back);
            }
            else
                ShelfObj(inter, "Heladera", "heladera_1", new Vector3(1.45f, FL, 21.55f), 180, 1.4f, 0.5f, 4, 2, 0.28f, 0.42f, true, new Vector3(1.45f, FL, 20.55f));

            // --- Mostrador ---
            var counterGo = Shapes.Empty(inter, "Mostrador", Vector3.zero);
            var ct = counterGo.transform;
            W.TBoxB(ct, "Cuerpo", new Vector3(2.6f, FL, 18.6f), new Vector3(0.6f, CT - FL - 0.04f, 3.2f), W.Madera);
            W.TBoxB(ct, "Frente", new Vector3(2.28f, FL, 18.6f), new Vector3(0.03f, CT - FL - 0.08f, 3.22f), TexKit.Mat("madera", new Color(0.75f, 0.6f, 0.45f)), 0, false);
            Shapes.Box(ct, "Tapa", new Vector3(2.6f, CT - 0.04f, 18.6f), new Vector3(0.72f, 0.04f, 3.32f), new Color(0.85f, 0.82f, 0.75f), true);
            var co = counterGo.AddComponent<CheckoutCounter>();
            co.customerPoint = Shapes.Empty(ct, "PuntoCliente", new Vector3(1.75f, FL, 18.4f), new Vector3(0, 90, 0)).transform;
            co.queuePoints = new[]
            {
                Shapes.Empty(ct, "Fila1", new Vector3(0.95f, FL, 18.4f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(ct, "Fila2", new Vector3(0.15f, FL, 18.4f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(ct, "Fila3", new Vector3(-0.65f, FL, 18.4f), new Vector3(0, 90, 0)).transform,
            };
            co.itemArea = Shapes.Empty(ct, "ZonaProductos", new Vector3(2.45f, CT, 18.75f)).transform;
            co.scanPoint = Shapes.Empty(ct, "Lector", new Vector3(2.58f, CT + 0.06f, 18.05f)).transform;
            co.bagPoint = Shapes.Empty(ct, "Embolsado", new Vector3(2.45f, CT, 17.3f)).transform;
            co.cashPoint = Shapes.Empty(ct, "PlataCliente", new Vector3(2.42f, CT, 17.72f)).transform;
            Shapes.Box(ct, "LectorBase", new Vector3(2.58f, CT, 18.05f), new Vector3(0.18f, 0.03f, 0.16f), new Color(0.1f, 0.1f, 0.1f), false);
            Shapes.Box(ct, "LectorVidrio", new Vector3(2.58f, CT + 0.03f, 18.05f), new Vector3(0.12f, 0.003f, 0.1f), Mats.Emissive(new Color(1f, 0.15f, 0.1f), 1.5f), false);
            Shapes.Box(ct, "BolsaPapel", new Vector3(2.42f, CT, 17.15f), new Vector3(0.18f, 0.25f, 0.12f), new Color(0.8f, 0.68f, 0.5f), false);
            // balanza vieja de almacén (decorativa)
            // balanza de almacén: del lado del cliente, mirando hacia él
            var balModel = ModelLib.Place(ct, "Modelos/Balanza", "Balanza", new Vector3(2.45f, CT, 19.95f), -90);
            Transform bal;
            if (balModel != null) { bal = balModel.transform; ModelLib.FitCollider(balModel); }
            else
            {
                bal = Shapes.Empty(ct, "Balanza", new Vector3(2.62f, CT, 19.95f)).transform;
                Shapes.Box(bal, "Base", Vector3.zero, new Vector3(0.3f, 0.12f, 0.25f), new Color(0.75f, 0.15f, 0.12f), false);
                Shapes.Cyl(bal, "Plato", new Vector3(0, 0.12f, 0), 0.3f, 0.02f, new Color(0.8f, 0.8f, 0.82f), false);
                Shapes.Box(bal, "Cuadrante", new Vector3(0, 0.12f, 0.08f), new Vector3(0.24f, 0.25f, 0.06f), Color.white, false);
                bal.gameObject.AddComponent<BoxCollider>().size = new Vector3(0.3f, 0.4f, 0.3f);
            }
            var bd = bal.gameObject.AddComponent<Decor>(); bd.displayName = "Balanza de almacén"; bd.action = "[E] Mirar"; bd.message = "Una balanza de las de antes. (En la Fase 3 se usa para la carne y las verduras sueltas.)";
            // caja registradora (modelo 3D) con las teclas hacia el jugador
            var reg = ModelLib.Place(ct, "Modelos/CajaRegistradora", "CajaRegistradora", new Vector3(2.62f, CT, 19.3f), 90);
            if (reg != null)
            {
                ModelLib.FitCollider(reg);
                var rdc = reg.AddComponent<Decor>(); rdc.displayName = "Caja registradora"; rdc.action = "";
            }

            var scr = Shapes.Empty(ct, "PantallaCaja", new Vector3(2.8f, CT, 19.85f), new Vector3(0, 90, 0));
            var screen = scr.AddComponent<RegisterScreen>();
            screen.counter = co; screen.Build(); co.screen = screen;

            var dr2 = Shapes.Empty(ct, "CajonBilletes", new Vector3(2.72f, FL + 0.6f, 18.3f), new Vector3(0, 90, 0));
            var drawer = dr2.AddComponent<CashDrawer>(); drawer.Build(); co.drawer = drawer;

            var tr = Shapes.Empty(ct, "BandejaVuelto", new Vector3(2.76f, CT, 17.6f));
            var tray = tr.AddComponent<ChangeTray>(); tray.drawer = drawer; tray.Build(); drawer.tray = tray; co.tray = tray;

            var btn = Shapes.Box(ct, "BotonEntregar", new Vector3(2.8f, CT, 17.22f), new Vector3(0.12f, 0.05f, 0.12f), new Color(0.85f, 0.15f, 0.1f), true);
            btn.AddComponent<GiveChangeButton>().counter = co;
            WorldLabel.Create(ct, "ENTREGAR\nVUELTO", new Vector3(2.8f, CT + 0.052f, 17.22f), new Vector2(0.12f, 0.1f), 18, Color.white, Vector3.up, null, Vector3.left);

            var pn = Shapes.Empty(ct, "Posnet", new Vector3(2.72f, CT, 18.6f), new Vector3(0, 90, 0));
            var posnet = pn.AddComponent<Posnet>(); posnet.counter = co; posnet.Build(); co.posnet = posnet;

            // computadora en el escritorio del fondo
            Mdl(inter, "SillaGiratoria", "SillaGiratoria", new Vector3(2.98f, FL, 21.25f), 90, true);
            Fan(estatico, new Vector3(-1.6f, FL + WH - 0.33f, 19.2f));
            var desk = Shapes.Empty(inter, "Computadora", new Vector3(3.55f, FL, 21.25f), new Vector3(0, -90, 0)).transform;
            W.TBoxB(desk, "Escritorio", Vector3.zero, new Vector3(1.1f, 0.75f, 0.6f), W.Madera);
            Shapes.Box(desk, "Monitor", new Vector3(0, 0.95f, 0.15f), new Vector3(0.5f, 0.36f, 0.05f), new Color(0.1f, 0.1f, 0.12f), true);
            Shapes.Box(desk, "PieMonitor", new Vector3(0, 0.75f, 0.15f), new Vector3(0.08f, 0.2f, 0.08f), new Color(0.1f, 0.1f, 0.12f), false);
            Shapes.Box(desk, "Teclado", new Vector3(0, 0.75f, -0.1f), new Vector3(0.4f, 0.02f, 0.14f), new Color(0.2f, 0.2f, 0.22f), false);
            WorldLabel.Create(desk, "PROVEEDOR\nPRECIOS · MEJORAS", new Vector3(0, 1.13f, 0.12f), new Vector2(0.46f, 0.32f), 40, new Color(0.7f, 0.9f, 1f), Vector3.back, new Color(0.08f, 0.2f, 0.35f));
            desk.gameObject.AddComponent<ComputerTerminal>();

            // puerta directa del almacén al depósito (detrás del mostrador)
            W.MakeDoor(inter, "Puerta almacén-depósito", new Vector3(4f, FL, 19.7f), -90, 0.9f, 2.3f, true, 95f);
            var lh = Shapes.Empty(inter, "LugarEtiquetadora", new Vector3(3.6f, FL + 0.79f, 16.55f), new Vector3(0, -90, 0));
            Shapes.Box(lh.transform, "Repisa", new Vector3(0, -0.02f, 0), new Vector3(0.3f, 0.02f, 0.25f), Mats.Madera, false);
            s.labelerHook = Hook(lh, "labeler", "Lugar de la etiquetadora", new Vector3(0.35f, 0.3f, 0.35f));

            // cartel ABIERTO/CERRADO colgado en la vidriera izquierda (mirando desde afuera)
            var sign = Shapes.Empty(inter, "CartelAbiertoCerrado", new Vector3(-2.2f, FL + 1.55f, 16.22f));
            var board = Shapes.BoxC(sign.transform, "Tabla", Vector3.zero, new Vector3(0.7f, 0.28f, 0.02f), Color.white, true);
            Shapes.Beam(sign.transform, "Hilo", new Vector3(-0.3f, 0.14f, 0), new Vector3(0, 0.5f, 0), 0.005f, Color.black);
            Shapes.Beam(sign.transform, "Hilo", new Vector3(0.3f, 0.14f, 0), new Vector3(0, 0.5f, 0), 0.005f, Color.black);
            var os = sign.AddComponent<OpenSign>();
            os.frontText = WorldLabel.Create(sign.transform, "CERRADO", new Vector3(0, 0, -0.012f), new Vector2(0.66f, 0.26f), 130, Color.red, Vector3.back);
            os.backText = WorldLabel.Create(sign.transform, "CERRADO", new Vector3(0, 0, 0.012f), new Vector2(0.66f, 0.26f), 130, Color.red, Vector3.forward);
            os.board = board.GetComponent<Renderer>();

            // cuadros y almanaque viejo en la pared
            W.TBox(estatico, "Almanaque", new Vector3(-3.88f, FL + 2.3f, 20.9f), new Vector3(0.02f, 0.5f, 0.35f), TexKit.Mat("revoque", new Color(1f, 0.9f, 0.7f)), 0, false);

            var bin = Shapes.Cyl(inter, "TachoBasura", new Vector3(1.9f, FL, 14.4f), 0.45f, 0.75f, new Color(0.2f, 0.4f, 0.25f), true);
            bin.AddComponent<TrashBin>();
            return s;
        }

        static ToolHook Hook(GameObject go, string kind, string label, Vector3 size)
        {
            var h = go.AddComponent<ToolHook>();
            h.acceptsKind = kind;
            h.label = label;
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.center = new Vector3(0, size.y / 2, 0);
            bc.size = size;
            return h;
        }

        static Shelf ShelfObj(Transform parent, string name, string id, Vector3 pos, float yaw, float width, float depth, int levels, int slots,
            float firstY, float spacing, bool fridge, Vector3 customerPos, bool drawFrame = true)
        {
            if (!drawFrame)
            {
                var g = Shapes.Empty(parent, name, pos, new Vector3(0, yaw, 0));
                var sh = g.AddComponent<Shelf>();
                sh.shelfId = id; sh.isFridge = fridge; sh.displayName = fridge ? "Heladera" : "Estantería";
                sh.width = width; sh.depth = depth; sh.levels = levels; sh.slotsPerLevel = slots;
                sh.firstLevelY = firstY; sh.levelSpacing = spacing;
                sh.customerPoint = Shapes.Empty(parent, name + "_PuntoCliente", customerPos).transform;
                return sh;
            }
            var go = Shapes.Empty(parent, name, pos, new Vector3(0, yaw, 0));
            var t = go.transform;
            Material frame = fridge ? Mats.Get(new Color(0.92f, 0.92f, 0.94f), 0.5f) : W.Madera;
            Material board = fridge ? Mats.Get(new Color(0.85f, 0.88f, 0.9f), 0.5f) : TexKit.Mat("madera", new Color(1f, 0.9f, 0.8f));
            float top = firstY + levels * spacing + 0.05f;
            W.TBoxB(t, "LadoI", new Vector3(-width / 2 - 0.02f, 0, 0), new Vector3(0.04f, top, depth + 0.04f), frame);
            W.TBoxB(t, "LadoD", new Vector3(width / 2 + 0.02f, 0, 0), new Vector3(0.04f, top, depth + 0.04f), frame);
            W.TBoxB(t, "Fondo", new Vector3(0, 0, -depth / 2 - 0.02f), new Vector3(width + 0.08f, top, 0.03f), frame);
            W.TBoxB(t, "Techo", new Vector3(0, top, 0), new Vector3(width + 0.08f, 0.04f, depth + 0.04f), frame);
            W.TBoxB(t, "Zocalo", Vector3.zero, new Vector3(width, firstY - 0.01f, depth), frame);
            for (int l = 0; l < levels; l++)
                W.TBoxB(t, "Estante", new Vector3(0, firstY + l * spacing - 0.02f, 0), new Vector3(width, 0.02f, depth), board);
            if (fridge)
            {
                Shapes.Prim(PrimitiveType.Cube, t, "PuertaVidrio", new Vector3(0, (firstY + top) / 2, depth / 2 + 0.03f), new Vector3(width, top - firstY, 0.015f),
                    Mats.Glass(new Color(0.75f, 0.9f, 1f, 0.18f)), false);
                Shapes.Box(t, "LuzInterior", new Vector3(0, top - 0.04f, 0.05f), new Vector3(width * 0.9f, 0.02f, 0.05f), Mats.Emissive(new Color(0.9f, 0.95f, 1f), 1.5f), false);
                Shapes.Box(t, "Marco", new Vector3(0, top, depth / 2 + 0.03f), new Vector3(width + 0.08f, 0.12f, 0.04f), new Color(0.2f, 0.4f, 0.75f), false);
                WorldLabel.Create(t, "LÁCTEOS", new Vector3(0, top + 0.06f, depth / 2 + 0.06f), new Vector2(width, 0.1f), 80, Color.white, Vector3.forward);
            }
            var shelf = go.AddComponent<Shelf>();
            shelf.shelfId = id;
            shelf.displayName = fridge ? "Heladera" : "Estantería";
            shelf.isFridge = fridge;
            shelf.width = width; shelf.depth = depth; shelf.levels = levels; shelf.slotsPerLevel = slots;
            shelf.firstLevelY = firstY; shelf.levelSpacing = spacing;
            shelf.customerPoint = Shapes.Empty(parent, name + "_PuntoCliente", customerPos).transform;
            return shelf;
        }

        // ------------------------------------------------------------------
        public class DepRefs
        {
            public ToolHook hoe, can, bucket;
            public Transform[] seedSpots, boxSpots;
            public Transform drop;
        }

        static DepRefs Deposito()
        {
            var d = new DepRefs();
            var inter = Shapes.Empty(interactivo, "Deposito", Vector3.zero).transform;
            WorldParts.RoomLight(estatico, new Vector3(7.5f, FL + WH - 0.65f, 19.5f), 9f, 1.6f);
            WorldParts.RoomLight(estatico, new Vector3(7.5f, FL + WH - 0.65f, 25f), 9f, 1.6f);
            WorldLabel.Create(estatico, "DEPÓSITO", new Vector3(11.15f, FL + 3.15f, 21.5f), new Vector2(3.5f, 0.4f), 260, new Color(0.15f, 0.15f, 0.15f), Vector3.right);
            // portón corredizo abierto
            W.TBoxB(estatico, "PortonCorredizo", new Vector3(11.2f, FL, 25.9f), new Vector3(0.06f, 3.0f, 4.1f), TexKit.Mat("madera", new Color(0.65f, 0.75f, 0.65f)), 0, true);
            W.TBoxB(estatico, "RielPorton", new Vector3(11.25f, FL + 3.05f, 23.5f), new Vector3(0.06f, 0.08f, 9f), W.Hierro, 0, false);

            // tablero de herramientas en la pared del fondo
            W.TBoxB(estatico, "Tablero", new Vector3(6.4f, FL + 0.85f, 27.83f), new Vector3(3.2f, 1.4f, 0.04f), TexKit.Mat("madera", new Color(0.8f, 0.8f, 0.75f)), 0, false);
            WorldLabel.Create(estatico, "HERRAMIENTAS", new Vector3(6.4f, FL + 2.4f, 27.8f), new Vector2(2f, 0.2f), 120, new Color(0.15f, 0.1f, 0.05f), Vector3.back);
            d.hoe = Hook(Shapes.Empty(inter, "LugarAzada", new Vector3(5.1f, FL + 0.02f, 27.6f)), "hoe", "Lugar de la azada", new Vector3(0.4f, 1.4f, 0.3f));
            W.TBoxB(estatico, "RepisaHerr", new Vector3(6.9f, FL + 0.8f, 27.6f), new Vector3(2.0f, 0.04f, 0.45f), W.Madera);
            d.can = Hook(Shapes.Empty(inter, "LugarRegadera", new Vector3(6.4f, FL + 0.84f, 27.58f), new Vector3(0, 180, 0)), "can", "Lugar de la regadera", new Vector3(0.45f, 0.45f, 0.45f));
            d.bucket = Hook(Shapes.Empty(inter, "LugarBalde", new Vector3(7.4f, FL + 0.84f, 27.58f)), "bucket", "Lugar del balde", new Vector3(0.4f, 0.45f, 0.4f));

            // mesa de trabajo con las semillas (contra la pared del frente)
            var mesa = Shapes.Empty(estatico, "MesaTrabajo", new Vector3(6.3f, FL, 16.6f)).transform;
            W.TBoxB(mesa, "Tapa", new Vector3(0, 0.82f, 0), new Vector3(2.4f, 0.05f, 0.75f), W.Madera);
            foreach (var p in new[] { new Vector3(-1.1f, 0, -0.3f), new Vector3(1.1f, 0, -0.3f), new Vector3(-1.1f, 0, 0.3f), new Vector3(1.1f, 0, 0.3f) })
                W.TBoxB(mesa, "Pata", p, new Vector3(0.06f, 0.82f, 0.06f), W.Madera);
            d.seedSpots = new[]
            {
                Shapes.Empty(inter, "Semillas1", new Vector3(5.7f, FL + 0.9f, 16.6f)).transform,
                Shapes.Empty(inter, "Semillas2", new Vector3(6.3f, FL + 0.9f, 16.6f)).transform,
                Shapes.Empty(inter, "Semillas3", new Vector3(6.9f, FL + 0.9f, 16.6f)).transform,
            };
            // estanterías metálicas del depósito con bolsas (decorativas)
            Vector3[] rackPos = { new Vector3(4.45f, FL, 22.6f), new Vector3(10.55f, FL, 26.4f) };
            for (int i = 0; i < 2; i++)
            {
                var r = Shapes.Empty(estatico, "Rack", rackPos[i], new Vector3(0, 90, 0)).transform;
                Color metal = new Color(0.4f, 0.45f, 0.55f);
                foreach (var x in new[] { -1f, 1f }) foreach (var z in new[] { -0.25f, 0.25f })
                    Shapes.Box(r, "Parante", new Vector3(x, 0, z), new Vector3(0.04f, 2.2f, 0.04f), metal, false);
                for (int k = 0; k < 4; k++)
                {
                    Shapes.Box(r, "Estante", new Vector3(0, 0.15f + k * 0.65f, 0), new Vector3(2.0f, 0.03f, 0.5f), metal, true);
                    for (int j = 0; j < 3; j++)
                        if (rnd.NextDouble() < 0.7)
                            Shapes.Box(r, "Bolsa", new Vector3(-0.6f + j * 0.6f, 0.18f + k * 0.65f, 0), new Vector3(0.45f, 0.25f, 0.35f),
                                j % 2 == 0 ? new Color(0.85f, 0.8f, 0.65f) : new Color(0.7f, 0.6f, 0.45f), false);
                }
            }
            d.boxSpots = new[]
            {
                Shapes.Empty(inter, "Caja1", new Vector3(9.6f, FL + 0.05f, 18f)).transform,
                Shapes.Empty(inter, "Caja2", new Vector3(9.6f, FL + 0.05f, 18.7f)).transform,
                Shapes.Empty(inter, "Caja3", new Vector3(9.6f, FL + 0.05f, 19.4f)).transform,
            };
            d.drop = Shapes.Empty(inter, "DescargaProveedor", new Vector3(12.4f, 0.05f, 21.5f), new Vector3(0, -90, 0)).transform;
            return d;
        }

        // ------------------------------------------------------------------
        static SleepBed Rooms()
        {
            var r = Shapes.Empty(estatico, "Ambientes", Vector3.zero).transform;
            var inter = Shapes.Empty(interactivo, "Casa", Vector3.zero).transform;
            float y = FL;

            // Puertas interiores (se abren y cierran con E)
            W.MakeDoor(inter, "Puerta almacén-comedor", new Vector3(-3.8f, y, 22f), 0, 0.9f, 2.3f, true, 95f);
            W.MakeDoor(inter, "Puerta del depósito", new Vector3(4f, y, 24.6f), -90, 1.0f, 2.3f, true, -95f);
            W.MakeDoor(inter, "Puerta de la cocina", new Vector3(-4f, y, 24.6f), -90, 1.0f, 2.3f, true, 95f);
            W.MakeDoor(inter, "Puerta del dormitorio", new Vector3(-7.4f, y, 22f), 0, 0.9f, 2.3f, true, -95f);
            W.MakeDoor(inter, "Puerta del baño", new Vector3(-7.6f, y, 26.3f), -90, 0.8f, 2.2f, false, 95f);
            W.MakeDoor(inter, "Puerta del patio", new Vector3(-0.5f, y, 28f), 0, 1.0f, 2.4f, true, 95f, W.MaderaVerde);

            // --- Comedor: mueble bajo con la tele vieja, alfombra, ventilador, biblioteca y perchero ---
            if (Mdl(r, "MuebleBajo", "MuebleBajo", new Vector3(1.6f, y, 22.35f), 0, true) != null)
            {
                var tv = Mdl(inter, "Tele", "TeleVieja", new Vector3(1.35f, y + 0.338f, 22.4f), 0, true);
                if (tv != null) { var td = tv.AddComponent<Decor>(); td.displayName = "Televisor viejo"; td.action = "[E] Prender la tele"; td.message = "Pasan el noticiero del campo: \"la hacienda viene firme\"."; }
                Mdl(r, "Libros", "Libros", new Vector3(2.05f, y + 0.338f, 22.4f), 0);
            }
            Mdl(r, "AlfombraRect", "Alfombra", new Vector3(0.3f, y + 0.003f, 25.2f), 0);
            Fan(estatico, new Vector3(-1.8f, y + WH - 0.33f, 24.4f));
            Mdl(r, "Biblioteca", "Biblioteca", new Vector3(-3.6f, y, 27.0f), 90, true);
            Mdl(r, "Perchero", "Perchero", new Vector3(-3.4f, y, 23.0f), 0, true);
            // cocina: alacenas, olla y sartén en la cocina a leña, tabla con pan, platos, taza, tacho
            Mdl(r, "Alacena", "Alacena", new Vector3(-7.0f, y + 1.5f, 27.66f), 180);
            Mdl(r, "Alacena", "Alacena", new Vector3(-6.2f, y + 1.5f, 27.66f), 180);
            Mdl(r, "Olla", "Olla", new Vector3(-9.5f, y + 0.87f, 23.5f), 20);
            Mdl(r, "Sarten", "Sarten", new Vector3(-9.45f, y + 0.87f, 23.0f), 60);
            Mdl(r, "TablaPicar", "TablaPicar", new Vector3(-6.9f, y + 0.9f, 27.5f), 0);
            Mdl(r, "PanDecor", "Pan", new Vector3(-6.9f, y + 0.93f, 27.5f), 15);
            Mdl(r, "Palote", "Palote", new Vector3(-6.25f, y + 0.9f, 27.45f), 5);
            Mdl(r, "Plato", "Plato", new Vector3(-6.65f, y + 0.765f, 24.38f), 0);
            Mdl(r, "Plato", "Plato", new Vector3(-5.95f, y + 0.765f, 24.82f), 0);
            Mdl(r, "Taza", "Taza", new Vector3(-5.85f, y + 0.765f, 24.4f), 40);
            Mdl(r, "Bowl", "Bowl", new Vector3(-6.75f, y + 0.765f, 24.85f), 0);
            Mdl(r, "Tacho", "Tacho", new Vector3(-4.5f, y, 26.4f), 0, true);
            // dormitorio: lámpara de pie
            Mdl(r, "LamparaPie", "LamparaDePie", new Vector3(-4.5f, y, 16.65f), 0, true);

            // --- Comedor ---
            WorldParts.RoomLight(estatico, new Vector3(0, y + WH - 0.7f, 25f), 8f, 1.5f);
            FurnitureKit.DiningSet(r, new Vector3(0.3f, y, 25.2f), 2.0f, 1.0f, 6);
            FurnitureKit.Sideboard(r, new Vector3(3.55f, y, 26.6f), -90);
            W.TBox(r, "Cuadro", new Vector3(-3.88f, y + 1.8f, 26.5f), new Vector3(0.03f, 0.6f, 0.8f), TexKit.Mat("madera", new Color(0.5f, 0.6f, 0.75f)), 0, false);
            var clock = Shapes.Cyl(r, "Reloj", new Vector3(0, y + 2.4f, 22.13f), 0.4f, 0.04f, Color.white, false);
            clock.transform.localEulerAngles = new Vector3(90, 0, 0);

            // --- Cocina ---
            WorldParts.RoomLight(estatico, new Vector3(-6f, y + WH - 0.7f, 24.5f), 8f, 1.5f);
            var stove = FurnitureKit.WoodStove(inter, new Vector3(-9.5f, y, 23.2f), 90, out var fire);
            fire.SetActive(false);
            var cd = stove.AddComponent<Decor>();
            cd.displayName = "Cocina económica a leña";
            cd.toggleObject = fire;
            cd.action = "[E] Prender el fuego";
            cd.actionOff = "[E] Apagar el fuego";
            cd.message = "Prendiste la cocina a leña. Se siente el calorcito.";
            cd.messageOff = "Apagaste el fuego.";
            FurnitureKit.KitchenCounter(r, new Vector3(-5.9f, y, 27.55f), 180, 3.2f);
            var hel = FurnitureKit.SiamFridge(inter, new Vector3(-4.55f, y, 23.0f), -90);
            var hd = hel.AddComponent<Decor>();
            hd.displayName = "Heladera Siam"; hd.action = "[E] Abrir"; hd.message = "Hay un poco de soda, manteca y salame. (La mercadería del almacén va en la heladera del local.)";
            FurnitureKit.DiningSet(r, new Vector3(-6.3f, y, 24.6f), 1.4f, 0.8f, 4);
            var mate = Shapes.Empty(inter, "PavaYMate", new Vector3(-6.3f, y + 0.765f, 24.6f)).transform;
            FurnitureKit.Kettle(mate, new Vector3(0.15f, 0, 0), 30);
            FurnitureKit.Mate(mate, new Vector3(-0.12f, 0, 0.05f));
            var mc = mate.gameObject.AddComponent<BoxCollider>(); mc.center = new Vector3(0, 0.1f, 0); mc.size = new Vector3(0.5f, 0.25f, 0.3f);
            var md = mate.gameObject.AddComponent<Decor>();
            md.displayName = "Pava y mate"; md.action = "[E] Cebarse unos mates"; md.message = "¡Qué rico mate! Arrancás el día con todo.";
            var radio = FurnitureKit.Radio(inter, new Vector3(-4.5f, y + 0.9f, 27.6f), 180);
            var rd = radio.AddComponent<Decor>();
            rd.displayName = "Radio a válvulas"; rd.action = "[E] Prender la radio"; rd.message = "Suena un chamamé... y el pronóstico: \"buen tiempo para la siembra\".";

            // --- Baño ---
            WorldParts.RoomLight(estatico, new Vector3(-8.8f, y + WH - 0.7f, 26.7f), 4f, 1f);
            FurnitureKit.Toilet(r, new Vector3(-9.55f, y, 26.2f), 90);
            FurnitureKit.PedestalSink(r, new Vector3(-8.45f, y, 25.8f), 0);
            Shapes.Box(r, "Espejo", new Vector3(-8.45f, y + 1.3f, 25.49f), new Vector3(0.5f, 0.6f, 0.02f), new Color(0.75f, 0.85f, 0.9f), false);
            FurnitureKit.Bathtub(r, new Vector3(-9.05f, y, 27.45f), 0);

            // --- Dormitorio ---
            WorldParts.RoomLight(estatico, new Vector3(-7f, y + WH - 0.7f, 19f), 8f, 1.4f);
            var cama = FurnitureKit.IronBed(inter, new Vector3(-8.85f, y, 18.8f), 0, new Color(0.6f, 0.2f, 0.22f));
            var bed = cama.AddComponent<SleepBed>();
            bed.wakeUpPoint = Shapes.Empty(interactivo, "Despertar", new Vector3(-7.2f, y + 0.1f, 18.8f), new Vector3(0, 90, 0)).transform;
            FurnitureKit.Wardrobe(r, new Vector3(-4.45f, y, 20.6f), -90);
            FurnitureKit.Nightstand(r, new Vector3(-9.6f, y, 17.6f), 90);
            var vl = FurnitureKit.Lamp(inter, new Vector3(-9.6f, y + 0.6f, 17.6f));
            var vd = vl.transform.parent.gameObject.AddComponent<Decor>();
            vd.displayName = "Velador"; vd.action = "[E] Prender / apagar"; vd.toggleLight = vl; vd.sound = "key";
            W.TBoxB(r, "Baúl", new Vector3(-7.5f, y, 16.6f), new Vector3(0.9f, 0.5f, 0.5f), TexKit.Mat("madera", new Color(0.7f, 0.55f, 0.45f)));
            if (Mdl(r, "AlfombraRedonda", "Alfombra", new Vector3(-7.0f, y + 0.003f, 19f), 0) == null)
                Shapes.Cyl(r, "Alfombra", new Vector3(-7.2f, y + 0.005f, 19f), 1.8f, 0.01f, new Color(0.55f, 0.35f, 0.3f), false);
            return bed;
        }

        // ------------------------------------------------------------------
        static void Patio()
        {
            var p = Shapes.Empty(estatico, "Patio", Vector3.zero).transform;
            W.Aljibe(interactivo, new Vector3(-6.5f, 0, 33f));
            // parral sobre el patio, junto a la puerta de atrás
            for (float x = -3f; x <= 3.1f; x += 3f)
                foreach (float z in new[] { 29f, 32f })
                    W.TBoxB(p, "PostePArral", new Vector3(x, 0, z), new Vector3(0.12f, 2.6f, 0.12f), W.Madera);
            for (int i = 0; i < 6; i++)
                Shapes.Beam(p, "Alambre", new Vector3(-3, 2.6f, 29 + i * 0.6f), new Vector3(3, 2.6f, 29 + i * 0.6f), 0.02f, new Color(0.4f, 0.4f, 0.4f));
            var leaves = new MeshKit.Builder(1);
            var r = new System.Random(5);
            for (int i = 0; i < 14; i++)
                MeshKit.AddBlob(leaves, 0, new Vector3(-2.6f + (i % 7) * 0.9f, 2.75f, 29.4f + (i / 7) * 2f), new Vector3(1.4f, 0.4f, 1.8f), r.Next(), 0.3f);
            MeshKit.Object(p, "Parra", MeshKit.Persist("parra", () => leaves.Build()), new[] { Mats.Get(new Color(0.3f, 0.5f, 0.2f), 0.2f) }, Vector3.zero);
            Mdl(p, "Lavarropas", "Lavarropas", new Vector3(3.3f, 0, 28.75f), 0, true);
            // tendedero con ropa
            Shapes.Beam(p, "Tendedero", new Vector3(5, 1.8f, 31), new Vector3(10, 1.8f, 33), 0.015f, new Color(0.3f, 0.3f, 0.3f));
            W.TBoxB(p, "PosteTend", new Vector3(5, 0, 31), new Vector3(0.08f, 1.9f, 0.08f), W.Madera);
            W.TBoxB(p, "PosteTend", new Vector3(10, 0, 33), new Vector3(0.08f, 1.9f, 0.08f), W.Madera);
            Color[] ropa = { Color.white, new Color(0.3f, 0.45f, 0.75f), new Color(0.8f, 0.3f, 0.3f), new Color(0.9f, 0.85f, 0.6f) };
            for (int i = 0; i < 5; i++)
            {
                var a = Vector3.Lerp(new Vector3(5, 1.8f, 31), new Vector3(10, 1.8f, 33), 0.15f + i * 0.17f);
                Shapes.BoxC(p, "Ropa", a - Vector3.up * 0.3f, new Vector3(0.5f, 0.6f, 0.01f), ropa[i % 4], false, new Vector3(0, -22, 0));
            }
            // leña apilada
            for (int i = 0; i < 12; i++)
                Shapes.CylC(p, "Leño", new Vector3(-9f + (i % 4) * 0.25f, 0.12f + (i / 4) * 0.22f, 29.3f), 0.2f, 1.0f, new Color(0.45f, 0.32f, 0.2f), new Vector3(90, 0, 0));
        }

        // ------------------------------------------------------------------
        static Transform[] Farmland()
        {
            var f = Shapes.Empty(interactivo, "Terrenos", Vector3.zero).transform;
            noGrass.Add(R(-46.6f, 14.4f, -14.4f, 46.6f));
            Plot(f, "lote_1", "Lote 1", new Vector3(-22f, 0, 23f), new Vector2(14, 16), true, 0);
            Plot(f, "lote_2", "Lote 2", new Vector3(-22f, 0, 39f), new Vector2(14, 14), false, 35000);
            Plot(f, "lote_3", "Lote 3", new Vector3(-38f, 0, 30.5f), new Vector2(16, 30), false, 60000);
            W.Sign(estatico, new Vector3(-14.2f, 0, 25.2f), 90, "TERRENOS DE SIEMBRA", new Vector2(1.8f, 0.4f));
            return new[]
            {
                Shapes.Empty(interactivo, "Cajon1", new Vector3(-15.8f, 0.08f, 19.6f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(interactivo, "Cajon2", new Vector3(-15.8f, 0.08f, 20.3f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(interactivo, "Cajon3", new Vector3(-15.8f, 0.08f, 21.0f), new Vector3(0, 90, 0)).transform,
            };
        }

        static void Plot(Transform parent, string id, string name, Vector3 center, Vector2 size, bool owned, int price)
        {
            var go = Shapes.Empty(parent, name, center);
            var lp = go.AddComponent<LandPlot>();
            lp.plotId = id; lp.displayName = name; lp.size = size; lp.owned = owned; lp.price = price;
            var ground = MeshKit.Object(go.transform, "Suelo", MeshKit.Box(new Vector3(size.x - 0.2f, 0.012f, size.y - 0.2f)),
                new[] { owned ? TexKit.Mat("tierra", new Color(0.95f, 0.9f, 0.85f)) : TexKit.Mat("pasto", Color.white) }, Vector3.zero);
            lp.ground = ground.GetComponent<Renderer>();
            // estacas de esquina (blanqueadas a la cal)
            foreach (var c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                W.TBoxB(go.transform, "Estaca", new Vector3(c.x * (size.x / 2 - 0.2f), 0, c.y * (size.y / 2 - 0.2f)), new Vector3(0.08f, 0.6f, 0.08f), W.Revoque, 0, false);
            var sale = Shapes.Empty(go.transform, "EnVenta", Vector3.zero);
            if (!owned)
                W.Sign(sale.transform, new Vector3(0, 0, -size.y / 2 + 0.6f), 180, name.ToUpper() + "\nEN VENTA " + Money.Fmt(price) + "\n(computadora → Mejoras)", new Vector2(2.2f, 0.9f));
            lp.forSaleVisual = sale;
            W.Sign(go.transform, new Vector3(size.x / 2 - 1.2f, 0, -size.y / 2 + 0.4f), 180, name.ToUpper(), new Vector2(0.9f, 0.3f));
        }

        // ------------------------------------------------------------------
        static Transform BackField()
        {
            var b = Shapes.Empty(estatico, "Campo de atrás", Vector3.zero).transform;
            // Tranquera de entrada al camino de tierra
            var gate = Shapes.Empty(interactivo, "Tranquera", new Vector3(16f, 0, 6.2f)).transform;
            var wood = TexKit.Mat("madera", new Color(0.8f, 0.78f, 0.72f));
            W.TBoxB(gate, "PosteI", new Vector3(0, 0, 0), new Vector3(0.3f, 1.9f, 0.3f), W.Madera);
            W.TBoxB(gate, "PosteD", new Vector3(4, 0, 0), new Vector3(0.3f, 1.9f, 0.3f), W.Madera);
            var leaf = Shapes.Empty(gate, "Hoja", new Vector3(0.18f, 0, 0)).transform;
            for (int i = 0; i < 5; i++)
                W.TBoxB(leaf, "Travesaño", new Vector3(1.85f, 0.2f + i * 0.28f, 0), new Vector3(3.7f, 0.1f, 0.06f), wood, 0, false);
            W.TBoxB(leaf, "Vertical", new Vector3(0.05f, 0.15f, 0), new Vector3(0.1f, 1.3f, 0.08f), wood, 0, false);
            W.TBoxB(leaf, "Vertical", new Vector3(3.65f, 0.15f, 0), new Vector3(0.1f, 1.3f, 0.08f), wood, 0, false);
            Shapes.Beam(leaf, "Diagonal", new Vector3(0.1f, 0.25f, 0.04f), new Vector3(3.6f, 1.35f, 0.04f), 0.09f, new Color(0.7f, 0.68f, 0.62f));
            var col = leaf.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(1.85f, 0.8f, 0); col.size = new Vector3(3.7f, 1.4f, 0.12f);
            var fg = gate.gameObject.AddComponent<FarmGate>();
            fg.leaf = leaf; fg.openAngle = -95f; fg.startOpen = true;
            WorldLabel.Create(gate, "LA TRANQUERA", new Vector3(2, 2.2f, 0), new Vector2(3.6f, 0.4f), 260, new Color(0.3f, 0.18f, 0.08f), Vector3.back, new Color(0.9f, 0.85f, 0.7f));
            W.TBoxB(gate, "Arco", new Vector3(2, 1.9f, 0), new Vector3(4.3f, 0.12f, 0.15f), W.Madera, 0, false);
            W.TBoxB(gate, "ArcoI", new Vector3(0, 1.9f, 0), new Vector3(0.12f, 0.55f, 0.12f), W.Madera, 0, false);
            W.TBoxB(gate, "ArcoD", new Vector3(4, 1.9f, 0), new Vector3(0.12f, 0.55f, 0.12f), W.Madera, 0, false);

            // Galpón grande del tambo y la fábrica de dulce de leche (se habilita en la Fase 4)
            var t = Shapes.Empty(b, "Galpon del tambo", new Vector3(20, 0, 63)).transform;
            noGrass.Add(R(9.5f, 55.5f, 30.5f, 70.5f));
            W.TBoxB(t, "Contrapiso", Vector3.zero, new Vector3(20, 0.1f, 14), W.Cemento);
            W.Wall(t, "Pared", new Vector3(10, 0, -7), new Vector3(-10, 0, -7), 4.5f, 0.25f, W.Ladrillo, false, new W.Opening(7f, 13f, 0, 4.0f));
            W.Wall(t, "Pared", new Vector3(-10, 0, 7), new Vector3(10, 0, 7), 4.5f, 0.25f, W.Ladrillo, false);
            W.Wall(t, "Pared", new Vector3(-10, 0, -7), new Vector3(-10, 0, 7), 4.5f, 0.25f, W.Ladrillo, false, new W.Opening(3, 5, 1.5f, 3f, true), new W.Opening(9, 11, 1.5f, 3f, true));
            W.Wall(t, "Pared", new Vector3(10, 0, 7), new Vector3(10, 0, -7), 4.5f, 0.25f, W.Ladrillo, false, new W.Opening(3, 5, 1.5f, 3f, true), new W.Opening(9, 11, 1.5f, 3f, true));
            var roof = MeshKit.Object(t, "Techo curvo", MeshKit.CurvedRoof(20.8f, 3.2f, 14.8f), new[] { W.ChapaMat }, new Vector3(0, 4.5f, 0));
            roof.transform.localRotation = Quaternion.identity;
            foreach (float z in new[] { -7f, 7f })
            {
                var arch = new List<Vector2>();
                for (int i = 0; i <= 16; i++) { float k = i / 16f; arch.Add(new Vector2(Mathf.Lerp(-10, 10, k), 4.5f + 3.2f * (1 - Mathf.Pow(2 * k - 1, 2)))); }
                var bb = new MeshKit.Builder(1);
                MeshKit.AddExtrusion(bb, 0, ArchCCW(arch), 0.25f);
                var gable = MeshKit.Object(t, "Frontis", MeshKit.Persist("frontisTambo", () => bb.Build()), new[] { W.Revoque }, new Vector3(0, 0, z));
                gable.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            // portón cerrado con cartel
            W.TBoxB(t, "Porton", new Vector3(0, 0, -7.05f), new Vector3(6f, 4.0f, 0.12f), TexKit.Mat("madera", new Color(0.6f, 0.45f, 0.35f)));
            WorldLabel.Create(t, "TAMBO · FÁBRICA DE DULCE DE LECHE", new Vector3(0, 5.2f, -7.3f), new Vector2(12f, 0.7f), 450, new Color(0.35f, 0.15f, 0.08f), Vector3.back);
            WorldLabel.Create(t, "Se habilita en la Fase 4", new Vector3(0, 2.2f, -7.15f), new Vector2(5f, 0.4f), 200, Color.white, Vector3.back, new Color(0, 0, 0, 0.5f));

            // Molino y tanque australiano junto a los corrales
            W.Molino(b, new Vector3(36, 0, 46));
            W.TanqueAustraliano(b, new Vector3(36, 0, 39), 6.5f);
            Shapes.Box(b, "Bebedero", new Vector3(31, 0, 39), new Vector3(3.5f, 0.6f, 0.7f), new Color(0.55f, 0.57f, 0.6f), true);
            W.Sign(b, new Vector3(26, 0, 28.6f), 180, "CORRALES\n(se habilitan en la Fase 2)", new Vector2(2.2f, 0.8f));
            return gate;
        }

        /// <summary>Asegura sentido antihorario para la silueta del frontis.</summary>
        static List<Vector2> ArchCCW(List<Vector2> pts)
        {
            float area = 0;
            for (int i = 0; i < pts.Count; i++) { var p = pts[i]; var q = pts[(i + 1) % pts.Count]; area += p.x * q.y - q.x * p.y; }
            if (area < 0) pts.Reverse();
            return pts;
        }

        static void Fences()
        {
            var f = Shapes.Empty(estatico, "Alambrados", Vector3.zero).transform;
            // Perímetro del campo (abierto frente al almacén y en la tranquera)
            W.Fence(f, new Vector3(-56, 0, 6), new Vector3(-56, 0, 82));
            W.Fence(f, new Vector3(46, 0, 6), new Vector3(46, 0, 82));
            W.Fence(f, new Vector3(-56, 0, 82), new Vector3(46, 0, 82));
            W.Fence(f, new Vector3(-56, 0, 6), new Vector3(-11, 0, 6));
            W.Fence(f, new Vector3(15, 0, 6), new Vector3(16, 0, 6), 1f);
            W.Fence(f, new Vector3(20.2f, 0, 6), new Vector3(46, 0, 6));
            // Terrenos de siembra (entrada del lado este, frente a la casa)
            W.Fence(f, new Vector3(-46.5f, 0, 14.5f), new Vector3(-14.5f, 0, 14.5f), 2.5f);
            W.Fence(f, new Vector3(-46.5f, 0, 46.5f), new Vector3(-14.5f, 0, 46.5f), 2.5f);
            W.Fence(f, new Vector3(-46.5f, 0, 14.5f), new Vector3(-46.5f, 0, 46.5f), 2.5f);
            W.Fence(f, new Vector3(-14.5f, 0, 14.5f), new Vector3(-14.5f, 0, 18.5f), 2.5f);
            W.Fence(f, new Vector3(-14.5f, 0, 23.5f), new Vector3(-14.5f, 0, 46.5f), 2.5f);
            // Corrales (cerrados hasta la fase 2)
            W.Fence(f, new Vector3(24, 0, 29), new Vector3(42, 0, 29));
            W.Fence(f, new Vector3(24, 0, 50), new Vector3(42, 0, 50));
            W.Fence(f, new Vector3(24, 0, 29), new Vector3(24, 0, 50));
            W.Fence(f, new Vector3(42, 0, 29), new Vector3(42, 0, 50));
        }

        static void Trees()
        {
            var t = Shapes.Empty(estatico, "Arboles", Vector3.zero).transform;
            int v = 0;
            // Cortina de eucaliptos al fondo y al oeste
            for (float x = -60; x <= 52; x += 6.5f) NatureKit.Place(NatureKit.Tree.Eucalipto, t, new Vector3(x + Rf * 2, 0, 86 + Rf * 3), v++, Rf * 360, 0.9f + Rf * 0.3f);
            for (float z = 12; z <= 80; z += 7f) NatureKit.Place(NatureKit.Tree.Eucalipto, t, new Vector3(-61 + Rf * 2, 0, z), v++, Rf * 360, 0.9f + Rf * 0.3f);
            // Álamos a lo largo del camino de tierra
            for (float z = 10; z <= 52; z += 5.5f) NatureKit.Place(NatureKit.Tree.Alamo, t, new Vector3(23.6f, 0, z), v++, Rf * 360, 0.9f + Rf * 0.2f);
            // Paraísos dando sombra
            // Árboles de sombra (modelos del kit de plantas; si no están, paraísos hechos por código)
            var shade = new[] { new Vector3(-8, 0, 9), new Vector3(11.5f, 0, 9.5f), new Vector3(7, 0, 35), new Vector3(-8.5f, 0, 35.5f), new Vector3(-12, 0, 48),
                                new Vector3(-50, 0, 52), new Vector3(-30, 0, 55), new Vector3(38, 0, 62), new Vector3(4, 0, 62), new Vector3(-20, 0, 8.5f) };
            for (int i = 0; i < shade.Length; i++)
                if (Mdl(t, "Arbol" + (1 + i % 3), "Arbol", shade[i], Rf * 360, false, 0.9f + Rf * 0.4f) == null)
                    NatureKit.Place(NatureKit.Tree.Paraiso, t, shade[i], v++, Rf * 360);
                else
                {
                    var col = new GameObject("ColliderTronco"); col.transform.SetParent(t, false); col.transform.localPosition = shade[i];
                    var cc = col.AddComponent<CapsuleCollider>(); cc.radius = 0.3f; cc.height = 3f; cc.center = Vector3.up * 1.5f;
                }
            // Plantas alrededor de la casa: flores, arbustos, cerco vivo y matas de pasto alto
            for (float z = 17f; z <= 27f; z += 2f) Mdl(t, z % 4 < 2 ? "Flores1" : "Flores2", "Flores", new Vector3(-10.8f, 0, z), Rf * 360);
            foreach (var bp in new[] { new Vector3(-11.2f, 0, 14.4f), new Vector3(12.3f, 0, 14.2f), new Vector3(12.2f, 0, 27.6f), new Vector3(-11, 0, 29), new Vector3(-16, 0, 12.5f) })
                Mdl(t, "Arbusto" + (1 + rnd.Next(3)), "Arbusto", bp, Rf * 360, false, 0.8f + Rf * 0.4f);
            for (float x = -40; x <= -14; x += 2.4f) Mdl(t, "CercoVivo", "CercoVivo", new Vector3(x, 0, 6.8f), 0);
            for (int i = 0; i < 60; i++)
            {
                Vector3 gp;
                int side = rnd.Next(4);
                if (side == 0) gp = new Vector3(-55.3f, 0, 8 + Rf * 72);
                else if (side == 1) gp = new Vector3(45.3f, 0, 8 + Rf * 72);
                else if (side == 2) gp = new Vector3(-55 + Rf * 100, 0, 81.3f);
                else gp = new Vector3(-46 + Rf * 31, 0, 14f);
                Mdl(t, "PastoAlto", "PastoAlto", gp, Rf * 360, false, 0.7f + Rf * 0.8f);
            }
            // Arbustos sueltos
            for (int i = 0; i < 25; i++)
            {
                var p = new Vector3(-55 + Rf * 100, 0, 48 + Rf * 30);
                if (p.x > 8 && p.x < 32 && p.z < 72) continue;
                NatureKit.Place(NatureKit.Tree.Arbusto, t, p, v++, Rf * 360, 0.7f + Rf * 0.6f);
            }
            // Montecitos del otro lado de la ruta
            for (int i = 0; i < 14; i++) NatureKit.Place(NatureKit.Tree.Eucalipto, t, new Vector3(-150 + Rf * 30, 0, -12 - Rf * 25), v++, Rf * 360);
            for (int i = 0; i < 10; i++) NatureKit.Place(NatureKit.Tree.Eucalipto, t, new Vector3(130 + Rf * 25, 0, -15 - Rf * 30), v++, Rf * 360);
            for (int i = 0; i < 8; i++) NatureKit.Place(NatureKit.Tree.Paraiso, t, new Vector3(Rf * 160 - 80, 0, -96 - Rf * 10), v++, Rf * 360);
        }

        static void Hills()
        {
            var go = new GameObject("Sierras de fondo");
            go.transform.SetParent(estatico, false);
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>().sharedMaterial = Mats.Get("Sierras", new Color(0.4f, 0.48f, 0.32f), 0.05f, 0f);
            go.AddComponent<HillsBackdrop>();
        }

        static Material GrassMat(string name, Color baseC, Color tip)
        {
            string path = Mats.Folder + "/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var sh = Shader.Find("LaTranquera/Pasto");
            if (m == null)
            {
                m = sh != null ? new Material(sh) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            if (sh != null && m.shader != sh) m.shader = sh;
            m.SetColor("_ColorBase", baseC);
            m.SetColor("_ColorTip", tip);
            m.SetColor("_BaseColor", tip);
            m.enableInstancing = true;
            EditorUtility.SetDirty(m);
            return m;
        }

        static void Grass()
        {
            var go = new GameObject("Pasto");
            var gf = go.AddComponent<GrassField>();
            gf.areaMin = new Vector2(-95, -40);
            gf.areaMax = new Vector2(95, 95);
            gf.materials = new[]
            {
                GrassMat("Pasto1", new Color(0.2f, 0.33f, 0.11f), new Color(0.48f, 0.62f, 0.25f)),
                GrassMat("Pasto2", new Color(0.22f, 0.36f, 0.12f), new Color(0.56f, 0.68f, 0.3f)),
                GrassMat("Pasto3", new Color(0.18f, 0.3f, 0.1f), new Color(0.42f, 0.56f, 0.22f)),
                GrassMat("PastoSeco", new Color(0.4f, 0.38f, 0.2f), new Color(0.78f, 0.72f, 0.45f)),
            };
            gf.exclusions = new List<Rect>(noGrass);
        }

        // ------------------------------------------------------------------
        static void Systems(StoreRefs store, DepRefs dep, SleepBed bed, Transform[] crateSpots)
        {
            var sys = new GameObject("_Sistemas");
            var gm = sys.AddComponent<GameManager>();
            sys.AddComponent<TimeManager>();
            sys.AddComponent<Economy>();
            sys.AddComponent<StoreManager>();
            var dm = sys.AddComponent<DeliveryManager>();
            sys.AddComponent<CustomerSpawner>();
            sys.AddComponent<TrafficSpawner>();
            sys.AddComponent<HUD>();
            sys.AddComponent<ComputerUI>();
            var ng = sys.AddComponent<NewGameSetup>();
            ng.hoeHook = dep.hoe; ng.canHook = dep.can; ng.bucketHook = dep.bucket; ng.labelerHook = store.labelerHook;
            ng.crateSpots = crateSpots;
            ng.seedSpots = dep.seedSpots;
            ng.boxSpots = dep.boxSpots;
            gm.newGameSetup = ng;
            gm.bed = bed;

            Transform Pt(Transform parent, string n, float x, float z) => Shapes.Empty(parent, n, new Vector3(x, 0.03f, z)).transform;

            // Camioneta del proveedor: entra por la tranquera, descarga en el portón del depósito, da la vuelta y sale
            dm.dropPoint = dep.drop;
            var route = Shapes.Empty(sys.transform, "RutaCamioneta", Vector3.zero).transform;
            dm.truckRoute = new[] { Pt(route, "E0", 230, 1.8f), Pt(route, "E1", 26, 1.8f), Pt(route, "E2", 18, 5), Pt(route, "E3", 18, 13), Pt(route, "E4", 16.2f, 18), Pt(route, "E5", 15f, 21.5f) };
            dm.truckExit = new[] { Pt(route, "S0", 15.5f, 27), Pt(route, "S1", 18.5f, 33), Pt(route, "S2", 21, 30), Pt(route, "S3", 19, 22), Pt(route, "S4", 18, 12),
                                   Pt(route, "S5", 18, 5), Pt(route, "S6", 12, 1.8f), Pt(route, "S7", -230, 1.8f) };

            // El lechero en Rastrojero: por el camino de tierra hasta el tambo y vuelta
            var milk = sys.AddComponent<MilkmanVisit>();
            var mr = Shapes.Empty(sys.transform, "RutaLechero", Vector3.zero).transform;
            milk.routeIn = new[] { Pt(mr, "L0", -230, -1.8f), Pt(mr, "L1", 10, -1.8f), Pt(mr, "L2", 18, 4), Pt(mr, "L3", 18, 30), Pt(mr, "L4", 18, 48), Pt(mr, "L5", 20, 53.5f) };
            milk.routeOut = new[] { Pt(mr, "M0", 24, 52), Pt(mr, "M1", 21, 46), Pt(mr, "M2", 18, 38), Pt(mr, "M3", 18, 6), Pt(mr, "M4", 22, 1.8f)
                , Pt(mr, "M5", 230, -1.8f) };

            var nav = new GameObject("CaminoClientes").AddComponent<StoreNav>();
            var nt = nav.transform;
            nav.spawnWest = Shapes.Empty(nt, "AparecenOeste", new Vector3(-38, 0.03f, 4.8f)).transform;
            nav.spawnEast = Shapes.Empty(nt, "AparecenEste", new Vector3(38, 0.03f, 4.8f)).transform;
            nav.curbWest = Shapes.Empty(nt, "BanquinaOeste", new Vector3(-7, 0.03f, 4.8f)).transform;
            nav.curbEast = Shapes.Empty(nt, "BanquinaEste", new Vector3(7, 0.03f, 4.8f)).transform;
            nav.parking = Shapes.Empty(nt, "Estacionamiento", new Vector3(0, 0.03f, 10f)).transform;
            nav.doorOutside = Shapes.Empty(nt, "PuertaAfuera", new Vector3(0, FL, 14.6f)).transform;
            nav.doorInside = Shapes.Empty(nt, "PuertaAdentro", new Vector3(0, FL, 16.9f)).transform;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        static void Player()
        {
            var p = new GameObject("Jugador");
            p.transform.position = new Vector3(0, 0.15f, 10f);
            var cc = p.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0, 0.9f, 0); cc.stepOffset = 0.35f; cc.slopeLimit = 50f;
            var pc = p.AddComponent<PlayerController>();
            var hands = p.AddComponent<PlayerHands>();
            var pivot = Shapes.Empty(p.transform, "CamaraPivot", new Vector3(0, 1.65f, 0)).transform;
            pc.cameraPivot = pivot;
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(pivot, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 1200f;
            cam.fieldOfView = 70f;
            camGo.AddComponent<AudioListener>();
            var anchor = Shapes.Empty(camGo.transform, "Manos", Vector3.zero).transform;
            hands.holdAnchor = anchor;
            hands.cam = cam;
            var inter = camGo.AddComponent<Interactor>();
            inter.cam = cam;
            inter.hands = hands;
        }
    }
}
