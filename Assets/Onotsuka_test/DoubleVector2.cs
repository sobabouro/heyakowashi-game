using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleVector2
{
    public double x, y;

    public DoubleVector2(Vector2 v) {
        this.x = (double)v.x;
        this.y = (double)v.y;
    }
    public DoubleVector2(double x, double y) {
        this.x = x;
        this.y = y;
    }

    public Vector2 putBackVector2 {
        get{
            return new Vector2((float)x, (float)y);
        }
    }

    public override string ToString() {
        return $"({x}, {y})";
    }

    public double sqrMagnitude {
        get{
            return x * x + y * y;
        }
    }
    public double magnitude {
        get{
            return System.Math.Sqrt(this.sqrMagnitude);
        }
    }
    public DoubleVector2 normalized {
        get { 
            return new DoubleVector2(x / this.magnitude, y / this.magnitude); 
            }
    }
    public static DoubleVector2 zero {
        get { return new DoubleVector2(0, 0); }
    }
    public static DoubleVector2 one {
        get { return new DoubleVector2(1, 1); }
    }
    public static DoubleVector2 up {
        get { return new DoubleVector2(0, 1); }
    }
    public static DoubleVector2 down {
        get { return new DoubleVector2(0, -1); }
    }
    public static DoubleVector2 right {
        get { return new DoubleVector2(1, 0); }
    }
    public static DoubleVector2 left {
        get { return new DoubleVector2(-1, 0); }
    }
    public static DoubleVector2 Lerp(DoubleVector2 a, DoubleVector2 b, double t) {
        return new DoubleVector2(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }
    public static double Dot(DoubleVector2 a, DoubleVector2 b) {
        return a.x * b.x + a.y * b.y;
    }
    public static double Cross(DoubleVector2 a, DoubleVector2 b) {
        return a.x * b.y - a.y * b.x;
    }
    public static DoubleVector2 operator +(DoubleVector2 a, DoubleVector2 b) {
        return new DoubleVector2(a.x + b.x, a.y + b.y);
    }
    public static DoubleVector2 operator -(DoubleVector2 a, DoubleVector2 b) {
        return new DoubleVector2(a.x - b.x, a.y - b.y);
    }
    public static DoubleVector2 operator *(DoubleVector2 a, double d) {
        return new DoubleVector2(a.x * d, a.y * d);
    }
    public static DoubleVector2 operator /(DoubleVector2 a, double d) {
        return new DoubleVector2(a.x / d, a.y / d);
    }
}
