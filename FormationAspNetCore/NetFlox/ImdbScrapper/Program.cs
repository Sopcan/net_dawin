using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NetFlox.DAL;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImdbScrapper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var topUri = new Uri(@"https://www.imdb.com/chart/top");
            //await ScrapTopMovies(topUri, @"..\..\movies.txt", @"..\..\persons.txt");
            var movies = ReadJsonMovies(@"..\..\movies.txt");
            var persons = ReadJsonPersons(@"..\..\persons.txt");


            await SeedDatabase(movies, persons);
            Console.WriteLine($"Database seeded.");
        }

        private static ImdbMovie[] ReadJsonMovies(string moviesFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamReader sr = new StreamReader(moviesFilePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<ImdbMovie[]>(reader);
            }
        }

        private static ImdbPerson[] ReadJsonPersons(string personsFilePath)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamReader sr = new StreamReader(personsFilePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<ImdbPerson[]>(reader);
            }
        }

        private static async Task ScrapTopMovies(Uri topUri, string moviesFilePath, string personsFilePath)
        {
            var movieUris = await GetTopMovies(topUri);
            Console.WriteLine($"Top downloaded.");

            var movies = await Task.WhenAll(movieUris.Select(uri => Task.Run(() => GetMovie(new Uri(uri)))));
            Console.WriteLine($"{movies.Length} movies downloaded.");

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(moviesFilePath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, movies);
            }

            var personUris = movies.SelectMany(m => m.Actor.Where(p => p.Type == "Person"))
                .Concat(movies.SelectMany(m => m.Creator.Where(p => p.Type == "Person")))
                .Concat(movies.SelectMany(m => m.Director.Where(p => p.Type == "Person")))
                .Select(p => p.Url)
                .Distinct()
                .Select(url => new Uri(topUri, url))
                .ToList();


            var personList = personUris.AsParallel().Select(uri => GetPerson(uri).GetAwaiter().GetResult()).ToList();

            using (StreamWriter sw = new StreamWriter(@"c:\dev\persons.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, personList);
            }
        }


        public static async Task SeedDatabase(ImdbMovie[] movies, ImdbPerson[] persons)
        {
            using (var entities = new NetFloxEntities())
            {
                await entities.Database.EnsureDeletedAsync();
                await entities.Database.EnsureCreatedAsync();

                entities.Utilisateurs.Add(new Utilisateur
                {
                    Nom = "Demo",
                    Prenom = "demo",
                    AdresseEmail = "demo@demo.com",
                    DateInscription = DateTime.Now,
                });

                var actorRole = new Role { Id = 1, Libelle = "Acteur" };
                var directorRole = new Role { Id = 2, Libelle = "Réalisateur" };
                var creatorRole = new Role { Id = 3, Libelle = "Créateur" };
                entities.Roles.Add(actorRole);
                entities.Roles.Add(directorRole);
                entities.Roles.Add(creatorRole);
                
                var celebrites = persons.ToDictionary(p => p.Url, p => new Celebrite
                {
                    Nom = p.Name,
                    UrlPhoto = p.Image?.ToString(),
                    DateNaissance = p.BirthDate,
                });
                entities.Celebrites.AddRange(celebrites.Values);
                foreach (var movie in movies)
                {
                    var film = new Film
                    {
                        Titre = movie.Name,
                        Description = movie.Description,
                        UrlAffiche = movie.Image?.ToString(),
                        Pays = movie.Countries.FirstOrDefault(),
                        Annee = movie.DatePublished.Year,
                    };
                    entities.Films.Add(film);
                    var actors = movie.Actor
                        ?.Where(p => celebrites.ContainsKey(p.Url))
                        .Select(p => new RoleCelebriteFilm { Film = film, Celebrite = celebrites[p.Url], Role = actorRole })
                        .ToList();
                    var directors = movie.Director
                        ?.Where(p => celebrites.ContainsKey(p.Url))
                        .Select(p => new RoleCelebriteFilm { Film = film, Celebrite = celebrites[p.Url], Role = directorRole })
                        .ToList();
                    var creators = movie.Creator
                        ?.Where(p => celebrites.ContainsKey(p.Url))
                        .Select(p => new RoleCelebriteFilm { Film = film, Celebrite = celebrites[p.Url], Role = creatorRole })
                        .ToList();
                    if (actors != null)
                    {
                        entities.RoleCelebriteFilms.AddRange(actors);
                    }
                    if (directors != null)
                    {
                        entities.RoleCelebriteFilms.AddRange(directors);
                    }
                    if (creators != null)
                    {
                        entities.RoleCelebriteFilms.AddRange(creators);
                    }
                }
                await entities.SaveChangesAsync();
            }
        }

        private static async Task<List<string>> GetTopMovies(Uri topUri)
        {
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(topUri);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var table = doc.DocumentNode.SelectSingleNode(@"//table[@data-caller-name = 'chart-top250movie']");
                var rows = table.SelectNodes("./tbody/tr");
                var uris = rows
                    .Select(row => row.SelectSingleNode(@"./td[@class = 'titleColumn']/a"))
                    .Select(link => link.GetAttributeValue("href", null))
                    .Select(href => new Uri(topUri, href).GetLeftPart(UriPartial.Path))
                    .ToList();
                return uris;
            }
        }

        private static async Task<ImdbMovie> GetMovie(Uri movieUri)
        {
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(movieUri);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var scriptNode = doc.DocumentNode.SelectSingleNode(@"//script[@type = 'application/ld+json']");
                var movie = JsonConvert.DeserializeObject<ImdbMovie>(scriptNode.InnerHtml);

                var countryNodes = doc.DocumentNode.SelectNodes(@"//div[@id = 'titleDetails']/div[contains(h4/text(), 'Country')]/a");
                movie.Countries = countryNodes?.Select(link => link.InnerText).ToList() ?? new List<string>();

                var castNodes = doc.DocumentNode.SelectNodes(@"//table[@class = 'cast_list']/tr");
                if (castNodes != null)
                {
                    var actors = new List<ImdbMovie.Person>();
                    foreach (var node in castNodes)
                    {
                        var personLink = node.SelectSingleNode(@"./td[2]/a");
                        if (personLink != null)
                        {
                            var personName = personLink.InnerText?.Trim();
                            var personHref = personLink.GetAttributeValue("href", null);
                            if (!string.IsNullOrEmpty(personName) && !string.IsNullOrWhiteSpace(personHref))
                            {
                                var characterName = node.SelectSingleNode(@"./td[@class = 'character']")?.InnerText?.Trim();
                                actors.Add(new ImdbMovie.Person
                                {
                                    Type = "Person",
                                    Name = personName,
                                    Url = new Uri(movieUri, personHref).AbsolutePath,
                                    CharacterName = characterName,
                                });
                            }
                        }
                    }
                    if (actors.Count > 1)
                    {
                        movie.Actor = actors;
                    }
                }
                return movie;
            }
        }

        private static async Task<ImdbPerson> GetPerson(Uri personUri)
        {
            using (var client = new HttpClient())
            {
                var html = await client.GetStringAsync(personUri);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var scriptNode = doc.DocumentNode.SelectSingleNode(@"//script[@type = 'application/ld+json']");
                var person = JsonConvert.DeserializeObject<ImdbPerson>(scriptNode.InnerHtml);
                Console.Out.WriteLineAsync(person.Name);
                return person;
            }
        }
    }


    public class ImdbMovie
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Url { get; set; }
        public string Name { get; set; }
        public Uri Image { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> Genre { get; set; }

        public string ContentRating { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Person>))]
        public List<Person> Actor { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Person>))]
        public List<Person> Director { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<Person>))]
        public List<Person> Creator { get; set; }

        public string Description { get; set; }


        public List<string> Countries { get; set; }

        public DateTime DatePublished { get; set; }
        public string Keywords { get; set; }

        public class Person
        {
            [JsonProperty("@type")]
            public string Type { get; set; }

            public string Url { get; set; }

            public string Name { get; set; }

            public string CharacterName { get; set; }
        }
    }

    public class ImdbPerson
    {
        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Url { get; set; }
        public string Name { get; set; }
        public Uri Image { get; set; }
        public string Description { get; set; }

        public DateTime BirthDate { get; set; }
    }
}

