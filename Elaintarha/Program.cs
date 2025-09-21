using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace ZooApp
{
    public interface IFeedable
    {
        void Feed(string food);
    }

    public interface IFlyable
    {
        void Fly();
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Lion), typeDiscriminator: "lion")]
    [JsonDerivedType(typeof(Parrot), typeDiscriminator: "parrot")]
    [JsonDerivedType(typeof(Snake), typeDiscriminator: "snake")]
    public abstract class Animal
    {
        private string _name;
        private int _age;

        public string Name
        {
            get => _name;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name is required.");
                _name = value.Trim();
            }
        }

        public int Age
        {
            get => _age;
            private set
            {
                if (value < 0)
                    throw new ArgumentException("Age must be >= 0.");
                _age = value;
            }
        }

        public abstract string Species { get; }
        public abstract string MakeSound();

        protected Animal(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public override string ToString() => $"{Species} '{Name}', age {Age}";
    }

    public sealed class Lion : Animal, IFeedable
    {
        public bool IsAlpha { get; private set; }

        public Lion(string name, int age, bool isAlpha) : base(name, age)
        {
            IsAlpha = isAlpha;
        }

        public override string Species => "Lion";
        public override string MakeSound() => "Roar";
        public void Feed(string food) => Console.WriteLine($"{Name} eats {food}.");
    }

    public sealed class Parrot : Animal, IFeedable, IFlyable
    {
        public List<string> Vocabulary { get; private set; }

        public Parrot(string name, int age, IEnumerable<string> vocabulary) : base(name, age)
        {
            Vocabulary = vocabulary?
                .Select(w => w.Trim())
                .Where(w => w.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
        }

        public override string Species => "Parrot";
        public override string MakeSound() => "Squawk";
        public void Feed(string food) => Console.WriteLine($"{Name} pecks {food}.");
        public void Fly() => Console.WriteLine($"{Name} flies around.");
        public string Speak(int index)
        {
            if (Vocabulary.Count == 0) return "(silence)";
            if (index < 0 || index >= Vocabulary.Count) return "(doesn't know this word)";
            return Vocabulary[index];
        }
    }

    public sealed class Snake : Animal, IFeedable
    {
        public bool IsVenomous { get; private set; }

        public Snake(string name, int age, bool isVenomous) : base(name, age)
        {
            IsVenomous = isVenomous;
        }

        public override string Species => "Snake";
        public override string MakeSound() => "Hiss";
        public void Feed(string food) => Console.WriteLine($"{Name} swallows {food}.");
    }

    public interface IAnimalRepository
    {
        Task<List<Animal>> LoadAsync();
        Task SaveAsync(IEnumerable<Animal> animals);
    }

    public sealed class JsonAnimalRepository : IAnimalRepository
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        public JsonAnimalRepository(string path)
        {
            _path = path;
        }

        public async Task<List<Animal>> LoadAsync()
        {
            if (!System.IO.File.Exists(_path)) return new List<Animal>();
            using var fs = System.IO.File.OpenRead(_path);
            var animals = await JsonSerializer.DeserializeAsync<List<Animal>>(fs, _options);
            return animals ?? new List<Animal>();
        }

        public async Task SaveAsync(IEnumerable<Animal> animals)
        {
            using var fs = System.IO.File.Create(_path);
            await JsonSerializer.SerializeAsync(fs, animals.ToList(), _options);
        }
    }

    public sealed class ZooService
    {
        private readonly IAnimalRepository _repo;
        private readonly List<Animal> _animals;

        public ZooService(IAnimalRepository repo, IEnumerable<Animal>? seed = null)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _animals = seed?.ToList() ?? new List<Animal>();
        }

        public IReadOnlyList<Animal> Animals => _animals;

        public void AddAnimal(Animal a)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            _animals.Add(a);
        }

        public IEnumerable<string> MakeAllSounds()
            => _animals.Select(a => $"{a.Species} {a.Name}: {a.MakeSound()}");

        public Task SaveAsync() => _repo.SaveAsync(_animals);

        public async Task LoadAsync()
        {
            _animals.Clear();
            _animals.AddRange(await _repo.LoadAsync());
        }
    }

    public static class ConsoleUi
    {
        public static async Task RunAsync()
        {
            var repo = new JsonAnimalRepository("animals.json");
            var zoo = new ZooService(repo);
            await zoo.LoadAsync();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nEläintarhan hallinta");
                Console.WriteLine("1) List animals");
                Console.WriteLine("2) Add lion");
                Console.WriteLine("3) Add parrot");
                Console.WriteLine("4) Add snake");
                Console.WriteLine("5) Make all sounds");
                Console.WriteLine("6) Save");
                Console.WriteLine("7) Load");
                Console.WriteLine("0) Exit");
                Console.Write("> ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        if (zoo.Animals.Count == 0)
                            Console.WriteLine("No animals.");
                        else
                            foreach (var a in zoo.Animals) Console.WriteLine(a);
                        break;

                    case "2":
                        Console.Write("Name: ");
                        var ln = Console.ReadLine() ?? "";
                        Console.Write("Age: ");
                        _ = int.TryParse(Console.ReadLine(), out var la);
                        Console.Write("Is alpha (y/n): ");
                        var laa = (Console.ReadLine() ?? "n")
                                  .StartsWith("y", StringComparison.OrdinalIgnoreCase);
                        zoo.AddAnimal(new Lion(ln, la, laa));
                        Console.WriteLine("Lion added.");
                        break;

                    case "3":
                        Console.Write("Name: ");
                        var pn = Console.ReadLine() ?? "";
                        Console.Write("Age: ");
                        _ = int.TryParse(Console.ReadLine(), out var pa);
                        Console.Write("Vocabulary (comma-separated): ");
                        var words = (Console.ReadLine() ?? "")
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        zoo.AddAnimal(new Parrot(pn, pa, words));
                        Console.WriteLine("Parrot added.");
                        break;

                    case "4":
                        Console.Write("Name: ");
                        var sn = Console.ReadLine() ?? "";
                        Console.Write("Age: ");
                        _ = int.TryParse(Console.ReadLine(), out var sa);
                        Console.Write("Is venomous (y/n): ");
                        var sv = (Console.ReadLine() ?? "n")
                                 .StartsWith("y", StringComparison.OrdinalIgnoreCase);
                        zoo.AddAnimal(new Snake(sn, sa, sv));
                        Console.WriteLine("Snake added.");
                        break;

                    case "5":
                        foreach (var s in zoo.MakeAllSounds()) Console.WriteLine(s);
                        break;

                    case "6":
                        await zoo.SaveAsync();
                        Console.WriteLine("Saved.");
                        break;

                    case "7":
                        await zoo.LoadAsync();
                        Console.WriteLine("Loaded.");
                        break;

                    case "0":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Unknown choice.");
                        break;
                }
            }
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            var animals = new List<Animal>
            {
                new Lion("Simba", 5, true),
                new Parrot("Polly", 2, new[]{ "Hello", "Cracker" }),
                new Snake("Nagini", 4, true)
            };

            foreach (var line in animals.Select(a => $"{a.Species} {a.Name}: {a.MakeSound()}"))
                Console.WriteLine(line);

            Console.WriteLine("\nLaunching console UI (optional). Press Ctrl+C to quit at any time.\n");

            await ConsoleUi.RunAsync();
        }
    }
}
