using BlueBerryFinance.API.Infrastructure.Utils.Helpers.Interfaces;
using System.Reflection;

namespace BlueBerryFinance.API.Infrastructure.Utils.Helpers
{
    public class PromptLoader : IPromptLoader
    {
        public string Load(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = $"BlueBerryFinance.API.{resourceName.Replace("/", ".").Replace("\\", ".")}";

            using var stream = assembly.GetManifestResourceStream(fullResourceName)
                ?? throw new InvalidOperationException($"Prompt resource '{fullResourceName}' not found.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
