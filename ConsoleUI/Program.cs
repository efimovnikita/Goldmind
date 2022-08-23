using System.CommandLine;
using Library;
using Library.Models;
using LiteDB;

namespace ConsoleUI
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            Option<Mode> modeOption = new("--mode", () => Mode.Distillation, "Select app mode")
            {
                IsRequired = true
            };
            modeOption.AddAlias("-m");

            RootCommand rootCommand = new("Sample app for System.CommandLine");
            rootCommand.AddOption(modeOption);

            rootCommand.SetHandler(mode =>
            {
                if (mode is Mode.Add)
                {
                    RunAddMode();
                    return;
                }

                RunDistillationMode();
            },
            modeOption);

            return await rootCommand.InvokeAsync(args);
        }

        private static void RunDistillationMode()
        {
            using LiteDatabase db = new(Constants.DbPath);
            ILiteCollection<Headlist> collection = db.GetCollection<Headlist>(Constants.CollectionName);

            List<Headlist> headlists = collection.Query().ToList();
            Console.WriteLine($"Headlists count: {headlists.Count}");

            List<Headlist> completedHeadlists = headlists.Where(headlist => headlist.IsContainSuccessfulSetOfExpressions()).ToList();
            Console.WriteLine($"Completed headlists count: {completedHeadlists.Count}");

            List<Headlist> readyToDistillation = headlists.Where(headlist => headlist.IsReadyToDistillation()).ToList();

            if (readyToDistillation.Count == 0)
            {
                Console.WriteLine("Headlists ready for distillation not found");
                return;
            }

            List<(int Number, Headlist Headlist)> orderedList = Methods.GetOrderedList(readyToDistillation);

            foreach ((int Number, Headlist Headlist) valueTuple in orderedList)
            {
                Console.WriteLine($"{valueTuple.Number}) {valueTuple.Headlist.Name} (expressions: {valueTuple.Headlist.Expressions.Count})");
            }

            Console.WriteLine();

            Console.Write("Select headlist: ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();

            bool parseResult = Int32.TryParse(keyInfo.KeyChar.ToString(), out int inputNumber);

            if (parseResult == false)
            {
                Console.WriteLine("Incorrect input");
                return;
            }

            (int _, Headlist? selectedHeadlist) = orderedList.FirstOrDefault(tuple => tuple.Number.Equals(inputNumber));
            if (selectedHeadlist == null)
            {
                Console.WriteLine("Incorrect input");
                return;
            }

            List<WordExpression> expressions = selectedHeadlist.GetUnsuccessfulExpressions();

            for (int i = 0; i < expressions.Count; i++)
            {
                WordExpression expression = expressions[i];

                Console.Write($"{i + 1}) {expression.Meaning} - ");
                string? name = Console.ReadLine();
                if (String.IsNullOrEmpty(name))
                {
                    PrintFailMessage(expression);
                    continue;
                }

                if (name.Equals(expression.Name, StringComparison.OrdinalIgnoreCase) == false)
                {
                    PrintFailMessage(expression);
                    continue;
                }

                expression.Status = true;
            }

            foreach (WordExpression expression in selectedHeadlist.GetUnsuccessfulExpressions())
            {
                Console.WriteLine($"Please type '{expression.Name} - {expression.Meaning}'");
                Console.ReadLine();
            }

            selectedHeadlist.LastDistillation = DateTime.Now;
            collection.Update(selectedHeadlist);
        }

        private static void PrintFailMessage(WordExpression expression) => 
            Console.WriteLine($"Fail. Right answer is \"{expression.Name}\"");

        private static void RunAddMode()
        {
            using LiteDatabase db = new(Constants.DbPath);
            ILiteCollection<Headlist> collection = db.GetCollection<Headlist>(Constants.CollectionName);

            Headlist headlist = new() { Id = ObjectId.NewObjectId(), LastDistillation = DateTime.Now };

            ConsoleKey result = ConsoleKey.C;

            while (result.Equals(ConsoleKey.C))
            {
                Console.Write("Enter expression: ");
                string? name = Console.ReadLine();

                Console.Write("Enter meaning of expression: ");
                string? meaning = Console.ReadLine();

                if (String.IsNullOrEmpty(name) != true && String.IsNullOrEmpty(meaning) != true)
                {
                    headlist.AddExpression(new WordExpression { Name = name, Meaning = meaning});
                }

                Console.Write("Continue adding expressions (c) or exit (e): ");
                result = Console.ReadKey().Key;
                Console.WriteLine();
            }

            if (headlist.Expressions.Any())
            {
                collection.Insert(headlist);
            }
        }
    }

    public enum Mode
    {
        Add, Distillation
    }
}