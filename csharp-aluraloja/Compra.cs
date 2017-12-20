using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alura.Loja.Testes.ConsoleApp
{
    public class Compra
    {
        public int Id { get; set; }
        public int Quantidade { get; internal set; }
        public int ProdutoId { get; internal set; }
        public Produto Produto { get; internal set; }
        public Double Preco { get; internal set; }

        public override string ToString()
        {
            return $"Compra de {this.Quantidade} do Produto: {this.Produto.Nome} no valor de: {this.Preco}";
        }
    }
}
