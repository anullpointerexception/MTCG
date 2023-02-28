using static MTCG.Model.Cards.Card;

namespace MTCG.Model.Deal
{
    public class Deal
    {
        public int Id { get; set; }

        public CardType Type { get; set; }
        public Guid CardId { get; set; }

        public double MinDamage { get; set; }

    }
}
