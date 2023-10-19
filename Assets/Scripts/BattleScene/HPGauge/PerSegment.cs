using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mergepins
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PerSegment : Graphic
    {
        public float width { get { return _width; } set { _width = value; SetPosBase(); SetCenterPos(); UpdateSegmentsPos(); } }
        public float height { get { return _height; } set { _height = value; SetPosBase(); UpdateSegmentsPos(); } }
        public float offset { get { return _offset; } set { _offset = value; SetCenterPos(); UpdateSegmentsPos(); } }
        public float angle { get { return _angle; } set { _angle = value; SetAngle(); UpdateSegmentsPos(); } }

        protected override void Awake()
        {
            base.Awake();
            SetAngle();
            SetPosBase();
            SetCenterPos();
            UpdateSegmentsPos();
        }

        private float _width = 180;
        private float _height = 26;
        private float _offset = 8;
        private float _angle = 35;

        private Matrix2x2 jacobian;
        private void SetAngle()
        {
            float radian = (90 - _angle) * Mathf.PI / 180;
            jacobian = new Matrix2x2(1, Mathf.Cos(radian), 0, Mathf.Sin(radian));
        }

        private Vector2[] pos_base;
        private void SetPosBase()
        {
            pos_base = new Vector2[6] {
                new Vector2(0, _width) / 2,
                new Vector2(_height, _width - _height) / 2,
                new Vector2(_height, -(_width - _height)) / 2,
                new Vector2(0, -_width) / 2,
                new Vector2(-_height, -(_width - _height)) / 2,
                new Vector2(-_height, _width - _height) / 2
            };
        }

        private Vector2[] center_pos;
        private void SetCenterPos()
        {
            float distance = _width / 2 + _offset;
            center_pos = new Vector2[2] {
                new Vector2(0, distance),
                new Vector2(0, -distance)
            };
        }

        private SegmentElement[] segments = new SegmentElement[2];
        private void UpdateSegmentsPos()
        {
            for (int i = 0; i < 2; i++)
                segments[i] = new SegmentElement(pos_base, Matrix2x2.E, center_pos[i], jacobian);
            SetVerticesDirty();
        }

        private bool On = false;
        public void Display(bool on)
        {
            this.On = on;
            SetVerticesDirty();
        }

        public void SetColor(Color color)
        {
            for (int i = 0; i < 2; i++)
            {
                if (segments[i] == null) continue;
                for (int j = 0; j < 6; j++)
                    segments[i].colors[j] = color;
            }
            SetVerticesDirty();
        }

        public void SetColor(Color color, int index)
        {
            if (!(index >= 0 && index <= 1))
            {
                Debug.Log("第2引数は0から1の整数にしてください");
                return;
            }
            if (segments[index] == null) return;
            for (int i = 0; i < 6; i++)
                segments[index].colors[i] = color;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            for (int i = 0; i < 2; i++)
            {
                if (segments[i] == null) return;

                segments[i].On = On;

                segments[i].AddVert(vh, i);
                segments[i].Display(vh);
            }
        }

    }
}
