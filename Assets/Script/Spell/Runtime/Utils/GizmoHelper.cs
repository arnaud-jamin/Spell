using UnityEngine;
using System.Collections;

namespace Spell
{
    public class GizmosHelper : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        public static void DrawWireCylinder(Vector3 position, Quaternion rotation, float radius, float height, int segment = 12)
        {
            var p1 = position + rotation * Vector3.up * height;
            var p2 = position - rotation * Vector3.up * height;
            DrawCircle(p1, rotation * MathHelper.RotationX_90, radius, segment);
            DrawCircle(p2, rotation * MathHelper.RotationX_90, radius, segment);
            Gizmos.DrawLine(p1 + rotation * Vector3.right * radius, p2 + rotation * Vector3.right * radius);
            Gizmos.DrawLine(p1 - rotation * Vector3.right * radius, p2 - rotation * Vector3.right * radius);
            Gizmos.DrawLine(p1 + rotation * Vector3.forward * radius, p2 + rotation * Vector3.forward * radius);
            Gizmos.DrawLine(p1 - rotation * Vector3.forward * radius, p2 - rotation * Vector3.forward * radius);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawWireCapsule(Vector3 position, Quaternion rotation, float radius, float height, int segment = 12)
        {
            var p1 = position + rotation * Vector3.up * height;
            var p2 = position - rotation * Vector3.up * height;
            DrawCircle(p1, rotation * MathHelper.RotationX_90, radius, segment);
            DrawCircle(p2, rotation * MathHelper.RotationX_90, radius, segment);
            DrawArc(p1, rotation, radius, 0, 180, segment);
            DrawArc(p1, rotation * MathHelper.RotationY_90, radius, 0, 180, segment);
            DrawArc(p2, rotation * MathHelper.RotationX_180, radius, 0, 180, segment);
            DrawArc(p2, rotation * MathHelper.RotationX_180 * MathHelper.RotationY_90, radius, 0, 180, segment);
            Gizmos.DrawLine(p1 + rotation * Vector3.right * radius, p2 + rotation * Vector3.right * radius);
            Gizmos.DrawLine(p1 - rotation * Vector3.right * radius, p2 - rotation * Vector3.right * radius);
            Gizmos.DrawLine(p1 + rotation * Vector3.forward * radius, p2 + rotation * Vector3.forward * radius);
            Gizmos.DrawLine(p1 - rotation * Vector3.forward * radius, p2 - rotation * Vector3.forward * radius);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawArrow(Vector3 p0, Vector3 p1, Vector3 up, float width, float length)
        {
            var forward = (p1 - p0).normalized;
            Vector3 right;
            MathHelper.CreateBasis(forward, up, out right, out up);

            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p1 - forward * length - right * width);
            Gizmos.DrawLine(p1, p1 - forward * length + right * width);
            Gizmos.DrawLine(p1, p1 - forward * length - up * width);
            Gizmos.DrawLine(p1, p1 - forward * length + up * width);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawPoint(Vector3 position, float size)
        {
            size *= 0.5f;
            Gizmos.DrawLine(position - Vector3.up * size, position + Vector3.up * size);
            Gizmos.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size);
            Gizmos.DrawLine(position - Vector3.left * size, position + Vector3.left * size);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawCircle(Vector3 position, Quaternion rotation, float radius, int segments)
        {
            DrawArc(position, rotation, radius, 0, 360, segments);
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawArc(Vector3 position, Quaternion rotation, float radius, float start, float end, int segments)
        {
            if (segments < 1)
                return;

            float step = (end - start) / segments;
            for (uint i = 0; i < segments; i++)
            {
                uint j = i + 1;
                float angle0 = start + i * step;
                float angle1 = start + j * step;
                Vector3 p0 = new Vector3(radius * Mathf.Cos(angle0 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle0 * Mathf.Deg2Rad));
                Vector3 p1 = new Vector3(radius * Mathf.Cos(angle1 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle1 * Mathf.Deg2Rad));
                Gizmos.DrawLine(position + rotation * p0, position + rotation * p1);
            }
        }

        //-----------------------------------------------------------------------------------------
        public static void DrawPie(Vector3 position, Quaternion rotation, float radius, float start, float end, uint segments)
        {
            if (segments < 1)
                return;

            float step = (end - start) / segments;
            for (uint i = 0; i < segments; i++)
            {
                uint j = i + 1;
                float angle0 = start + (i * step);
                float angle1 = start + (j * step);
                Vector3 p0 = new Vector3(radius * Mathf.Cos(angle0 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle0 * Mathf.Deg2Rad));
                Vector3 p1 = new Vector3(radius * Mathf.Cos(angle1 * Mathf.Deg2Rad), 0.0f, radius * Mathf.Sin(angle1 * Mathf.Deg2Rad));
                Gizmos.DrawLine(position + rotation * p0, position + rotation * p1);

                if (i == 0)
                {
                    Gizmos.DrawLine(position, rotation * p0);
                }

                if (i == segments - 1)
                {
                    Gizmos.DrawLine(position, rotation * p1);
                }
            }
        }
    }
}
