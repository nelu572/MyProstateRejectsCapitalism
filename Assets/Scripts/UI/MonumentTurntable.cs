using UnityEngine;

namespace MyProstateRejectsCapitalism.UI
{
    public sealed class MonumentTurntable : MonoBehaviour
    {
        private const double SecondsPerTurn = 12d;
        private Quaternion startingRotation;
        private double startingTime;
        private double brakeStart;
        private double brakePhase;
        private float brakeDuration;

        private void Awake()
        {
            startingRotation = transform.localRotation;
            startingTime = Time.timeAsDouble;
        }

        private void Update()
        {
            double cycle = (Time.timeAsDouble - startingTime) % SecondsPerTurn;
            if (brakeDuration > 0)
            {
                float t = Mathf.Clamp01((float)(Time.unscaledTimeAsDouble - brakeStart) / brakeDuration);
                // Integrate a smoothstep speed ramp so the angle stays continuous at both ends.
                cycle = brakePhase + brakeDuration * (t - t * t * t * (1 - .5f * t));
            }
            float angle = (float)(cycle * 360d / SecondsPerTurn);
            transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up) * startingRotation;
        }

        internal void EaseToStop(float duration)
        {
            brakePhase = (Time.timeAsDouble - startingTime) % SecondsPerTurn;
            brakeStart = Time.unscaledTimeAsDouble;
            brakeDuration = duration;
        }
    }
}
