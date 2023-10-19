using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mergepins
{
    public struct Matrix2x2 : IEquatable<Matrix2x2>
    {
        private float[][] elements;

        public Matrix2x2(float element00, float element01, float element10, float element11)
        {
            elements = new float[2][] { new float[2] { element00, element01 }, new float[2] { element10, element11 } };
        }

        public Matrix2x2(float[,] array)
        {
            elements = new float[2][] { new float[2] { array[0, 0], array[0, 1] }, new float[2] { array[1, 0], array[1, 1] } };
        }

        public Matrix2x2(Vector2 column0, Vector2 column1)
        {
            elements = new float[2][] { new float[2] { column0[0], column1[0] }, new float[2] { column0[1], column1[1] } };
        }

        public float this[int index]
        {
            get { return elements[index < 2 ? 0 : 1][index % 2]; }
            set { elements[index < 2 ? 0 : 1][index % 2] = value; }
        }

        public float this[int row, int column]
        {
            get { return elements[row % 2][column % 2]; }
            set { elements[row % 2][column % 2] = value; }
        }

        public static readonly Matrix2x2 E = new Matrix2x2(1, 0, 0, 1);
        public static readonly Matrix2x2 I = E;
        public static readonly Matrix2x2 O = new Matrix2x2(0, 0, 0, 0);
        public static readonly Matrix2x2 Rotation90 = new Matrix2x2(0, -1, 1, 0);

        public float Determinant()
        {
            return elements[0][0] * elements[1][1] - elements[0][1] * elements[1][0];
        }

        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(elements[0][0], elements[1][0], elements[0][1], elements[1][1]);
        }

        public bool Equals(Matrix2x2 other)
        {
            return (this[0] == other[0]) && (this[1] == other[1]) && (this[2] == other[2]) && (this[3] == other[3]);
        }

        public Vector2 GetColumn(int index)
        {
            index %= 2;
            return new Vector2(elements[0][index], elements[1][index]);
        }

        public Vector2 GetRow(int index)
        {
            index %= 2;
            return new Vector2(elements[index][0], elements[index][1]);
        }

        public Matrix2x2 Plus(Matrix2x2 other)
        {
            return new Matrix2x2(this[0] + other[0], this[1] + other[1], this[2] + other[2], this[3] + other[3]);
        }

        public Matrix2x2 Minus(Matrix2x2 other)
        {
            return new Matrix2x2(this[0] - other[0], this[1] - other[1], this[2] - other[2], this[3] - other[3]);
        }

        public Matrix2x2 Multiply(float alpha)
        {
            return new Matrix2x2(alpha * this[0], alpha * this[1], alpha * this[2], alpha * this[3]);
        }

        public Vector2 Multiply(Vector2 vector2)
        {
            return new Vector2(
            elements[0][0] * vector2[0] + elements[0][1] * vector2[1],
            elements[1][0] * vector2[0] + elements[1][1] * vector2[1]
            );
        }

        public Matrix2x2 Multiply(Matrix2x2 other)
        {
            return new Matrix2x2(
            elements[0][0] * other[0, 0] + elements[0][1] * other[1, 0],
            elements[0][0] * other[0, 1] + elements[0][1] * other[1, 1],
            elements[1][0] * other[0, 0] + elements[1][1] * other[1, 0],
            elements[1][0] * other[0, 1] + elements[1][1] * other[1, 1]
            );
        }

        public override string ToString()
        {
            return "Matrix2x2: [ [ " + this[0].ToString() + " , " + this[1].ToString() + " ] , [ " + this[2].ToString() + " , " + this[3].ToString() + " ] ]";
        }

        public static Matrix2x2 operator +(Matrix2x2 A, Matrix2x2 B) => A.Plus(B);
        public static Matrix2x2 operator -(Matrix2x2 A, Matrix2x2 B) => A.Minus(B);
        public static Matrix2x2 operator -(Matrix2x2 A) => O - A;
        public static Matrix2x2 operator *(float alpha, Matrix2x2 A) => A.Multiply(alpha);
        public static Matrix2x2 operator *(Matrix2x2 A, float alpha) => A.Multiply(alpha);
        public static Vector2 operator *(Matrix2x2 A, Vector2 a) => A.Multiply(a);
        public static Matrix2x2 operator *(Matrix2x2 A, Matrix2x2 B) => A.Multiply(B);
    }
}
