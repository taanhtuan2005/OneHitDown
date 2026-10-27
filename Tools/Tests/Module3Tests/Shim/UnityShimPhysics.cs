// Trần Văn Tuyên, phiên bản 1.0
// Phần 2 của shim UnityEngine: Physics2D, Components, Attributes và TestRuntime tất định.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeField : Attribute
    {
    }

    public enum RigidbodyType2D
    {
        Dynamic,
        Kinematic,
        Static
    }

    public class PhysicsMaterial2D : Object
    {
        public float friction { get; set; } = 0.4f;
        public float bounciness { get; set; }
    }

    public class Collider2D : Behaviour
    {
        public bool isTrigger { get; set; }
        public PhysicsMaterial2D sharedMaterial { get; set; }
    }

    public class BoxCollider2D : Collider2D
    {
        public Vector2 size { get; set; } = Vector2.one;
        public Vector2 offset { get; set; }
    }

    public class CircleCollider2D : Collider2D
    {
        public float radius { get; set; } = 0.5f;
        public Vector2 offset { get; set; }
    }

    public class Rigidbody2D : Component
    {
        public RigidbodyType2D bodyType { get; set; } = RigidbodyType2D.Dynamic;
        public float gravityScale { get; set; } = 1f;
        public bool freezeRotation { get; set; }
        public Vector2 linearVelocity { get; set; }
    }

    public struct ContactPoint2D
    {
        public Vector2 point;
        public Vector2 normal;

        public ContactPoint2D(Vector2 point, Vector2 normal)
        {
            this.point = point;
            this.normal = normal;
        }
    }

    public class Collision2D
    {
        private readonly List<ContactPoint2D> contacts = new List<ContactPoint2D>();

        public Collider2D collider { get; set; }
        public Collider2D otherCollider { get; set; }
        public Rigidbody2D rigidbody { get; set; }
        public GameObject gameObject => collider != null ? collider.gameObject : null;
        public Transform transform => collider != null ? collider.transform : null;

        public int contactCount => contacts.Count;

        public ContactPoint2D GetContact(int index) => contacts[index];

        public void AddContact(Vector2 point, Vector2 normal)
        {
            contacts.Add(new ContactPoint2D(point, normal));
        }

        public static Collision2D Create(Collider2D hitCollider, Vector2 normal)
        {
            Collision2D col = new Collision2D { collider = hitCollider };
            col.AddContact(Vector2.zero, normal);
            return col;
        }
    }

    public class Sprite : Object
    {
        public string Name { get; set; }

        public static Sprite Create(string name)
        {
            return new Sprite { Name = name };
        }
    }

    public class SpriteRenderer : Component
    {
        public Sprite sprite { get; set; }
        public bool enabled { get; set; } = true;
        public int sortingLayerID { get; set; }
        public int sortingOrder { get; set; }
    }

    public class Animator : Behaviour
    {
        private readonly List<string> playedStates = new List<string>();

        public string CurrentState { get; private set; } = string.Empty;

        public IReadOnlyList<string> PlayedStates => playedStates;

        public void Play(string stateName, int layer = 0, float normalizedTime = 0f)
        {
            CurrentState = stateName;
            playedStates.Add(stateName);
        }

        public void ResetHistory()
        {
            playedStates.Clear();
            CurrentState = string.Empty;
        }
    }

    public class ParticleSystem : Component
    {
        public bool isPlaying { get; private set; }

        public int PlayCount { get; private set; }
        public int StopCount { get; private set; }

        public void Play()
        {
            isPlaying = true;
            PlayCount++;
        }

        public void Stop()
        {
            isPlaying = false;
            StopCount++;
        }
    }

    /// <summary>
    /// Runtime điều khiển vòng lặp coroutine, Destroy trễ và thời gian tất định cho test.
    /// Cho phép mô phỏng từng frame hoặc bước nhảy thời gian lớn trong 1 micro-giây CPU.
    /// </summary>
    public static class TestRuntime
    {
        internal class CoroutineState
        {
            public MonoBehaviour owner;
            public IEnumerator enumerator;
            public float waitRemaining;
            public bool done;
        }

        private struct ScheduledDestroyEntry
        {
            public Object target;
            public float executeAt;
        }

        private static readonly List<CoroutineState> coroutines = new List<CoroutineState>();
        private static readonly List<ScheduledDestroyEntry> pendingDestroys = new List<ScheduledDestroyEntry>();

        public static float Clock { get; private set; }

        public static void Reset()
        {
            Clock = 0f;
            coroutines.Clear();
            pendingDestroys.Clear();
            Debug.ClearLogs();
            Time.deltaTime = 0.02f;
            Time.fixedDeltaTime = 0.02f;
        }

        internal static Coroutine StartCoroutine(MonoBehaviour owner, IEnumerator routine)
        {
            CoroutineState state = new CoroutineState
            {
                owner = owner,
                enumerator = routine,
                waitRemaining = 0f,
                done = false
            };
            coroutines.Add(state);

            // Chạy ngay frame đầu tiên (giống Unity khi gọi StartCoroutine)
            StepCoroutine(state, 0f);

            return new Coroutine { State = state };
        }

        internal static void StopCoroutine(MonoBehaviour owner, Coroutine routine)
        {
            if (routine?.State != null)
            {
                routine.State.done = true;
            }
        }

        internal static void StopAllCoroutines(MonoBehaviour owner)
        {
            for (int i = 0; i < coroutines.Count; i++)
            {
                if (ReferenceEquals(coroutines[i].owner, owner))
                {
                    coroutines[i].done = true;
                }
            }
        }

        internal static void ScheduleDestroy(Object obj, float delay)
        {
            pendingDestroys.Add(new ScheduledDestroyEntry
            {
                target = obj,
                executeAt = Clock + delay
            });
        }

        /// <summary>Tua nhanh thời gian <paramref name="seconds"/> với bước nhảy nhỏ <paramref name="step"/>.</summary>
        public static void AdvanceTime(float seconds, float step = 0.02f)
        {
            float target = Clock + seconds;
            while (Clock < target)
            {
                float dt = Math.Min(step, target - Clock);
                if (dt <= 0.00001f)
                {
                    break;
                }

                Time.deltaTime = dt;
                Clock += dt;

                // Cập nhật Destroy trễ
                for (int i = pendingDestroys.Count - 1; i >= 0; i--)
                {
                    if (Clock >= pendingDestroys[i].executeAt)
                    {
                        Object.Destroy(pendingDestroys[i].target);
                        pendingDestroys.RemoveAt(i);
                    }
                }

                // Cập nhật coroutines
                for (int i = 0; i < coroutines.Count; i++)
                {
                    CoroutineState state = coroutines[i];
                    if (state.done)
                    {
                        continue;
                    }

                    StepCoroutine(state, dt);
                }

                coroutines.RemoveAll(c => c.done);
            }
        }

        private static void StepCoroutine(CoroutineState state, float dt)
        {
            if (state.waitRemaining > 0f)
            {
                state.waitRemaining -= dt;
                if (state.waitRemaining > 0f)
                {
                    return;
                }
            }

            try
            {
                if (!state.enumerator.MoveNext())
                {
                    state.done = true;
                    return;
                }

                object cur = state.enumerator.Current;
                if (cur is WaitForSeconds wfs)
                {
                    state.waitRemaining = wfs.Seconds;
                }
                else
                {
                    // yield return null (chờ 1 frame tiếp theo)
                    state.waitRemaining = 0f;
                }
            }
            catch
            {
                state.done = true;
                throw;
            }
        }

        /// <summary>Gọi hàm lifecycle (Awake, OnEnable, OnDisable, Start, v.v.) qua reflection.</summary>
        public static void InvokeLifecycle(object target, string methodName, params object[] parameters)
        {
            if (target == null)
            {
                return;
            }

            Type type = target.GetType();
            MethodInfo method = null;
            while (type != null && method == null)
            {
                method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }

            if (method != null)
            {
                method.Invoke(target, parameters);
            }
        }

        /// <summary>Set giá trị cho field private [SerializeField].</summary>
        public static void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Type type = target.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }

            if (field == null)
            {
                throw new InvalidOperationException($"Không tìm thấy field '{fieldName}' trên {target.GetType().FullName}");
            }

            field.SetValue(target, value);
        }

        public static object GetField(object target, string fieldName)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Type type = target.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }

            if (field == null)
            {
                throw new InvalidOperationException($"Không tìm thấy field '{fieldName}' trên {target.GetType().FullName}");
            }

            return field.GetValue(target);
        }
    }
}