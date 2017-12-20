using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alura.Loja.Testes.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var contexto = new LojaContext())
            {
                var cliente = contexto
                    .Clientes
                    .Include(c => c.EnderecoDeEntrega)
                    .FirstOrDefault();

                Console.WriteLine($"Endereço de Entrega: {cliente.EnderecoDeEntrega.Logradouro}");

                var produto = contexto
                    .Produtos
                    .Where(p => p.Id == 5005)
                    .FirstOrDefault();

                // aplicando filtro em objetos relacionados
                contexto.Entry(produto)
                    .Collection(p => p.Compras)
                    .Query()
                    .Where(c => c.Preco > 10)
                    .Load();

                Console.WriteLine($"Mostrando as compras do produto: {produto.Nome}");
                foreach (var item in produto.Compras)
                {
                    Console.WriteLine(item);
                }
            }
        }

        private static void ExibeCompras()
        {
            //JOIN uma para muitos
            using (var contexto = new LojaContext())
            {
                var cliente = contexto
                    .Clientes
                    .Include(c => c.EnderecoDeEntrega)
                    .FirstOrDefault();

                Console.WriteLine($"Endereço de Entrega: {cliente.EnderecoDeEntrega.Logradouro}");

                var produto = contexto
                    .Produtos
                    .Include(p => p.Compras)
                    .Where(p => p.Id == 5005)
                    .FirstOrDefault();

                Console.WriteLine($"Mostrando as compras do produto: {produto.Nome}");
                foreach (var item in produto.Compras)
                {
                    Console.WriteLine(item);
                }
            }
        }

        private static void ExibePromocoes()
        {
            //JOIN muitos para muitos
            using (var contexto = new LojaContext())
            {
                var promocao = contexto
                    .Promocoes
                    .Include(p => p.Produtos)
                    .ThenInclude(pp => pp.Produto)
                    .FirstOrDefault();


                foreach (var item in promocao.Produtos)
                {
                    Console.WriteLine(item.Produto);
                }
            }
        }

        private static void IncluirPromocao()
        {
            using (var contexto = new LojaContext())
            {
                var promocao = new Promocao();
                promocao.Descricao = "Queima Total jan/18";
                promocao.DataInicio = new DateTime(2018, 1, 1);
                promocao.DataTermino = new DateTime(2018, 1, 31);

                var produtos = contexto
                    .Produtos
                    .Where(p => p.Categoria == "Bebidas")
                    .ToList();

                foreach (var item in produtos)
                {
                    promocao.IncluiProduto(item);
                }

                contexto.Promocoes.Add(promocao);
            }
        }

        private static void TestarRelacionamentoUmParaUm()
        {
            var fulano = new Cliente();
            fulano.Nome = "Fulaninho de Tal";
            fulano.EnderecoDeEntrega = new Endereco()
            {
                Numero = 12,
                Logradouro = "Rua Piratininga",
                Complemento = "Ap 63a",
                Bairro = "Barcelona",
                Cidade = "São Caetando do Sul"
            };

            using (var contexto = new LojaContext())
            {
                // código para gerar log SQL
                GerarLog(contexto);

                contexto.Clientes.Add(fulano);
                contexto.SaveChanges();
            }
        }

        private static void TestarRelacionamentoMuitosParaMuitos()
        {
            var p1 = new Produto() { Nome = "Suco de Laranja", Categoria = "Bebida", PrecoUnitario = 10.90, Unidade = "Litros" };
            var p2 = new Produto() { Nome = "Café", Categoria = "Bebida", PrecoUnitario = 4.90, Unidade = "Gramas" }; ;
            var p3 = new Produto() { Nome = "Macarrão", Categoria = "Alimentos", PrecoUnitario = 3.20, Unidade = "Gramas" };

            var promocaoDePascoa = new Promocao();
            promocaoDePascoa.Descricao = "Páscoa Feliz";
            promocaoDePascoa.DataInicio = DateTime.Now;
            promocaoDePascoa.DataTermino = DateTime.Now.AddMonths(3);

            promocaoDePascoa.IncluiProduto(p1);
            promocaoDePascoa.IncluiProduto(p2);
            promocaoDePascoa.IncluiProduto(p3);

            using (var contexto = new LojaContext())
            {
                // código para gerar log SQL
                GerarLog(contexto);

                contexto.Promocoes.Add(promocaoDePascoa);
                contexto.SaveChanges();
            }
        }

        private static void TestarRelacionamentoUmParaMuitos()
        {
            var caneta = new Produto();
            caneta.Nome = "Caneta esferográfica azul";
            caneta.Categoria = "Papelaria";
            caneta.PrecoUnitario = 1.50;
            caneta.Unidade = "Unidade";

            var compra = new Compra();
            compra.Quantidade = 2;
            compra.Produto = caneta;
            compra.Preco = caneta.PrecoUnitario * compra.Quantidade;

            using (var contexto = new LojaContext())
            {
                // código para gerar log SQL
                GerarLog(contexto);

                contexto.Compras.Add(compra);
                contexto.SaveChanges();
            }
        }

        private static void VerificadorDeEstados(LojaContext contexto)
        {
            // desliga o monitoramento, melhora performance
            //contexto.ChangeTracker.AutoDetectChangesEnabled = false; 

            // código para gerar log SQL
            GerarLog(contexto);

            // limpar e adicionar registros no repositório
            IniciarRegistros();

            var produtos = contexto.Produtos.ToList();
            ExibeEntries(contexto.ChangeTracker.Entries(), "UNCHANGED");

            // estados: unchanged -> modified
            var p1 = produtos.First();
            p1.Nome = "Into The Wild";
            ExibeEntries(contexto.ChangeTracker.Entries(), "MODIFIED");

            //estados: added
            var novoProduto = new Produto()
            {
                Nome = "Rocky Balboa",
                Categoria = "Filmes",
                PrecoUnitario = 25.99
            };
            contexto.Produtos.Add(novoProduto);
            ExibeEntries(contexto.ChangeTracker.Entries(), "ADDED");

            //estado: detached
            contexto.Produtos.Remove(novoProduto);
            var entry = contexto.Entry(novoProduto);
            ExibeEntries(contexto.ChangeTracker.Entries(), "DETACHED");

            //estados: deleted
            contexto.Produtos.Remove(contexto.Produtos.First());
            ExibeEntries(contexto.ChangeTracker.Entries(), "DELETED");

            contexto.SaveChanges();
            ExibeEntries(contexto.ChangeTracker.Entries(), "UNCHANGED");
        }

        private static void IniciarRegistros()
        {
            ExcluirProdutos();
            GravarUsandoEntity();
        }

        private static void GerarLog(LojaContext contexto)
        {
            var serviceProvider = contexto.GetInfrastructure<IServiceProvider>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.AddProvider(SqlLoggerProvider.Create());
        }

        private static void ExibeEntries(IEnumerable<EntityEntry> entries, string stateInfo)
        {
            Console.WriteLine("========"+stateInfo+"========");
            foreach (var e in entries)
            {
                Console.WriteLine(e.Entity.ToString() + " - " + e.State);
            }
        }

        private static void AtualizarProduto()
        {
            GravarUsandoEntity();
            RecuperarProdutos();

            using (var repo = new ProdutoDAOEntity())
            {
                Produto primeiro = repo.Produtos().First();

                primeiro.Nome = "Into The Wild";
                repo.Atualizar(primeiro);
            }

            RecuperarProdutos();
        }

        private static void ExcluirProdutos()
        {
            using (var repo = new ProdutoDAOEntity())
            {
                IList<Produto> produtos = repo.Produtos();

                foreach (var p in produtos)
                {
                    repo.Remover(p);
                }

            }
        }

        private static void RecuperarProdutos()
        {
            using (var repo = new ProdutoDAOEntity())
            {
                IList<Produto> produtos = repo.Produtos();
                Console.WriteLine("Foram encontrados {0} produtos", produtos.Count);

                foreach(var p in produtos)
                {
                    Console.WriteLine(p.Nome);
                }
            }
        }

        private static void GravarUsandoEntity()
        {
            Produto p1 = new Produto();
            p1.Nome = "Harry Potter e a Ordem da Fênix";
            p1.Categoria = "Livros";
            p1.PrecoUnitario = 19.89;

            Produto p2 = new Produto();
            p2.Nome = "Senhor dos Anéis 1";
            p2.Categoria = "Livros";
            p2.PrecoUnitario = 19.89;

            Produto p3 = new Produto();
            p3.Nome = "O Monge e o Executivo";
            p3.Categoria = "Livros";
            p3.PrecoUnitario = 19.89;

            using (var repo = new ProdutoDAOEntity())
            {
                repo.Adicionar(p1);
                repo.Adicionar(p2);
                repo.Adicionar(p3);
            }
        }

    }
}
