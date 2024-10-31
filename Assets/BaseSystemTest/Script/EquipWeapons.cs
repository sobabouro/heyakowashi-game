using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation;

public class EquipWeapons : MonoBehaviour
{
    [SerializeField]
    private GameObject container;

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
        if (Input.GetMouseButtonDown(0))
        {
            EquipWeapon();
        }
    }

    public void EquipWeapon()
    {
        if (isEquipWeapon)
        {
            // 既に武器を装備している場合の処理（武器を捨てる）
            foreach (Transform child in container.transform)
            {
                Transform hostTransform;
                Rigidbody rigidbody;
                if (originalParent.gameObject.GetComponent<Container>() == null)
                {
                    // Rigidbodyの追加と調整
                    rigidbody = child.gameObject.AddComponent<Rigidbody>();
                    rigidbody.useGravity = true;
                    rigidbody.mass = breakableObject_mass;

                    hostTransform = child.gameObject.transform;
                }
                else
                {
                    rigidbody = originalParent.GetComponent<Rigidbody>();

                    originalParent.position = child.position;
                    originalParent.rotation = child.rotation;
                    hostTransform = originalParent;
                }
                

                // HoloLens2での操作での座標移動の対象をcontainerにする
                if (child.gameObject.GetComponent<ObjectManipulator>() != null)
                {
                    child.gameObject.GetComponent<ObjectManipulator>().HostTransform = hostTransform;
                }

                // Breakerクラスに保存されるrigidbodyに登録
                child.gameObject.GetComponent<Breaker>().SetRigidbody(rigidbody);

                child.transform.SetParent(originalParent);
            }

            this.gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", originalColor);

            isEquipWeapon = false;
        }
        else
        {
            // 武器を装備していないときの処理（武器を装備する）
            if (weaponCollider == null) return;
            if (weaponCollider.gameObject.GetComponent<Breaker>() == null) return;

            // コンテナの子オブジェクトにされるrigidbodyの破棄
            Rigidbody rigidbody = weaponCollider.gameObject.GetComponent<Rigidbody>();
            Destroy(rigidbody);
            // 自身の親の設定
            originalParent = weaponCollider.gameObject.transform.parent;
            weaponCollider.gameObject.transform.SetParent(container.transform);
            // 座標の調整
            weaponCollider.gameObject.transform.position = container.transform.position;
            weaponCollider.gameObject.transform.rotation = container.transform.rotation;
            // Containerクラスの登録オブジェクトを自身にする
            container.GetComponent<Container>().SetRegisteredObject(weaponCollider.gameObject);

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
