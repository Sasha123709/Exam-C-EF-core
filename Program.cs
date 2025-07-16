
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;


class User
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Date { get; set; } 
}

class Question
{
    public string Text { get; set; }
    public List<string> Options { get; set; } = new List<string>();
    
    public List<int> CorrectAnswers { get; set; } = new List<int>();
}

class Quiz
{
    public string Category { get; set; } 
    public List<Question> Questions { get; set; } = new List<Question>();
}

class QuizResult
{
    public string Login { get; set; }
    public string QCategory { get; set; }
    public int Score { get; set; }
    public DateTime DateT { get; set; }
}


class Program
{
    const string UsersFile = "users.json";
    const string QuizzesFile = "quizzes.json";
    const string ResultsFile = "results.json";

   
    static Dictionary<string, User> users = new Dictionary<string, User>();
    static Dictionary<string, Quiz> quizzes = new Dictionary<string, Quiz>();
    static List<QuizResult> results = new List<QuizResult>();

    static void Main()
    {
        LoadData();

       
        if (!users.ContainsKey("admin"))
        {
            users["admin"] = new User { Login = "admin", Password = "admin", Date = "04.04.2011" };
            SaveUsers();
        }

        Console.WriteLine("Quizzes!");
        User User = null;
        
        while (User == null)
        {
            Console.WriteLine("\n1. Sing in");
            Console.WriteLine("2. Sing up");
            Console.Write("Your choice: ");
            string ch = Console.ReadLine();
            if (ch == "1")
                User = Login();
            else if (ch == "2")
                User = Register();
            else
                Console.WriteLine("Else.");
        }

       
        if (User.Login == "admin")
            AdminMenu(User);
        else
            UserMenu(User);
    }

    static void LoadData()
    {
        if (File.Exists(UsersFile))
        {
            string json = File.ReadAllText(UsersFile);
            users = JsonSerializer.Deserialize<Dictionary<string, User>>(json);
        }
        if (users == null)
            users = new Dictionary<string, User>();

        if (File.Exists(QuizzesFile))
        {
            string json = File.ReadAllText(QuizzesFile);
            quizzes = JsonSerializer.Deserialize<Dictionary<string, Quiz>>(json);
        }
        if (quizzes == null) quizzes = new Dictionary<string, Quiz>();

        if (File.Exists(ResultsFile))
        {
            string json = File.ReadAllText(ResultsFile);
            results = JsonSerializer.Deserialize<List<QuizResult>>(json);
        }
        if (results == null)
            results = new List<QuizResult>();
    }

    static void SaveUsers()
    {
        string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(UsersFile, json);
    }

    static void SaveQuizzes()
    {
        string json = JsonSerializer.Serialize(quizzes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(QuizzesFile, json);
    }

    static void SaveResults()
    {
        string json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ResultsFile, json);
    }



    static User Login()
    {
        Console.Write("Login: ");
        string login = Console.ReadLine();
        Console.Write("Password: ");
        string password = Console.ReadLine();

        if (users.ContainsKey(login))
        {
            if (users[login].Password == password)
                return users[login];
            else
            {
                Console.WriteLine("Fail password.");
                return null;
            }
        }
        else
        {
            Console.WriteLine("Error.");
            return null;
        }
    }

    static User Register()
    {
        Console.Write("Login: ");
        string login = Console.ReadLine();
        if (users.ContainsKey(login))
        {
            Console.WriteLine("Already done.");
            return null;
        }
        Console.Write("Password: ");
        string password = Console.ReadLine();
        Console.Write("Date of birthday: ");
        string db = Console.ReadLine();

        User newUser = new User { Login = login, Password = password, Date = db };
        users[login] = newUser;
        SaveUsers();
        Console.WriteLine("Sing up was succesful");
        return newUser;
    }

    static void UserMenu(User user)
    {
        while (true)
        {
            Console.WriteLine($"Welcome, {user.Login}!");
            Console.WriteLine("1. New quizis");
            Console.WriteLine("2. Show results last quizis");
            Console.WriteLine("3. Show TOP-20 Users from Quizis");
            Console.WriteLine("4. Chang settings");
            Console.WriteLine("0. Exit");
            Console.Write("Your choice: ");
            string ch = Console.ReadLine();
            if (ch == "1")
                StartQuiz(user);
            else if (ch == "2")
                ViewMyResults(user);
            else if (ch == "3")
                ViewTop20();
            else if (ch == "4")
                ChangeSettings(user);
            else if (ch == "0")
            {
                Console.WriteLine("Bye Bye!");
                break;
            }
            else
                Console.WriteLine("Error!!");
        }
    }

