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
            GUILayout.Label("Joy-Con ���ڑ�����Ă��܂���");
            return;
        }

        if (!m_joycons.Any(c => c.isLeft))
        {
            GUILayout.Label("Joy-Con (L) ���ڑ�����Ă��܂���");
            return;
        }

        if (!m_joycons.Any(c => !c.isLeft))
        {
            GUILayout.Label("Joy-Con (R) ���ڑ�����Ă��܂���");
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
            GUILayout.Label("������Ă���{�^���F" + pressedButton);
            //GUILayout.Label(string.Format("�X�e�B�b�N�F({0}, {1})", stick[0], stick[1]));

            GUILayout.Label("RAW�f�[�^");
            GUILayout.Label("�@�W���C���F" + gyr_r);
            GUILayout.Label("�@�����x�@�F" + accel_r);

            GUILayout.Label("�X�������߂邽�߂̃f�[�^(G)");
            GUILayout.Label("�@�W���C���F" + gyro_g);
            GUILayout.Label("�@�����x�@�F" + accel_g);

            GUILayout.Label("�X��");
            GUILayout.Label("�@�S�������@�F" + orientation);
            GUILayout.Label("�@�I�C���[�p�F" + euler);

            GUILayout.Label("���[���h���W��");
            GUILayout.Label("�@�����x�@�F" + accel_world);
            GUILayout.Label("�@�d�͉����x�F" + accel_gravity_world);
            GUILayout.Label("�@���I�����x�F" + accel_ac_world);
            GUILayout.Label("�@�����x�̑傫���F" + accel_world.magnitude);

            GUILayout.Label("���[���h���W��(m/s)");
            GUILayout.Label("�@���I�����x�F" + accel_ac_mps_world);
            GUILayout.Label("�@���x�@�F" + velocity_world);

            GUILayout.EndVertical();

        }

        GUILayout.EndHorizontal();
    }
}