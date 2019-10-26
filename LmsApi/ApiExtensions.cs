using System;

namespace Ojb500.EcfLms
{
    internal static class InternalApiExtensions
    {
        public static T GetOne<T>(this IApi api, string file, string org, string name)
        {
            var results = api.Get<T>(file, org, name);
            if (results.Length != 1)
            {
                if (results.Length > 1)
                {
                    throw new InvalidOperationException($"Expected 1 result, got {results.Length}");
                }
                return default;
            }
            return results[0];
        }
    }

    public static class ApiExtensions
    {
        public static Organisation GetOrganisation(this IApi api, int organisationId)
            => new Organisation(api, organisationId);
    }
}
