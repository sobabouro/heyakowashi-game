using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamage : MonoBehaviour
{
    [SerializeField]
    private Image damageImage;
    [SerializeField]
    private Color damageColor;
    [SerializeField]
    private Color deadColor;

    private bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        damageImage.color = Color.clear;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        damageImage.color = Color.Lerp(damageImage.color, Color.clear, Time.deltaTime);
    }

    public void Damaged()
    {
        if (isDead) return;
        damageImage.color = damageColor;
    }

    public void Dead()
    {
        isDead = true;
        damageImage.color = deadColor;
    }
}
