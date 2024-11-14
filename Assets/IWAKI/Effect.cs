using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    //[SerializeField, Tooltip("����������G�t�F�N�g(�p�[�e�B�N��)")]
    //private ParticleSystem particle;

    //[SerializeField, Tooltip("�G�t�F�N�g�𔭐�������Ώۂ̃I�u�W�F�N�g���X�g")]
    //private List<GameObject> targetObjects; // �C���X�y�N�^�[�Ŏw��\�ȃI�u�W�F�N�g���X�g

    ///// <summary>
    ///// �Փ˂�����
    ///// </summary>
    ///// <param name="collision"></param>
    //private void OnCollisionEnter(Collision collision)
    //{
    //    // �Փ˂����I�u�W�F�N�g���w�胊�X�g�Ɋ܂܂�Ă���ꍇ
    //    if (targetObjects.Contains(collision.gameObject))
    //    {
    //        // �Փ˒n�_���擾
    //        ContactPoint contact = collision.contacts[0];
    //        Vector3 hitPosition = contact.point;
    //        Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

    //        // �p�[�e�B�N���V�X�e���̃C���X�^���X�𐶐�
    //        ParticleSystem newParticle = Instantiate(particle);
    //        // �Փ˒n�_�Ƀp�[�e�B�N����z�u
    //        newParticle.transform.position = hitPosition;
    //        // �Փ˂̌����ɉ�]������
    //        newParticle.transform.rotation = hitRotation;
    //        // �p�[�e�B�N���V�X�e����GameObject�ɒǏ]�����邽�߂ɐe��ݒ肷��
    //        newParticle.transform.SetParent(this.transform);
    //        // �p�[�e�B�N���𔭐�������
    //        newParticle.Play();

    //        // ��莞�Ԍ�ɍ폜����
    //        Destroy(newParticle.gameObject, newParticle.main.duration);
    //    }
    //}

    [System.Serializable]
    public struct WeaponEffect
    {
        public GameObject weapon; // ����I�u�W�F�N�g
        public ParticleSystem particle; // ���̕���ɑΉ�����G�t�F�N�g
    }

    [SerializeField, Tooltip("���킲�ƂɈقȂ�G�t�F�N�g�̐ݒ�")]
    private List<WeaponEffect> weaponEffects = new List<WeaponEffect>();

    [SerializeField, Tooltip("�G�t�F�N�g�̏o���ʒu�����炷�I�t�Z�b�g")]
    private Vector3 offsetPosition;

    // Dictionary �ɕϊ����Č�����������
    private Dictionary<GameObject, ParticleSystem> effectDictionary;

    private void Awake()
    {
        // weaponEffects���X�g����Dictionary��������
        effectDictionary = new Dictionary<GameObject, ParticleSystem>();
        foreach (var weaponEffect in weaponEffects)
        {
            if (weaponEffect.weapon != null && weaponEffect.particle != null)
            {
                effectDictionary[weaponEffect.weapon] = weaponEffect.particle;
            }
        }
    }

    /// <summary>
    /// �G�t�F�N�g�𔭐������郁�\�b�h�iUnityEvent�Ŏg�p�j
    /// </summary>
    /// <param name="useHitPosition">true�Ȃ�Փˈʒu�Afalse�Ȃ�I�u�W�F�N�g�̒��S����G�t�F�N�g�𔭐�������</param>
    public void TriggerEffect(bool useHitPosition)
    {
        // �Փ˂�������ɑΉ�����G�t�F�N�g���ݒ肳��Ă��邩�m�F
        foreach (var entry in effectDictionary)
        {
            // �G�t�F�N�g�𔭐�����������𖞂������킪�����
            if (entry.Key.activeInHierarchy) // ����̕��킪�A�N�e�B�u�ł��邩�𔻒�
            {
                Vector3 effectPosition = useHitPosition ? entry.Key.transform.position + offsetPosition : transform.position + offsetPosition;
                Quaternion effectRotation = Quaternion.identity; // �C�ӂ̉�]

                // �p�[�e�B�N���V�X�e���̃C���X�^���X�𐶐�
                ParticleSystem newParticle = Instantiate(entry.Value);
                newParticle.transform.position = effectPosition;
                newParticle.transform.rotation = effectRotation;
                newParticle.transform.SetParent(this.transform);
                newParticle.Play();

                // ��莞�Ԍ�ɍ폜
                Destroy(newParticle.gameObject, newParticle.main.duration);

                // �G�t�F�N�g�𔭐���������I��
                break;
            }
        }
    }

}
