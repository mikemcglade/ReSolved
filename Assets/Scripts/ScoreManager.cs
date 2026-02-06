using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    int currentScore;

    // Start is called before the first frame update
    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
  
    }

   public void AddPoint()
   {
        currentScore++;
        scoreText.text = currentScore.ToString();
   }
}
