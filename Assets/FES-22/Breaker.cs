using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// breakable.cs ‚Å’è‹`‚·‚é
// public enum Type { plane, slash, crash, pierce }

public class Breaker : MonoBehaviour
{
    // ©•ª‚ÌeƒIƒuƒWƒFƒNƒg
    [SerializeField]
    private Container _container = null;

    [SerializeField, Tooltip("Šî‘bUŒ‚—Í")]
    private int _baseATK = default;
    [SerializeField, Tooltip("‘®«")]
    private Type _type = Type.plane;
    // ‘¬“x‚ğæ“¾‚·‚é‚½‚ß‚ÌRigidbody
    [SerializeField]
    private Rigidbody my_rigidbody;
    // ƒ_ƒ[ƒW‚ª”­¶‚·‚é‚½‚ß‚É•K—v‚ÈÅ’áŒÀ‚Ì‘¬“x
    [SerializeField]
    private float _velocity_threshold = 0;

    public Type Type { get { return _type; } }

    private void Start()
    {
<<<<<<< HEAD
        
    }

=======

    }
>>>>>>> FES-22-å£Šã™å´ã®ã‚ªãƒ–ã‚¸ã‚§ã‚¯ãƒˆã®ã‚³ãƒ³ãƒãƒ¼ãƒãƒ³ãƒˆã®åˆ¶ä½œ

    private int CalcATK(Vector3 other_velocity)
    {
        float velocity = (my_rigidbody.velocity - other_velocity).magnitude;
        if (velocity < _velocity_threshold) velocity = 0;
        int finalATK = (int)(_baseATK * velocity);
        return finalATK;
    }

    /// <summary>
    /// UŒ‚‚·‚éƒƒ\ƒbƒhBƒIƒuƒWƒFƒNƒg‚ÆÕ“Ë‚ÉŒÄ‚Ño‚·B
    /// </summary>
    /// <param name="collision">Õ“Ëƒf[ƒ^‘S”Ê</param>
    public void Attack(Collision collision)
    {
        Container container = collision.gameObject.GetComponent<Container>();
        Breakable breakable;
        if (container != null)
        {
            breakable = container.GetRegisteredObject().GetComponent<Breakable>();
        }
        else
        {
            breakable = collision.gameObject.GetComponent<Breakable>();
        }
        
        if (breakable == null) return;

        Rigidbody otherRigitbody = collision.gameObject.GetComponent<Rigidbody>();
        int finalATK = CalcATK(otherRigitbody.velocity);
        breakable.ReciveAttack(finalATK, this);

        Debug.Log("Attack! : " + this.gameObject + " to " + breakable + " : " + finalATK + " : " + otherRigitbody.velocity + " : " + my_rigidbody.velocity);
    }

    public Container GetContainer()
    {
        return _container;
    }

    public void SetRigidbody(Rigidbody rigidbody)
    {
        my_rigidbody = rigidbody;
    }
}
