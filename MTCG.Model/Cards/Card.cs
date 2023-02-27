namespace MTCG.Model.Cards
{
    public class Card
    {

        public Guid cardId { get; set; }
        public string Name { get; set; }
        public int Damage { get; set; }
        public ElementType elementType { get; set; }

        public CardType cardType { get; set; }

        public Card(string name, int damage, ElementType elementType, CardType cardType)
        {
            this.Name = name;
            this.Damage = damage;
            this.elementType = elementType;
            this.cardType = cardType;
        }

        public enum ElementType
        {
            Fire,
            Water,
            Normal
        }

        public enum CardType
        {
            Spell,
            Monster
        }
    }
}
