using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell
{
    public static class Utils
    {
        //-----------------------------------------------------------------------------------------
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        //-----------------------------------------------------------------------------------------
        public static int EnumCount<T>()
        {
            return Enum.GetValues(typeof(T)).Length;
        }

        //-----------------------------------------------------------------------------------------
        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        //-----------------------------------------------------------------------------------------
        public static int GetEnumCount<TEnum>() where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum)).Length;
        }

        //-----------------------------------------------------------------------------------------
        public static bool IsSameOrSubclassOf(this Type potentialDescendant, Type potentialBase)
        {
            return potentialDescendant.IsSubclassOf(potentialBase) || potentialDescendant == potentialBase;
        }

        //-----------------------------------------------------------------------------------------
        public static Type GetListItemType(this Type potentialListType)
        {
            if (potentialListType.IsGenericType && potentialListType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return potentialListType.GetGenericArguments()[0];
            }
            else
            {
                return potentialListType;
            }
        }

        //-----------------------------------------------------------------------------------------
        public static string FormatTime(float time)
        {
            int totalSeconds = (int)time;
            int milliseconds = (int)((time - (float)totalSeconds) * 100);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds - minutes * 60;

            if (minutes > 0)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, milliseconds);
            }
            else
            {
                return string.Format("{0:D2}:{1:D2}", seconds, milliseconds);
            }
        }

        //-----------------------------------------------------------------------------------------
        public static void Do<T>(this GameObject gameObject, Action<T> op) where T : MonoBehaviour
        {
            if (gameObject != null)
            {
                var c = gameObject.GetComponent<T>();
                if (c != null)
                {
                    op(c);
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        public static void Do<T>(this Component component, Action<T> op) where T : MonoBehaviour
        {
            if (component != null)
            {
                var c = component.GetComponent<T>();
                if (c != null)
                {
                    op(c);
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        public static R Get<T, R>(this GameObject gameObject, Func<T, R> op) where T : MonoBehaviour
        {
            if (gameObject != null)
            {
                var c = gameObject.GetComponent<T>();
                if (c != null)
                {
                    return op(c);
                }
            }

            return default(R);
        }

        //-----------------------------------------------------------------------------------------
        public static R Get<T, R>(this Component component, Func<T, R> op) where T : MonoBehaviour
        {
            if (component != null)
            {
                var c = component.GetComponent<T>();
                if (c != null)
                {
                    return op(c);
                }
            }

            return default(R);
        }

        //-----------------------------------------------------------------------------------------
        public static bool SetValueType<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        //-----------------------------------------------------------------------------------------
        public static bool SetReferenceType<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
    }
}
