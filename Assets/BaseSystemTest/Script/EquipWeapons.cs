using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class EquipWeapons : MonoBehaviour
{
    [SerializeField]
    private GameObject equipWeapon;
    private Collider weaponCollider = null;
    private bool isEquipWeapon = false;
    private int breakableObject_mass = 1000;
    private Color originalColor;  // ���̐F��ێ�
    private Transform originalParent;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = this.gameObject.GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            EquipWeapon();
        }
#endif
    }

    public void EquipWeapon()
    {
        if (isEquipWeapon)
        {
            // ���ɕ���𑕔����Ă���ꍇ�̏����i������̂Ă�j
            
            // FixedJoint�̍폜�i�A���̉����j
            Destroy(this.gameObject.GetComponent<FixedJoint>());

            this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", originalColor);

            isEquipWeapon = false;
        }
        else
        {
            // ����𑕔����Ă��Ȃ��Ƃ��̏����i����𑕔�����j

            if (weaponCollider == null) return;
            if (weaponCollider.gameObject.GetComponent<Breaker>() == null) return;

            // ����̎w��
            equipWeapon = weaponCollider.gameObject;

            // �I�u�W�F�N�g�̓����̈ˑ��Ώۂ̐ݒ�i�A���̎��s�j
            FixedJoint fixedJoint = this.gameObject.AddComponent<FixedJoint>();
            fixedJoint.connectedBody = equipWeapon.GetComponent<Rigidbody>();

            this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(originalColor.r, originalColor.g, originalColor.b, 0.0f));

            isEquipWeapon = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        weaponCollider = other;
    }

    private void OnTriggerExit(Collider other)
    {
        weaponCollider = null;
    }

}
