namespace Ojb500.EcfLms
{
    public static class ApiExtensions
    {
        /// <summary>
        /// Creates an <see cref="Organisation"/> for the given ECF LMS organisation ID.
        /// This is the main entry point for the library.
        /// </summary>
        public static Organisation GetOrganisation(this IModel api, int organisationId)
            => new Organisation(api, organisationId);
    }
}
