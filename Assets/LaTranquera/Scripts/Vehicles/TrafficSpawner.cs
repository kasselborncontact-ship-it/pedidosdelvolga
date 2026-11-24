using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Autos viejos que pasan por la ruta en las dos manos (en Argentina se maneja por la derecha).
    /// La ruta va a lo largo del eje X, centrada en Z = roadZ.
    /// </summary>
    public class TrafficSpawner : MonoBehaviour
    {
        public float roadZ = 0f;
        public float laneOffset = 1.8f;
        public float halfLength = 260f;
        public Vector2 interval = new Vector2(6f, 18f);
        public Vector2 speedKmh = new Vector2(55f, 85f);
        float next;

        void Update()
        {
            if (Time.time < next) return;
            float h = TimeManager.I != null ? TimeManager.I.Hour : 12f;
            bool night = h < 6f || h > 21f;
            next = Time.time + Random.Range(interval.x, interval.y) * (night ? 3f : 1f);
            SpawnOne();
        }

        void SpawnOne()
        {
            bool east = Random.value < 0.5f; // hacia +X
            var go = new GameObject("Auto");
            // Yendo hacia +X, la mano derecha es -Z
            float z = roadZ + (east ? -laneOffset : laneOffset);
            go.transform.position = new Vector3(east ? -halfLength : halfLength, 0, z);
            go.transform.rotation = Quaternion.LookRotation(east ? Vector3.right : Vector3.left);
            var kind = (VehicleVisuals.Kind)Random.Range(0, 3);
            // si hay modelo de camión, de vez en cuando pasa uno (más lento)
            bool truck = VehicleVisuals.HasModel(VehicleVisuals.Kind.Camion) && Random.value < 0.25f;
            if (truck) kind = VehicleVisuals.Kind.Camion;
            VehicleVisuals.Build(kind, go.transform, VehicleVisuals.ClassicColors[Random.Range(0, VehicleVisuals.ClassicColors.Length)]);
            var car = go.AddComponent<PassingCar>();
            car.speed = Random.Range(speedKmh.x, speedKmh.y) / 3.6f * (truck ? 0.7f : 1f);
            car.limit = halfLength + 5f;
        }
    }
}
