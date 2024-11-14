using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JoyconViwe : MonoBehaviour
{
    private static readonly Joycon.Button[] m_buttons =
        Enum.GetValues(typeof(Joycon.Button)) as Joycon.Button[];

    private List<Joycon> m_joycons;

    private void Start()
    {
        m_joycons = JoyconManager.Instance.j;

        if (m_joycons == null || m_joycons.Count <= 0) return;
    }

    private void OnGUI()
    {
        var style = GUI.skin.GetStyle("label");
        style.fontSize = 24;

        if (m_joycons == null || m_joycons.Count <= 0)
        {
            GUILayout.Label("Joy-Con が接続されていません");
            return;
        }

        if (!m_joycons.Any(c => c.isLeft))
        {
            GUILayout.Label("Joy-Con (L) が接続されていません");
            return;
        }

        if (!m_joycons.Any(c => !c.isLeft))
        {
            GUILayout.Label("Joy-Con (R) が接続されていません");
            return;
        }

        GUILayout.BeginHorizontal(GUILayout.Width(1200));

        foreach (var joycon in m_joycons)
        {
            Joycon.Button? pressedButton = null;
            foreach (Joycon.Button button in m_buttons)
            {
                if (joycon.GetButton(button))
                {
                    pressedButton = button;
                }
            }

            var isLeft = joycon.isLeft;
            var name = isLeft ? "Joy-Con (L)" : "Joy-Con (R)";
            // var stick = joycon.GetStick();

            var gyr_r = joycon.GetGyroRaw();
            var accel_r = joycon.GetAccelRaw();

            var gyro_g = joycon.GetGyro();
            var accel_g = joycon.GetAccel();

            var orientation = joycon.GetVector();
            var euler = joycon.GetVector().eulerAngles;

            var accel_world = joycon.GetAccelRawInWorld();
            var accel_gravity_world = joycon.GetAccelGravityInWorld();
            var accel_ac_world = joycon.GetAccelACInWorld();

            var accel_ac_mps_world = joycon.GetAccelACmpsInWorld();
            var velocity_world = joycon.GetVelocityInWorld();


            GUILayout.BeginVertical(GUILayout.Width(600));
            GUILayout.Label(name);
            GUILayout.Label("押されているボタン：" + pressedButton);
            //GUILayout.Label(string.Format("スティック：({0}, {1})", stick[0], stick[1]));

            GUILayout.Label("RAWデータ");
            GUILayout.Label("　ジャイロ：" + gyr_r);
            GUILayout.Label("　加速度　：" + accel_r);

            GUILayout.Label("傾きを求めるためのデータ(G)");
            GUILayout.Label("　ジャイロ：" + gyro_g);
            GUILayout.Label("　加速度　：" + accel_g);

            GUILayout.Label("傾き");
            GUILayout.Label("　４次元数　：" + orientation);
            GUILayout.Label("　オイラー角：" + euler);

            GUILayout.Label("ワールド座標軸");
            GUILayout.Label("　加速度　：" + accel_world);
            GUILayout.Label("　重力加速度：" + accel_gravity_world);
            GUILayout.Label("　動的加速度：" + accel_ac_world);
            GUILayout.Label("　加速度の大きさ：" + accel_world.magnitude);

            GUILayout.Label("ワールド座標軸(m/s)");
            GUILayout.Label("　動的加速度：" + accel_ac_mps_world);
            GUILayout.Label("　速度　：" + velocity_world);

            GUILayout.EndVertical();

        }

        GUILayout.EndHorizontal();
    }
}