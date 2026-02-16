namespace NDepend.Mcp.Services {
    public static class ServiceCollectionExtensions {

        public static IServiceCollection WithNDependToolsServices(this IServiceCollection services) {
            services.AddSingleton<INDependService, NDependService>();
            return services;
        }

        public static IMcpServerBuilder WithNDependTools(this IMcpServerBuilder builder) {
            var toolAssembly = typeof(ServiceCollectionExtensions).Assembly;
            return builder
                .WithToolsFromAssembly(toolAssembly)
                .WithPromptsFromAssembly(toolAssembly);
        }
    }
}