    public delegate void Send();

    public static event Send SendMessage;
    

    static void StartQuiz(User user)
    {
        SendMessage?.Invoke();
        if (quizzes.Count == 0)
        {
            Console.WriteLine("Error.");
            return;
        }

        Console.WriteLine("\nCategory of quizis:");
        int idx = 1;
        foreach (var quiz in quizzes.Values)
        {
            Console.WriteLine($"{idx}. {quiz.Category}");
            idx++;
        }
        Console.WriteLine($"{idx}. Random quizes (quations from different categoryes)");

        Console.Write("Your choice: ");
        string choice = Console.ReadLine();
        int choiceNum;
        if (!int.TryParse(choice, out choiceNum) || choiceNum < 1 || choiceNum > quizzes.Count + 1)
        {
            Console.WriteLine("Error.");
            return;
        }

        List<Question> quizQuestions = new List<Question>();
        string chosenCategory = "";

        if (choiceNum == quizzes.Count + 1)
        {
            
            foreach (var q in quizzes.Values)
                quizQuestions.AddRange(q.Questions);
            chosenCategory = "Random";
        }
        else
        {
            
            chosenCategory = quizzes.Values.ElementAt(choiceNum - 1).Category;
            quizQuestions = new List<Question>(quizzes.Values.ElementAt(choiceNum - 1).Questions);
        }

        if (quizQuestions.Count < 20)
        {
            Console.WriteLine("Less than 20 quetions.");
            return;
        }

        Random rnd = new Random();
        quizQuestions = quizQuestions.OrderBy(x => rnd.Next()).Take(20).ToList();

        int correctCount = 0;
        for (int i = 0; i < quizQuestions.Count; i++)
        {
            Question q = quizQuestions[i];
            Console.WriteLine($"Question {i + 1}: {q.Text}");
            for (int j = 0; j < q.Options.Count; j++)
                Console.WriteLine($"{j + 1}. {q.Options[j]}");

            Console.Write("Numder of correct answers: ");
            string answerInput = Console.ReadLine();
            var answers = answerInput.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => { int a; return int.TryParse(s, out a) ? a : 0; }).Where(a => a > 0).OrderBy(a => a).ToList();
            var correct = q.CorrectAnswers.OrderBy(a => a).ToList();
            if (answers.SequenceEqual(correct))
            {
                Console.WriteLine("Correct!");
                correctCount++;
            }
            else
                Console.WriteLine("Uncorrect!");
        }

        Console.WriteLine($"\nFinished. Your score is {correctCount}.");

        QuizResult res = new QuizResult
        {
            Login = user.Login,
            QCategory = chosenCategory,
            Score = correctCount,
            DateT = DateTime.Now
        };
        results.Add(res);
        SaveResults();

