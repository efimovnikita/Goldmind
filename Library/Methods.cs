using Library.Models;

namespace Library
{
    public static class Methods
    {
        public static List<(int Number, Headlist Headlist)> GetOrderedList(List<Headlist> headlists)
        {
            IEnumerable<int> range = Enumerable.Range(1, headlists.Count).ToList();
            List<(int Number, Headlist Headlist)> orderedList = range.Zip(headlists).ToList();
            return orderedList;
        }
    }
}