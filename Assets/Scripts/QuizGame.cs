using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizGame : MonoBehaviour
{
    public Image m_background;
    public Text m_text;
    public GameObject m_rightPrefab;
    public GameObject m_wrongPrefab;
    public AudioSource m_rightSound;
    public AudioSource m_wrongSound;
    public float m_tiltAng = 45.0f;
    public float m_resetAng = 30.0f;
    public int score;

    /// <summary>
    /// This is a single message to display
    /// </summary>
    public class QuizElement
    {
        // we only have a single string in here, but made it a class in case you ever want to expand
        public string text;
    }

    /// <summary>
    /// This is a whole list of QuizElements
    /// </summary>
    public class QuizList
    {
        // this is nothing but a list, but like QuizElement, the class allows us to expand easily
        public List<QuizElement> elements;
    }

    // The Quiz
    QuizList m_list;
    int m_listElement = -1;

    /// <summary>
    /// Bundle up all the potential input values
    /// These are "abstracted" away from the input mechanism
    /// This "decouples" the input commands from the input mechanism
    /// This makes it easy to create alternative input mechanisms (keyboard, joystick, AI, etc)
    /// </summary>
    class QuizInput
    {
        public bool up = false;
        public bool down = false;
        public bool center = true;
    }

    // the elements
    Color m_origBackgroundColor;
    Color m_origTextColor;
    Color m_rightBackgroundColor;
    string m_rightText;
    Color m_rightTextColor;
    Color m_wrongBackgroundColor;
    string m_wrongText;
    Color m_wrongTextColor;

    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;  // this keeps the phone app from going into sleep mode

        // remember the original color of the text and background
        m_origBackgroundColor = m_background.color;
        m_origTextColor = m_text.color;

        {   // remember the colors for the "correct" response
            Image image = m_rightPrefab.GetComponentInChildren<Image>();
            if (image)
            {
                m_rightBackgroundColor = image.color;
            }
            Text text = m_rightPrefab.GetComponentInChildren<Text>();
            if (text)
            {
                m_rightText = text.text;
                m_rightTextColor = text.color;
            }
        }
        {   // remember the colors for the "wrong" response
            Image image = m_wrongPrefab.GetComponentInChildren<Image>();
            if (image)
            {
                m_wrongBackgroundColor = image.color;
            }
            Text text = m_wrongPrefab.GetComponentInChildren<Text>();
            if (text)
            {
                m_wrongText = text.text;
                m_wrongTextColor = text.color;
            }
        }

        {   // TODO load in the TextAsset for the specified "file"
            var textfile = Resources.Load<TextAsset>(GameManager.GetXMLFile());
            if (textfile != null){
                StringReader textRead = new StringReader(textfile.text);
                XmlSerializer seralizer = new XmlSerializer(typeof(QuizList));
                m_list = seralizer.Deserialize(textRead) as QuizList;
                Resources.UnloadAsset(textfile);
            }
        }

        // begin the game
        StartCoroutine(RunGame());

#if false   //create example quizlist
            //it can be hard to get the xml format correct, so it's useful to have Unity make one for you the first time.
        {
            string[] s_gameNames =
            {
                "Tony Hawk",
                "Zelda",
                "GTA",
                "Mario",
                "Uncharted",
                "GoldenEye",
                "BioShock",
                "Half-Life",
                "Halo",
                "Pac-Man"
            };
            m_list = new QuizList();
            m_list.elements = new List<QuizElement>();
            foreach (string title in s_gameNames)
            {
                QuizElement element = new QuizElement();
                element.text = title;
                m_list.elements.Add(element);
            }
            XmlSerializer xmls = new XmlSerializer(typeof(QuizList));
            StreamWriter sw = new StreamWriter(Application.dataPath + "/Resources/Games.xml");
            xmls.Serialize(sw, m_list);
            sw.Close();
        }
#endif
    }

    float ReadAngle()
    {
        float ang = 0.0f;
        {   // TODO read the accelerometer, and turn that into an angle
            if (Input.acceleration.magnitude != 0){
                ang = Mathf.Asin(Input.acceleration.z / Input.acceleration.magnitude) * Mathf.Rad2Deg;
            }
        }
        return ang;
    }

    // void Update(){
    //     QuizInput input = ReadInput();
    //     if (input.up == true){
    //         m_text.text = "up";
    //     }
    //     if (input.down == true){
    //         m_text.text = "down";
    //     }
    //     if (input.center == true){
    //         m_text.text = "center";
    //     }
    // }


    QuizInput ReadInput()
    {
        QuizInput quizInput = new QuizInput();
        {   // TODO use ReadAngle and input.GetKey() to fill in quizInput
            float angle = ReadAngle();
            if (angle > m_tiltAng){
                quizInput.down = true;
            }
            else {
                quizInput.down = false;
            }

            if (angle < -m_tiltAng){
                quizInput.up = true;
            }
            else {
                quizInput.up = false;
            }

            if (angle >= -m_resetAng && angle <= m_resetAng){
                quizInput.center = true;
            }
            else {
                quizInput.center = false;
            }
        }
        return quizInput;
    }

    IEnumerator RunGame()
    {
        
        {   // TODO first wait for the phone to be upright
            score = 0;
            while(true){
                QuizInput input = ReadInput();
                if (input.center == false){
                    yield return null;
                }
                else if (input.center == true){
                    break;
                }
            }
        }
        Shuffle();

        // Reset to the beginning of the list
        m_listElement = -1;
        bool isDone = NextQuestion();

        while (false == isDone)
        {
            {   // TODO wait for a choice (up or down)
                // then wait for 2 seconds
                // then wait for the phone to be re-centered
                // finally advance to the next question by calling NextQuestion()

                QuizInput input = ReadInput();

                if (input.up == true){
                    Correct();
                    yield return new WaitForSecondsRealtime(2.0f);
                    while(true){
                        if (ReadInput().center == false){
                            yield return null;
                        }
                        else if (ReadInput().center == true){
                            isDone = NextQuestion();
                            break;
                        }
                    }
                }
                else if (input.down == true){
                    Wrong();
                    yield return new WaitForSecondsRealtime(2.0f);
                    while(true){
                        if (ReadInput().center == false){
                            yield return null;
                        }
                        else if (ReadInput().center == true){
                            isDone = NextQuestion();
                            break;
                        }
                    }
                }
                else {
                    yield return null;
                }
            }
        }

        // We are done... move on to the wrap-up screen

        GameManager.updateScore(score);


        SceneManager.LoadScene("End");
    }

    void Shuffle(){
        int n = m_list.elements.Count;
        var rnd = new System.Random();
        while (n != 0){
            int num = rnd.Next(0,n);
            QuizElement e1= m_list.elements[num];
            QuizElement e2= m_list.elements[m_list.elements.Count - 1];
            m_list.elements[num] = e2;
            m_list.elements[m_list.elements.Count - 1] = e1;
            n--;
        }
    }

    void Correct()
    {
        score += 2; 
        m_text.text = m_rightText;
        m_text.color = m_rightTextColor;
        m_background.color = m_rightBackgroundColor;
        if (null != m_rightSound)
        {
            m_rightSound.Play();
        }
    }

    void Wrong()
    {
        score--;
        m_text.text = m_wrongText;
        m_text.color = m_wrongTextColor;
        m_background.color = m_wrongBackgroundColor;
        if (null != m_wrongSound)
        {
            m_wrongSound.Play();
        }
    }

    /// <summary>
    /// Advance to the next question.
    /// Return true if we are out of questions and false if there are still more questions
    /// </summary>
    /// <returns>true if we are out of questions</returns>
    bool NextQuestion()
    {
        bool ret = true;

        {
            m_listElement++; 
            if (m_listElement < m_list.elements.Count){
                m_text.text = m_list.elements[m_listElement].text;
                ret = false;
            }
        }

        // reset the colors to the original colors for the question
        m_text.color = m_origTextColor;
        m_background.color = m_origBackgroundColor;
        return ret;
    }
}
