using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LaTranquera.EditorTools
{
    /// <summary>
    /// Menú "La Tranquera > Construir escena Fase 1".
    /// Arma TODA la escena jugable: mapa, ruta, almacén, huerta, pozo, galpón, casa,
    /// sistemas y jugador. Después podés mover y cambiar lo que quieras a mano.
    ///
    /// Coordenadas del mapa (metros): la ruta corre de oeste a este sobre Z = 0.
    /// El almacén está al norte de la ruta (Z 16 a 22), la huerta al oeste, el galpón al este
    /// y la casa atrás del almacén.
    /// </summary>
    public static class SceneBuilder
    {
        const string ScenePath = "Assets/LaTranquera/Scenes/Fase1.unity";
        const float CT = 0.96f; // altura de la tapa del mostrador

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
                EditorUtility.DisplayProgressBar("La Tranquera", "Cielo y luz...", 0.05f);
                var sun = Lighting();
                EditorUtility.DisplayProgressBar("La Tranquera", "Terreno y ruta...", 0.15f);
                Ground();
                Road();
                Paths();
                EditorUtility.DisplayProgressBar("La Tranquera", "Almacén...", 0.3f);
                var store = Store();
                EditorUtility.DisplayProgressBar("La Tranquera", "Galpón y casa...", 0.45f);
                var galpon = Galpon();
                var bed = House();
                EditorUtility.DisplayProgressBar("La Tranquera", "Huerta y pozo...", 0.55f);
                var crateSpots = Garden();
                Well();
                EditorUtility.DisplayProgressBar("La Tranquera", "Alambrados y árboles...", 0.7f);
                Fences();
                Trees();
                Hills();
                Grass();
                EditorUtility.DisplayProgressBar("La Tranquera", "Sistemas y jugador...", 0.85f);
                Systems(store, galpon, bed, crateSpots);
                Player();
                sun.GetComponent<DayNightCycle>().nightLights = new List<Light>(nightLights);

                foreach (Transform t in estatico) SetStatic(t.gameObject);

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

        static void SetStatic(GameObject go)
        {
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            foreach (Transform c in go.transform) SetStatic(c.gameObject);
        }

        static Rect R(float x0, float z0, float x1, float z1) => new Rect(x0, z0, x1 - x0, z1 - z0);

        /// <summary>Capa plana en el piso (camino, ripio). Sin collider, el piso ya lo tiene.</summary>
        static void Flat(string name, float x0, float z0, float x1, float z1, Color c, float h = 0.02f, bool grassFree = true)
        {
            Shapes.Box(estatico, name, new Vector3((x0 + x1) / 2, 0, (z0 + z1) / 2), new Vector3(x1 - x0, h, z1 - z0), c, false);
            if (grassFree) noGrass.Add(R(x0, z0, x1, z1));
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
            Shapes.Box(estatico, "Piso", new Vector3(0, -0.1f, 0), new Vector3(1400, 0.1f, 1400), new Color(0.38f, 0.5f, 0.24f), true);
            // Campo sembrado del otro lado de la ruta (surcos de soja)
            var campo = Shapes.Empty(estatico, "CampoSembrado", Vector3.zero).transform;
            Shapes.Box(campo, "Tierra", new Vector3(0, 0, -55), new Vector3(240, 0.02f, 70), new Color(0.4f, 0.32f, 0.22f), false);
            for (int i = 0; i < 34; i++)
                Shapes.Box(campo, "Surco", new Vector3(0, 0.02f, -22f - i * 2f), new Vector3(238, 0.18f, 0.9f), new Color(0.35f, 0.55f, 0.2f), false);
            noGrass.Add(R(-120, -90, 120, -20));
        }

        static void Road()
        {
            var road = Shapes.Empty(estatico, "Ruta", Vector3.zero).transform;
            Shapes.Box(road, "Asfalto", new Vector3(0, 0, 0), new Vector3(800, 0.04f, 7.2f), Mats.Asfalto, false);
            Shapes.Box(road, "LineaBordeN", new Vector3(0, 0.04f, 3.4f), new Vector3(800, 0.005f, 0.12f), Mats.Blanco, false);
            Shapes.Box(road, "LineaBordeS", new Vector3(0, 0.04f, -3.4f), new Vector3(800, 0.005f, 0.12f), Mats.Blanco, false);
            for (float x = -396; x < 400; x += 9)
                Shapes.Box(road, "LineaCentral", new Vector3(x, 0.04f, 0), new Vector3(3.5f, 0.005f, 0.12f), new Color(0.95f, 0.85f, 0.3f), false);
            Shapes.Box(road, "BanquinaN", new Vector3(0, 0, 4.6f), new Vector3(800, 0.03f, 2f), Mats.Ripio, false);
            Shapes.Box(road, "BanquinaS", new Vector3(0, 0, -4.6f), new Vector3(800, 0.03f, 2f), Mats.Ripio, false);
            noGrass.Add(R(-400, -5.7f, 400, 5.7f));

            foreach (float x in new float[] { -90, -60, -30, 0, 30, 60, 90 })
            {
                var l = WorldParts.StreetLight(road, new Vector3(x, 0, 5.4f), Vector3.back, Mathf.Abs(x) <= 30);
                if (l != null) nightLights.Add(l);
            }
            WorldParts.Sign(road, new Vector3(-14, 0, 5.2f), 180, "ALMACÉN LA TRANQUERA\nVerduras frescas · 50 m", new Vector2(2.4f, 0.9f));
            WorldParts.Sign(road, new Vector3(16, 0, -5.2f), 0, "RUTA PROVINCIAL 5", new Vector2(1.8f, 0.5f));
        }

        static void Paths()
        {
            Color dirt = new Color(0.55f, 0.43f, 0.3f);
            Flat("Estacionamiento (ripio)", -11, 5.6f, 16.5f, 15.5f, Mats.Ripio, 0.03f);
            Flat("Vereda", -4.6f, 14.3f, 4.6f, 16f, new Color(0.7f, 0.68f, 0.64f), 0.06f);
            Flat("Camino a la huerta", -13.5f, 22.0f, 0.7f, 23.4f, dirt);
            Flat("Camino lateral", -5.6f, 15.5f, -4.2f, 23.4f, dirt);
            Flat("Camino al pozo", -11.2f, 23.4f, -9.8f, 25.6f, dirt);
            Flat("Camino a la casa", -0.7f, 23.4f, 0.7f, 33.5f, dirt);
            Flat("Camino de la tranquera", 25f, 3.6f, 29f, 6.4f, Mats.Ripio, 0.03f);
            Flat("Camino a los corrales", 26.3f, 6.4f, 27.7f, 28f, dirt);
        }

        // ------------------------------------------------------------------
        public class StoreRefs
        {
            public Shelf shelfA, shelfB, fridge;
            public CheckoutCounter counter;
            public CashDrawer drawer;
            public ChangeTray tray;
            public Posnet posnet;
            public RegisterScreen screen;
            public ToolHook labelerHook;
            public Transform[] queue;
            public Transform customerPoint;
        }

        static StoreRefs Store()
        {
            var s = new StoreRefs();
            var b = Shapes.Empty(estatico, "Almacen (edificio)", Vector3.zero).transform;
            Color wall = Mats.Revoque;
            Shapes.Box(b, "PisoBaldosas", new Vector3(0, 0, 19), new Vector3(8, 0.03f, 6), new Color(0.78f, 0.72f, 0.62f), true);
            noGrass.Add(R(-4.3f, 15.7f, 4.3f, 22.3f));
            float H = 3.1f;
            WorldParts.Wall(b, "ParedFrente", new Vector3(-4, 0, 16), new Vector3(4, 0, 16), H, 0.15f, wall,
                new WorldParts.Opening(0.8f, 2.8f, 1.0f, 2.1f, true), new WorldParts.Opening(3.4f, 4.6f, 0, 2.25f), new WorldParts.Opening(5.4f, 7.4f, 1.05f, 2.1f, true));
            WorldParts.Wall(b, "ParedFondo", new Vector3(-4, 0, 22), new Vector3(4, 0, 22), H, 0.15f, wall);
            WorldParts.Wall(b, "ParedOeste", new Vector3(-4, 0, 16), new Vector3(-4, 0, 22), H, 0.15f, wall);
            WorldParts.Wall(b, "ParedEste", new Vector3(4, 0, 16), new Vector3(4, 0, 22), H, 0.15f, wall, new WorldParts.Opening(1.4f, 3f, 1.3f, 2.2f, true));
            // zócalo de ladrillo afuera
            Shapes.Box(b, "Zocalo", new Vector3(-2.3f, 0, 15.9f), new Vector3(3.4f, 0.6f, 0.06f), new Color(0.6f, 0.28f, 0.2f), false);
            Shapes.Box(b, "Zocalo", new Vector3(2.3f, 0, 15.9f), new Vector3(3.4f, 0.6f, 0.06f), new Color(0.6f, 0.28f, 0.2f), false);
            // techo de chapa
            Shapes.BoxC(b, "Techo", new Vector3(0, H + 0.15f, 19), new Vector3(8.8f, 0.06f, 7f), Mats.Get(Mats.Chapa, 0.5f, 0.6f), true, new Vector3(-4, 0, 0));
            // fachada con el nombre
            Shapes.Box(b, "Fachada", new Vector3(0, H, 15.98f), new Vector3(8.2f, 1.0f, 0.14f), wall, true);
            WorldLabel.Create(b, "ALMACÉN DE RAMOS GENERALES\nLA TRANQUERA", new Vector3(0, H + 0.5f, 15.9f), new Vector2(7.8f, 0.9f), 300, new Color(0.45f, 0.12f, 0.08f), Vector3.back);
            // alero sobre la puerta
            Shapes.BoxC(b, "Alero", new Vector3(0, 2.6f, 15.3f), new Vector3(3f, 0.05f, 1.4f), Mats.Get(Mats.Chapa, 0.5f, 0.6f), false, new Vector3(8, 0, 0));
            Shapes.Cyl(b, "ColumnaAlero", new Vector3(-1.4f, 0, 14.7f), 0.1f, 2.55f, Mats.Madera, true);
            Shapes.Cyl(b, "ColumnaAlero", new Vector3(1.4f, 0, 14.7f), 0.1f, 2.55f, Mats.Madera, true);
            // cortina de tiras en la puerta
            Color[] strips = { new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.5f, 0.9f), new Color(0.95f, 0.85f, 0.2f), new Color(0.2f, 0.7f, 0.3f) };
            for (int i = 0; i < 12; i++)
                Shapes.Box(b, "Tira", new Vector3(-0.55f + i * 0.1f, 1.0f, 16.0f), new Vector3(0.04f, 1.22f, 0.01f), strips[i % 4], false);
            nightLights.Add(WorldParts.RoomLight(b, new Vector3(0, 2.45f, 15.2f), 9f, 1.5f));
            WorldParts.RoomLight(b, new Vector3(0, 2.85f, 19f), 10f, 2.2f);
            WorldParts.RoomLight(b, new Vector3(2.8f, 2.85f, 18.5f), 6f, 1.2f);

            var inter = Shapes.Empty(interactivo, "Almacen", Vector3.zero).transform;

            // --- Estanterías ---
            s.shelfA = ShelfObj(inter, "Estanteria_1", "estanteria_1", new Vector3(-3.45f, 0.03f, 19f), 90, 2.4f, 0.42f, 4, 3, 0.12f, 0.44f, false, new Vector3(-2.5f, 0, 19f));
            s.shelfB = ShelfObj(inter, "Estanteria_2", "estanteria_2", new Vector3(-1.3f, 0.03f, 21.65f), 180, 2.0f, 0.42f, 4, 3, 0.12f, 0.44f, false, new Vector3(-1.3f, 0, 20.75f));
            s.fridge = ShelfObj(inter, "Heladera", "heladera_1", new Vector3(1.45f, 0.03f, 21.55f), 180, 1.4f, 0.5f, 4, 2, 0.28f, 0.42f, true, new Vector3(1.45f, 0, 20.55f));

            // --- Mostrador ---
            var counterGo = Shapes.Empty(inter, "Mostrador", Vector3.zero);
            var ct = counterGo.transform;
            Shapes.Box(ct, "Cuerpo", new Vector3(2.6f, 0.03f, 18.3f), new Vector3(0.6f, CT - 0.07f, 2.6f), Mats.Madera, true);
            Shapes.Box(ct, "Frente", new Vector3(2.29f, 0.03f, 18.3f), new Vector3(0.02f, CT - 0.1f, 2.62f), new Color(0.45f, 0.3f, 0.18f), false);
            Shapes.Box(ct, "Tapa", new Vector3(2.6f, CT - 0.04f, 18.3f), new Vector3(0.7f, 0.04f, 2.7f), new Color(0.82f, 0.78f, 0.7f), true);
            var co = counterGo.AddComponent<CheckoutCounter>();
            s.counter = co;
            co.customerPoint = Shapes.Empty(ct, "PuntoCliente", new Vector3(1.75f, 0, 18.4f), new Vector3(0, 90, 0)).transform;
            s.customerPoint = co.customerPoint;
            co.queuePoints = new[]
            {
                Shapes.Empty(ct, "Fila1", new Vector3(0.95f, 0, 18.4f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(ct, "Fila2", new Vector3(0.15f, 0, 18.4f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(ct, "Fila3", new Vector3(-0.65f, 0, 18.4f), new Vector3(0, 90, 0)).transform,
            };
            co.itemArea = Shapes.Empty(ct, "ZonaProductos", new Vector3(2.45f, CT, 18.75f)).transform;
            co.scanPoint = Shapes.Empty(ct, "Lector", new Vector3(2.58f, CT + 0.06f, 18.05f)).transform;
            co.bagPoint = Shapes.Empty(ct, "Embolsado", new Vector3(2.45f, CT, 17.3f)).transform;
            co.cashPoint = Shapes.Empty(ct, "PlataCliente", new Vector3(2.42f, CT, 17.72f)).transform;
            // lector de código de barras
            Shapes.Box(ct, "LectorBase", new Vector3(2.58f, CT, 18.05f), new Vector3(0.18f, 0.03f, 0.16f), new Color(0.1f, 0.1f, 0.1f), false);
            Shapes.Box(ct, "LectorVidrio", new Vector3(2.58f, CT + 0.03f, 18.05f), new Vector3(0.12f, 0.003f, 0.1f), Mats.Emissive(new Color(1f, 0.15f, 0.1f), 1.5f), false);
            Shapes.Box(ct, "BolsaPapel", new Vector3(2.42f, CT, 17.15f), new Vector3(0.18f, 0.25f, 0.12f), new Color(0.8f, 0.68f, 0.5f), false);

            // pantalla de la caja
            var scr = Shapes.Empty(ct, "PantallaCaja", new Vector3(2.8f, CT, 19.3f), new Vector3(0, 90, 0));
            s.screen = scr.AddComponent<RegisterScreen>();
            s.screen.counter = co;
            s.screen.Build();
            co.screen = s.screen;

            // cajón de billetes (abajo de la tapa, del lado del jugador)
            var dr = Shapes.Empty(ct, "CajonBilletes", new Vector3(2.72f, 0.7f, 18.3f), new Vector3(0, 90, 0));
            s.drawer = dr.AddComponent<CashDrawer>();
            s.drawer.Build();
            co.drawer = s.drawer;

            // bandeja del vuelto
            var tr = Shapes.Empty(ct, "BandejaVuelto", new Vector3(2.76f, CT, 17.6f));
            s.tray = tr.AddComponent<ChangeTray>();
            s.tray.drawer = s.drawer;
            s.tray.Build();
            s.drawer.tray = s.tray;
            co.tray = s.tray;

            // botón ENTREGAR
            var btn = Shapes.Box(ct, "BotonEntregar", new Vector3(2.8f, CT, 17.22f), new Vector3(0.12f, 0.05f, 0.12f), new Color(0.85f, 0.15f, 0.1f), true);
            btn.AddComponent<GiveChangeButton>().counter = co;
            WorldLabel.Create(ct, "ENTREGAR\nVUELTO", new Vector3(2.8f, CT + 0.052f, 17.22f), new Vector2(0.12f, 0.1f), 18, Color.white, Vector3.up, null, Vector3.left);

            // posnet
            var pn = Shapes.Empty(ct, "Posnet", new Vector3(2.72f, CT, 18.6f), new Vector3(0, 90, 0));
            s.posnet = pn.AddComponent<Posnet>();
            s.posnet.counter = co;
            s.posnet.Build();
            co.posnet = s.posnet;

            // computadora en el escritorio del fondo
            var desk = Shapes.Empty(inter, "Computadora", new Vector3(3.6f, 0.03f, 21.25f), new Vector3(0, -90, 0)).transform;
            Shapes.Box(desk, "Escritorio", Vector3.zero, new Vector3(1.1f, 0.75f, 0.6f), Mats.MaderaClara, true);
            Shapes.Box(desk, "Monitor", new Vector3(0, 0.95f, 0.15f), new Vector3(0.5f, 0.36f, 0.05f), new Color(0.1f, 0.1f, 0.12f), true);
            Shapes.Box(desk, "PieMonitor", new Vector3(0, 0.75f, 0.15f), new Vector3(0.08f, 0.2f, 0.08f), new Color(0.1f, 0.1f, 0.12f), false);
            Shapes.Box(desk, "Teclado", new Vector3(0, 0.75f, -0.1f), new Vector3(0.4f, 0.02f, 0.14f), new Color(0.2f, 0.2f, 0.22f), false);
            WorldLabel.Create(desk, "PROVEEDOR\nPRECIOS · MEJORAS", new Vector3(0, 1.13f, 0.12f), new Vector2(0.46f, 0.32f), 40, new Color(0.7f, 0.9f, 1f), Vector3.back, new Color(0.08f, 0.2f, 0.35f));
            desk.gameObject.AddComponent<ComputerTerminal>();

            // lugar de la etiquetadora
            var lh = Shapes.Empty(inter, "LugarEtiquetadora", new Vector3(3.6f, 0.82f, 20.45f), new Vector3(0, -90, 0));
            Shapes.Box(lh.transform, "Repisa", new Vector3(0, -0.02f, 0), new Vector3(0.3f, 0.02f, 0.25f), Mats.Madera, false);
            s.labelerHook = Hook(lh, "labeler", "Lugar de la etiquetadora", new Vector3(0.35f, 0.3f, 0.35f));

            // cartel ABIERTO/CERRADO colgado en la vidriera
            var sign = Shapes.Empty(inter, "CartelAbiertoCerrado", new Vector3(-2.2f, 1.45f, 16.15f));
            var board = Shapes.BoxC(sign.transform, "Tabla", Vector3.zero, new Vector3(0.7f, 0.28f, 0.02f), Color.white, true);
            Shapes.Beam(sign.transform, "Hilo", new Vector3(-0.3f, 0.14f, 0), new Vector3(0, 0.5f, 0), 0.005f, Color.black);
            Shapes.Beam(sign.transform, "Hilo", new Vector3(0.3f, 0.14f, 0), new Vector3(0, 0.5f, 0), 0.005f, Color.black);
            var os = sign.AddComponent<OpenSign>();
            os.frontText = WorldLabel.Create(sign.transform, "CERRADO", new Vector3(0, 0, -0.012f), new Vector2(0.66f, 0.26f), 130, Color.red, Vector3.back);
            os.backText = WorldLabel.Create(sign.transform, "CERRADO", new Vector3(0, 0, 0.012f), new Vector2(0.66f, 0.26f), 130, Color.red, Vector3.forward);
            os.board = board.GetComponent<Renderer>();

            // tacho de basura afuera
            var bin = Shapes.Cyl(inter, "TachoBasura", new Vector3(1.6f, 0.06f, 15.1f), 0.45f, 0.75f, new Color(0.2f, 0.4f, 0.25f), true);
            bin.AddComponent<TrashBin>();

            s.queue = co.queuePoints;
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
            float firstY, float spacing, bool fridge, Vector3 customerPos)
        {
            var go = Shapes.Empty(parent, name, pos, new Vector3(0, yaw, 0));
            var t = go.transform;
            Color frame = fridge ? new Color(0.92f, 0.92f, 0.94f) : new Color(0.5f, 0.55f, 0.6f);
            Color board = fridge ? new Color(0.85f, 0.88f, 0.9f) : new Color(0.75f, 0.62f, 0.45f);
            float top = firstY + levels * spacing + 0.05f;
            Shapes.Box(t, "LadoI", new Vector3(-width / 2 - 0.02f, 0, 0), new Vector3(0.04f, top, depth + 0.04f), frame, true);
            Shapes.Box(t, "LadoD", new Vector3(width / 2 + 0.02f, 0, 0), new Vector3(0.04f, top, depth + 0.04f), frame, true);
            Shapes.Box(t, "Fondo", new Vector3(0, 0, -depth / 2 - 0.02f), new Vector3(width + 0.08f, top, 0.03f), frame, true);
            Shapes.Box(t, "Techo", new Vector3(0, top, 0), new Vector3(width + 0.08f, 0.04f, depth + 0.04f), frame, true);
            Shapes.Box(t, "Zocalo", new Vector3(0, 0, 0), new Vector3(width, firstY - 0.01f, depth), frame, true);
            for (int l = 0; l < levels; l++)
                Shapes.Box(t, "Estante", new Vector3(0, firstY + l * spacing - 0.02f, 0), new Vector3(width, 0.02f, depth), board, true);
            if (fridge)
            {
                // puertas de vidrio (sin collider para poder poner y sacar), luz interior y motor
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
            var cp = Shapes.Empty(parent, name + "_PuntoCliente", customerPos);
            shelf.customerPoint = cp.transform;
            return shelf;
        }

        // ------------------------------------------------------------------
        public class GalponRefs
        {
            public ToolHook hoe, can, bucket;
            public Transform[] seedSpots, boxSpots;
            public Transform drop;
        }

        static GalponRefs Galpon()
        {
            var g = new GalponRefs();
            var b = Shapes.Empty(estatico, "Galpon (edificio)", Vector3.zero).transform;
            Color chapa = Mats.Chapa;
            Shapes.Box(b, "Contrapiso", new Vector3(13, 0, 20), new Vector3(5, 0.04f, 6), new Color(0.6f, 0.6f, 0.58f), true);
            noGrass.Add(R(10.3f, 16.8f, 15.7f, 23.2f));
            float H = 3.2f;
            WorldParts.Wall(b, "ChapaFrente", new Vector3(10.5f, 0, 17), new Vector3(15.5f, 0, 17), H, 0.06f, chapa, new WorldParts.Opening(0.9f, 4.1f, 0, 2.7f));
            WorldParts.Wall(b, "ChapaFondo", new Vector3(10.5f, 0, 23), new Vector3(15.5f, 0, 23), H, 0.06f, chapa);
            WorldParts.Wall(b, "ChapaOeste", new Vector3(10.5f, 0, 17), new Vector3(10.5f, 0, 23), H, 0.06f, chapa);
            WorldParts.Wall(b, "ChapaEste", new Vector3(15.5f, 0, 17), new Vector3(15.5f, 0, 23), H, 0.06f, chapa);
            Shapes.BoxC(b, "Techo", new Vector3(13, H + 0.2f, 20), new Vector3(5.6f, 0.05f, 6.6f), Mats.Get(Mats.Chapa, 0.5f, 0.6f), true, new Vector3(-5, 0, 0));
            WorldLabel.Create(b, "GALPÓN · DEPÓSITO", new Vector3(13, 2.95f, 16.95f), new Vector2(3.2f, 0.4f), 220, new Color(0.15f, 0.15f, 0.15f), Vector3.back);
            WorldParts.RoomLight(b, new Vector3(13, 2.9f, 20), 9f, 1.6f);

            var inter = Shapes.Empty(interactivo, "Galpon", Vector3.zero).transform;
            // tablero de herramientas en la pared del fondo
            Shapes.Box(b, "Tablero", new Vector3(12.6f, 0.9f, 22.9f), new Vector3(3f, 1.3f, 0.04f), Mats.MaderaGris, false);
            WorldLabel.Create(b, "HERRAMIENTAS", new Vector3(12.6f, 2.05f, 22.86f), new Vector2(2f, 0.2f), 120, Color.white, Vector3.back);
            var hoe = Shapes.Empty(inter, "LugarAzada", new Vector3(11.6f, 0.05f, 22.7f));
            g.hoe = Hook(hoe, "hoe", "Lugar de la azada", new Vector3(0.4f, 1.4f, 0.3f));
            Shapes.Box(b, "RepisaHerr", new Vector3(13.1f, 0.85f, 22.65f), new Vector3(1.8f, 0.04f, 0.45f), Mats.Madera, true);
            var can = Shapes.Empty(inter, "LugarRegadera", new Vector3(12.6f, 0.89f, 22.62f), new Vector3(0, 180, 0));
            g.can = Hook(can, "can", "Lugar de la regadera", new Vector3(0.45f, 0.45f, 0.45f));
            var bucket = Shapes.Empty(inter, "LugarBalde", new Vector3(13.6f, 0.89f, 22.62f));
            g.bucket = Hook(bucket, "bucket", "Lugar del balde", new Vector3(0.4f, 0.45f, 0.4f));
            // mesa de trabajo con las semillas
            var mesa = Shapes.Empty(b, "MesaTrabajo", new Vector3(11.1f, 0, 19.7f)).transform;
            Shapes.Box(mesa, "Tapa", new Vector3(0, 0.82f, 0), new Vector3(0.8f, 0.05f, 2.2f), Mats.MaderaClara, true);
            foreach (var p in new[] { new Vector3(-0.35f, 0, -1f), new Vector3(0.35f, 0, -1f), new Vector3(-0.35f, 0, 1f), new Vector3(0.35f, 0, 1f) })
                Shapes.Box(mesa, "Pata", p, new Vector3(0.06f, 0.82f, 0.06f), Mats.Madera, true);
            g.seedSpots = new[]
            {
                Shapes.Empty(inter, "Semillas1", new Vector3(11.1f, 0.9f, 19.1f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(inter, "Semillas2", new Vector3(11.1f, 0.9f, 19.6f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(inter, "Semillas3", new Vector3(11.1f, 0.9f, 20.1f), new Vector3(0, 90, 0)).transform,
            };
            g.boxSpots = new[]
            {
                Shapes.Empty(inter, "Caja1", new Vector3(13.4f, 0.05f, 18.4f)).transform,
                Shapes.Empty(inter, "Caja2", new Vector3(14.0f, 0.05f, 18.4f)).transform,
                Shapes.Empty(inter, "Caja3", new Vector3(14.6f, 0.05f, 18.4f)).transform,
            };
            g.drop = Shapes.Empty(inter, "DescargaProveedor", new Vector3(13f, 0.05f, 15.7f)).transform;
            return g;
        }

        static SleepBed House()
        {
            var b = Shapes.Empty(estatico, "Casa", Vector3.zero).transform;
            Color wall = new Color(0.95f, 0.93f, 0.88f);
            Shapes.Box(b, "Piso", new Vector3(0, 0, 36), new Vector3(6, 0.03f, 5), new Color(0.6f, 0.45f, 0.35f), true);
            noGrass.Add(R(-3.2f, 33.3f, 3.2f, 38.7f));
            float H = 2.8f;
            WorldParts.Wall(b, "Frente", new Vector3(-3, 0, 33.5f), new Vector3(3, 0, 33.5f), H, 0.15f, wall,
                new WorldParts.Opening(1.0f, 2.2f, 1.0f, 2.0f, true), new WorldParts.Opening(2.5f, 3.5f, 0, 2.15f), new WorldParts.Opening(4.0f, 5.2f, 1.0f, 2.0f, true));
            WorldParts.Wall(b, "Fondo", new Vector3(-3, 0, 38.5f), new Vector3(3, 0, 38.5f), H, 0.15f, wall);
            WorldParts.Wall(b, "Oeste", new Vector3(-3, 0, 33.5f), new Vector3(-3, 0, 38.5f), H, 0.15f, wall);
            WorldParts.Wall(b, "Este", new Vector3(3, 0, 33.5f), new Vector3(3, 0, 38.5f), H, 0.15f, wall);
            Color teja = new Color(0.68f, 0.3f, 0.2f);
            Shapes.BoxC(b, "TechoS", new Vector3(0, H + 0.55f, 34.7f), new Vector3(6.6f, 0.1f, 3.0f), teja, true, new Vector3(-25, 0, 0));
            Shapes.BoxC(b, "TechoN", new Vector3(0, H + 0.55f, 37.3f), new Vector3(6.6f, 0.1f, 3.0f), teja, true, new Vector3(25, 0, 0));
            Shapes.Box(b, "MuroTechoO", new Vector3(-3, H, 36), new Vector3(0.15f, 1.1f, 5f), wall, false);
            Shapes.Box(b, "MuroTechoE", new Vector3(3, H, 36), new Vector3(0.15f, 1.1f, 5f), wall, false);
            nightLights.Add(WorldParts.RoomLight(b, new Vector3(0, 2.5f, 36), 7f, 1.4f));
            // mesa y silla
            Shapes.Box(b, "Mesa", new Vector3(1.6f, 0.72f, 36.5f), new Vector3(1.0f, 0.04f, 0.8f), Mats.Madera, true);
            Shapes.Box(b, "PataMesa", new Vector3(1.6f, 0, 36.5f), new Vector3(0.1f, 0.72f, 0.1f), Mats.Madera, true);
            Shapes.Box(b, "Silla", new Vector3(1.6f, 0.42f, 35.8f), new Vector3(0.42f, 0.04f, 0.42f), Mats.Madera, false);
            // cama
            var inter = Shapes.Empty(interactivo, "Cama", new Vector3(-2.0f, 0.03f, 37.3f)).transform;
            Shapes.Box(inter, "Patas", Vector3.zero, new Vector3(1.0f, 0.35f, 2.0f), Mats.Madera, true);
            Shapes.Box(inter, "Colchon", new Vector3(0, 0.35f, 0), new Vector3(0.95f, 0.15f, 1.95f), new Color(0.9f, 0.9f, 0.85f), true);
            Shapes.Box(inter, "Frazada", new Vector3(0, 0.5f, -0.3f), new Vector3(0.97f, 0.04f, 1.3f), new Color(0.55f, 0.2f, 0.2f), false);
            Shapes.Box(inter, "Almohada", new Vector3(0, 0.5f, 0.7f), new Vector3(0.6f, 0.1f, 0.35f), Color.white, false);
            Shapes.Box(inter, "Respaldo", new Vector3(0, 0, 1.0f), new Vector3(1.0f, 0.95f, 0.06f), Mats.Madera, true);
            var bed = inter.gameObject.AddComponent<SleepBed>();
            bed.wakeUpPoint = Shapes.Empty(interactivo, "Despertar", new Vector3(-0.8f, 0.1f, 36.2f), new Vector3(0, 180, 0)).transform;
            return bed;
        }

        static Transform[] Garden()
        {
            var h = Shapes.Empty(interactivo, "Huerta", Vector3.zero).transform;
            Flat("TierraHuerta", -33.6f, 14.4f, -13.4f, 36.6f, new Color(0.5f, 0.42f, 0.3f), 0.015f);
            Vector3[] open = { new Vector3(-29, 0.015f, 19), new Vector3(-25, 0.015f, 19), new Vector3(-21, 0.015f, 19), new Vector3(-29, 0.015f, 25.5f), new Vector3(-25, 0.015f, 25.5f), new Vector3(-21, 0.015f, 25.5f) };
            Vector3[] locked = { new Vector3(-17, 0.015f, 19), new Vector3(-17, 0.015f, 25.5f), new Vector3(-29, 0.015f, 32), new Vector3(-25, 0.015f, 32), new Vector3(-21, 0.015f, 32), new Vector3(-17, 0.015f, 32) };
            int n = 1;
            foreach (var p in open) Bed(h, p, n++, false);
            foreach (var p in locked) Bed(h, p, n++, true);
            WorldParts.Sign(estatico, new Vector3(-13.2f, 0, 25.0f), 90, "HUERTA", new Vector2(1.2f, 0.4f));
            return new[]
            {
                Shapes.Empty(interactivo, "Cajon1", new Vector3(-14.6f, 0.05f, 19.6f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(interactivo, "Cajon2", new Vector3(-14.6f, 0.05f, 20.3f), new Vector3(0, 90, 0)).transform,
                Shapes.Empty(interactivo, "Cajon3", new Vector3(-14.6f, 0.05f, 21.0f), new Vector3(0, 90, 0)).transform,
            };
        }

        static void Bed(Transform parent, Vector3 pos, int n, bool locked)
        {
            var go = Shapes.Empty(parent, "Cantero_" + n, pos);
            var bed = go.AddComponent<GardenBed>();
            bed.bedId = "cantero_" + n;
            bed.displayName = "Cantero " + n;
            bed.locked = locked;
            bed.unlockPrice = 20000;
            bed.Build();
        }

        static void Well()
        {
            var w = Shapes.Empty(interactivo, "Pozo", new Vector3(-10.5f, 0, 26.6f)).transform;
            noGrass.Add(R(-11.6f, 25.5f, -9.4f, 27.7f));
            Color stone = new Color(0.62f, 0.6f, 0.56f);
            var ring = Shapes.Cyl(w, "Brocal", Vector3.zero, 1.3f, 0.8f, stone, true);
            ring.name = "Brocal";
            Shapes.Cyl(w, "BordeBrocal", new Vector3(0, 0.8f, 0), 1.4f, 0.08f, new Color(0.55f, 0.53f, 0.5f), false);
            Shapes.Cyl(w, "Agua", new Vector3(0, 0.81f, 0), 1.0f, 0.01f, new Color(0.08f, 0.12f, 0.18f), false);
            Shapes.Box(w, "PosteI", new Vector3(-0.7f, 0, 0), new Vector3(0.12f, 2.1f, 0.12f), Mats.Madera, true);
            Shapes.Box(w, "PosteD", new Vector3(0.7f, 0, 0), new Vector3(0.12f, 2.1f, 0.12f), Mats.Madera, true);
            Shapes.CylC(w, "Rodillo", new Vector3(0, 1.85f, 0), 0.14f, 1.4f, Mats.MaderaClara, new Vector3(0, 0, 90));
            Shapes.Beam(w, "Manivela", new Vector3(0.75f, 1.85f, 0), new Vector3(0.95f, 1.75f, 0.05f), 0.04f, new Color(0.3f, 0.3f, 0.3f));
            Shapes.BoxC(w, "TechitoA", new Vector3(0, 2.25f, -0.35f), new Vector3(1.7f, 0.04f, 0.9f), Mats.MaderaGris, false, new Vector3(30, 0, 0));
            Shapes.BoxC(w, "TechitoB", new Vector3(0, 2.25f, 0.35f), new Vector3(1.7f, 0.04f, 0.9f), Mats.MaderaGris, false, new Vector3(-30, 0, 0));
            var bucket = Shapes.Empty(w, "BaldeDelPozo", new Vector3(0, 1.1f, 0)).transform;
            Shapes.Cyl(bucket, "Balde", Vector3.zero, 0.26f, 0.28f, Mats.Chapa, false);
            Shapes.CylC(bucket, "Soga", new Vector3(0, 0.55f, 0), 0.02f, 0.6f, new Color(0.75f, 0.65f, 0.45f), Vector3.zero);
            var ws = w.gameObject.AddComponent<WaterSource>();
            ws.displayName = "Pozo de agua";
            ws.wellBucket = bucket;
            ws.bucketDrop = 1.0f;
        }

        static void Fences()
        {
            var f = Shapes.Empty(estatico, "Alambrados", Vector3.zero).transform;
            // Perímetro del campo
            WorldParts.Fence(f, new Vector3(-42, 0, 6), new Vector3(-42, 0, 58));
            WorldParts.Fence(f, new Vector3(44, 0, 6), new Vector3(44, 0, 58));
            WorldParts.Fence(f, new Vector3(-42, 0, 58), new Vector3(44, 0, 58));
            WorldParts.Fence(f, new Vector3(-42, 0, 6), new Vector3(-12, 0, 6));
            WorldParts.Fence(f, new Vector3(17, 0, 6), new Vector3(25, 0, 6));
            WorldParts.Fence(f, new Vector3(29, 0, 6), new Vector3(44, 0, 6));
            // Huerta (con entrada del lado este)
            WorldParts.Fence(f, new Vector3(-34, 0, 14), new Vector3(-13, 0, 14), 2.5f);
            WorldParts.Fence(f, new Vector3(-34, 0, 37), new Vector3(-13, 0, 37), 2.5f);
            WorldParts.Fence(f, new Vector3(-34, 0, 14), new Vector3(-34, 0, 37), 2.5f);
            WorldParts.Fence(f, new Vector3(-13, 0, 14), new Vector3(-13, 0, 21.5f), 2.5f);
            WorldParts.Fence(f, new Vector3(-13, 0, 23.6f), new Vector3(-13, 0, 37), 2.5f);
            // Zona de corrales todavía cerrada (fase 2)
            WorldParts.Fence(f, new Vector3(20, 0, 28), new Vector3(42, 0, 28));
            WorldParts.Fence(f, new Vector3(20, 0, 54), new Vector3(42, 0, 54));
            WorldParts.Fence(f, new Vector3(20, 0, 28), new Vector3(20, 0, 54));
            WorldParts.Fence(f, new Vector3(42, 0, 28), new Vector3(42, 0, 54));
            WorldParts.Sign(f, new Vector3(27, 0, 27.6f), 180, "CORRALES\n(se habilitan en la Fase 2)", new Vector2(2.2f, 0.8f));

            // Tranquera de entrada al campo
            var gate = Shapes.Empty(interactivo, "Tranquera", new Vector3(25, 0, 6)).transform;
            Color wood = new Color(0.6f, 0.57f, 0.5f);
            Shapes.Box(gate, "PosteI", new Vector3(0, 0, 0), new Vector3(0.28f, 1.8f, 0.28f), Mats.Madera, true);
            Shapes.Box(gate, "PosteD", new Vector3(4, 0, 0), new Vector3(0.28f, 1.8f, 0.28f), Mats.Madera, true);
            var leaf = Shapes.Empty(gate, "Hoja", new Vector3(0.18f, 0, 0)).transform;
            for (int i = 0; i < 5; i++)
                Shapes.Box(leaf, "Travesano", new Vector3(1.85f, 0.25f + i * 0.28f, 0), new Vector3(3.7f, 0.1f, 0.06f), wood, i == 0 || i == 4);
            Shapes.Box(leaf, "Vertical", new Vector3(0.05f, 0.2f, 0), new Vector3(0.1f, 1.3f, 0.08f), wood, false);
            Shapes.Box(leaf, "Vertical", new Vector3(3.65f, 0.2f, 0), new Vector3(0.1f, 1.3f, 0.08f), wood, false);
            Shapes.Beam(leaf, "Diagonal", new Vector3(0.1f, 0.3f, 0.04f), new Vector3(3.6f, 1.4f, 0.04f), 0.09f, wood);
            var col = leaf.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(1.85f, 0.8f, 0);
            col.size = new Vector3(3.7f, 1.4f, 0.12f);
            var fg = gate.gameObject.AddComponent<FarmGate>();
            fg.leaf = leaf;
            fg.openAngle = -95f;
        }

        static void Trees()
        {
            var t = Shapes.Empty(estatico, "Arboles", Vector3.zero).transform;
            // Cortina de eucaliptos al fondo
            for (float x = -52; x <= 54; x += 6.5f) WorldParts.Eucalyptus(t, new Vector3(x + (float)(rnd.NextDouble() * 2 - 1), 0, 63 + (float)(rnd.NextDouble() * 3)), rnd);
            // Fila de álamos al oeste
            for (float z = 10; z <= 60; z += 5f) WorldParts.Poplar(t, new Vector3(-47 + (float)(rnd.NextDouble()), 0, z), rnd);
            // Álamos junto al camino de los corrales
            for (float z = 9; z <= 25; z += 5f) WorldParts.Poplar(t, new Vector3(30, 0, z), rnd);
            // Paraísos dando sombra
            WorldParts.Paraiso(t, new Vector3(-9, 0, 9.5f), rnd);
            WorldParts.Paraiso(t, new Vector3(8.5f, 0, 27), rnd);
            WorldParts.Paraiso(t, new Vector3(-6, 0, 31), rnd);
            WorldParts.Paraiso(t, new Vector3(5.5f, 0, 41), rnd);
            // Montecito del otro lado de la ruta
            for (int i = 0; i < 14; i++)
                WorldParts.Eucalyptus(t, new Vector3(-150 + (float)rnd.NextDouble() * 30, 0, -12 - (float)rnd.NextDouble() * 25), rnd);
            for (int i = 0; i < 10; i++)
                WorldParts.Eucalyptus(t, new Vector3(130 + (float)rnd.NextDouble() * 25, 0, -15 - (float)rnd.NextDouble() * 30), rnd);
            for (int i = 0; i < 8; i++)
                WorldParts.Paraiso(t, new Vector3((float)(rnd.NextDouble() * 160 - 80), 0, -96 - (float)rnd.NextDouble() * 10), rnd);
        }

        static void Hills()
        {
            var go = new GameObject("Sierras de fondo");
            go.transform.SetParent(estatico, false);
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>().sharedMaterial = Mats.Get("Sierras", new Color(0.4f, 0.48f, 0.32f), 0.05f, 0f);
            go.AddComponent<HillsBackdrop>();
        }

        static void Grass()
        {
            var go = new GameObject("Pasto");
            var gf = go.AddComponent<GrassField>();
            gf.materials = new[]
            {
                Mats.Get("Pasto1", new Color(0.33f, 0.55f, 0.2f), 0.1f, 0f, false, null, true, true),
                Mats.Get("Pasto2", new Color(0.4f, 0.6f, 0.24f), 0.1f, 0f, false, null, true, true),
                Mats.Get("Pasto3", new Color(0.28f, 0.48f, 0.18f), 0.1f, 0f, false, null, true, true),
                Mats.Get("PastoSeco", new Color(0.7f, 0.65f, 0.38f), 0.1f, 0f, false, null, true, true),
            };
            gf.exclusions = new List<Rect>(noGrass);
            gf.exclusions.Add(R(-4.4f, 14.2f, 4.4f, 16.1f));
        }

        // ------------------------------------------------------------------
        static void Systems(StoreRefs store, GalponRefs galpon, SleepBed bed, Transform[] crateSpots)
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
            ng.hoeHook = galpon.hoe; ng.canHook = galpon.can; ng.bucketHook = galpon.bucket; ng.labelerHook = store.labelerHook;
            ng.crateSpots = crateSpots;
            ng.seedSpots = galpon.seedSpots;
            ng.boxSpots = galpon.boxSpots;
            gm.newGameSetup = ng;
            gm.bed = bed;

            dm.dropPoint = galpon.drop;
            var route = Shapes.Empty(sys.transform, "RutaCamioneta", Vector3.zero).transform;
            dm.truckRoute = new[]
            {
                Shapes.Empty(route, "Entrada0", new Vector3(230, 0, 1.8f)).transform,
                Shapes.Empty(route, "Entrada1", new Vector3(24, 0, 1.8f)).transform,
                Shapes.Empty(route, "Entrada2", new Vector3(15, 0, 6.5f)).transform,
                Shapes.Empty(route, "Entrada3", new Vector3(13, 0, 12.6f)).transform,
            };
            dm.truckExit = new[]
            {
                Shapes.Empty(route, "Salida0", new Vector3(9, 0, 11.5f)).transform,
                Shapes.Empty(route, "Salida1", new Vector3(2, 0, 7f)).transform,
                Shapes.Empty(route, "Salida2", new Vector3(-12, 0, 1.8f)).transform,
                Shapes.Empty(route, "Salida3", new Vector3(-230, 0, 1.8f)).transform,
            };

            var nav = new GameObject("CaminoClientes").AddComponent<StoreNav>();
            var nt = nav.transform;
            nav.spawnWest = Shapes.Empty(nt, "AparecenOeste", new Vector3(-38, 0.03f, 4.8f)).transform;
            nav.spawnEast = Shapes.Empty(nt, "AparecenEste", new Vector3(38, 0.03f, 4.8f)).transform;
            nav.curbWest = Shapes.Empty(nt, "BanquinaOeste", new Vector3(-7, 0.03f, 4.8f)).transform;
            nav.curbEast = Shapes.Empty(nt, "BanquinaEste", new Vector3(7, 0.03f, 4.8f)).transform;
            nav.parking = Shapes.Empty(nt, "Estacionamiento", new Vector3(0, 0.03f, 11f)).transform;
            nav.doorOutside = Shapes.Empty(nt, "PuertaAfuera", new Vector3(0, 0.06f, 15.2f)).transform;
            nav.doorInside = Shapes.Empty(nt, "PuertaAdentro", new Vector3(0, 0.03f, 16.9f)).transform;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        static void Player()
        {
            var p = new GameObject("Jugador");
            p.transform.position = new Vector3(0, 0.1f, 11f);
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
