using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// El lechero: todas las mañanas pasa en su Rastrojero por el camino de tierra hasta el tambo,
    /// espera un rato y se vuelve. (En la Fase 4 va a comprar la leche que sobra y retirar dulce de leche.)
    /// </summary>
    public class MilkmanVisit : MonoBehaviour
    {
        public float arriveHour = 7f;
        public Transform[] routeIn, routeOut;
        public float waitSeconds = 25f;
        int lastDay = -1;
        bool running;

        void Update()
        {
            var tm = TimeManager.I;
            if (tm == null || running || routeIn == null || routeIn.Length < 2) return;
            if (tm.Day != lastDay && tm.Hour >= arriveHour && tm.Hour < arriveHour + 1.5f)
            {
                lastDay = tm.Day;
                StartCoroutine(Visit());
            }
        }

        IEnumerator Visit()
        {
            running = true;
            var go = new GameObject("RastrojeroLechero");
            VehicleVisuals.Build(VehicleVisuals.Kind.Rastrojero, go.transform, new Color(0.5f, 0.68f, 0.8f), new Color(0.95f, 0.95f, 0.92f));
            // tachos de leche en la caja
            for (int i = 0; i < 4; i++)
                Shapes.Cyl(go.transform, "Tacho", new Vector3((i % 2 - 0.5f) * 0.55f, 0.85f, -1.3f - (i / 2) * 0.5f), 0.3f, 0.5f, Mats.Get(Mats.Chapa, 0.6f, 0.7f).color, false);
            go.transform.position = routeIn[0].position;
            go.transform.rotation = Quaternion.LookRotation(routeIn[1].position - routeIn[0].position);
            var drv = go.AddComponent<RouteDriver>();
            drv.speed = 7f;
            HUD.Toast("Pasa el lechero con el Rastrojero");
            yield return drv.Drive(routeIn, true);
            yield return new WaitForSeconds(waitSeconds);
            yield return drv.Drive(routeOut, false);
            Destroy(go);
            running = false;
        }
    }
}
