using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokemonApi.Services;
using PokemonApi.Models;

namespace PokemonApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonsController : ControllerBase
    {
        private readonly IPokemonService _pokemonService;

        public PokemonsController(IPokemonService pokemonService)
        {
            _pokemonService = pokemonService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pokemon>>> GetAll()
        {
            var pokemons = await _pokemonService.GetAllPokemonsAsync();
            return Ok(pokemons);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pokemon>> GetById(int id)
        {
            var pokemon = await _pokemonService.GetPokemonByIdAsync(id);
            if (pokemon == null)
            {
                return NotFound();
            }
            return Ok(pokemon);
        }

        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<Pokemon>>> GetByType(string type)
        {
            Console.WriteLine($"Received request for type: {type}");
            
            var pokemonsByType = await _pokemonService.GetPokemonsByTypeAsync(type);

            if (pokemonsByType == null || pokemonsByType.Count == 0)
            {
                Console.WriteLine($"No Pokémon found for type: {type}");
                return NotFound($"No Pokémon found with type: {type}");
            }

            Console.WriteLine($"Found {pokemonsByType.Count} Pokémon(s) of type {type}");
            return Ok(pokemonsByType);
        }

        [HttpPost]
        public async Task<ActionResult> Add([FromBody] Pokemon newPokemon)
        {
            await _pokemonService.AddPokemonAsync(newPokemon);
            return CreatedAtAction(nameof(GetById), new { id = newPokemon.Id }, newPokemon);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] Pokemon updatedPokemon)
        {
            await _pokemonService.UpdatePokemonAsync(id, updatedPokemon);
            return Ok(updatedPokemon);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _pokemonService.DeletePokemonAsync(id);
            return NoContent();
        }
    }
}
