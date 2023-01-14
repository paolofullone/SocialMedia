using CQRS.Core.Queries;

namespace CQRS.Core.Infrastructure
{
    public interface IQueryDispatcher<TEntity> // we are going to use generic types that will be one of our entities.
    {
        // o register handler recebe uma function delegate, que recebe um par�metro TQuery (que � um dos nossos objetos concretos de query)
        // e retorna uma Task com uma lista de entidades gen�ricas (que no nosso caso � uma lista de Posts com base em uma busca pela query type)
        // chamamos o par�metro de handler e TQuery extende BaseQuery.
        void RegisterHandler<TQuery>(Func<TQuery, Task<List<TEntity>>> handler) where TQuery : BaseQuery;

        // Na SendAsync passamos apenas BaseQuery de prop�sito para mostrar o Liskov Substitution Principle, que a classe base pode ser substitu�da
        // por uma classe concreta ou especializada sem afetar o funcionamento da aplica��o.
        Task<List<TEntity>> SendAsync(BaseQuery query);
    }

    // desta forma, nesta classe aplicou generics e liskov substitution principle.
}