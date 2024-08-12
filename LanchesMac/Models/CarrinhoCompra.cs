using LanchesMac.Context;
using Microsoft.EntityFrameworkCore;

namespace LanchesMac.Models
{
	//8 classe criada
	public class CarrinhoCompra
	{
		private readonly AppDbContext _context;

		public CarrinhoCompra(AppDbContext context)
		{
			_context = context;
		}

		public string CarrinhoCompraId { get; set; }
		public List<CarrinhoCompraItem> CarrinhoCompraItems { get; set; }

		public static CarrinhoCompra GetCarrinho(IServiceProvider services)
		{
			//define uma sessão
			ISession session =
				services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;

			//obtem um serviço do tipo do nosso contexto
			var context = services.GetService<AppDbContext>();
			
			//obtem ou gera o Id do carrinho
			string carrinhoId = session.GetString("CarrinhoId") ?? Guid.NewGuid().ToString();

			//atribui o id do carrinho na Sessão
			session.SetString("CarrinhoId", carrinhoId);

			//retorna o carrinho com o contexto e o Id atribuido ou obtido
			return new CarrinhoCompra(context)
			{
				CarrinhoCompraId = carrinhoId
			};
		}

		//9 46 Adicionar itens ao carrinho de compras

		public void AdicionarAoCarrinho(Lanche lanche)
		{
			// Verifica se o item já está no carrinho de compras
			var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
				s => s.Lanche.LancheId == lanche.LancheId &&
				s.CarrinhoCompraId == CarrinhoCompraId);

			// Se o item não estiver no carrinho, cria um novo item e adiciona ao carrinho
			if (carrinhoCompraItem == null )
			{
				carrinhoCompraItem = new CarrinhoCompraItem
				{
					CarrinhoCompraId = CarrinhoCompraId,
					Lanche = lanche,
					Quantidade = 1
				};
				_context.CarrinhoCompraItens.Add(carrinhoCompraItem);
			}
			else
			{
				// Se o item já estiver no carrinho, incrementa a quantidade
				carrinhoCompraItem.Quantidade++;
			}
			// Salva as mudanças no contexto de banco de dados
			_context.SaveChanges();
		}

		public int RemoverDoCarrinho(Lanche lanche)
		{
			var carrinhoCompraItem = _context.CarrinhoCompraItens.SingleOrDefault(
				   s => s.Lanche.LancheId == lanche.LancheId &&
				   s.CarrinhoCompraId == CarrinhoCompraId);

			var quantidadeLocal = 0;

			if (carrinhoCompraItem != null)
			{
				if (carrinhoCompraItem.Quantidade > 1)
				{
					carrinhoCompraItem.Quantidade--;
					quantidadeLocal = carrinhoCompraItem.Quantidade;
				}
				else
				{
					_context.CarrinhoCompraItens.Remove(carrinhoCompraItem);
				}
			}
			_context.SaveChanges();
			return quantidadeLocal;
		}

		public List<CarrinhoCompraItem> GetCarrinhoCompraItens()
		{
			return CarrinhoCompraItems ??
				   (CarrinhoCompraItems =
					   _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
						   .Include(s => s.Lanche)
						   .ToList());
		}

		public void LimparCarrinho()
		{
			var carrinhoItens = _context.CarrinhoCompraItens
								 .Where(carrinho => carrinho.CarrinhoCompraId == CarrinhoCompraId);

			_context.CarrinhoCompraItens.RemoveRange(carrinhoItens);
			_context.SaveChanges();
		}

		public decimal GetCarrinhoCompraTotal()
		{
			var total = _context.CarrinhoCompraItens.Where(c => c.CarrinhoCompraId == CarrinhoCompraId)
				.Select(c => c.Lanche.Preco * c.Quantidade).Sum();
			return total;
		}

	}
}
