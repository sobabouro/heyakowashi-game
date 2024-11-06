using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Grade : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private TMP_Text gradeText;

    [SerializeField]
    private int dRankBorder = 500;
    [SerializeField]
    private Color dColor;
    [SerializeField]
    private int cRankBorder = 1000;
    [SerializeField]
    private Color cColor;
    [SerializeField]
    private int bRankBorder = 1500;
    [SerializeField]
    private Color bColor;
    [SerializeField]
    private int aRankBorder = 2000;
    [SerializeField]
    private Color aColor;
    [SerializeField]
    private int sRankBorder = 2500;
    [SerializeField]
    private Color sColor;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowGrade(int userScore)
    {
        string grade = "";
        Color color;

        if (userScore < dRankBorder)
        {
            grade = "D";
            color = dColor;
        }else if (userScore < cRankBorder)
        {
            grade = "C";
            color = cColor;
        }
        else if (userScore < bRankBorder)
        {
            grade = "B";
            color = bColor;
        }
        else if (userScore < aRankBorder)
        {
            grade = "A";
            color = aColor;
        }
        else if (userScore < sRankBorder)
        {
            grade = "S";
            color = sColor;
        }
        else
        {
            grade = "?";
            color = new Color(1f, 1f, 1f, 0f);
        }

        gradeText.SetText(grade);
        gradeText.color = color;
    }
}
