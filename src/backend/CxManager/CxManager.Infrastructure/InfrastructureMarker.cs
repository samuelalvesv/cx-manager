using System.Reflection;
using Dapper;

[module: DapperAot]

namespace CxManager.Infrastructure;

public static class InfrastructureMarker
{
    public static readonly Assembly Assembly = typeof(InfrastructureMarker).Assembly;
}
