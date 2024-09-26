using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    plane,
    slash,
    crash,
    pierce
}

public class Breaker : MonoBehaviour
{
    [SerializeField, Tooltip("基礎攻撃力")]
    private int baseATK = default;
    [SerializeField, Tooltip("属性")]
    private Type type = Type.plane;

    // 速度を取得するためのRigitbody
    [SerializeField]
    private Rigidbody my_Rigitbody;



    private int CalcATK(float other_velocity)
    {
        float velocity = my_Rigitbody.velocity.magnitude - other_velocity;
        if (velocity < velocity_threshold) velocity = 0;
        int finalATK = (int)(baseATK * my_Rigitbody.velocity.magnitude);
        return finalATK;
    }

}
