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

}
