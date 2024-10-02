using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public enum Type { plane, slash, crash, pierce }

public class Breakable : MonoBehaviour
{
    [SerializeField, Tooltip("‘Ï‹v’l")]
    private int durability = default;
    [Header("‘®«‘Ï«")]
    [SerializeField, Tooltip("Ø’f‘Ï«")]
    private int slashResist = default;
    [SerializeField, Tooltip("ÕŒ‚‘Ï«"),]
    private int crashResist = default;
    [SerializeField, Tooltip("ŠÑ’Ê‘Ï«"),]
    private int pierceResist = default;
    [SerializeField, Tooltip("ƒXƒRƒA")]
    private int _score = default;

    // ‘®«‘Ï«‚Ì«‘
    private Dictionary<Type, int> resists = new Dictionary<Type, int>();
<<<<<<< HEAD
    // Œ‹‡‚µ‚Ä‚¢‚é‚Æ‚«‚ÌŒ‹‡‘Šè‚ÌBreakerƒNƒ‰ƒX
=======
    // Œ‹‡‚µ‚Ä‚¢‚é‚Æ‚«‚Ìe‚Ì‚ÌContainerƒNƒ‰ƒX
>>>>>>> FES-23-å£Šã•ã‚Œã‚‹å´ã‚ªãƒ–ã‚¸ã‚§ã‚¯ãƒˆã®ã‚³ãƒ³ãƒãƒ¼ãƒãƒ³ãƒˆã®åˆ¶ä½œ
    private Container container = null;

    [SerializeField]
    private float maxInterval = default;
    private float nowInterval = 0;
    private bool inInterval = false;

    private void Start()
    {
        resists.Add(Type.slash, slashResist);
        resists.Add(Type.crash, crashResist);
        resists.Add(Type.pierce, pierceResist);
        resists.Add(Type.plane, 0);
    }

    private void Update()
    {
        CalcInterval();
    }

    /// <summary>
    /// ˜A‘±‚ÅUŒ‚‚ğó‚¯‚È‚¢‚æ‚¤‚É‚·‚éƒCƒ“ƒ^[ƒoƒ‹
    /// </summary>
    private void CalcInterval()
    {
        if (inInterval)
        {
            nowInterval += Time.deltaTime;
            if (nowInterval > maxInterval)
            {
                nowInterval = 0;
                inInterval = false;
            }
        }
    }

    /// <summary>
    /// UŒ‚‚³‚ê‚½‚ÉŒÄ‚Ño‚·ƒƒ\ƒbƒhB
    /// </summary>
    /// <param name="receivedATK">ó‚¯‚éUŒ‚—Í</param>
    /// <param name="breaker">UŒ‚‚µ‚½‘¤‚Ìî•ñ</param>
    /// <returns></returns>
    public bool ReciveAttack(int receivedATK, Breaker breaker)
    {
        if (inInterval) return false;
        inInterval = true;

        int damage = CalcDamage(receivedATK, breaker.Type);
        Debug.Log($"damage: {damage}");
        durability -= damage;
        Debug.Log($"durability: {durability}");
        if (durability < 0)
        {
            Break(breaker);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ‘Ï‹v’l‚ª‚O‚É‚È‚è‰ó‚ê‚é‚Æ‚«‚Ìƒƒ\ƒbƒh
    /// </summary>
    /// <param name="breaker">`UŒ‚‚µ‚½‘¤‚Ìî•ñ</param>
    private void Break(Breaker breaker)
    {
        Debug.Log("Break");
        /*addScore(_score);*/
        if (container != null)
        {
<<<<<<< HEAD
            this.gameObject.transform.parent.gameObject.GetComponent<Container>().SetMainRegister();
=======
            container.SetMainRegister();
>>>>>>> FES-23-å£Šã•ã‚Œã‚‹å´ã‚ªãƒ–ã‚¸ã‚§ã‚¯ãƒˆã®ã‚³ãƒ³ãƒãƒ¼ãƒãƒ³ãƒˆã®åˆ¶ä½œ
        }
        switch (breaker.Type)
        {
            case Type.slash:
                // SlashƒNƒ‰ƒX‚ğŒÄ‚Ño‚·
                Debug.Log("Destroy! : " + this.gameObject);
                Destroy(this.gameObject);
                break;
            case Type.crash:
                Debug.Log("Destroy! : " + this.gameObject);
                // CrashƒNƒ‰ƒX‚ğŒÄ‚Ño‚·
                Debug.Log("Destroy! : " + this.gameObject);
                Destroy(this.gameObject);
                break;
            case Type.pierce:
                // PierceƒNƒ‰ƒX‚ğŒÄ‚Ño‚·
                container = breaker.GetContainer();
<<<<<<< HEAD
                durability = this.gameObject.GetComponent<Pierce>().Connect(breaker);
=======
                durability = this.gameObject.GetComponent<Pierce>().Connect(container);
>>>>>>> FES-23-å£Šã•ã‚Œã‚‹å´ã‚ªãƒ–ã‚¸ã‚§ã‚¯ãƒˆã®ã‚³ãƒ³ãƒãƒ¼ãƒãƒ³ãƒˆã®åˆ¶ä½œ
                break;
            default:
                break;
        }
    }


    /// <summary>
    /// —^‚¦‚ç‚ê‚½UŒ‚—Í‚Æ‘®«A©g‚Ì‘Ï«AÅI“I‚Èƒ_ƒ[ƒW‚Ì’l‚ğŒvZ‚·‚éB
    /// </summary>
    /// <param name="receivedATK">ó‚¯‚éUŒ‚—Í</param>
    /// <param name="attackType">ó‚¯‚éUŒ‚‚Ì‘®«</param>
    /// <returns></returns>
    private int CalcDamage(int receivedATK, Type attackType)
    {
        int damage = receivedATK - resists[attackType];
        if (damage < 0) damage = 0;
        return damage;
    }

}
