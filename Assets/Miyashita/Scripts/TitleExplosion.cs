using UnityEngine;

public class TitleExplosion : MonoBehaviour
{
    public GameObject titleText; // �^�C�g���̐e�I�u�W�F�N�g
    public float explosionForce = 500f; // ������
    public float explosionRadius = 5f; // �����͈̔�

    public void ExplodeTitle()
    {
        // �^�C�g���̊e������Rigidbody��ǉ����Ĕ���������
        foreach (Transform part in titleText.transform)
        {
            var rb = part.gameObject.AddComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, titleText.transform.position, explosionRadius);
        }
    }
}
