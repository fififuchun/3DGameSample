using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mergepins
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class SevenSegment : Graphic
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

        private float _width = 100;
        private float _height = 30;
        private float _offset = 6;
        private float _angle = 25;

        private Matrix2x2 jacobian;
        private void SetAngle()
        {
            float radian = (90 - _angle) * Mathf.PI / 180;
            jacobian = new Matrix2x2(1, Mathf.Cos(radian), 0, Mathf.Sin(radian));
        }

        private static Matrix2x2[] rot_mats = new Matrix2x2[7]{
            Matrix2x2.E,
            Matrix2x2.Rotation90,
            Matrix2x2.Rotation90,
            Matrix2x2.E,
            Matrix2x2.Rotation90,
            Matrix2x2.Rotation90,
            Matrix2x2.E
        };

        private Vector2[] pos_base;
        private void SetPosBase()
        {
            pos_base = new Vector2[6] {
                new Vector2(-_width, 0) / 2,
                new Vector2(-(_width - _height), _height) / 2,
                new Vector2(_width - _height, _height) / 2,
                new Vector2(_width, 0) / 2,
                new Vector2(_width - _height, -_height) / 2,
                new Vector2(-(_width - _height), -_height) / 2
            };
        }

        private Vector2[] center_pos;
        private void SetCenterPos()
        {
            float distance = _width / 2 + _offset;
            center_pos = new Vector2[7] {
                new Vector2(0, 2*distance),
                new Vector2(distance, distance),
                new Vector2(distance, -distance),
                new Vector2(0, -2*distance),
                new Vector2(-distance, -distance),
                new Vector2(-distance, distance),
                new Vector2(0, 0)
            };
        }

        private SegmentElement[] segments = new SegmentElement[7];
        private void UpdateSegmentsPos()
        {
            for (int i = 0; i < 7; i++)
                segments[i] = new SegmentElement(pos_base, rot_mats[i], center_pos[i], jacobian);
            SetVerticesDirty();
        }

        private SevenSegmentNumber number = SevenSegmentNumber.None;
        public void DisplayNumber(int number)
        {
            switch (number)
            {
                case 0: this.number = SevenSegmentNumber.Zero; break;
                case 1: this.number = SevenSegmentNumber.One; break;
                case 2: this.number = SevenSegmentNumber.Two; break;
                case 3: this.number = SevenSegmentNumber.Three; break;
                case 4: this.number = SevenSegmentNumber.Four; break;
                case 5: this.number = SevenSegmentNumber.Five; break;
                case 6: this.number = SevenSegmentNumber.Six; break;
                case 7: this.number = SevenSegmentNumber.Seven; break;
                case 8: this.number = SevenSegmentNumber.Eight; break;
                case 9: this.number = SevenSegmentNumber.Nine; break;
                default: this.number = SevenSegmentNumber.None; break;
            }
            SetVerticesDirty();
        }

        public void SetColor(Color color)
        {
            for (int i = 0; i < 7; i++)
            {
                if (segments[i] == null) continue;
                for (int j = 0; j < 6; j++)
                    segments[i].colors[j] = color;
            }
            SetVerticesDirty();
        }

        public void SetColor(Color color, int index)
        {
            if (!(index >= 0 && index <= 6))
            {
                Debug.Log("第2引数は0から6の整数にしてください");
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

            for (int i = 0; i < 7; i++)
            {
                if (segments[i] == null) return;

                int flag = 1 << i;
                segments[i].On = ((int)number & flag) == flag;

                segments[i].AddVert(vh, i);
                segments[i].Display(vh);
            }
        }
    }
}