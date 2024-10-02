using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class Pierce : MonoBehaviour
{
    [SerializeField]
    private int durabilityRecoveryAmount;

    // Žh“Ë‘®«‚É‚æ‚éŒ‹‡‚ÌŠJŽn
    public int Connect(Container container)
    {
        // ƒRƒ“ƒeƒi‚ÌŽqƒIƒuƒWƒFƒNƒg‚É‚³‚ê‚érigidbody‚Ì”jŠü
        Rigidbody rigidbody = this.gameObject.GetComponent<Rigidbody>();
        Destroy(rigidbody);

        // Ž©g‚Ìe‚ðBreaker.container‚É‚·‚é
<<<<<<< HEAD
        this.gameObject.transform.SetParent(breaker.GetContainer().gameObject.transform);

        // ContainerƒNƒ‰ƒX‚Ì“o˜^ƒIƒuƒWƒFƒNƒg‚ðŽ©g‚É‚·‚é
        GameObject container = breaker.GetContainer().gameObject;
        container.GetComponent<Container>().SetRegisteredObject(this.gameObject);   

        // ‰ñ•œ‚·‚é‘Ï‹v’l‚ð•Ô‚·
        return durabilityRecoveryAmount; 
=======
        this.gameObject.transform.SetParent(container.gameObject.transform);

        // ContainerƒNƒ‰ƒX‚Ì“o˜^ƒIƒuƒWƒFƒNƒg‚ðŽ©g‚É‚·‚é
        container.SetRegisteredObject(this.gameObject);

        // ‰ñ•œ‚·‚é‘Ï‹v’l‚ð•Ô‚·
        return durabilityRecoveryAmount;
>>>>>>> FES-7-çªå±žæ€§ã«ã‚ˆã‚‹ã‚ªãƒ–ã‚¸ã‚§ã‚¯ãƒˆã®ç ´å£Šå‡¦ç†
    }

    // Œ‹‡‚·‚éÀ•W‚ÌÝ’è
    private void DecideConnectPosition()
    {

    }
}