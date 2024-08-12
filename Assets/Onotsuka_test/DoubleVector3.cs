using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleVector3 : MonoBehaviour
{
    public double x, y, z;

    public DoubleVector3(Vector3 v) {
        this.x = (double)x;
        this.y = (double)y;
        this.z = (double)z;
    }
    public DoubleVector3(double x, double y, double z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 putBackVector3 {
        get{
            return new Vector3((float)x, (float)y, (float)z);
        }
    }

    public double sqrMagnitude {
        get{
            return x * x + y * y + z * z;
        }
    }
    public double magnitude {
        get{
            return System.Math.Sqrt(this.sqrMagnitude);
        }
    }
    public DoubleVector3 normalized {
        get { 
            return new DoubleVector3(x / this.magnitude, y / this.magnitude, z / this.magnitude); 
            }
    }
    public static DoubleVector3 zero {
        get { return new DoubleVector3(0, 0, 0); }
    }
    public static DoubleVector3 one {
        get { return new DoubleVector3(1, 1, 1); }
    }
    public static DoubleVector3 forward {
        get { return new DoubleVector3(0, 0, 1); }
    }
    public static DoubleVector3 back {
        get { return new DoubleVector3(0, 0, -1); }
    } 
    public static DoubleVector3 up {
        get { return new DoubleVector3(0, 1, 0); }
    }
    public static DoubleVector3 down {
        get { return new DoubleVector3(0, -1, 0); }
    }
    public static DoubleVector3 right {
        get { return new DoubleVector3(1, 0, 0); }
    }
    public static DoubleVector3 left {
        get { return new DoubleVector3(-1, 0, 0); }
    }
    public static DoubleVector3 Lerp(DoubleVector3 a, DoubleVector3 b, double t) {
        return new DoubleVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
    }
    public static double Dot(DoubleVector3 a, DoubleVector3 b) {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public static DoubleVector3 Cross(DoubleVector3 a, DoubleVector3 b) {
        return new DoubleVector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }
    public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b) {
        return new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    public static DoubleVector3 operator -(DoubleVector3 a, DoubleVector3 b) {
        return new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    public static DoubleVector3 operator *(DoubleVector3 a, double d) {
        return new DoubleVector3(a.x * d, a.y * d, a.z * d);
    }
    public static DoubleVector3 operator /(DoubleVector3 a, double d) {
        return new DoubleVector3(a.x / d, a.y / d, a.z / d);
    }
}
