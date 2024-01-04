namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            //var result = RemoveBooks(db);

            //Console.WriteLine(result);
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {

            try
            {
                AgeRestriction ageRestriction = Enum.Parse<AgeRestriction>(command, true);

                string[] books = context.Books
                    .Where(b => b.AgeRestriction == ageRestriction)
                    .OrderBy(b => b.Title)
                    .Select(b => b.Title)
                    .ToArray();

                return string.Join(Environment.NewLine, books);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            string[] goldenBooks = context.Books
                .Where(b => b.Copies < 5000 && b.EditionType
                                            == EditionType.Gold)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, goldenBooks);
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .Select(b => new
                {
                    Price = b.Price.ToString("f2"),
                    b.Title
                })
                .ToArray();
            StringBuilder sb = new StringBuilder();
            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price}");
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            string[] books = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, books);
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            List<string> separatedInput = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.ToLower())
                .ToList();

            string[] books = context.Books
                .Where(b => b.BookCategories
                    .Any(bc => separatedInput.Contains(bc.Category.Name.ToLower())))
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, books);
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            //int day = int.Parse(date.Split('-')[0]);
            //int month = int.Parse(date.Split('-')[1]);
            //int year = int.Parse(date.Split('-')[2]);

            DateTime finalDate = DateTime.Parse(date);

            var books = context.Books
                .Where(b => b.ReleaseDate < finalDate)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    Price = b.Price.ToString("f2")
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName)
                .Select(a => new
                {
                    FullName = a.FirstName + " " + a.LastName
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var a in authors)
            {
                sb.AppendLine(a.FullName);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var bookTitles = context.Books
                .OrderBy(b => b.Title)
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, bookTitles);
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(b =>
                b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title,
                    AuthorName = b.Author.FirstName + " " + b.Author.LastName
                })
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var b in books)
            {
                sb.AppendLine($"{b.Title} ({b.AuthorName})");
            }

            return sb.ToString().TrimEnd();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(b => b.Title.Length > lengthCheck)
                .ToArray();

            return books.Count();
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(a => new
                {
                    FullNameAuthor = a.FirstName + " " + a.LastName,
                    BooksCopies = a.Books
                    .Sum(b => b.Copies)
                })
                .ToArray()
                .OrderByDescending(b => b.BooksCopies);

            StringBuilder sb = new StringBuilder();

            foreach (var a in authors)
            {

                sb.AppendLine($"{a.FullNameAuthor} - {a.BooksCopies}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categoriesBookProfit = context.Categories
                .Select(c => new
                {
                    TotalProfit = c.CategoryBooks
                    .Sum(cb => cb.Book.Copies * cb.Book.Price),
                    CategoryName = c.Name
                })
                .ToArray()
                .OrderByDescending(c => c.TotalProfit)
                .ThenBy(c => c.CategoryName);

            StringBuilder sb = new StringBuilder();

            foreach (var c in categoriesBookProfit)
            {
                sb.AppendLine($"{c.CategoryName} ${c.TotalProfit:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var books = context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    CategoryName = c.Name,
                    MostRecentBooks = c.CategoryBooks
                    .OrderByDescending(cb => cb.Book.ReleaseDate)
                    .Take(3)
                    .Select(cb => new
                    {
                        BookTitle = cb.Book.Title,
                        ReleaseDate = cb.Book.ReleaseDate.Value.Year
                    })
                    .ToArray()
                })
                .ToArray();

            foreach (var c in books)
            {
                sb.AppendLine($"--{c.CategoryName}");

                foreach (var b in c.MostRecentBooks)
                {
                    sb.AppendLine($"{b.BookTitle} ({b.ReleaseDate})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var booksToIncrease = context.Books
                .Where(b => b.ReleaseDate.Value.Year < 2010 &&
                            b.ReleaseDate.HasValue)
                .ToArray();

            foreach (var book in booksToIncrease)
            {
                book.Price += 5;
            }

            //context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Copies < 4200)
                .ToArray();

            var removedBooks = books.Count();

            context.RemoveRange(books);

            context.SaveChanges();

            return removedBooks;
        }
    }
}


