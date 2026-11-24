using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Arma una persona low poly con colores al azar (algunos con boina o sombrero).
    /// Para usar personajes de Quaternius: poné prefabs en Resources/Clientes/ (cliente_0, cliente_1, ...)
    /// y se elige uno al azar.
    /// </summary>
    public static class CustomerVisuals
    {
        static readonly Color[] Skins = { new Color(0.96f, 0.8f, 0.66f), new Color(0.85f, 0.65f, 0.48f), new Color(0.62f, 0.44f, 0.3f), new Color(0.45f, 0.3f, 0.2f) };
        static readonly Color[] Shirts = { new Color(0.8f, 0.2f, 0.2f), new Color(0.2f, 0.4f, 0.75f), new Color(0.95f, 0.85f, 0.3f), new Color(0.3f, 0.6f, 0.35f), new Color(0.9f, 0.9f, 0.9f), new Color(0.5f, 0.3f, 0.6f), new Color(0.9f, 0.5f, 0.2f) };
        static readonly Color[] Pants = { new Color(0.2f, 0.25f, 0.4f), new Color(0.25f, 0.22f, 0.2f), new Color(0.45f, 0.4f, 0.3f), new Color(0.15f, 0.15f, 0.15f) };
        static readonly Color[] Hair = { new Color(0.1f, 0.08f, 0.06f), new Color(0.35f, 0.22f, 0.1f), new Color(0.75f, 0.6f, 0.3f), new Color(0.7f, 0.7f, 0.7f) };

        public class Parts
        {
            public Transform body, legL, legR, armL, armR, basket;
        }

        public static Parts Build(Transform parent, System.Random r)
        {
            var p = new Parts();
            var prefabs = Resources.LoadAll<GameObject>("Clientes");
            if (prefabs != null && prefabs.Length > 0)
            {
                var inst = Object.Instantiate(prefabs[r.Next(prefabs.Length)], parent);
                Shapes.RemoveAllColliders(inst);
                p.body = inst.transform;
            }
            else
            {
                Color skin = Skins[r.Next(Skins.Length)], shirt = Shirts[r.Next(Shirts.Length)], pant = Pants[r.Next(Pants.Length)], hair = Hair[r.Next(Hair.Length)];
                bool tall = r.NextDouble() > 0.5;
                float h = tall ? 1.0f : 0.94f;
                var body = Shapes.Empty(parent, "Cuerpo", Vector3.zero).transform;
                body.localScale = Vector3.one * h;
                p.body = body;
                p.legL = Shapes.Empty(body, "PiernaI", new Vector3(-0.1f, 0.85f, 0)).transform;
                p.legR = Shapes.Empty(body, "PiernaD", new Vector3(0.1f, 0.85f, 0)).transform;
                Shapes.BoxC(p.legL, "Pierna", new Vector3(0, -0.42f, 0), new Vector3(0.15f, 0.85f, 0.17f), pant, false);
                Shapes.BoxC(p.legR, "Pierna", new Vector3(0, -0.42f, 0), new Vector3(0.15f, 0.85f, 0.17f), pant, false);
                Shapes.BoxC(p.legL, "Zapato", new Vector3(0, -0.83f, 0.04f), new Vector3(0.16f, 0.07f, 0.26f), new Color(0.15f, 0.1f, 0.08f), false);
                Shapes.BoxC(p.legR, "Zapato", new Vector3(0, -0.83f, 0.04f), new Vector3(0.16f, 0.07f, 0.26f), new Color(0.15f, 0.1f, 0.08f), false);
                Shapes.BoxC(body, "Torso", new Vector3(0, 1.18f, 0), new Vector3(0.44f, 0.62f, 0.25f), shirt, false);
                p.armL = Shapes.Empty(body, "BrazoI", new Vector3(-0.28f, 1.44f, 0)).transform;
                p.armR = Shapes.Empty(body, "BrazoD", new Vector3(0.28f, 1.44f, 0)).transform;
                Shapes.BoxC(p.armL, "Brazo", new Vector3(0, -0.3f, 0), new Vector3(0.12f, 0.6f, 0.14f), shirt, false);
                Shapes.BoxC(p.armR, "Brazo", new Vector3(0, -0.3f, 0), new Vector3(0.12f, 0.6f, 0.14f), shirt, false);
                Shapes.BoxC(p.armL, "Mano", new Vector3(0, -0.63f, 0), new Vector3(0.1f, 0.1f, 0.1f), skin, false);
                Shapes.BoxC(p.armR, "Mano", new Vector3(0, -0.63f, 0), new Vector3(0.1f, 0.1f, 0.1f), skin, false);
                Shapes.BoxC(body, "Cuello", new Vector3(0, 1.52f, 0), new Vector3(0.12f, 0.08f, 0.12f), skin, false);
                Shapes.BoxC(body, "Cabeza", new Vector3(0, 1.67f, 0), new Vector3(0.24f, 0.26f, 0.24f), skin, false);
                Shapes.BoxC(body, "OjoI", new Vector3(-0.055f, 1.7f, 0.121f), new Vector3(0.035f, 0.035f, 0.01f), Mats.Negro, false);
                Shapes.BoxC(body, "OjoD", new Vector3(0.055f, 1.7f, 0.121f), new Vector3(0.035f, 0.035f, 0.01f), Mats.Negro, false);
                int hat = r.Next(4);
                if (hat == 0) // boina
                    Shapes.Prim(PrimitiveType.Cylinder, body, "Boina", new Vector3(0, 1.82f, -0.01f), new Vector3(0.3f, 0.03f, 0.3f), new Color(0.15f, 0.15f, 0.2f), false);
                else if (hat == 1) // sombrero de paja
                {
                    Shapes.Prim(PrimitiveType.Cylinder, body, "Ala", new Vector3(0, 1.8f, 0), new Vector3(0.45f, 0.01f, 0.45f), new Color(0.85f, 0.75f, 0.45f), false);
                    Shapes.Prim(PrimitiveType.Cylinder, body, "Copa", new Vector3(0, 1.86f, 0), new Vector3(0.26f, 0.06f, 0.26f), new Color(0.85f, 0.75f, 0.45f), false);
                }
                else
                    Shapes.BoxC(body, "Pelo", new Vector3(0, 1.79f, -0.02f), new Vector3(0.26f, 0.08f, 0.27f), hair, false);
            }

            // Canasta de mimbre que aparece cuando agarra algo
            var basketParent = p.armR != null ? p.armR : p.body;
            var basket = Shapes.Empty(basketParent, "Canasta", p.armR != null ? new Vector3(0.05f, -0.72f, 0.05f) : new Vector3(0.3f, 0.8f, 0.2f)).transform;
            Shapes.Box(basket, "Cesta", new Vector3(0, -0.18f, 0), new Vector3(0.3f, 0.16f, 0.22f), new Color(0.7f, 0.55f, 0.3f), false);
            Shapes.BoxC(basket, "Asa", new Vector3(0, 0f, 0), new Vector3(0.02f, 0.16f, 0.02f), new Color(0.6f, 0.45f, 0.25f), false);
            basket.gameObject.SetActive(false);
            p.basket = basket;
            return p;
        }
    }
}
