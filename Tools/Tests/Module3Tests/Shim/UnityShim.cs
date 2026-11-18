// Trần Văn Tuyên, phiên bản 1.0
// Shim UnityEngine tối thiểu cho kiểm thử headless (không cần Unity Editor).
// Mọi kiểu ở đây mô phỏng đúng ngữ nghĩa API mà 6 script Module 3 sử dụng.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    /// <summary>Gốc chung mô phỏng UnityEngine.Object, có ngữ nghĩa "null" giống Unity sau khi Destroy.</summary>
    public class Object
    {
        internal bool destroyed;

        /// <summary>Đối tượng đã bị Destroy chưa.</summary>
        public bool IsDestroyed => destroyed;

        /// <summary>Hủy đối tượng ngay lập tức.</summary>
        public static void Destroy(Object obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.destroyed = true;
            if (obj is GameObject go)
            {
                go.DeactivateInternal();
            }
        }

        /// <summary>Hủy đối tượng sau <paramref name="t"/> giây (mô phỏng bằng đồng hồ test).</summary>
        public static void Destroy(Object obj, float t)
        {
            TestRuntime.ScheduleDestroy(obj, t);
        }

        /// <summary>Hủy đối tượng ngay lập tức.</summary>
        public static void DestroyImmediate(Object obj)
        {
            Destroy(obj);
        }

        private static bool IsNullLike(Object obj)
        {
            return obj is null || obj.destroyed;
        }

        public static bool operator ==(Object a, Object b)
        {
            bool aNull = IsNullLike(a);
            bool bNull = IsNullLike(b);
            if (aNull && bNull)
            {
                return true;
            }

            if (aNull || bNull)
            {
                return false;
            }

            return ReferenceEquals(a, b);
        }

        public static bool operator !=(Object a, Object b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Object other)
            {
                return this == other;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
        }
    }

    /// <summary>Mô phỏng Transform với vị trí trong không gian.</summary>
    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; } = Vector3.one;
        public Vector3 eulerAngles { get; set; }

        public GameObject parent { get; internal set; }
    }

    /// <summary>Mô phỏng GameObject: chứa danh sách component và Tag/Layer.</summary>
    public class GameObject : Object
    {
        private readonly List<Component> components = new List<Component>();

        public GameObject(string name = "GameObject")
        {
            this.name = name;
            transform = new Transform();
            transform.Bind(this);
            components.Add(transform);
        }

        public string name { get; set; }
        public string tag { get; set; } = "Untagged";
        public int layer { get; set; }
        public bool activeSelf { get; private set; } = true;
        public bool activeInHierarchy => activeSelf;
        public Transform transform { get; }

        /// <summary>Thêm component kiểu <typeparamref name="T"/> (không tự gọi Awake như Unity).</summary>
        public T AddComponent<T>() where T : Component, new()
        {
            T component = new T();
            component.Bind(this);
            components.Add(component);
            return component;
        }

        /// <summary>Lấy component đầu tiên khớp kiểu (kể cả lớp con); trả null nếu không có.</summary>
        public T GetComponent<T>() where T : class
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T match)
                {
                    return match;
                }
            }

            return null;
        }

        /// <summary>Toàn bộ component khớp kiểu.</summary>
        public T[] GetComponents<T>() where T : class
        {
            List<T> result = new List<T>();
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T match)
                {
                    result.Add(match);
                }
            }

            return result.ToArray();
        }

        public bool CompareTag(string expected)
        {
            return string.Equals(tag, expected, StringComparison.Ordinal);
        }

        /// <summary>Bật/tắt object; tắt sẽ gọi OnDisable trên các MonoBehaviour giống Unity.</summary>
        public void SetActive(bool value)
        {
            if (activeSelf == value)
            {
                return;
            }

            activeSelf = value;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is MonoBehaviour behaviour)
                {
                    TestRuntime.InvokeLifecycle(behaviour, value ? "OnEnable" : "OnDisable");
                }
            }
        }

        internal void DeactivateInternal()
        {
            activeSelf = false;
        }

        internal IReadOnlyList<Component> Components => components;
    }

    /// <summary>Mô phỏng Component: truy cập component cùng GameObject qua tag.</summary>
    public class Component : Object
    {
        public GameObject gameObject { get; private set; }

        public Transform transform => gameObject != null ? gameObject.transform : null;

        public string name
        {
            get => gameObject != null ? gameObject.name : string.Empty;
            set
            {
                if (gameObject != null)
                {
                    gameObject.name = value;
                }
            }
        }

        public string tag
        {
            get => gameObject != null ? gameObject.tag : "Untagged";
            set
            {
                if (gameObject != null)
                {
                    gameObject.tag = value;
                }
            }
        }

        internal void Bind(GameObject owner)
        {
            gameObject = owner;
        }

        public T GetComponent<T>() where T : class
        {
            return gameObject != null ? gameObject.GetComponent<T>() : null;
        }

        public bool TryGetComponent<T>(out T component) where T : class
        {
            component = GetComponent<T>();
            return component != null;
        }

        public bool CompareTag(string expected)
        {
            return gameObject != null && gameObject.CompareTag(expected);
        }
    }

    /// <summary>Mô phỏng Behaviour (có cờ enabled).</summary>
    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;

        public bool isActiveAndEnabled => enabled && gameObject != null && gameObject.activeInHierarchy;
    }

    /// <summary>Mô phỏng MonoBehaviour: coroutine chạy trên đồng hồ test tất định.</summary>
    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return TestRuntime.StartCoroutine(this, routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            TestRuntime.StopCoroutine(this, routine);
        }

        public void StopAllCoroutines()
        {
            TestRuntime.StopAllCoroutines(this);
        }
    }

    /// <summary>Handle coroutine trả về từ StartCoroutine.</summary>
    public class Coroutine : Object
    {
        internal TestRuntime.CoroutineState State { get; set; }

        public bool IsDone => State == null || State.done;
    }

    public class WaitForSeconds
    {
        public WaitForSeconds(float seconds)
        {
            Seconds = seconds;
        }

        public float Seconds { get; }
    }

    public class WaitForSecondsRealtime : WaitForSeconds
    {
        public WaitForSecondsRealtime(float seconds) : base(seconds)
        {
        }
    }

    /// <summary>Mô phỏng Time: giá trị do TestRuntime điều khiển tất định.</summary>
    public static class Time
    {
        public static float deltaTime { get; internal set; } = 0.02f;

        public static float unscaledDeltaTime => deltaTime;

        public static float time => TestRuntime.Clock;

        public static float fixedDeltaTime { get; internal set; } = 0.02f;
    }

    /// <summary>Mô phỏng Debug: gom log để test kiểm chứng cảnh báo dữ liệu Inspector.</summary>
    public static class Debug
    {
        private static readonly List<string> messages = new List<string>();

        public static IReadOnlyList<string> Messages => messages;

        public static void ClearLogs()
        {
            messages.Clear();
        }

        public static void Log(object message)
        {
            messages.Add("LOG: " + message);
        }

        public static void Log(object message, Object context)
        {
            messages.Add("LOG: " + message);
        }

        public static void LogWarning(object message)
        {
            messages.Add("WARN: " + message);
        }

        public static void LogWarning(object message, Object context)
        {
            messages.Add("WARN: " + message);
        }

        public static void LogError(object message)
        {
            messages.Add("ERROR: " + message);
        }

        public static void LogError(object message, Object context)
        {
            messages.Add("ERROR: " + message);
        }

        public static bool Contains(string fragment)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].IndexOf(fragment, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Mô phỏng Vector2 với đúng ngữ nghĩa normalize của Unity.</summary>
    public struct Vector2
    {
        public const float kEpsilon = 1E-05f;

        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);
        public static Vector2 up => new Vector2(0f, 1f);
        public static Vector2 down => new Vector2(0f, -1f);
        public static Vector2 left => new Vector2(-1f, 0f);
        public static Vector2 right => new Vector2(1f, 0f);

        public float magnitude => Mathf.Sqrt((x * x) + (y * y));

        public float sqrMagnitude => (x * x) + (y * y);

        public Vector2 normalized
        {
            get
            {
                float mag = magnitude;
                if (mag > kEpsilon)
                {
                    return new Vector2(x / mag, y / mag);
                }

                return zero;
            }
        }

        /// <summary>Chuẩn hóa tại chỗ; vector zero giữ nguyên zero (không chia cho 0).</summary>
        public void Normalize()
        {
            float mag = magnitude;
            if (mag > kEpsilon)
            {
                x /= mag;
                y /= mag;
            }
            else
            {
                x = 0f;
                y = 0f;
            }
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);

        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);

        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);

        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);

        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.x * d, a.y * d);

        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);

        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0f);

        public static implicit operator Vector2(Vector3 v) => new Vector2(v.x, v.y);

        public static bool operator ==(Vector2 a, Vector2 b) => (a - b).sqrMagnitude < 9.99999944E-11f;

        public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

        public override bool Equals(object other) => other is Vector2 v && x == v.x && y == v.y;

        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);

        public override string ToString() => string.Format("({0:F2}, {1:F2})", x, y);
    }

    /// <summary>Mô phỏng Vector3.</summary>
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 zero => new Vector3(0f, 0f, 0f);
        public static Vector3 one => new Vector3(1f, 1f, 1f);
        public static Vector3 up => new Vector3(0f, 1f, 0f);

        public float magnitude => Mathf.Sqrt((x * x) + (y * y) + (z * z));

        public float sqrMagnitude => (x * x) + (y * y) + (z * z);

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);

        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;

        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);

        public override bool Equals(object other) => other is Vector3 v && this == v;

        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);

        public override string ToString() => string.Format("({0:F2}, {1:F2}, {2:F2})", x, y, z);
    }

    /// <summary>Mô phỏng Mathf với các hàm mà Module 3 sử dụng.</summary>
    public static class Mathf
    {
        public const float PI = 3.14159274f;
        public const float Epsilon = 1.401298E-45f;
        public const float Infinity = float.PositiveInfinity;

        public static float Sin(float f) => (float)Math.Sin(f);
        public static float Cos(float f) => (float)Math.Cos(f);
        public static float Sqrt(float f) => (float)Math.Sqrt(f);
        public static float Abs(float f) => Math.Abs(f);
        public static int Abs(int f) => Math.Abs(f);
        public static float Max(float a, float b) => a > b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
        public static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Sign(float f) => f >= 0f ? 1f : -1f;
        public static float Lerp(float a, float b, float t) => a + ((b - a) * Clamp01(t));
        public static bool Approximately(float a, float b) => Math.Abs(b - a) < Max(1E-06f * Max(Math.Abs(a), Math.Abs(b)), Epsilon * 8f);
        public static int RoundToInt(float f) => (int)Math.Round(f);
    }
}