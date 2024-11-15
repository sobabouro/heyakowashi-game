using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayOneTimeParticle : MonoBehaviour
{
    [SerializeField]
    public Vector3 position;

    [SerializeField]
    public GameObject targetPositionFromObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play(ParticleSystem particleSystem)
    {
        if (particleSystem == null) return;

        if (targetPositionFromObject != null) position = targetPositionFromObject.transform.position;

        ParticleSystem newParticle = Instantiate(particleSystem);
        newParticle.transform.position = position;

        newParticle.Play();

        // àÍíËéûä‘å„Ç…çÌèú
        Destroy(newParticle.gameObject, newParticle.main.duration);
    }
}
