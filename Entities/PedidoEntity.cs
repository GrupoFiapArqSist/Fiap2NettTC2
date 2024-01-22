using System.Collections.Generic;

namespace DurableFunction.Entities
{
    public class PedidoEntity
    {
        public string NumeroPedido { get; set; }       
        public string Status { get; set; }
        public List<PedidoItemEntity> PedidoItens { get; set; }
    }
}
