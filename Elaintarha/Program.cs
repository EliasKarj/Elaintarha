using System;
using Elaintarha;

namespace Elaintarha
{
    abstract class  Animal
    {
        public string Name { get; private set; }
        public int Age { get; private set; }
        protected Animal(string name, int age)
        {
            Name = name;
            Age = age;
        }
        public abstract string MakeSound();
    }

    class Lion : Animal
    {
        public bool IsAlpha { get; private set; }
        public Lion(string name, int age, bool isAlpha) : base (name, age)
        {
            IsAlpha = isAlpha;
        }
        public override string MakeSound() => "Rawr";
    }
    class Parrot : Animal
    {
        public string[] Vocabulary { get; private set; }

        public Parrot(string name, int age, string[] vocabulary) : base(name, age)
        {
            Vocabulary = vocabulary;
        }
        public override string MakeSound() => "Sqweek";

        public string Speak(int index)
        {
            if (Vocabulary.Length == 0) return "(Hiljaa)";
            if (index < 0 || index >= Vocabulary.Length) return "(Ei osaa sanoa tätä)";
            return Vocabulary[index];
        }
    }

    class Program
    {
        static void Main()
        {
        Animal simba = new Lion("Simba", 5, true);
        Console.WriteLine($"{simba.Name} sanoo: {simba.MakeSound()}");
        }
    }
}