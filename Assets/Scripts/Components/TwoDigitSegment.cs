using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mergepins
{
    public class TwoDigitSegment : MonoBehaviour
    {
        [SerializeField] private SevenSegment sevenSegment10;
        [SerializeField] private SevenSegment sevenSegment1;
        [SerializeField] private float distance;
        private Vector2 dis_vec;

        private void Awake()
        {
            dis_vec = new Vector2(distance / 2, 0);
            ((RectTransform)sevenSegment10.transform).anchoredPosition = -dis_vec;
        }

        public void DisplayNumber(int number)
        {
            int ten_place = (number % 100) / 10;
            int one_place = number % 10;

            sevenSegment1.DisplayNumber(one_place);

            if (ten_place == 0)
            {
                sevenSegment10.DisplayNumber(-1);
                ((RectTransform)sevenSegment1.transform).anchoredPosition = Vector2.zero;
            }
            else
            {
                sevenSegment10.DisplayNumber(ten_place);
                ((RectTransform)sevenSegment1.transform).anchoredPosition = dis_vec;
            }
        }

        public void SetColor(Color color)
        {
            sevenSegment10.SetColor(color);
            sevenSegment1.SetColor(color);
        }
    }
}
