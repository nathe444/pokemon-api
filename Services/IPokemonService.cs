using System.Collections.Generic;
using PokemonApi.Models;
using System.Threading.Tasks;

namespace PokemonApi.Services
{
    public interface IPokemonService
    {
        Task<List<Pokemon>> GetAllPokemonsAsync();
        Task<Pokemon?> GetPokemonByIdAsync(int id);
        Task<List<Pokemon>> GetPokemonsByTypeAsync(string type);
        Task AddPokemonAsync(Pokemon pokemon);
        Task UpdatePokemonAsync(int id, Pokemon pokemon);
        Task DeletePokemonAsync(int id);
    }
}
