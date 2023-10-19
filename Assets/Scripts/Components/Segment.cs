using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mergepins
{
    public enum SevenSegmentNumber
    {
        None = 0b0000000,
        Zero = 0b0111111,
        One = 0b0000110,
        Two = 0b1011011,
        Three = 0b1001111,
        Four = 0b1100110,
        Five = 0b1101101,
        Six = 0b1111101,
        Seven = 0b0100111,
        Eight = 0b1111111,
        Nine = 0b1101111
    }

    public class SegmentElement
    {
        public bool On = true;
        private Vector2[] positions = new Vector2[6];
        public Color[] colors = new Color[6];

        public SegmentElement(Vector2[] pos_base, Matrix2x2 rot, Vector2 center_pos, Matrix2x2 jacobian)
        {
            for (int i = 0; i < 6; i++)
                colors[i] = Color.white;

            if (pos_base.Length != 6)
            {
                Debug.Log("配列要素数が6ではありません。");
                for (int i = 0; i < 6; i++)
                    positions[i] = Vector2.zero;
                return;
            }

            for (int i = 0; i < 6; i++)
                positions[i] = jacobian * (rot * pos_base[i] + center_pos);
        }

        private int idx;
        public void AddVert(VertexHelper vh, int idx)
        {
            this.idx = idx * 6;
            for (var i = 0; i < 6; i++)
                vh.AddVert(new UIVertex { position = positions[i], color = colors[i] });
        }

        public void Display(VertexHelper vh)
        {
            if (!On) return;
            vh.AddTriangle(idx + 0, idx + 1, idx + 5);
            vh.AddTriangle(idx + 1, idx + 4, idx + 5);
            vh.AddTriangle(idx + 1, idx + 2, idx + 4);
            vh.AddTriangle(idx + 2, idx + 3, idx + 4);
        }
    }
}