        var categoryResults = results.Where(r => r.QCategory == chosenCategory).OrderByDescending(r => r.Score).ThenBy(r => r.DateT).ToList();
        int rank = categoryResults.FindIndex(r => r == res) + 1;
        Console.WriteLine($"Your place in this quiz: {rank}");
    }

    static void ViewMyResults(User user)
    {
        var myResults = results.Where(r => r.Login == user.Login).OrderByDescending(r => r.DateT).ToList();
        if (myResults.Count == 0)
        {
            Console.WriteLine("Error.");
            return;
        }
        Console.WriteLine("\nYour results:");
        foreach (var r in myResults)
            Console.WriteLine($"{r.DateT}: Category - {r.QCategory}, Result - {r.Score} з 20");
    }

    static void ViewTop20()
    {
        Console.Write("Enter category: ");
        string category = Console.ReadLine();
        var catResults = results.Where(r => r.QCategory.Equals(category, StringComparison.OrdinalIgnoreCase)).OrderByDescending(r => r.Score).ThenBy(r => r.DateT).Take(20).ToList();
        if (catResults.Count == 0)
        {
            Console.WriteLine("Error.");
            return;
        }
        Console.WriteLine($"\nTOP-20 \"{category}\":");
        int pos = 1;
        foreach (var r in catResults)
        {
            Console.WriteLine($"{pos}. {r.Login} - {r.Score} from 20 ({r.DateT})");
            pos++;
        }
    }

    static void ChangeSettings(User user)
    {
        Console.WriteLine("\n1. Change password");
        Console.WriteLine("2. Change DOB");
        Console.Write("Your choice: ");
        string ch = Console.ReadLine();
        if (ch == "1")
        {
            Console.Write("Enter new password: ");
            user.Password = Console.ReadLine();
            SaveUsers();
            Console.WriteLine("Password was changed.");
        }
        else if (ch == "2")
        {
            Console.Write("Enter new DOB: ");
            user.Date = Console.ReadLine();
            SaveUsers();
            Console.WriteLine("DOB was changed.");
        }
        else
            Console.WriteLine("Error.");
    }



   
    static void AdminMenu(User admin)
    {
        while (true)
        {
            Console.WriteLine($"\nAdmin-Menu ({admin.Login}):");
            Console.WriteLine("1. Redact quizes");
            Console.WriteLine("2. Show users results");
            Console.WriteLine("0. Exit");
            Console.Write("Your choice: ");
            string ch = Console.ReadLine();
            if (ch == "1")
                AdminQuizMenu();
            else if (ch == "2")
            {
                
                if (results.Count == 0)
                    Console.WriteLine("Error.");
                else
                    foreach (var r in results)
                        Console.WriteLine($"{r.DateT}: {r.Login} - {r.QCategory} - {r.Score} from 20");
            }
            else if (ch == "0")
            {
                Console.WriteLine("Bye Bye!");
                break;
            }
            else
                Console.WriteLine("Error.");
        }
    }

    static void AdminQuizMenu()
    {
        while (true)
        {
            Console.WriteLine("\nMenu redact quizes:");
            Console.WriteLine("1. Create new quiz");
            Console.WriteLine("2. Add questions");
            Console.WriteLine("0. Back");
            Console.Write("Your choice: ");
            string ch = Console.ReadLine();
            if (ch == "1")
            {
                Console.Write("Enter name: ");
                string category = Console.ReadLine();
                if (quizzes.ContainsKey(category))
                    Console.WriteLine("Already done.");
                else
                {
                    quizzes[category] = new Quiz { Category = category };
                    SaveQuizzes();
                    Console.WriteLine("New quiz was created!");
                }
            }
            else if (ch == "2")
            {
                if (quizzes.Count == 0)
                {
                    Console.WriteLine("Error.");
                    continue;
                }
                Console.WriteLine("Category:");
                int idx = 1;
                foreach (var q in quizzes.Values)
                {
                    Console.WriteLine($"{idx}. {q.Category} (questions: {q.Questions.Count})");
                    idx++;
                }
                Console.Write("Number of category: ");
                string sel = Console.ReadLine();
                int selNum;
                if (!int.TryParse(sel, out selNum) || selNum < 1 || selNum > quizzes.Count)
                {
                    Console.WriteLine("Error.");
                    continue;
                }
                Quiz chosenQuiz = quizzes.Values.ElementAt(selNum - 1);
                AddQuestionToQuiz(chosenQuiz);
                SaveQuizzes();
            }
            else if (ch == "0")
                break;
            else
                Console.WriteLine("Error.");
        }
    }

    static void AddQuestionToQuiz(Quiz quiz)
    {
        Console.Write("Question text: ");
        string text = Console.ReadLine();
        Question q = new Question { Text = text };

        Console.Write("How many variants of correct answer? ");
        int optionsCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < optionsCount; i++)
        {
            Console.Write($"Enter variants of answer {i + 1}: ");
            q.Options.Add(Console.ReadLine());
        }
        Console.Write("How many correct? ");
        int correctCount = int.Parse(Console.ReadLine());
        Console.Write("Enter numbers of correct questions: ");
        var correct = Console.ReadLine().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => int.Parse(s)).ToList();
        q.CorrectAnswers = correct;
        quiz.Questions.Add(q);
        Console.WriteLine("Questions was added.");
    }

}
