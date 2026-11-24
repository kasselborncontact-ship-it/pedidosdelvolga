using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LaTranquera
{
    /// <summary>
    /// Saca una "foto" de cada producto para el catálogo del proveedor.
    /// Arma el objeto lejos del mapa, lo fotografía con una cámara propia y guarda la imagen.
    /// Si preferís imágenes dibujadas, poné un PNG en Resources/Iconos/ con el id (ej.: "caja_yerba.png").
    /// </summary>
    public static class IconRenderer
    {
        static readonly Dictionary<string, Texture> cache = new Dictionary<string, Texture>();
        static Camera cam;
        static Transform stage;
        static readonly Vector3 StagePos = new Vector3(4000, 300, 4000);

        public static Texture Get(string key, Func<Transform, GameObject> build)
        {
            if (cache.TryGetValue(key, out var tex) && tex != null) return tex;
            var custom = Resources.Load<Texture2D>("Iconos/" + key);
            if (custom != null) { cache[key] = custom; return custom; }

            Setup();
            var go = build(stage);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(0, 30, 0);
            var b = Shapes.RenderBounds(go);
            float radius = Mathf.Max(0.05f, b.extents.magnitude);
            float dist = radius / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.05f;
            Vector3 dir = new Vector3(0, 0.55f, -1f).normalized;
            cam.transform.position = b.center + dir * dist;
            cam.transform.LookAt(b.center);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = dist + radius * 3f;

            var rt = RenderTexture.GetTemporary(160, 160, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            var req = new RenderPipeline.StandardRequest();
            if (RenderPipeline.SupportsRenderRequest(cam, req))
            {
                req.destination = rt;
                RenderPipeline.SubmitRenderRequest(cam, req);
            }
            else cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var t2 = new Texture2D(160, 160, TextureFormat.RGBA32, false);
            t2.ReadPixels(new Rect(0, 0, 160, 160), 0, 0);
            t2.Apply();
            RenderTexture.active = prev;
            cam.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            UnityEngine.Object.DestroyImmediate(go);
            cache[key] = t2;
            return t2;
        }

        static void Setup()
        {
            if (cam != null) return;
            var root = new GameObject("_EstudioDeFotos");
            UnityEngine.Object.DontDestroyOnLoad(root);
            root.transform.position = StagePos;
            stage = Shapes.Empty(root.transform, "Escenario", Vector3.zero).transform;
            var camGo = new GameObject("CamaraFotos");
            camGo.transform.SetParent(root.transform, false);
            cam = camGo.AddComponent<Camera>();
            cam.enabled = false;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.93f, 0.9f, 0.82f, 1f);
            cam.fieldOfView = 30f;
            var lightGo = new GameObject("LuzFotos");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(-1.5f, 2.5f, -2f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 12f;
            l.intensity = 6f;
            var lightGo2 = new GameObject("LuzRelleno");
            lightGo2.transform.SetParent(root.transform, false);
            lightGo2.transform.localPosition = new Vector3(2f, 1f, -1.5f);
            var l2 = lightGo2.AddComponent<Light>();
            l2.type = LightType.Point;
            l2.range = 10f;
            l2.intensity = 2.5f;
        }
    }
}
