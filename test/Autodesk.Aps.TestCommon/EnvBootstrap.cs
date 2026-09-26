namespace Autodesk.Aps.TestCommon;

/// <summary>
/// 	Loads a local <c>.env</c> file so a developer run reads the same values CI reads.
/// </summary>
/// <remarks>
/// 	Deliberately not a <c>[ModuleInitializer]</c>.
/// 	A module initializer in a referenced library fires on first access to a type in that
/// 	module, which is ordering-sensitive relative to <c>[ClassInitialize]</c>.
/// 	<see cref="ApsTestConfig"/> calls <see cref="Ensure"/> from its static constructor instead,
/// 	which cannot fire too late.
/// </remarks>
internal static class EnvBootstrap
{
    private static readonly Lazy<bool> _loaded = new(() =>
    {
        // CI reads only the process environment. A stray .env file must not shadow a GitHub secret.
        // Reading IsCi here is safe: this runs inside the static constructor of ApsTestConfig, on the same thread.
        if (ApsTestConfig.IsCi)
        {
            return false;
        }

        // NoClobber keeps a value that is already set in the environment.
        // Without it, a .env file would silently replace a value the developer set on purpose.
        DotNetEnv.Env.NoClobber().TraversePath().Load();

        return true;
    });

    internal static void Ensure() => _ = _loaded.Value;
}
