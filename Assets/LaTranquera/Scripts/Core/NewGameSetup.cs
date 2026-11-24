using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Lo que tenés al empezar una partida nueva: poca plata, 3 cajones, las herramientas
    /// en sus ganchos, un sobre de cada semilla y un par de cajas de mercadería para arrancar.
    /// </summary>
    public class NewGameSetup : MonoBehaviour
    {
        [Header("Plata inicial")]
        public int startBank = 40000;
        /// <summary>Cambio en el cajón: x10 de $100, x10 de $200, x8 de $500, x8 de $1.000, x2 de $2.000 ($19.000).</summary>
        public int[] startDrawer = { 10, 10, 8, 8, 2, 0, 0 };

        [Header("Lugares")]
        public ToolHook hoeHook, canHook, bucketHook, labelerHook;
        public Transform[] crateSpots;
        public Transform[] seedSpots;
        public Transform[] boxSpots;

        public void Spawn()
        {
            Upgrades.Reset();
            foreach (var lp in LandPlot.All) lp.SetOwned(lp.plotId == "lote_1");
            Economy.I.bank = startBank;
            if (CashDrawer.I != null) CashDrawer.I.SetCounts(startDrawer);
            TimeManager.I.totalMinutes = 7 * 60; // día 1, 07:00

            Hang(WorldFactory.Create("hoe", "", Vector3.zero, Quaternion.identity), hoeHook);
            Hang(WorldFactory.Create("can", "", Vector3.zero, Quaternion.identity), canHook);
            Hang(WorldFactory.Create("bucket", "", Vector3.zero, Quaternion.identity), bucketHook);
            Hang(WorldFactory.Create("labeler", "", Vector3.zero, Quaternion.identity), labelerHook);

            foreach (var s in crateSpots) WorldFactory.Crate(s.position, s.rotation);

            string[] crops = { "lechuga", "tomate", "zanahoria" };
            for (int i = 0; i < crops.Length && i < seedSpots.Length; i++)
                WorldFactory.Seeds(crops[i], CropDB.Get(crops[i]).seedsPerPacket, seedSpots[i].position, seedSpots[i].rotation);

            // Mercadería para el primer día (mientras crece la huerta)
            string[] boxes = { "yerba", "fideos", "leche" };
            for (int i = 0; i < boxes.Length && i < boxSpots.Length; i++)
                WorldFactory.Box(boxes[i], ProductDB.Get(boxes[i]).pack, false, boxSpots[i].position, boxSpots[i].rotation);

            Economy.NotifyChanged();
        }

        static void Hang(Carryable c, ToolHook hook)
        {
            if (c == null) return;
            if (hook != null) c.FreezeAt(hook.transform.position, hook.transform.rotation);
        }
    }
}
