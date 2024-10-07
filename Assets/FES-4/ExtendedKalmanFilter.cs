using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedKalmanFilter
{
    private double[] x_k = { 0, 0, 0 };

    private Matrix P_k = new Matrix(new double[3, 3] { 
        { 0, 0, 0 }, 
        { 0, 0, 0 }, 
        { 0, 0, 0 } 
    });

    private Matrix H = new Matrix(new double[2, 3] { { 1, 0, 0 }, { 0, 1, 0 } });

    /// <summary>
    /// èÛë‘ï˚íˆéÆÇÃf
    /// </summary>
    private double[] f(double[] x, double[] u)
    {
        double[] x_k = new double[3]; 
        double u_x = u[0];
        double u_y = u[1];
        double u_z = u[2];
        double sx0 = Math.Sin(x[0]);
        double sx1 = Math.Sin(x[1]);
        double cx0 = Math.Cos(x[0]);
        double cx1 = Math.Cos(x[1]);
        x_k[0] = x[0] + u_x + u_y * sx0 * sx1 / cx1 + u_z * cx0 * sx1 / cx1;
        x_k[1] = x[1] + u_y * cx0 - u_z * sx0;
        x_k[2] = x[1] + u_y * sx0 / cx1 - u_z * cx0 / cx1;

        return x_k;
    }

    private double[] h(double[] x)
    {
        return new double[2] { x[0], x[1] };
    }

    private double[] Predict_x(double[] x, double[] u)
    {
        return f(x, u);
    }
    private Matrix Predict_P(Matrix P, Matrix F, Matrix Q)
    {
        return F * P * F.transpose + Q;
    }

    private Matrix Calc_F(double[] x, double[] u)
    {
        double u_y = u[1];
        double u_z = u[2];
        double sx0 = Math.Sin(x[0]);
        double sx1 = Math.Sin(x[1]);
        double cx0 = Math.Cos(x[0]);
        double cx1 = Math.Cos(x[1]);
        Matrix F = new Matrix ( new double[3, 3] {
                {1 + u_y * cx0 * sx1 / sx1 - u_z * sx0 * sx1 / cx1, u_y * sx0 / (cx1 * cx1) + u_z * cx0 / (cx1 * cx1), 0 },
                {-u_y * sx0 - u_z * cx0, 1, 0 },
                {u_y * cx0 / cx1 - u_z * sx0 / cx1, u_y * sx0 * sx1 / (cx1 * cx1) + u_z * cx0 * sx1 / (cx1 * cx1), 1 }
        } );
        return F;
    }


    private Matrix Calc_H()
    {
        return H;
    }
    
    private double[] Update_y_res(double[] z, double[] x)
    {
        return new double[] { z[0] - h(x)[0], z[1] - h(x)[1] };
    }

    private Matrix Update_S(Matrix P, Matrix H, Matrix R) {
        Matrix S = H * P * H.transpose + R;
        return S; // 2Å~2çsóÒ
    }

    private Matrix Update_K(Matrix P, Matrix H, Matrix S) {
        Matrix K = P * H.transpose * S.inverse;
        return K; // 3Å~2çsóÒ
    }

    private double[] Update_x(double[] x, double[] y_res, Matrix K)
    {
        double[] d =  K * y_res;
        return new double[3] { x[0] + d[0], x[1] + d[1], x[2] + d[2] };
    }

    private Matrix Update_P(Matrix P, Matrix H, Matrix K)
    {
        Matrix I = Matrix.Identity(3);
        P = (I - K * H) * P;
        return P;
    }

    public double[] ekf(double[] u, double[] z, Matrix R, Matrix Q)
    {
        // Predict
        Matrix F = Calc_F(x_k, u);
        double[] predict_x = Predict_x(x_k, u);
        Matrix H = Calc_H();
        Matrix predict_P = Predict_P(P_k, F, Q);

        // Update
        double[] y_res = Update_y_res(z, predict_x);
        Matrix S = Update_S(predict_P, H, R);
        Matrix K = Update_K(predict_P, H, S);
        x_k = Update_x(predict_x, y_res, K);
        P_k = Update_P(predict_P, H, K);
        return x_k;
    }


}


