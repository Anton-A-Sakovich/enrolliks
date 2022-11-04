using System.Collections.Generic;
using Enrolliks.Persistence;

namespace Enrolliks.Web.Controllers.People
{
    public enum GetAllPeopleResultType
    {
        Success = 0,
        RepositoryFailure = 1,
    }

    public class GetAllPeopleResultModel
    {
        public GetAllPeopleResultModel(IGetAllPeopleResult result)
        {
            switch (result)
            {
                case IGetAllPeopleResult.Success(var people):
                    Type = GetAllPeopleResultType.Success;
                    People = people;
                    break;

                case IGetAllPeopleResult.RepositoryFailure:
                    Type = GetAllPeopleResultType.RepositoryFailure;
                    break;
            }
        }

        public GetAllPeopleResultType Type { get; }

        public IList<Person>? People { get; }
    }
}
