using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class showScore : MonoBehaviour
{
    public int score;
    public Text textElement;
    // Start is called before the first frame update
    void Start()
    {
        score = GameManager.final_score;
        textElement.text = "Score: " + score.ToString(); 
    }

    // Update is called once per frame
    void Update()
    {
        score = GameManager.final_score;
        textElement.text = "Score: " + score.ToString(); 
    }
}
