using System;

namespace Ojb500.EcfLms
{
    public static class ApiExtensions
    {
        public static Organisation GetOrganisation(this IModel api, int organisationId)
            => new Organisation(api, organisationId);
    }
}
