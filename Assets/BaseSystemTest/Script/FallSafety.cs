using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallSafety : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.gameObject.transform.position += new Vector3(0, 2.0f, 0);
        collision.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
    }
}
