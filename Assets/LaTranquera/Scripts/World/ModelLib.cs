using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Pone modelos 3D (FBX) que están en Resources. Los modelos ya vienen preparados:
    /// tamaño real, apoyados en el piso y con el frente mirando a +Z.
    /// Arregla lo que el importador deja mal: los vidrios quedan transparentes y se sacan los colliders.
    /// Si el modelo no está, devuelve null y se usa la forma hecha por código.
    /// </summary>
    public static class ModelLib
    {
        public static bool Has(string path) => Resources.Load<GameObject>(path) != null;

        /// <summary>Crea el modelo dentro de un objeto "contenedor" ubicado en pos y girado yaw grados.</summary>
        public static GameObject Place(Transform parent, string path, string name, Vector3 localPos, float yaw)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null) return null;
            var holder = new GameObject(name);
            holder.transform.SetParent(parent, false);
            holder.transform.localPosition = localPos;
            holder.transform.localEulerAngles = new Vector3(0, yaw, 0);
            var inst = Object.Instantiate(prefab, holder.transform);
            inst.name = "Modelo";
            Fix(inst);
            return holder;
        }

        /// <summary>Vidrios transparentes y sin colliders.</summary>
        public static void Fix(GameObject go)
        {
            Shapes.RemoveAllColliders(go);
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    string n = mats[i].name.ToLower();
                    if (n.Contains("glass") || n.Contains("vidrio"))
                    {
                        mats[i] = Mats.Glass(new Color(0.75f, 0.85f, 0.9f, 0.22f));
                        changed = true;
                    }
                    else if (n.Contains("branch") || n.Contains("hedge") || n.Contains("shrubbery") || n.Contains("grass") || n.Contains("flower") || n.Contains("clover") || n.Contains("leaf"))
                    {
                        mats[i] = Mats.ClipCopy(mats[i]); // hojas con transparencia recortada
                        changed = true;
                    }
                }
                if (changed) r.sharedMaterials = mats;
            }
        }

        /// <summary>Pone el modelo con una escala (árboles más grandes o más chicos).</summary>
        public static GameObject Place(Transform parent, string path, string name, Vector3 localPos, float yaw, float scale)
        {
            var g = Place(parent, path, name, localPos, yaw);
            if (g != null) g.transform.localScale = Vector3.one * scale;
            return g;
        }

        /// <summary>Agrega un BoxCollider que envuelve todo el modelo (en el contenedor).</summary>
        public static BoxCollider FitCollider(GameObject holder, bool trigger = false)
        {
            var b = Shapes.RenderBounds(holder);
            var bc = holder.AddComponent<BoxCollider>();
            bc.center = holder.transform.InverseTransformPoint(b.center);
            var s = holder.transform.InverseTransformVector(b.size);
            bc.size = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            bc.isTrigger = trigger;
            return bc;
        }
    }
}
