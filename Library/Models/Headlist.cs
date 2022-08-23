using LiteDB;

namespace Library.Models
{
    public class Headlist
    {
        public ObjectId Id { get; set; }
        public string Name => $"Headlist ({LastDistillation})";
        public DateTime LastDistillation { get; set; }
        public List<WordExpression> Expressions { get; set; } = new();

        public bool IsReadyToDistillation() => DateTime.Now > LastDistillation.AddDays(Constants.DaysBetweenDistillations) && IsContainSuccessfulSetOfExpressions() == false;
        public bool IsContainSuccessfulSetOfExpressions() => Expressions.All(expression => expression.Status);

        public List<WordExpression> GetUnsuccessfulExpressions() =>
            Expressions.Where(expression => expression.Status == false).ToList();

        public void AddExpression(WordExpression expression)
        {
            if (Expressions.Count == Constants.ExpressionsPerHeadlist)
            {
                return;
            }

            if (Expressions.FirstOrDefault(e => e.Name.Equals(expression.Name, StringComparison.OrdinalIgnoreCase)) is null)
            {
                Expressions.Add(expression);
            }
        }
    }
}