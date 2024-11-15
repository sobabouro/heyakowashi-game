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
    private Color originalColor;  // 元の色を保持
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
            // 既に武器を装備している場合の処理（武器を捨てる）
            
            // FixedJointの削除（連結の解除）
            Destroy(this.gameObject.GetComponent<FixedJoint>());

            this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", originalColor);

            isEquipWeapon = false;
        }
        else
        {
            // 武器を装備していないときの処理（武器を装備する）

            if (weaponCollider == null) return;
            if (weaponCollider.gameObject.GetComponent<Breaker>() == null) return;

            // 武器の指定
            equipWeapon = weaponCollider.gameObject;

            // オブジェクトの動きの依存対象の設定（連結の実行）
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

    public bool GetIsEquipWeapon()
    {
        return isEquipWeapon;
    }

}
