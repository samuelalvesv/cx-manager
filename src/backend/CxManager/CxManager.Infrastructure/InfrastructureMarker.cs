using System.Reflection;

namespace CxManager.Infrastructure;

public static class InfrastructureMarker
{
    public static readonly Assembly Assembly = typeof(InfrastructureMarker).Assembly;
}
