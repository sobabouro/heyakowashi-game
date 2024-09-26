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
    [SerializeField, Tooltip("Šî‘bUŒ‚—Í")]
    private int baseATK = default;
    [SerializeField, Tooltip("‘®«")]
    private Type type = Type.plane;

    // ‘¬“x‚ğæ“¾‚·‚é‚½‚ß‚ÌRigitbody
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
