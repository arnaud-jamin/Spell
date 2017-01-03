using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Spell
{
    public static class MathHelper
    {
        //-----------------------------------------------------------------------------------------
        public static Quaternion RotationX_90 = Quaternion.Euler(90, 0, 0);
        public static Quaternion RotationX_180 = Quaternion.Euler(180, 0, 0);
        public static Quaternion RotationY_90 = Quaternion.Euler(0, 90, 0);

        // ----------------------------------------------------------------------------------------
        public static Vector2 Multiply(this Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x * v2.x, v1.y * v2.y);
        }

        // ----------------------------------------------------------------------------------------
        public static Vector2 Multiply(this Vector2 v1, float x, float y)
        {
            return new Vector2(v1.x * x, v1.y * y);
        }

        // ----------------------------------------------------------------------------------------
        public static Vector3 Multiply(this Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }

        // ----------------------------------------------------------------------------------------
        public static Vector3 Multiply(this Vector3 v1, float x, float y, float z)
        {
            return new Vector3(v1.x * x, v1.y * y, v1.z * z);
        }

        //-----------------------------------------------------------------------------------------
        public static float Step(float value, float step)
        {
            if (step <= 0)
                return value;

            return (Mathf.Sign(value)) * (Mathf.Floor(Math.Abs(value) / step)) * step;
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 Step(Vector2 value, Vector2 step)
        {
            value.x = Step(value.x, step.x);
            value.y = Step(value.y, step.y);
            return value;
        }

        // ----------------------------------------------------------------------------------------
        public static float Deadzone(float value, float min, float max)
        {
            return Mathf.Sign(value) * RescaleAndClamp(Mathf.Abs(value), min, max);
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 Deadzone(Vector2 input, float deadzone)
        {
            if (input.magnitude < deadzone)
                return Vector2.zero;

            return input.normalized * ((input.magnitude - deadzone) / (1 - deadzone));
        }

        //-----------------------------------------------------------------------------------------
        //This function is used to clamp the joystick values (usually to 1)
        public static Vector2 ClampToMax(Vector2 input, float minDeadZoneX, float minDeadZoneY,float clampValue)
        {
            if (Mathf.Abs(input.x) > minDeadZoneX)
            {
                input.x = clampValue * input.x / Mathf.Abs(input.x);
            }
            if (Mathf.Abs(input.y) > minDeadZoneY)
            {
                input.y = clampValue * input.y / Mathf.Abs(input.y);
            }

            return input;
        }

        //-----------------------------------------------------------------------------------------
        //This function is used to clamp the joystick values (usually to 0)
        public static Vector2 ClampToMin(Vector2 input, float maxDeadZoneX, float maxDeadZoneY, float clampValue)
        {
            if (Mathf.Abs(input.x) < maxDeadZoneX)
            {
                input.x = clampValue;
            }
            if (Mathf.Abs(input.y) < maxDeadZoneY)
            {
                input.y = clampValue;
            }

            return input;
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 FilterInput(Vector2 input, float deadzone, float power)
        {
            float lenght = input.magnitude;
            float newLenght = RescaleAndClamp(lenght, deadzone, 1);
            input = input.normalized * newLenght;
            input = input.normalized * Mathf.Pow(input.magnitude, power);
            return input;
        }

        //-----------------------------------------------------------------------------------------
        public static float Rescale(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        //-----------------------------------------------------------------------------------------
        public static float RescaleAndClamp(float value, float min, float max)
        {
            return Mathf.Clamp(Rescale(value, min, max), 0.0f, 1.0f);
        }

        //-----------------------------------------------------------------------------------------
        public static float RescaleAndClamp(float value, float min, float max, float newMin, float newMax)
        {
            return Mathf.Lerp(newMin, newMax, RescaleAndClamp(value, min, max));
        }

        //-----------------------------------------------------------------------------------------
        public static float Damping(float src, float dst, float dt, float factor)
        {
            return factor == 0 ? dst : ((src * factor) + (dst * dt)) / (factor + dt);
        }

        //-----------------------------------------------------------------------------------------
        public static Vector3 Damping(Vector2 src, Vector2 dst, float dt, float factor)
        {
            return factor == 0 ? dst : ((src * factor) + (dst * dt)) / (factor + dt);
        }

        //-----------------------------------------------------------------------------------------
        public static Vector3 Damping(Vector3 src, Vector3 dst, float dt, float factor)
        {
            return factor == 0 ? dst : ((src * factor) + (dst * dt)) / (factor + dt);
        }

        //-----------------------------------------------------------------------------------------
        public static Color Damping(Color src, Color dst, float dt, float factor)
        {
            return factor == 0 ? dst : ((src * factor) + (dst * dt)) / (factor + dt);
        }

        //-----------------------------------------------------------------------------------------
        public static Color ChangeAlpha(Color src, float a)
        {
            return new Color(src.r, src.g, src.b, a);
        }

        //-----------------------------------------------------------------------------------------
        public static bool LineLineIntersection(Vector2 ps1, Vector2 pe1, Vector2 ps2, Vector2 pe2, out Vector2 intersection)
        {
            // Get A,B,C of first line - points : ps1 to pe1
            float A1 = pe1.y - ps1.y;
            float B1 = ps1.x - pe1.x;
            float C1 = A1 * ps1.x + B1 * ps1.y;

            // Get A,B,C of second line - points : ps2 to pe2
            float A2 = pe2.y - ps2.y;
            float B2 = ps2.x - pe2.x;
            float C2 = A2 * ps2.x + B2 * ps2.y;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
            {
                intersection = Vector2.zero;
                return false;
            }
            else
            {
                intersection = new Vector2((B2 * C1 - B1 * C2) / delta, (A1 * C2 - A2 * C1) / delta);
                return true;
            }
        }

        // ---------------------------------------------------------------------------------------
        public static bool GetDirection(Vector3 position, Vector3 target, ref float distance, ref Vector3 direction, float distanceTest = 0.0001f)
        {
            Vector3 diff = target - position;
            distance = diff.magnitude;
            if (distance < distanceTest)
            {
                direction = Vector3.zero; 
                return false;
            }
            else
            {
                direction = (diff / distance);
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------
        public static bool TestCircleCircle(Vector2 v1, float radius1, Vector2 v2, float radius2)
        {
            var sqDist = (v1 - v2).sqrMagnitude;
            var sqRad = (radius1 + radius2) * (radius1 + radius2);
            return sqDist <= sqRad;
        }

        //-----------------------------------------------------------------------------------------
        public static bool TestSphereSphere(Vector3 v1, float radius1, Vector3 v2, float radius2)
        {
            var sqDist = (v1 - v2).sqrMagnitude;
            var sqRad = (radius1 + radius2) * (radius1 + radius2);
            return sqDist <= sqRad;
        }

        //---------------------------------------------------------------------
        public static Vector3 PointLineProjection(Vector3 point, Vector3 line0, Vector3 line1)
        {
            var dir = (line1 - line0).normalized;
            return line0 + dir * Vector3.Dot(dir, point - line0);
        }

        //---------------------------------------------------------------------
        public static Vector3 PointSegmentProjection(Vector3 point, Vector3 segment0, Vector3 segment1)
        {
            var u = segment1 - segment0;
            float t = Vector3.Dot(point - segment0, u) / Vector3.Dot(u, u);
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return segment0 + t * u;
        }

        //---------------------------------------------------------------------
        public static float PointLineDistance(Vector3 point, Vector3 line0, Vector3 line1)
        {
            return Vector3.Distance(point, PointLineProjection(point, line0, line1));
        }

        //---------------------------------------------------------------------
        public static float PositiveModulo(float x, float m)
        { 
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        //---------------------------------------------------------------------
        public static int PositiveModulo(int x, int m)
        {
            var r = x % m;
            return r < 0 ? r + m : r;
        }

        //-----------------------------------------------------------------------------------------
        public static Vector3 Max(this Vector3 v1, float v2)
        {
            return new Vector3(Mathf.Max(v1.x, v2), Mathf.Max(v1.y, v2), Mathf.Max(v1.z, v2));
        }

        //-----------------------------------------------------------------------------------------
        public static Vector3 Min(this Vector3 v1, float v2)
        {
            return new Vector3(Mathf.Min(v1.x, v2), Mathf.Min(v1.y, v2), Mathf.Min(v1.z, v2));
        }

        //-----------------------------------------------------------------------------------------
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 Max(this Vector2 v1, float v2)
        {
            return new Vector2(Mathf.Max(v1.x, v2), Mathf.Max(v1.y, v2));
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 Min(this Vector2 v1, float v2)
        {
            return new Vector2(Mathf.Min(v1.x, v2), Mathf.Min(v1.y, v2));
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        //-----------------------------------------------------------------------------------------
        // Trilinear Interpolation for a cube of size (1, 1, 1)
        // http://paulbourke.net/miscellaneous/interpolation/
        // In general the box will not be of unit size nor will it be aligned at the origin. 
        // Simple translation and scaling (possibly of each axis independently) can be used 
        // to transform into and then out of this simplified situation.
        public static float TrilinearInterpolation(float x, float y, float z, float c1_000, float c2_100, float c3_010, float c4_001, float c5_101, float c6_011, float c7_110, float c8_111)
        {
            return (c1_000 * (1 - x) * (1 - y) * (1 - z))
                 + (c2_100 * x * (1 - y) * (1 - z))
                 + (c3_010 * (1 - x) * y * (1 - z))
                 + (c4_001 * (1 - x) * (1 - y) * z)
                 + (c5_101 * x * (1 - y) * z)
                 + (c6_011 * (1 - x) * y * z)
                 + (c7_110 * x * y * (1 - z))
                 + (c8_111 * x * y * z);
        }

        //-----------------------------------------------------------------------------------------
        // Compute the bounding box of an object represented by his extends and his a transformation.
        public static Bounds ComputeBoundingBox(Matrix4x4 transform, Vector3 extends)
        {
            var points = new Vector3[8];
            points[0] = new Vector3(-extends.x, -extends.y, -extends.z); // - - - 
            points[1] = new Vector3( extends.x, -extends.y, -extends.z); // + - - 
            points[2] = new Vector3(-extends.x,  extends.y, -extends.z); // - + - 
            points[3] = new Vector3(-extends.x, -extends.y,  extends.z); // - - + 
            points[4] = new Vector3( extends.x,  extends.y, -extends.z); // + + - 
            points[5] = new Vector3( extends.x, -extends.y,  extends.z); // + - + 
            points[6] = new Vector3(-extends.x,  extends.y,  extends.z); // - + + 
            points[7] = new Vector3( extends.x,  extends.y,  extends.z); // + + + 

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 0; i < points.Length; ++i)
            {
                var p = transform.MultiplyPoint3x4(points[i]);

                min.x = Mathf.Min(p.x, min.x);
                min.y = Mathf.Min(p.y, min.y);
                min.z = Mathf.Min(p.z, min.z);

                max.x = Mathf.Max(p.x, max.x);
                max.y = Mathf.Max(p.y, max.y);
                max.z = Mathf.Max(p.z, max.z);
            }
            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        //-----------------------------------------------------------------------------------------
        public static void CreateBasis(Vector3 v, Vector3 up, out Vector3 x, out Vector3 y)
        {
            if (v.sqrMagnitude > 0.0001f)
            {
                var q = Quaternion.LookRotation(v, up);
                x = q * Vector3.right;
                y = q * Vector3.up;
            }
            else
            {
                x = Vector3.right;
                y = Vector3.up;
            }
        }

        //-----------------------------------------------------------------------------------------
        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = v.x;
            float ty = v.y;
            v.x = (cos * tx) - (sin * ty);
            v.y = (sin * tx) + (cos * ty);
            return v;
        }

        //-----------------------------------------------------------------------------------------
        public static float AspectRatioFromFov(float verticalFov, float horizontalFov)
        {
            return Mathf.Tan(horizontalFov * Mathf.Deg2Rad * 0.5f) / Mathf.Tan(verticalFov * Mathf.Deg2Rad * 0.5f);
        }

        //-----------------------------------------------------------------------------------------
        public static float SquareDistance(Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).sqrMagnitude;
        }

    }
}
