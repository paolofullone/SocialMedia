using CQRS.Core.Queries;

namespace CQRS.Core.Infrastructure
{
    public interface IQueryDispatcher<TEntity> // we are going to use generic types that will be one of our entities.
    {
        // o register handler recebe uma function delegate, que recebe um parâmetro TQuery (que é um dos nossos objetos concretos de query)
        // e retorna uma Task com uma lista de entidades genéricas (que no nosso caso é uma lista de Posts com base em uma busca pela query type)
        // chamamos o parâmetro de handler e TQuery extende BaseQuery.
        void RegisterHandler<TQuery>(Func<TQuery, Task<List<TEntity>>> handler) where TQuery : BaseQuery;

        // Na SendAsync passamos apenas BaseQuery de propósito para mostrar o Liskov Substitution Principle, que a classe base pode ser substituída
        // por uma classe concreta ou especializada sem afetar o funcionamento da aplicação.
        Task<List<TEntity>> SendAsync(BaseQuery query);
    }

    // desta forma, nesta classe aplicou generics e liskov substitution principle.
}