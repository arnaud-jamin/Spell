using UnityEngine;

namespace Spell
{
    public static class ColorExtensions
    {
        public static Color AddHSV(this Color color, float h, float s, float v)
        {
            float _h, _s, _v;
            Color.RGBToHSV(color, out _h, out _s, out _v);
            _h = Mathf.Clamp01(_h + h);
            _s = Mathf.Clamp01(_s + s);
            _v = Mathf.Clamp01(_v + v);
            return Color.HSVToRGB(_h, _s, _v);
        }
    }
}
