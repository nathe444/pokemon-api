using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PokemonApi.Models;

namespace PokemonApi.Services
{
    public class PokemonService : IPokemonService
    {
        private readonly IMongoCollection<Pokemon> _pokemonCollection;
        private readonly ILogger<PokemonService> _logger;

        public PokemonService(IConfiguration configuration, ILogger<PokemonService> logger)
        {
            _logger = logger;

            try 
            {
                var connectionString = configuration["MongoDB:ConnectionString"];
                var databaseName = configuration["MongoDB:DatabaseName"];

                _logger.LogInformation($"Attempting to connect to MongoDB at {connectionString}");

                var mongoClient = new MongoClient(connectionString);
                var mongoDatabase = mongoClient.GetDatabase(databaseName);

                _pokemonCollection = mongoDatabase.GetCollection<Pokemon>("Pokemons");

                // Test connection
                var pingCommand = new BsonDocument("ping", 1);
                var pingResult = mongoDatabase.RunCommand<BsonDocument>(pingCommand);
                
                _logger.LogInformation($"Successfully connected to MongoDB database: {databaseName}");

                // Seed data if collection is empty
                if (_pokemonCollection.CountDocuments(FilterDefinition<Pokemon>.Empty) == 0)
                {
                    _logger.LogInformation("Pokemon collection is empty. Seeding initial data.");
                    SeedInitialData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MongoDB database");
                throw;
            }
        }

        private void SeedInitialData()
        {
            var initialPokemons = new List<Pokemon>
            {
                new Pokemon { Name = "Pikachu", Type = "Electric", Ability = "speed", Level = 2 },
                new Pokemon { Name = "Charmander", Type = "Fire", Ability = "speed", Level = 2 },
                new Pokemon { Name = "Bulbasaur", Type = "Grass", Ability = "speed", Level = 2 }
            };

            _pokemonCollection.InsertMany(initialPokemons);
            _logger.LogInformation($"Inserted {initialPokemons.Count} initial Pokemon records");
        }

        public async Task<List<Pokemon>> GetAllPokemonsAsync()
        {
            try
            {
                var pokemons = await _pokemonCollection.Find(_ => true).ToListAsync();
                _logger.LogInformation($"Retrieved {pokemons.Count} Pokemon records");
                return pokemons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all Pokemon records");
                return new List<Pokemon>();
            }
        }

        public async Task<Pokemon?> GetPokemonByIdAsync(int id)
        {
            try
            {
                var pokemon = await _pokemonCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (pokemon != null)
                {
                    _logger.LogInformation($"Retrieved Pokemon record with id {id}");
                }
                else
                {
                    _logger.LogInformation($"Pokemon record with id {id} not found");
                }
                return pokemon;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve Pokemon record by id {id}");
                return null;
            }
        }

        public async Task<List<Pokemon>> GetPokemonsByTypeAsync(string type)
        {
            try
            {
                var pokemons = await _pokemonCollection.Find(p => 
                    !string.IsNullOrEmpty(p.Type) && 
                    p.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToListAsync();
                _logger.LogInformation($"Retrieved {pokemons.Count} Pokemon records of type {type}");
                return pokemons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve Pokemon records by type");
                return new List<Pokemon>();
            }
        }

        public async Task AddPokemonAsync(Pokemon pokemon)
        {
            try
            {
                // Generate a unique Id for the new Pokemon
                var maxIdPokemon = await _pokemonCollection.Find(FilterDefinition<Pokemon>.Empty)
                    .SortByDescending(p => p.Id)
                    .FirstOrDefaultAsync();
                pokemon.Id = (maxIdPokemon?.Id ?? 0) + 1;

                await _pokemonCollection.InsertOneAsync(pokemon);
                _logger.LogInformation($"Inserted new Pokemon record with id {pokemon.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add new Pokemon record");
                throw;
            }
        }

        public async Task UpdatePokemonAsync(int id, Pokemon pokemon)
        {
            try
            {
                var filter = Builders<Pokemon>.Filter.Eq(p => p.Id, id);
                var updateBuilder = Builders<Pokemon>.Update;
                var updates = new List<UpdateDefinition<Pokemon>>();

                if (!string.IsNullOrEmpty(pokemon.Name))
                    updates.Add(updateBuilder.Set(p => p.Name, pokemon.Name));

                if (!string.IsNullOrEmpty(pokemon.Type))
                    updates.Add(updateBuilder.Set(p => p.Type, pokemon.Type));

                if (!string.IsNullOrEmpty(pokemon.Ability))
                    updates.Add(updateBuilder.Set(p => p.Ability, pokemon.Ability));

                if (pokemon.Level > 0)
                    updates.Add(updateBuilder.Set(p => p.Level, pokemon.Level));

                if (updates.Any())
                {
                    var combinedUpdate = updateBuilder.Combine(updates);
                    await _pokemonCollection.UpdateOneAsync(filter, combinedUpdate);
                    _logger.LogInformation($"Updated Pokemon record with id {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Pokemon record");
                throw;
            }
        }

        public async Task DeletePokemonAsync(int id)
        {
            try
            {
                await _pokemonCollection.DeleteOneAsync(p => p.Id == id);
                _logger.LogInformation($"Deleted Pokemon record with id {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Pokemon record");
                throw;
            }
        }
    }
}
