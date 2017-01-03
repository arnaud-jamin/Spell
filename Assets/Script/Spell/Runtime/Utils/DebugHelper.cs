using UnityEngine;

namespace Spell
{
    public class DebugHelper 
    {
        //-----------------------------------------------------------------------------------------
        public static void DrawPoint(Vector3 position, float size, Color color, float duration = 0.0f)
        {
            size *= 0.5f;
            Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color, duration);
            Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color, duration);
            Debug.DrawLine(position - Vector3.left * size, position + Vector3.left * size, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawBox(Vector3 position, Quaternion rotation, Vector3 extends, Color color, float duration = 0.0f)
        {
            Vector3 x = rotation * Vector3.forward * extends.x;
            Vector3 y = rotation * Vector3.up * extends.y;
            Vector3 z = rotation * Vector3.right * extends.z;

            var posUp = position + y;
            Debug.DrawLine(posUp - x + z, posUp + x + z, color, duration);
            Debug.DrawLine(posUp - x - z, posUp + x - z, color, duration);
            Debug.DrawLine(posUp - x - z, posUp - x + z, color, duration);
            Debug.DrawLine(posUp + x - z, posUp + x + z, color, duration);

            var posDn = position - y;
            Debug.DrawLine(posDn - x + z, posDn + x + z, color, duration);
            Debug.DrawLine(posDn - x - z, posDn + x - z, color, duration);
            Debug.DrawLine(posDn - x - z, posDn - x + z, color, duration);
            Debug.DrawLine(posDn + x - z, posDn + x + z, color, duration);

            Debug.DrawLine(posUp + x + z, posDn + x + z, color, duration);
            Debug.DrawLine(posUp - x - z, posDn - x - z, color, duration);
            Debug.DrawLine(posUp - x + z, posDn - x + z, color, duration);
            Debug.DrawLine(posUp + x - z, posDn + x - z, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawBox(Vector3 position, float size, Color color, float duration = 0.0f)
        {
            DrawBox(position, Vector3.one * size, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawBox(Vector3 position, Vector3 size, Color color, float duration = 0.0f)
        {
            size *= 0.5f;
            Vector3 x = Vector3.Scale(Vector3.forward, size);
            Vector3 y = Vector3.Scale(Vector3.up, size);
            Vector3 z = Vector3.Scale(Vector3.right, size);
            var posUp = position + y;
            Debug.DrawLine(posUp - x + z, posUp + x + z, color, duration);
            Debug.DrawLine(posUp - x - z, posUp + x - z, color, duration);
            Debug.DrawLine(posUp - x - z, posUp - x + z, color, duration);
            Debug.DrawLine(posUp + x - z, posUp + x + z, color, duration);
            var posDn = position - y;
            Debug.DrawLine(posDn - x + z, posDn + x + z, color, duration);
            Debug.DrawLine(posDn - x - z, posDn + x - z, color, duration);
            Debug.DrawLine(posDn - x - z, posDn - x + z, color, duration);
            Debug.DrawLine(posDn + x - z, posDn + x + z, color, duration);
            //--
            Debug.DrawLine(posUp + x + z, posDn + x + z, color, duration);
            Debug.DrawLine(posUp - x - z, posDn - x - z, color, duration);
            Debug.DrawLine(posUp - x + z, posDn - x + z, color, duration);
            Debug.DrawLine(posUp + x - z, posDn + x - z, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawCircle(Vector3 position, Quaternion rotation, float radius, int segments, Color color, float duration = 0.0f)
        {
            DrawArc(position, rotation, radius, 0, 360, segments, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawSphere(Vector3 position, Quaternion rotation, float radius, int segments, Color color, float duration = 0.0f)
        {
            DrawArc(position, rotation, radius, 0, 360, segments, color, duration);
            DrawArc(position, rotation * Quaternion.Euler(90, 0, 0), radius, 0, 360, segments, color, duration);
            DrawArc(position, rotation * Quaternion.Euler(0, 90, 0), radius, 0, 360, segments, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawSphereCast(Ray position, float radius, float length, int segments, Color color, float duration = 0.0f)
        {
            DebugHelper.DrawSphere(position.origin, Quaternion.identity, radius, segments, color, duration);
            DebugHelper.DrawSphere(position.origin + position.direction * length, Quaternion.identity, radius, segments, color, duration);

            Debug.DrawLine(position.origin + Vector3.left * radius, position.origin + Vector3.left * radius + position.direction * length, color, duration);
            Debug.DrawLine(position.origin + Vector3.forward * radius, position.origin + Vector3.forward * radius + position.direction * length, color, duration);
            Debug.DrawLine(position.origin + Vector3.right * radius, position.origin + Vector3.right * radius + position.direction * length, color, duration);
            Debug.DrawLine(position.origin + Vector3.back * radius, position.origin + Vector3.back * radius + position.direction * length, color, duration);
            Debug.DrawLine(position.origin + Vector3.up * radius, position.origin + Vector3.up * radius + position.direction * length, color, duration);
            Debug.DrawLine(position.origin + Vector3.down * radius, position.origin + Vector3.down * radius + position.direction * length, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawArrow(Vector3 p0, Vector3 p1, Vector3 up, float width, float length, Color color, float duration = 0.0f)
        {
            var forward = (p1 - p0).normalized;
            Vector3 right;
            MathHelper.CreateBasis(forward, up, out right, out up);

            Debug.DrawLine(p0, p1, color, duration);
            Debug.DrawLine(p1, p1 - forward * length - right * width, color, duration);
            Debug.DrawLine(p1, p1 - forward * length + right * width, color, duration);
            Debug.DrawLine(p1, p1 - forward * length - up * width, color, duration);
            Debug.DrawLine(p1, p1 - forward * length + up * width, color, duration);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawArc(Vector3 position, Quaternion rotation, float radius, float start, float end, int segments, Color color, float duration = 0.0f)
        {
            if (segments < 1)
                return;

            float step = (end - start) / segments;
            for (uint i = 0; i < segments; i++)
            {
                uint j = i + 1;
                float angle0 = start + i * step;
                float angle1 = start + j * step;
                Vector4 p0 = new Vector3(radius * Mathf.Cos(angle0 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle0 * Mathf.Deg2Rad));
                Vector4 p1 = new Vector3(radius * Mathf.Cos(angle1 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle1 * Mathf.Deg2Rad));
                Debug.DrawLine(position + rotation * p0, position + rotation * p1, color, duration);
            }
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawPie(Vector3 position, Quaternion rotation, float radius, float start, float end, int segments, Color color, float duration = 0.0f)
        {
            if (segments < 1)
                return;

            if ((end - start) >= 360)
            {
                DrawCircle(position, rotation, radius, segments, color, duration);
                return;
            }

            float step = (end - start) / segments;
            for (uint i = 0; i < segments; i++)
            {
                uint j = i + 1;
                float angle0 = start + (i * step);
                float angle1 = start + (j * step);
                Vector3 p0 = new Vector3(radius * Mathf.Cos(angle0 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle0 * Mathf.Deg2Rad));
                Vector3 p1 = new Vector3(radius * Mathf.Cos(angle1 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle1 * Mathf.Deg2Rad));
                Debug.DrawLine(position + rotation * p0, position + rotation * p1, color, duration);

                if (i == 0)
                {
                    Debug.DrawLine(position, rotation * p0, color, duration);
                }

                if (i == segments - 1)
                {
                    Debug.DrawLine(position, rotation * p1, color, duration);
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawPlane(Plane plane, Vector2 offset, float size, Vector3 up, Color color)
        {
            var q = Quaternion.LookRotation(plane.normal, up);
            var x = q * Vector3.right;
            var y = q * Vector3.up;

            var p = (plane.normal * -plane.distance) + (x * offset.x) + (y * offset.y);
            var sx = x * size;
            var sy = y * size;

            Debug.DrawLine(p - sx - sy, p + sx - sy, color);
            Debug.DrawLine(p - sx + sy, p + sx + sy, color);
            Debug.DrawLine(p - sx - sy, p - sx + sy, color);
            Debug.DrawLine(p + sx - sy, p + sx + sy, color);
            DrawArrow(p, p + plane.normal * size * 0.2f, up, size * 0.02f, size * 0.02f, color);
        }
    }
}
