using System.Collections;
using MyProstateRejectsCapitalism.Characters;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.Gameplay
{
    internal sealed class ProstateFailureSequence
    {
        private const float FadeStart = 1.10f;
        private const float FullyBlack = 2.26f;
        private const float LeadInSeconds = 2.34f;
        private static readonly int Invasion = Shader.PropertyToID("_Invasion");
        private static readonly int Veil = Shader.PropertyToID("_Veil");
        private readonly Transform owner;
        private readonly FirstPersonPlayer player;
        private readonly Material liquidTemplate;
        private FailureSpill spill;
        private Material pressureVeil;

        internal float Elapsed { get; private set; }
        internal float PressureSurge => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, .42f, Elapsed));
        internal float Blackout { get; private set; }

        internal ProstateFailureSequence(Transform owner, FirstPersonPlayer player, Material liquidTemplate)
        {
            this.owner = owner;
            this.player = player;
            this.liquidTemplate = liquidTemplate;
        }

        internal IEnumerator Play(CanvasGroup fade)
        {
            var view = player.ViewCamera.transform;
            var start = view.localPosition;
            var eye = view.position;
            var angle = view.localEulerAngles;
            float pitch = Mathf.DeltaAngle(0f, angle.x);
            float yaw = Mathf.DeltaAngle(0f, angle.y);
            float lowEye = Mathf.Min(.29f, start.y);
            var poses = new[]
            {
                new Beat(0f, pitch, 0f, 0f, 0f, 0f, 0f),
                new Beat(.15f, pitch - 4f, 0f, -4f, -.018f, 0f, -.024f),
                new Beat(.69f, 56f, 0f, -3f, .015f, .045f, .058f),
                new Beat(.93f, 56f, -1f, -6f, .025f, .058f, .040f),
                new Beat(1.17f, 45f, -4f, 8f, -.042f, .13f, -.058f),
                new Beat(1.43f, 48f, 4f, 30f, .114f, .43f, .018f),
                new Beat(1.56f, 43f, 2f, 21f, .088f, .49f, .024f),
                new Beat(2.18f, 32f, 10f, 60f, .278f, .84f, .10f),
                new Beat(LeadInSeconds, 36f, 12f, 62f, .337f, 1f, .12f)
            };

            player.enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            spill = FailureSpill.TryCreate(owner, player.transform, liquidTemplate);
            pressureVeil = CreatePressureVeil(fade);
            fade.blocksRaycasts = true;

            int next = 1;
            while (true)
            {
                while (next < poses.Length - 1 && Elapsed > poses[next].Time)
                    next++;
                var a = poses[next - 1];
                var b = poses[next];
                float blend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(a.Time, b.Time, Elapsed));
                var bodyOffset = Vector3.Lerp(a.Offset, b.Offset, blend);
                var local = start + new Vector3(bodyOffset.x, -(start.y - lowEye) * bodyOffset.y, bodyOffset.z);
                var target = view.parent.TransformPoint(local);
                var movement = target - eye;
                if (movement.sqrMagnitude > .0001f && Physics.SphereCast(eye, .065f, movement.normalized,
                    out var obstacle, movement.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    target = eye + movement.normalized * Mathf.Max(0f, obstacle.distance - .022f);

                view.SetPositionAndRotation(target, view.parent.rotation * Quaternion.Euler(
                    Mathf.Lerp(a.Pitch, b.Pitch, blend), yaw + Mathf.Lerp(a.Yaw, b.Yaw, blend),
                    Mathf.Lerp(a.Roll, b.Roll, blend)));
                spill?.Sample(Elapsed);
                Blackout = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(FadeStart, FullyBlack, Elapsed));
                fade.alpha = Blackout;
                if (pressureVeil != null)
                {
                    pressureVeil.SetFloat(Invasion, PressureSurge);
                    pressureVeil.SetFloat(Veil, PressureSurge * (1f - .35f * Blackout));
                }
                if (Elapsed >= LeadInSeconds)
                    yield break;
                yield return null;
                Elapsed = Mathf.Min(LeadInSeconds, Elapsed + Time.deltaTime);
            }
        }

        private static Material CreatePressureVeil(CanvasGroup fade)
        {
            var shader = Resources.Load<Shader>("ProstateFailureVeil");
            if (shader == null)
            {
                Debug.LogError("Missing failure veil shader.");
                return null;
            }

            var image = new GameObject("Failure pressure veil", typeof(RectTransform), typeof(RawImage))
                .GetComponent<RawImage>();
            var rect = image.rectTransform;
            rect.SetParent(fade.transform.parent, false);
            rect.SetSiblingIndex(fade.transform.GetSiblingIndex());
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            image.raycastTarget = false;
            image.material = new Material(shader);
            return image.material;
        }

        internal void Dispose()
        {
            spill?.Dispose();
            if (pressureVeil != null)
                Object.Destroy(pressureVeil);
        }

        private readonly struct Beat
        {
            internal readonly float Time, Pitch, Yaw, Roll;
            internal readonly Vector3 Offset;

            internal Beat(float time, float pitch, float yaw, float roll, float lateral, float drop, float forward)
            {
                Time = time;
                Pitch = pitch;
                Yaw = yaw;
                Roll = roll;
                Offset = new Vector3(lateral, drop, forward);
            }
        }

        private sealed class FailureSpill
        {
            private const int StreamPoints = 18;
            private readonly Vector3 origin, impact, lateral;
            private readonly Material airborne, wetSurface;
            private readonly LineRenderer stream;
            private readonly Transform[] drops = new Transform[7];
            private readonly Transform stain;
            private readonly Mesh stainMesh;

            internal static FailureSpill TryCreate(Transform owner, Transform body, Material template)
            {
                if (template == null)
                    return null;
                var forward = body.forward;
                var testOrigin = body.position + Vector3.up * 1.15f;
                if (!FindFloor(testOrigin + forward * .93f, out var floor)
                    && !FindFloor(testOrigin + forward * .46f, out floor))
                    return null;
                var origin = body.position + forward * .28f + Vector3.up * .71f;
                return new FailureSpill(owner, body.right, forward, origin, floor, template);
            }

            private static bool FindFloor(Vector3 point, out RaycastHit floor) =>
                Physics.Raycast(point, Vector3.down, out floor, 2.55f, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore) && floor.normal.y > .55f;

            private FailureSpill(Transform owner, Vector3 lateral, Vector3 forward, Vector3 origin,
                RaycastHit floor, Material template)
            {
                this.lateral = lateral;
                this.origin = origin;
                impact = floor.point + floor.normal * .012f;
                var group = new GameObject("Failure spill").transform;
                group.SetParent(owner, false);
                airborne = new Material(template) { color = new Color(1f, .72f, .18f, .76f) };
                wetSurface = new Material(template) { color = new Color(.69f, .52f, .12f, .43f) };
                wetSurface.SetFloat("_Smoothness", .78f);

                var line = new GameObject("Falling liquid thread", typeof(LineRenderer));
                line.transform.SetParent(group, false);
                stream = line.GetComponent<LineRenderer>();
                stream.sharedMaterial = airborne;
                stream.shadowCastingMode = ShadowCastingMode.Off;
                stream.receiveShadows = false;
                stream.positionCount = StreamPoints;
                stream.widthCurve = new AnimationCurve(new Keyframe(0f, .32f), new Keyframe(.25f, 1f),
                    new Keyframe(.8f, .78f), new Keyframe(1f, .22f));
                stream.numCapVertices = 3;
                stream.enabled = false;

                for (int i = 0; i < drops.Length; i++)
                {
                    var drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    drop.name = "Falling drop " + (i + 1);
                    drop.transform.SetParent(group, false);
                    var collider = drop.GetComponent<Collider>();
                    collider.enabled = false;
                    Object.Destroy(collider);
                    var renderer = drop.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = airborne;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    drop.SetActive(false);
                    drops[i] = drop.transform;
                }

                var patch = new GameObject("Collecting floor stain", typeof(MeshFilter), typeof(MeshRenderer));
                stain = patch.transform;
                stain.SetParent(group, false);
                stain.SetPositionAndRotation(floor.point + floor.normal * .007f,
                    Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, floor.normal), floor.normal));
                stainMesh = MakeStain();
                patch.GetComponent<MeshFilter>().sharedMesh = stainMesh;
                var surface = patch.GetComponent<MeshRenderer>();
                surface.sharedMaterial = wetSurface;
                surface.shadowCastingMode = ShadowCastingMode.Off;
                patch.SetActive(false);
            }

            internal void Sample(float elapsed)
            {
                float head = Mathf.InverseLerp(.42f, .72f, elapsed);
                float tail = Mathf.InverseLerp(.96f, 1.24f, elapsed);
                stream.enabled = head > tail;
                if (stream.enabled)
                {
                    stream.widthMultiplier = Mathf.Lerp(.026f, .009f, tail);
                    for (int i = 0; i < StreamPoints; i++)
                    {
                        float t = Mathf.Lerp(tail, head, i / (float)(StreamPoints - 1));
                        stream.SetPosition(i, Arc(t) + lateral * (Mathf.Sin(t * 19f - elapsed * 31f)
                            * .013f * Mathf.Sin(t * Mathf.PI)));
                    }
                }

                for (int i = 0; i < drops.Length; i++)
                {
                    var drop = drops[i];
                    float t = (elapsed - (.49f + i * .081f)) / (.31f + i % 3 * .024f);
                    bool airborneNow = t > 0f && t < 1f;
                    drop.gameObject.SetActive(airborneNow);
                    if (!airborneNow)
                        continue;
                    float spread = ((i % 2 == 0 ? -1f : 1f) * (.034f + .013f * (i % 3))) * Mathf.Sin(t * Mathf.PI);
                    drop.position = Arc(t) + lateral * spread;
                    float width = .019f + i % 3 * .004f;
                    drop.localScale = new Vector3(width, width * 1.5f, width);
                }

                float pool = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.70f, 1.53f, elapsed));
                stain.gameObject.SetActive(pool > 0f);
                stain.localScale = new Vector3(Mathf.Lerp(.19f, 1f, pool), 1f, pool);
            }

            private Vector3 Arc(float t) => Vector3.Lerp(origin, impact, t)
                + Vector3.up * (.48f * t * (1f - t));

            private static Mesh MakeStain()
            {
                const int count = 36;
                var vertices = new Vector3[count + 1];
                var triangles = new int[count * 3];
                for (int i = 0; i < count; i++)
                {
                    float angle = 2f * Mathf.PI * i / count;
                    float edge = 1f + .14f * Mathf.Sin(3f * angle + .7f) + .07f * Mathf.Cos(5f * angle);
                    vertices[i + 1] = new Vector3(Mathf.Cos(angle) * edge * .145f, 0f,
                        Mathf.Sin(angle) * edge * .24f);
                    triangles[i * 3 + 1] = (i + 1) % count + 1;
                    triangles[i * 3 + 2] = i + 1;
                }
                var mesh = new Mesh { name = "Failure spill footprint", vertices = vertices, triangles = triangles };
                mesh.RecalculateNormals();
                return mesh;
            }

            internal void Dispose()
            {
                Object.Destroy(airborne);
                Object.Destroy(wetSurface);
                Object.Destroy(stainMesh);
            }
        }
    }
}
