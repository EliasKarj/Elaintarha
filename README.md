# Elaintarha


## Luokkakaavio

```mermaid
classDiagram
direction TB

class Animal {
  <<abstract>>
  - string _name
  - int _age
  + string Name
  + int Age
  + string Species
  + string MakeSound()
}

class IFeedable {
  <<interface>>
  + void Feed(string food)
}

class IFlyable {
  <<interface>>
  + void Fly()
}

class Lion {
  + bool IsAlpha
  + string Species
  + string MakeSound()
  + void Feed(string food)
}

class Parrot {
  + List<string> Vocabulary
  + string Species
  + string MakeSound()
  + void Feed(string food)
  + void Fly()
}

class Snake {
  + bool IsVenomous
  + string Species
  + string MakeSound()
  + void Feed(string food)
}

class IAnimalRepository {
  <<interface>>
  + Task<List<Animal>> LoadAsync()
  + Task SaveAsync(IEnumerable<Animal> animals)
}

class JsonAnimalRepository {
  - string _path
  + JsonAnimalRepository(string path)
  + Task<List<Animal>> LoadAsync()
  + Task SaveAsync(IEnumerable<Animal> animals)
}

class ZooService {
  - IAnimalRepository _repo
  - List<Animal> _animals
  + IReadOnlyList<Animal> Animals
  + void AddAnimal(Animal a)
  + IEnumerable<string> MakeAllSounds()
  + Task SaveAsync()
  + Task LoadAsync()
}

class ConsoleUi {
  + Task RunAsync()
}

class Program {
  + Task Main()
}

Animal <|-- Lion
Animal <|-- Parrot
Animal <|-- Snake

IFeedable <|.. Lion
IFeedable <|.. Parrot
IFeedable <|.. Snake

IFlyable  <|.. Parrot

IAnimalRepository <|.. JsonAnimalRepository

ZooService --> IAnimalRepository : depends on
ZooService o--> Animal : manages *
ConsoleUi  --> ZooService
Program    --> ZooService
