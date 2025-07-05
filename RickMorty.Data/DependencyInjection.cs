using Microsoft.Extensions.DependencyInjection;
using RickMorty.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickMorty.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddData(this IServiceCollection services)
        {
            services.AddData<ICharacterRepository, CharacterRepository>();
            return services;
        } 
    }
}
