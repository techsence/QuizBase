using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class QuizSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText; // UI text to display the question
    public List<Button> answerButtons; // List of buttons for answer choices
    public Button hintButton; // Button to use a hint
    public GameObject correctImage; // Image shown when answer is correct
    public Button nextButton; // Button to go to next question
    public GameObject answerPopup; // Popup shown after answering
    public GameObject hintPopup; // Popup shown when hint is used
    public TextMeshProUGUI scoreText; // UI text to display current score
    public TextMeshProUGUI coinText; // UI text to display current coin count
    public TextMeshProUGUI timeText; // UI text to display timer

    private List<QuizQuestion> questions; // List of all quiz questions
    private int currentQuestionIndex = 0; // Index of the current question
    private bool hintUsed = false; // Tracks if hint has been used
    private bool answered = false; // Tracks if current question is answered

    private int score = 0; // Player's score
    private int coins = 0; // Player's coin count
    private float timer = 0f; // Timer for each question

    void Start()
    {
        // Hide UI elements at start
        correctImage.SetActive(false);
        nextButton.gameObject.SetActive(false);
        answerPopup.SetActive(false);
        hintPopup.SetActive(false);

        // Load questions and show first question
        questions = LoadQuestionsFromCSV("questions");
        ShowQuestion();
    }

    void Update()
    {
        // Update the timer and time UI if not answered
        if (!answered)
        {
            timer += Time.deltaTime;
            timeText.text = "Time: " + timer.ToString("F1") + "s";
        }
    }

    void ShowQuestion()
    {
        // If no more questions, end quiz
        if (currentQuestionIndex >= questions.Count)
        {
            Debug.Log("Quiz Finished!");
            questionText.text = "Quiz Finished!";
            foreach (var btn in answerButtons)
                btn.gameObject.SetActive(false);

            hintButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);
            answerPopup.SetActive(false);
            hintPopup.SetActive(false);
            return;
        }

        // Reset states and UI for current question
        hintUsed = false;
        answered = false;
        correctImage.SetActive(false);
        nextButton.gameObject.SetActive(false);
        answerPopup.SetActive(false);
        hintPopup.SetActive(false);
        timer = 0f; // Reset timer

        var question = questions[currentQuestionIndex];
        questionText.text = question.question; // Display question text

        for (var i = 0; i < answerButtons.Count; i++)
        {
            var btn = answerButtons[i];
            btn.interactable = true;
            btn.gameObject.SetActive(i < question.answers.Length); // Hide unused buttons

            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = question.answers[i]; // Set button text

            var index = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnAnswerClicked(index)); // Assign answer click
        }

        // Set up hint button
        hintButton.interactable = true;
        hintButton.onClick.RemoveAllListeners();
        hintButton.onClick.AddListener(OnHintClicked);

        // Update score and coin UI
        scoreText.text = "Score: " + score;
        coinText.text = coins.ToString();
    }

    void OnAnswerClicked(int selectedIndex)
    {
        if (answered) return; // Prevent multiple answers

        answered = true;
        var question = questions[currentQuestionIndex];

        if (selectedIndex == question.correctAnswerIndex)
        {
            Debug.Log("Correct!");
            correctImage.SetActive(true); // Show correct mark
            score += 10; // Add score
            coins += 1; // Add coin
        }
        else
        {
            Debug.Log("Wrong!");
        }

        answerPopup.SetActive(true); // Show answer popup

        foreach (var btn in answerButtons)
            btn.interactable = false; // Disable buttons

        hintButton.interactable = false; // Disable hint

        // Setup next button
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextQuestion);
    }

    void OnHintClicked()
    {
        if (hintUsed) return; // Only allow one hint per question

        var question = questions[currentQuestionIndex];
        var wrongIndexes = new List<int>();

        // Collect wrong answer indices
        for (var i = 0; i < question.answers.Length; i++)
        {
            if (i != question.correctAnswerIndex)
                wrongIndexes.Add(i);
        }

        // Randomly disable one wrong answer
        var randomWrong = wrongIndexes[Random.Range(0, wrongIndexes.Count)];
        answerButtons[randomWrong].interactable = false;

        hintUsed = true;
        hintButton.interactable = false;
        hintPopup.SetActive(true); // Show hint popup
    }

    void NextQuestion()
    {
        currentQuestionIndex++; // Move to next question
        ShowQuestion();
    }

    List<QuizQuestion> LoadQuestionsFromCSV(string fileName)
    {
        var loadedQuestions = new List<QuizQuestion>();
        var csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV file not found!");
            return loadedQuestions;
        }

        var reader = new StringReader(csvFile.text);
        var isFirstLine = true;

        // Parse each line of the CSV
        while (reader.Peek() > -1)
        {
            var line = reader.ReadLine();
            if (isFirstLine) { isFirstLine = false; continue; }

            var values = line.Split(',');
            if (values.Length < 6) continue;

            var q = new QuizQuestion
            {
                question = values[0],
                answers = new string[] { values[1], values[2], values[3], values[4] },
                correctAnswerIndex = int.Parse(values[5])
            };

            loadedQuestions.Add(q);
        }

        return loadedQuestions;
    }

    [System.Serializable]
    public class QuizQuestion
    {
        public string question; // Question text
        public string[] answers; // Answer options
        public int correctAnswerIndex; // Index of the correct answer
    }
}
